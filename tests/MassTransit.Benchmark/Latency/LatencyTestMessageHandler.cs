namespace MassTransitBenchmark.Latency;

using System.Threading;
using System.Threading.Tasks;
using Commands;
using NServiceBus;


public class LatencyTestMessageHandler :
    IHandleMessages<LatencyTestMessage>
{
    readonly IReportConsumerMetric _report;

    public LatencyTestMessageHandler(IReportConsumerMetric report)
    {
        _report = report;
    }

    public async Task Handle(LatencyTestMessage message, IMessageHandlerContext context)
    {
        var current = Interlocked.Increment(ref MessageLatencyConsumer.CurrentConsumerCount);
        var maxConsumerCount = MessageLatencyConsumer.MaxConsumerCount;
        if (current > maxConsumerCount)
            Interlocked.CompareExchange(ref MessageLatencyConsumer.MaxConsumerCount, current, maxConsumerCount);

        try
        {
            await _report.Consumed<LatencyTestMessage>(message.CorrelationId).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.Decrement(ref MessageLatencyConsumer.CurrentConsumerCount);
        }
    }
}
