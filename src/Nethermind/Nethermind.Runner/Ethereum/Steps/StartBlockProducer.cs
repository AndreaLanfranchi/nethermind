//  Copyright (c) 2018 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nethermind.AuRa.Rewards;
using Nethermind.AuRa.Validators;
using Nethermind.Blockchain;
using Nethermind.Blockchain.Rewards;
using Nethermind.Blockchain.TxPools;
using Nethermind.Blockchain.Validators;
using Nethermind.Core;
using Nethermind.Core.Specs;
using Nethermind.Evm;
using Nethermind.Logging;
using Nethermind.Runner.Ethereum.Context;
using Nethermind.Runner.Ethereum.Subsystems;
using Nethermind.Store;

namespace Nethermind.Runner.Ethereum.Steps
{
    [RunnerStepDependencies(typeof(StartBlockProcessor), typeof(SetupKeyStore), typeof(ReviewBlockTree))]
    public abstract class StartBlockProducer : IStep, ISubsystemStateAware
    {
        private readonly EthereumRunnerContext _context;

        public StartBlockProducer(EthereumRunnerContext context)
        {
            _context = context;
        }

        public Task Execute()
        {
            IInitConfig initConfig = _context.Config<IInitConfig>();
            if (initConfig.IsMining)
            {
                BuildProducer();
                if (_context.BlockProducer == null) throw new StepDependencyException(nameof(_context.BlockProducer));

                _context.BlockProducer.Start();

                SubsystemStateChanged?.Invoke(this, new SubsystemStateEventArgs(EthereumSubsystemState.Running));
            }

            return Task.CompletedTask;
        }

        protected virtual void BuildProducer()
        {
            if (_context.ChainSpec == null) throw new StepDependencyException(nameof(_context.ChainSpec));
            throw new NotSupportedException($"Mining in {_context.ChainSpec.SealEngineType} mode is not supported");
        }

        protected BlockProducerContext GetProducerChain()
        {
            ReadOnlyDbProvider readOnlyDbProvider = new ReadOnlyDbProvider(_context.DbProvider, false);
            ReadOnlyBlockTree readOnlyBlockTree = new ReadOnlyBlockTree(_context.BlockTree);
            ReadOnlyTxProcessingEnv readOnlyTxProcessingEnv = new ReadOnlyTxProcessingEnv(readOnlyDbProvider, readOnlyBlockTree, _context.SpecProvider, _context.LogManager);
            BlockProcessor blockProcessor = CreateBlockProcessor(readOnlyTxProcessingEnv, readOnlyDbProvider);
            OneTimeChainProcessor chainProcessor = new OneTimeChainProcessor(readOnlyDbProvider, new BlockchainProcessor(readOnlyBlockTree, blockProcessor, _context.RecoveryStep, _context.LogManager, false));
            PendingTxSelector pendingTxSelector = new PendingTxSelector(_context.TxPool, readOnlyTxProcessingEnv.StateProvider, _context.LogManager);

            return new BlockProducerContext
            {
                ChainProcessor = chainProcessor, 
                ReadOnlyStateProvider = readOnlyTxProcessingEnv.StateProvider, 
                PendingTxSelector = pendingTxSelector
            };
        }

        protected virtual BlockProcessor CreateBlockProcessor(ReadOnlyTxProcessingEnv readOnlyTxProcessingEnv, IReadOnlyDbProvider readOnlyDbProvider)
        {
            if (_context.SpecProvider == null) throw new StepDependencyException(nameof(_context.SpecProvider));
            if (_context.BlockValidator == null) throw new StepDependencyException(nameof(_context.BlockValidator));
            if (_context.RewardCalculatorSource == null) throw new StepDependencyException(nameof(_context.RewardCalculatorSource));
            if (_context.ReceiptStorage == null) throw new StepDependencyException(nameof(_context.ReceiptStorage));
            if (_context.TxPool == null) throw new StepDependencyException(nameof(_context.TxPool));

            return new BlockProcessor(
                _context.SpecProvider,
                _context.BlockValidator,
                _context.RewardCalculatorSource.Get(readOnlyTxProcessingEnv.TransactionProcessor),
                readOnlyTxProcessingEnv.TransactionProcessor,
                readOnlyDbProvider.StateDb,
                readOnlyDbProvider.CodeDb,
                readOnlyTxProcessingEnv.StateProvider,
                readOnlyTxProcessingEnv.StorageProvider,
                _context.TxPool,
                _context.ReceiptStorage,
                _context.LogManager);
        }

        public event EventHandler<SubsystemStateEventArgs>? SubsystemStateChanged;

        public EthereumSubsystem MonitoredSubsystem => EthereumSubsystem.Mining;
    }
}