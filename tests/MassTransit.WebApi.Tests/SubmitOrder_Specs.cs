namespace MassTransit.WebApi.Tests
{
    using System;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Contracts;
    using Microsoft.AspNetCore.Mvc.Testing;
    using NUnit.Framework;
    using Testing;


    public class Submitting_an_order
    {
        [Test]
        public async Task Should_have_the_submitted_status()
        {
            await using WebApplicationFactory<SubmitOrderStartup> application = new WebApplicationFactory<SubmitOrderStartup>()
                .WithWebHostBuilder(builder => builder.ConfigureServices(services => services.AddMassTransitTestHarness()));

            var testHarness = application.Services.GetTestHarness();

            using var client = application.CreateClient();

            ISagaStateMachineTestHarness<OrderStateMachine, OrderSaga>? sagaTestHarness =
                testHarness.GetSagaStateMachineHarness<OrderStateMachine, OrderSaga>();

            const string submitOrderUrl = "/Order";

            var orderId = NewId.NextGuid();

            var submitOrderResponse = await client.PostAsync(submitOrderUrl, JsonContent.Create(new SubmitOrder { OrderId = orderId }));

            submitOrderResponse.EnsureSuccessStatusCode();
            var orderStatus = await submitOrderResponse.Content.ReadFromJsonAsync<OrderStatus>();

            Assert.That(orderStatus, Is.Not.Null);
            Assert.That(orderStatus!.OrderId, Is.EqualTo(orderId));

            Assert.That(await sagaTestHarness.Consumed.Any<SubmitOrder>(x => x.Context.Message.OrderId == orderId), Is.True);

            Guid? sagaExists = await sagaTestHarness.Exists(orderId, x => x.Submitted);
            Assert.That(sagaExists.HasValue);
            Assert.That(sagaExists!.Value, Is.EqualTo(orderId));

            var getOrderStatusUrl = $"/Order?id={orderId:D}";

            var orderStatusResponse = await client.GetAsync(getOrderStatusUrl);
            orderStatusResponse.EnsureSuccessStatusCode();

            orderStatus = await orderStatusResponse.Content.ReadFromJsonAsync<OrderStatus>();

            Assert.That(orderStatus, Is.Not.Null);
            Assert.That(orderStatus!.OrderId, Is.EqualTo(orderId));
            Assert.That(orderStatus.Status, Is.EqualTo(nameof(OrderStateMachine.Submitted)));
        }
    }
}
