﻿/*
 * Copyright (c) 2018 Demerzel Solutions Limited
 * This file is part of the Nethermind library.
 *
 * The Nethermind library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The Nethermind library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using Nevermind.Core;
using Nevermind.Discovery.Messages;
using Nevermind.Discovery.RoutingTable;

namespace Nevermind.Discovery.Lifecycle
{
    public class NodeLifecycleManagerFactory : INodeLifecycleManagerFactory
    {
        private readonly INodeFactory _nodeFactory;
        private readonly INodeTable _nodeTable;
        private readonly ILogger _logger;
        private readonly IDiscoveryConfigurationProvider _discoveryConfigurationProvider;
        private readonly IMessageFactory _messageFactory;
        private readonly IEvictionManager _evictionManager;

        public NodeLifecycleManagerFactory(INodeFactory nodeFactory, INodeTable nodeTable, ILogger logger, IDiscoveryConfigurationProvider discoveryConfigurationProvider, IMessageFactory messageFactory, IEvictionManager evictionManager)
        {
            _nodeFactory = nodeFactory;
            _nodeTable = nodeTable;
            _logger = logger;
            _discoveryConfigurationProvider = discoveryConfigurationProvider;
            _messageFactory = messageFactory;
            _evictionManager = evictionManager;
        }

        public IDiscoveryManager DiscoveryManager { private get; set; }

        public INodeLifecycleManager CreateNodeLifecycleManager(Node node)
        {
            if (DiscoveryManager == null)
            {
                throw new Exception("DiscoveryManager has to be set");
            }
            return new NodeLifecycleManager(node, DiscoveryManager, _nodeTable, _logger, _discoveryConfigurationProvider, _messageFactory, _evictionManager);
        }

        public INodeLifecycleManager CreateNodeLifecycleManager(byte[] id, string host, int port)
        {
            if (DiscoveryManager == null)
            {
                throw new Exception("DiscoveryManager has to be set");
            }
            var node = _nodeFactory.CreateNode(id, host, port);
            return new NodeLifecycleManager(node, DiscoveryManager, _nodeTable, _logger, _discoveryConfigurationProvider, _messageFactory, _evictionManager);
        }
    }
}