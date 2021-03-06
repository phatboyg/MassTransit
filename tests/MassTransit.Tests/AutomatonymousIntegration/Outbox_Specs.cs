namespace MassTransit.Tests.AutomatonymousIntegration
{
    using System;
    using System.Threading.Tasks;
    using Automatonymous;
    using MassTransit.Saga;
    using NUnit.Framework;
    using TestFramework;


    [TestFixture]
    public class Responding_through_the_outbox :
        InMemoryTestFixture
    {
        [Test]
        public void Should_receive_the_fault_message()
        {
            Assert.That(
                async () => await _client.GetResponse<StartupComplete>(new Start {FailToStart = true}, TestCancellationToken),
                Throws.TypeOf<RequestFaultException>());
        }

        [Test]
        public async Task Should_receive_the_response_message()
        {
            Response<StartupComplete> complete = await _client.GetResponse<StartupComplete>(new Start(), TestCancellationToken);
        }

        [OneTimeSetUp]
        public void Setup()
        {
            _client = Bus.CreateRequestClient<Start>(InputQueueAddress, TestTimeout);
        }

        protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
        {
            _machine = new TestStateMachine();
            _repository = new InMemorySagaRepository<Instance>();

            configurator.UseInMemoryOutbox();

            configurator.StateMachineSaga(_machine, _repository);
        }

        TestStateMachine _machine;
        InMemorySagaRepository<Instance> _repository;
        IRequestClient<Start> _client;


        class Instance :
            SagaStateMachineInstance
        {
            public State CurrentState { get; set; }
            public Guid CorrelationId { get; set; }
        }


        class TestStateMachine :
            MassTransitStateMachine<Instance>
        {
            public TestStateMachine()
            {
                Initially(
                    When(Started, x => x.Data.FailToStart)
                        .Then(context => throw new IntentionalTestException()),
                    When(Started, x => x.Data.FailToStart == false)
                        .Respond(new StartupComplete())
                        .TransitionTo(Running));
            }

            public State Running { get; private set; }
            public Event<Start> Started { get; private set; }
        }


        public class Start :
            CorrelatedBy<Guid>
        {
            public Start()
            {
                CorrelationId = NewId.NextGuid();
            }

            public bool FailToStart { get; set; }

            public Guid CorrelationId { get; private set; }
        }


        class StartupComplete
        {
        }
    }
}
