// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit
{
    using System;
    using BusConfigurators;
	using Pipeline.Configuration;
    using RabbitMQ.Client;
    using Transports.RabbitMq;

	/// <summary>
	/// Extensions for configuring a RabbitMq-based endpoint.
	/// </summary>
	public static class RabbitMqConfigurationExtensions
	{
		/// <summary>
		/// <para>This specifies that the routing conventions for RabbitMQ should be used by MassTransit.
		/// Without these conventions, the automatic routing for RabbitMQ won't happen, and you'd have
		/// to manually find and send messages to the endpoints. This method calls
		/// <see cref="RabbitMqServiceBusExtensions.UseRabbitMq{T}(T)"/> in turn.</para>
		/// 
		/// <para>If you are using RMQ you *probably* want to call this method when configuring
		/// your bus!</para>
		/// </summary>
		/// <param name="configurator">The configurator that is used to configure
		/// the message bus instance.</param>
		public static void UseRabbitMqRouting(this ServiceBusConfigurator configurator)
		{
			configurator.SetSubscriptionObserver((bus,coordinator) => new RabbitMqSubscriptionBinder(bus));

			var busConfigurator = new PostCreateBusBuilderConfigurator(bus =>
				{
					var interceptorConfigurator = new OutboundMessageInterceptorConfigurator(bus.OutboundPipeline);

					// make sure we publish correctly through this interceptor; works on the outgoing pipeline
					interceptorConfigurator.Create(new PublishEndpointInterceptor(bus));
				});

			configurator.AddBusConfigurator(busConfigurator);

			configurator.UseRabbitMq();
		}

		/// <summary>
		/// Gets the URI from the data in the connection factory.
		/// </summary>
		/// <param name="factory">Factory to scan for data.</param>
		/// <returns>A URI corresponding to the endpoint of this factory.</returns>
        public static Uri GetUri(this ConnectionFactory factory)
        {
            return new UriBuilder("rabbitmq", factory.HostName, factory.Port, factory.VirtualHost)
                {
                    UserName = factory.UserName,
                    Password = factory.Password,
                }.Uri;
        }
	}
}