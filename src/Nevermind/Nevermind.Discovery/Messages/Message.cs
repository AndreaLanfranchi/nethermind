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
using System.Linq;

namespace Nevermind.Discovery.Messages
{
    public abstract class Message
    {
        public byte[] Content { get; set; }

        public byte[] Mdc { get; set; }
        public byte[] Signature { get; set; }
        public byte[] Type { get; set; }
        public byte[] Data { get; set; }

        public string Host { get; set; }
        public int Port { get; set; }

        public byte[] GetNodeId()
        {
            //TODO recover public key from signature
            return Signature;
        }

        public MessageType? MessageType => Type != null && Type.Any() ? GetMessageType() : (MessageType?)null;

        public override string ToString()
        {
            return $"Type: {MessageType}, NodeId: {GetNodeId()}, Host: {Host}, Port: {Port}";
        }

        private MessageType? GetMessageType()
        {
            if (Enum.TryParse(Type[0].ToString(), out MessageType msgType))
            {
                return msgType;
            }

            return null;
        }
    }
}
