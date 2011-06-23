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
    using System.Collections.Generic;
    using System.Threading;
    using Magnum.Extensions;
    using Ultralight.Client;

    public class StompSubscriptionService : IBusService
    {
        private IServiceBus _bus;

        /// <summary>
        ///   Starts the specified bus.
        /// </summary>
        /// <param name = "bus">The bus.</param>
        public void Start(IServiceBus bus)
        {
            _bus = bus;

            var transports = new Queue<StompTransport>();
            transports.Enqueue(_bus.Endpoint.InboundTransport as StompTransport);

            transports
                .Each(transport =>
                          {
                              transport.StompClient.Subscribe(transport.Address.Uri.PathAndQuery);
                              WaitForSubscriptionConformation(transport.StompClient, transport.Address.Uri.PathAndQuery);
                          });
        }

        /// <summary>
        ///   Stops this instance.
        /// </summary>
        public void Stop()
        {
            var transports = new Queue<StompTransport>();
            transports.Enqueue(_bus.Endpoint.InboundTransport as StompTransport);

            transports.Each(transport => transport.StompClient.Unsubscribe(transport.Address.Uri.PathAndQuery));
        }

        private static void WaitForSubscriptionConformation(StompClient client, string queue)
        {
            var subscribed = false;
            var retryCount = 20;
            var message = "connected to:" + queue;
            var originalMessageHandler = client.OnMessage;
            
            client.OnMessage = null;
            client.OnMessage = msg => subscribed = msg.Body == message;

            while (!subscribed && retryCount > 0)
            {
                client.Send(queue, message);

                Thread.Sleep(1500);
                retryCount--;
            }

            client.OnMessage = originalMessageHandler;

            if (retryCount == 0)
            {
                throw new InvalidOperationException("Timeout waiting for stomp broker to respond");
            }
        }

        public void Dispose()
        {
        }
    }
}