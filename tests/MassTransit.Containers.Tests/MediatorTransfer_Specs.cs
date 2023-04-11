namespace MassTransit.Containers.Tests
{
    using System.Threading.Tasks;
    using Mediator;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;
    using Testing;
    using TransferComponents;
    using TransferComponents.BusComponents;
    using TransferComponents.MediatorComponents;


    namespace TransferComponents
    {
        using System;


        namespace MediatorComponents
        {
            public class SubmitOrderConsumer :
                IConsumer<SubmitOrder>
            {
                public async Task Consume(ConsumeContext<SubmitOrder> context)
                {
                    await context.Publish(new OrderSubmitted(context.Message.OrderId));
                }
            }
        }


        namespace BusComponents
        {
            public class OrderSubmittedConsumer :
                IConsumer<OrderSubmitted>
            {
                public async Task Consume(ConsumeContext<OrderSubmitted> context)
                {
                }
            }
        }


        public record SubmitOrder(Guid OrderId);


        public record OrderSubmitted(Guid OrderId);
    }


    [TestFixture]
    public class Transferring_publish_endpoint_to_the_bus
    {
        [Test]
        public async Task Should_publish_to_the_bus()
        {
            await using var provider = new ServiceCollection()
                .AddMediator(x =>
                {
                    x.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();

                    x.ConfigureMediator((context,cfg) =>
                    {
                        cfg.UseBusTransfer(context, BusTransferOptions.Publish);
                    });
                })
                .AddMassTransitTestHarness(x =>
                {
                    x.AddConsumersFromNamespaceContaining<OrderSubmittedConsumer>();
                })
                .BuildServiceProvider(true);

            var harness = provider.GetTestHarness();

            await harness.Start();

            var mediator = harness.Scope.ServiceProvider.GetRequiredService<IScopedMediator>();

            await mediator.Send(new SubmitOrder(NewId.NextGuid()));

            Assert.That(await harness.Consumed.Any<OrderSubmitted>(), Is.True);

        }
    }
}
