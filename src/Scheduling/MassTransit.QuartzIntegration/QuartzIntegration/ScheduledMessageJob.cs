#nullable enable
namespace MassTransit.QuartzIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using Quartz;


    public class ScheduledMessageJob :
        IJob
    {
        readonly IBus _bus;

        public ScheduledMessageJob(IBus bus)
        {
            _bus = bus;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobData = context.MergedJobDataMap;

            var contentType = new ContentType(jobData.GetString("ContentType"));
            var destinationAddress = new Uri(jobData.GetString("Destination"));
            var body = jobData.GetString("Body") ?? string.Empty;
            var messageType = jobData.GetString("MessageType")?.Split(';')?.ToArray() ?? Array.Empty<string>();

            try
            {
                var pipe = new ForwardScheduledMessagePipe(contentType, context, body, destinationAddress);

                var endpoint = await _bus.GetSendEndpoint(destinationAddress).ConfigureAwait(false);

                var scheduled = new Scheduled();

                await endpoint.Send(scheduled, pipe, context.CancellationToken).ConfigureAwait(false);

                LogContext.Debug?.Log("Schedule Executed: {Key} {Schedule}", context.JobDetail.Key, context.Trigger.GetNextFireTimeUtc());
            }
            catch (Exception ex)
            {
                LogContext.Error?.Log(ex, "Failed to send scheduled message: {MessageType} {DestinationAddress}", messageType, destinationAddress);

                throw new JobExecutionException(ex, context.RefireCount < 5);
            }
        }


        class Scheduled
        {
        }


        class ForwardScheduledMessagePipe :
            IPipe<SendContext>
        {
            readonly string _body;
            readonly ContentType? _contentType;
            readonly Uri? _destinationAddress;
            readonly IJobExecutionContext _executionContext;

            public ForwardScheduledMessagePipe(ContentType? contentType, IJobExecutionContext executionContext, string body, Uri? destinationAddress)
            {
                _contentType = contentType;
                _executionContext = executionContext;
                _body = body;
                _destinationAddress = destinationAddress;
            }

            public Task Send(SendContext context)
            {
                var deserializer = context.Serialization.GetMessageDeserializer(_contentType);

                var messageContext = new JobDataMessageContext(_executionContext, deserializer);

                var body = deserializer.GetMessageBody(_body);

                var serializerContext = deserializer.Deserialize(body, messageContext, _destinationAddress);

                if (messageContext.MessageId.HasValue)
                    context.MessageId = messageContext.MessageId;

                context.RequestId = messageContext.RequestId;
                context.ConversationId = messageContext.ConversationId;
                context.CorrelationId = messageContext.CorrelationId;
                context.InitiatorId = messageContext.InitiatorId;
                context.SourceAddress = messageContext.SourceAddress;
                context.ResponseAddress = messageContext.ResponseAddress;
                context.FaultAddress = messageContext.FaultAddress;

                if (messageContext.ExpirationTime.HasValue)
                    context.TimeToLive = messageContext.ExpirationTime.Value.ToUniversalTime() - DateTime.UtcNow;

                foreach (KeyValuePair<string, object> header in messageContext.Headers.GetAll())
                    context.Headers.Set(header.Key, header.Value);

                context.Serializer = serializerContext.GetMessageSerializer();

                return Task.CompletedTask;
            }

            public void Probe(ProbeContext context)
            {
            }
        }
    }
}
