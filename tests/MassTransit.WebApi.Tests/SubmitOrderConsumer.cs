namespace MassTransit.WebApi.Tests
{
    using System.Threading.Tasks;
    using Contracts;
    using Microsoft.Extensions.Logging;


    public class SubmitOrderConsumer :
        IConsumer<SubmitOrder>,
        IConsumer<UpdateOrder>
    {
        readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }

        // [HttpGet]
        // [ProducesResponseType(typeof(SubmitOrder), StatusCodes.Status200OK)]
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            _logger.LogInformation("Submit Order: {OrderId}", context.Message.OrderId);

            await context.Publish(new OrderSubmitted { OrderId = context.Message.OrderId });
        }

        public Task Consume(ConsumeContext<UpdateOrder> context)
        {
            return Task.CompletedTask;
        }
    }
}
