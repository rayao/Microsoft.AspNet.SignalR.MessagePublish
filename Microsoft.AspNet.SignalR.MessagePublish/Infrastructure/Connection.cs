// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.SignalR.Json;
using Microsoft.AspNet.SignalR.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    public class Connection : IConnection
    {
        private readonly IMessageBus _bus;
        private readonly JsonSerializer _serializer;
        private readonly string _baseSignal;
        private readonly string _connectionId;

        private readonly IMemoryPool _pool;

        public Connection(IMessageBus newMessageBus,
                          JsonSerializer jsonSerializer,
                          string baseSignal,
                          string connectionId,
                          IMemoryPool pool)
        {
            _bus = newMessageBus;
            _serializer = jsonSerializer;
            _baseSignal = baseSignal;
            _connectionId = connectionId;
            _pool = pool;
        }

        public string DefaultSignal
        {
            get
            {
                return _baseSignal;
            }
        }

        public Action<TextWriter> WriteCursor { get; set; }

        public string Identity
        {
            get
            {
                return _connectionId;
            }
        }

        public Task Send(ConnectionMessage message)
        {
            if (!String.IsNullOrEmpty(message.Signal) &&
                message.Signals != null)
            {
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture,
                                  "Resources.Error_AmbiguousMessage",
                                  message.Signal,
                                  String.Join(", ", message.Signals)));
            }

            if (message.Signals != null)
            {
                return MultiSend(message.Signals, message.Value, message.ExcludedSignals);
            }
            else
            {
                Message busMessage = CreateMessage(message.Signal, message.Value);

                busMessage.Filter = GetFilter(message.ExcludedSignals);

                //if (busMessage.WaitForAck)
                //{
                //    Task ackTask = _ackHandler.CreateAck(busMessage.CommandId);
                //    return _bus.Publish(busMessage).Then(task => task, ackTask);
                //}
                if (busMessage.WaitForAck)
                {
                    throw new NotSupportedException("Ack not supported");
                }

                return _bus.Publish(busMessage);
            }
        }

        private Task MultiSend(IList<string> signals, object value, IList<string> excludedSignals)
        {
            if (signals.Count == 0)
            {
                // If there's nobody to send to then do nothing
                return Task.CompletedTask;
            }

            // Serialize once
            ArraySegment<byte> messageBuffer = GetMessageBuffer(value);
            string filter = GetFilter(excludedSignals);

            var tasks = new Task[signals.Count];

            // Send the same data to each connection id
            for (int i = 0; i < signals.Count; i++)
            {
                var message = new Message(_connectionId, signals[i], messageBuffer);

                if (!String.IsNullOrEmpty(filter))
                {
                    message.Filter = filter;
                }

                tasks[i] = _bus.Publish(message);
            }

            // Return a task that represents all
            return Task.WhenAll(tasks);
        }

        private static string GetFilter(IList<string> excludedSignals)
        {
            if (excludedSignals != null)
            {
                return String.Join("|", excludedSignals);
            }

            return null;
        }

        private Message CreateMessage(string key, object value)
        {
            ArraySegment<byte> messageBuffer = GetMessageBuffer(value);

            var message = new Message(_connectionId, key, messageBuffer);

            //var command = value as Command;
            //{
            //    message.CommandId = command.Id;
            //    message.WaitForAck = command.WaitForAck;
            //}

            return message;
        }

        private ArraySegment<byte> GetMessageBuffer(object value)
        {
            ArraySegment<byte> messageBuffer;
            // We can't use "as" like we do for Command since ArraySegment is a struct
            if (value is ArraySegment<byte>)
            {
                // We assume that any ArraySegment<byte> is already JSON serialized
                messageBuffer = (ArraySegment<byte>)value;
            }
            else
            {
                messageBuffer = SerializeMessageValue(value);
            }
            return messageBuffer;
        }

        private ArraySegment<byte> SerializeMessageValue(object value)
        {
            using (var writer = new MemoryPoolTextWriter(_pool))
            {
                _serializer.Serialize(value, writer);
                writer.Flush();

                var data = writer.Buffer;

                var buffer = new byte[data.Count];

                Buffer.BlockCopy(data.Array, data.Offset, buffer, 0, data.Count);

                return new ArraySegment<byte>(buffer);
            }
        }

    }
}