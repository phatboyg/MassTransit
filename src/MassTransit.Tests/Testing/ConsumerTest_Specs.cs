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
namespace MassTransit.Tests.Testing
{
	using System;
	using System.IO;
	using Magnum.TestFramework;
	using MassTransit.Serialization;
	using MassTransit.Testing;
	using MassTransit.Testing.ScenarioConfigurators;

	[Scenario]
	public class When_a_consumer_is_being_tested
	{
		ConsumerTest<BusTestScenario, Testsumer> _test;

		[When]
		public void A_consumer_is_being_tested()
		{
			_test = TestFactory.ForConsumer<Testsumer>()
				.InSingleBusScenario()
				.New(x =>
					{
						x.ConstructUsing(() => new Testsumer());

						x.Send(new A(), (scenario, context) => context.SendResponseTo(scenario.Bus));
					});

			_test.Execute();
		}

		[Finally]
		public void Teardown()
		{
			_test.Dispose();
			_test = null;
		}


		[Then]
		public void Should_send_the_initial_message_to_the_consumer()
		{
			_test.Sent.Any<A>().ShouldBeTrue();
		}

		[Then]
		public void Should_have_sent_the_response_from_the_consumer()
		{
			_test.Sent.Any<B>().ShouldBeTrue();
		}

		[Then]
		public void Should_receive_the_message_type_a()
		{
			_test.Received.Any<A>().ShouldBeTrue();
		}

		[Then]
		public void Should_have_called_the_consumer_method()
		{
			_test.Consumer.Received.Any<A>().ShouldBeTrue();
		}

		class Testsumer :
			Consumes<A>.All
		{
			public void Consume(A message)
			{
				this.MessageContext().Respond(new B());
			}
		}

		class A
		{
		}

		class B
		{
		}
	}

	[Scenario]
	public class When_a_context_consumer_is_being_tested
	{
		ConsumerTest<BusTestScenario, Testsumer> _test;

		[When]
		public void A_consumer_is_being_tested()
		{
			_test = TestFactory.ForConsumer<Testsumer>()
				.New(x =>
					{
						x.ConstructUsing(() => new Testsumer());

						x.Send(new A(), (scenario, context) => context.SendResponseTo(scenario.Bus));
					});

			_test.Execute();
		}

		[Finally]
		public void Teardown()
		{
			_test.Dispose();
			_test = null;
		}


		[Then]
		public void Should_send_the_initial_message_to_the_consumer()
		{
			_test.Sent.Any<A>().ShouldBeTrue();
		}

		[Then]
		public void Should_have_sent_the_response_from_the_consumer()
		{
			_test.Sent.Any<B>().ShouldBeTrue();
		}

		[Then]
		public void Should_receive_the_message_type_a()
		{
			_test.Received.Any<A>().ShouldBeTrue();
		}

		[Then]
		public void Should_have_called_the_consumer_method()
		{
			_test.Consumer.Received.Any<A>().ShouldBeTrue();
		}

		class Testsumer :
			Consumes<A>.Context
		{
			public void Consume(IConsumeContext<A> context)
			{
				context.Respond(new B());
			}
		}

		class A
		{
		}

		class B
		{
		}
	}

	[Scenario]
	public class When_configuring_builders
	{
		ConsumerTest<BusTestScenario, Testsumer> _test;

		[When]
		public void Giving_a_specific_configuration_builder_impl()
		{
			_test = TestFactory.ForConsumer<Testsumer>()
				.New(x =>
				{
					x.AddConfigurator(new BusTestScenarioBuilderConfiguratorImpl(sbc =>
						sbc.SetDefaultSerializer<AngrySerializer>()));

					x.ConstructUsing(() => new Testsumer());

					x.Send(new A(), (scenario, context) => context.SendResponseTo(scenario.Bus));
				});

			_test.Execute();
		}

		public class AngrySerializer
			: IMessageSerializer
		{
			public string ContentType { get; private set; }

			public void Serialize<T>(Stream stream, ISendContext<T> context) where T : class
			{
				throw new Exception("MERGHH!!! YOU shall not serialize today!");
			}

			public void Deserialize(IReceiveContext context)
			{
				throw new Exception("EERGG!!! YOU shall not deserialize today!");
			}
		}

		[Finally]
		public void Teardown()
		{
			_test.Dispose();
			_test = null;
		}

		class Testsumer :
			Consumes<A>.Context
		{
			public void Consume(IConsumeContext<A> context)
			{
				context.Respond(new B());
			}
		}

		[Then]
		public void Should_not_send_the_initial_message_to_the_consumer()
		{
			_test.Sent.Any<A>().ShouldBeFalse("because A was never correctly serialized to the transport");
		}

		[Then]
		public void Should_not_have_sent_the_response_from_the_consumer()
		{
			_test.Sent.Any<B>().ShouldBeFalse("because A was never correctly deserialized");
		}

		class A
		{
		}

		class B
		{
		}
	}
}