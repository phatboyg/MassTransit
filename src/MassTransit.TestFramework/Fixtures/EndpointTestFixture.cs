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
namespace MassTransit.TestFramework.Fixtures
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using BusConfigurators;
	using Configurators;
	using EndpointConfigurators;
	using Exceptions;
	using Magnum.Extensions;
	using MassTransit.Transports;
	using NUnit.Framework;
	using Saga;
	using Services.Subscriptions;

	[TestFixture]
	public class EndpointTestFixture<TTransportFactory> :
		AbstractTestFixture
		where TTransportFactory : ITransportFactory, new()
	{
		[TestFixtureSetUp]
		public void Setup()
		{
			if (EndpointFactoryConfigurator != null)
			{
				ConfigurationResult result = ConfigurationResultImpl.CompileResults(EndpointFactoryConfigurator.Validate());

				try
				{
					EndpointFactory = EndpointFactoryConfigurator.CreateEndpointFactory();
					EndpointFactoryConfigurator = null;

					_endpointCache = new EndpointCache(EndpointFactory);
					EndpointCache = new EndpointCacheProxy(_endpointCache);
				}
				catch (Exception ex)
				{
					throw new ConfigurationException(result, "An exception was thrown during endpoint cache creation", ex);
				}
			}

			ServiceBusFactory.ConfigureDefaultSettings(x =>
				{
					x.SetEndpointCache(EndpointCache);
					x.SetConcurrentConsumerLimit(4);
					x.SetReceiveTimeout(150.Milliseconds());
					x.EnableAutoStart();
				});
		}

		[TestFixtureTearDown]
		public void FixtureTeardown()
		{
			TeardownBuses();

			if (EndpointCache != null)
			{
				_endpointCache.Dispose();
				_endpointCache = null;
				EndpointCache = null;
			}

			ServiceBusFactory.ConfigureDefaultSettings(x => { x.SetEndpointCache(null); });
		}

		protected EndpointTestFixture()
		{
			Buses = new List<IServiceBus>();

			var defaultSettings = new EndpointFactoryDefaultSettings();

			EndpointFactoryConfigurator = new EndpointFactoryConfiguratorImpl(defaultSettings);
			EndpointFactoryConfigurator.AddTransportFactory<TTransportFactory>();
			EndpointFactoryConfigurator.SetPurgeOnStartup(true);
		}

		protected void AddTransport<T>()
			where T : ITransportFactory, new()
		{
			EndpointFactoryConfigurator.AddTransportFactory<T>();
		}

		protected IEndpointFactory EndpointFactory { get; private set; }

		protected void ConfigureEndpointFactory(Action<EndpointFactoryConfigurator> configure)
		{
			if (EndpointFactoryConfigurator == null)
				throw new ConfigurationException("The endpoint factory configurator has already been executed.");

			configure(EndpointFactoryConfigurator);
		}

		protected void ConnectSubscriptionService(ServiceBusConfigurator configurator,
		                                          ISubscriptionService subscriptionService)
		{
			configurator.AddService(BusServiceLayer.Session, () => new SubscriptionPublisher(subscriptionService));
			configurator.AddService(BusServiceLayer.Session, () => new SubscriptionConsumer(subscriptionService));
		}

		protected static InMemorySagaRepository<TSaga> SetupSagaRepository<TSaga>()
			where TSaga : class, ISaga
		{
			var sagaRepository = new InMemorySagaRepository<TSaga>();

			return sagaRepository;
		}


		protected EndpointFactoryConfigurator EndpointFactoryConfigurator;
		EndpointCache _endpointCache;

		void TeardownBuses()
		{
			Buses.Reverse().Each(bus => { bus.Dispose(); });
			Buses.Clear();
		}

		protected IList<IServiceBus> Buses { get; private set; }

		protected IEndpointCache EndpointCache { get; private set; }

		protected virtual IServiceBus SetupServiceBus(Uri uri, Action<ServiceBusConfigurator> configure)
		{
			IServiceBus bus = ServiceBusFactory.New(x =>
				{
					x.ReceiveFrom(uri);

					configure(x);
				});

			Buses.Add(bus);

			return bus;
		}

		protected virtual IServiceBus SetupServiceBus(Uri uri)
		{
			return SetupServiceBus(uri, x =>
				{
					ConfigureServiceBus(uri, x);
				});
		}

		protected virtual void ConfigureServiceBus(Uri uri, ServiceBusConfigurator configurator)
		{
		}
	}
}