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
    using Exceptions;
    using log4net;
    using Magnum.Extensions;
    using Magnum.Threading;
    using Ultralight.Client;

    public class StompTransportFactory : ITransportFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (StompTransportFactory));

        private readonly ReaderWriterLockedDictionary<Uri, StompClient> _connectionCache =
            new ReaderWriterLockedDictionary<Uri, StompClient>();

        private bool _disposed;

        /// <summary>
        ///   Gets the scheme.
        /// </summary>
        public string Scheme
        {
            get { return "stomp"; }
        }

        /// <summary>
        ///   Builds the loopback.
        /// </summary>
        /// <param name = "settings">The settings.</param>
        /// <returns></returns>
        public IDuplexTransport BuildLoopback(ITransportSettings settings)
        {
            var client = GetConnection(settings.Address);
            return new StompTransport(settings.Address, client);
        }

        /// <summary>
        ///   Builds the inbound.
        /// </summary>
        /// <param name = "settings">The settings.</param>
        /// <returns></returns>
        public IInboundTransport BuildInbound(ITransportSettings settings)
        {
            return BuildLoopback(settings);
        }

        /// <summary>
        ///   Builds the outbound.
        /// </summary>
        /// <param name = "settings">The settings.</param>
        /// <returns></returns>
        public IOutboundTransport BuildOutbound(ITransportSettings settings)
        {
            return BuildLoopback(settings);
        }

        /// <summary>
        ///   Builds the error.
        /// </summary>
        /// <param name = "settings">The settings.</param>
        /// <returns></returns>
        public IOutboundTransport BuildError(ITransportSettings settings)
        {
            return BuildLoopback(settings);
        }

        /// <summary>
        ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _connectionCache.Values.Each(
                    x =>
                        {
                            try
                            {
                                if (x.IsConnected)
                                    x.Disconnect();

                                x.Dispose();
                            }
                            catch (Exception ex)
                            {
                                Log.Warn("Failed to close Stomp connection.", ex);
                            }
                        });
            }

            _disposed = true;
        }

        /// <summary>
        ///   Gets the connection.
        /// </summary>
        /// <param name = "address">The address.</param>
        /// <returns></returns>
        private StompClient GetConnection(IEndpointAddress address)
        {
            EnsureProtocolIsCorrect(address.Uri);

            var serverAddress = new UriBuilder("ws", address.Uri.Host, address.Uri.Port).Uri;

            return _connectionCache
                .Retrieve(address.Uri,
                          () =>
                              {
                                  var client = new StompClient();
                                  client.Connect(serverAddress);
                                  return client;
                              });
        }

        /// <summary>
        ///   Ensures the protocol is correct.
        /// </summary>
        /// <param name = "address">The address.</param>
        private void EnsureProtocolIsCorrect(Uri address)
        {
            if (address.Scheme != Scheme)
                throw new EndpointException(address,
                                            string.Format("Address must start with 'stomp' not '{0}'", address.Scheme));
        }
    }
}