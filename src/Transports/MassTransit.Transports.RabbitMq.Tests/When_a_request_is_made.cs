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
namespace MassTransit.Transports.RabbitMq.Tests
{
	using System;
	using Magnum;
	using Magnum.TestFramework;
	using NUnit.Framework;
	using TestFramework;
	using Magnum.Extensions;

	public class When_a_request_is_made
		: Given_a_rabbitmq_bus
	{
		const string RemoteUri = "rabbitmq://localhost/responder_queue";

		Future<Response> _response;
		Guid _corrId;

		protected override void ConfigureServiceBus(System.Uri uri, BusConfigurators.ServiceBusConfigurator configurator)
		{
			base.ConfigureServiceBus(uri, configurator);

			_response = new Future<Response>();
		}

		[When]
		public void A_request_is_made()
		{
			_corrId = CombGuid.Generate();

			RemoteBus = SetupServiceBus(new Uri(RemoteUri), conf =>
				{
					conf.UseRabbitMqRouting();
					conf.Subscribe(s =>
						{
							s.Handler<Request>((cc, r) => cc.Respond(new Response(r.CorrelationId, "Real Response")));
						});
				});

			LocalBus.GetEndpoint(new Uri(RemoteUri))
				.SendRequest(new Request(_corrId), LocalBus, conf =>
					{
						conf.SetTimeout(150.Milliseconds());
						conf.Handle<Response>(r => _response.Complete(r));
					});
		}

		[Then]
		public void The_response_should_be_received()
		{
			_response.Value.CorrelationId.ShouldBeEqualTo(_corrId);
			_response.Value.Message.ShouldBeEqualTo("Real Response");
		}

		protected IServiceBus RemoteBus { get; private set; }

		[Serializable]
		class Request : CorrelatedBy<Guid>
		{

			[Obsolete]
			protected Request()
			{
			}

			public Request(Guid corrId)
			{
				CorrelationId = corrId;
			}

			public Guid CorrelationId { get; protected set; }
		}

		[Serializable]
		class Response : CorrelatedBy<Guid>
		{
			[Obsolete]
			protected Response()
			{
			}

			public Response(Guid corrId, string message)
			{
				CorrelationId = corrId;
				Message = message;
			}

			public Guid CorrelationId { get; protected set; }
			public string Message { get; protected set; }
		}
	}
}