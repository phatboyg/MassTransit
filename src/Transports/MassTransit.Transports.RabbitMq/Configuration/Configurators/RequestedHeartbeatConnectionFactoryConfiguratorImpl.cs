using System.Collections.Generic;
using MassTransit.Configurators;
using MassTransit.Transports.RabbitMq.Configuration.Builders;
using RabbitMQ.Client;

namespace MassTransit.Transports.RabbitMq.Configuration.Configurators
{
	public class RequestedHeartbeatConnectionFactoryConfiguratorImpl : ConnectionFactoryBuilderConfigurator
	{
		private readonly ushort _requestedHeartbeat;

		public RequestedHeartbeatConnectionFactoryConfiguratorImpl(ushort requestedHeartbeat)
		{
			_requestedHeartbeat = requestedHeartbeat;
		}

		public IEnumerable<ValidationResult> Validate()
		{
			return new ValidationResult[0];
		}

		public ConnectionFactoryBuilder Configure(ConnectionFactoryBuilder builder)
		{
			builder.Add(Configure);
			return builder;
		}

		private ConnectionFactory Configure(ConnectionFactory connectionFactory)
		{
			connectionFactory.RequestedHeartbeat = _requestedHeartbeat;
			return connectionFactory;
		}
	}
}