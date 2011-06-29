// Copyright 2011 Ernst Naezer, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

namespace MassTransit.Transports.Stomp
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text;
    using System.Threading;
    using Context;
    using Ultralight;
    using Ultralight.Client;
    using Util;

    public class StompTransport : TransportBase
    {
        private readonly ConcurrentQueue<StompMessage> _messages = new ConcurrentQueue<StompMessage>();

        /// <summary>
        ///   Initializes a new instance of the <see cref = "StompTransport" /> class.
        /// </summary>
        /// <param name = "address">The address.</param>
        /// <param name = "client">The client.</param>
        public StompTransport(IEndpointAddress address, StompClient client)
            : base(address)
        {
            StompClient = client;
            StompClient.OnMessage += msg => _messages.Enqueue(msg);
        }

        /// <summary>
        /// Gets the stomp client.
        /// </summary>
        public StompClient StompClient { get; private set; }

        public override void Receive(Func<IReceiveContext, Action<IReceiveContext>> callback, TimeSpan timeout)
        {
            GuardAgainstDisposed();

            StompMessage message;
            if (!_messages.TryDequeue(out message))
            {
                Thread.Sleep(50);
                return;
            }

            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(message.Body)))
            {
                var context = new ConsumeContext(ms);
                context.SetMessageId(message["id"]);

                if (SpecialLoggers.Messages.IsInfoEnabled)
                    SpecialLoggers.Messages.InfoFormat("RECV:{0}:{1}", Address, context.MessageId);

                using (ContextStorage.CreateContextScope(context))
                {
                    var receive = callback(context);

                    if (receive != null)
                    {
                        receive(context);
                    }
                    else
                    {
                        if (SpecialLoggers.Messages.IsInfoEnabled)
                            SpecialLoggers.Messages.InfoFormat("SKIP:{0}:{1}", Address, context.MessageId);
                    }
                }
            }
        }

        public override void Send(ISendContext context)
        {
            GuardAgainstDisposed();

            using (var body = new MemoryStream())
            {
                context.SerializeTo(body);

                var msg = Encoding.UTF8.GetString(body.ToArray());
                StompClient.Send(Address.Uri.PathAndQuery, msg);
            }
        }

        protected override void OnDisposing()
        {
            if (StompClient != null) StompClient.Disconnect();
        }
    }
}