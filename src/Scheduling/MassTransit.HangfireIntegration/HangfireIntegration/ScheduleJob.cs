namespace MassTransit.HangfireIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.Server;
    using Serialization;


    [Queue(HangfireEndpointOptions.DefaultQueueName)]
    class ScheduleJob
    {
        readonly IBus _bus;

        public ScheduleJob(IBus bus)
        {
            _bus = bus;
        }

        [HashCleanup]
        public async Task SendMessage(HangfireScheduledMessageData messageData, PerformContext performContext)
        {
            try
            {
                var contentType = string.IsNullOrWhiteSpace(messageData.ContentType) ? default : new ContentType(messageData.ContentType);
                var body = messageData.Body ?? string.Empty;

                var pipe = new ForwardScheduledMessagePipe(contentType, messageData, body, messageData.Destination);

                var endpoint = await _bus.GetSendEndpoint(messageData.Destination).ConfigureAwait(false);

                var scheduled = new Scheduled();

                await endpoint.Send(scheduled, pipe, performContext.CancellationToken.ShutdownToken).ConfigureAwait(false);

                LogContext.Debug?.Log("Schedule Executed: {JobId}, created at: {CreatedAt}", performContext.BackgroundJob.Id,
                    performContext.BackgroundJob.CreatedAt);
            }
            catch (Exception ex)
            {
                LogContext.Error?.Log(ex, "Failed to send scheduled message: {JobId}, created at: {CreatedAt}, destination: {DestinationAddress}",
                    performContext.BackgroundJob.Id, messageData.Destination, performContext.BackgroundJob.CreatedAt);

                throw new JobPerformanceException("Job Execution exception", ex);
            }
        }

        [RecurringScheduleDateTimeInterval]
        public async Task SendMessage(HangfireRecurringScheduledMessageData messageData, PerformContext performContext)
        {
            try
            {
                var contentType = string.IsNullOrWhiteSpace(messageData.ContentType) ? default : new ContentType(messageData.ContentType);
                var body = messageData.Body ?? string.Empty;

                var pipe = new ForwardScheduledMessagePipe(contentType, messageData, body, messageData.Destination);

                var endpoint = await _bus.GetSendEndpoint(messageData.Destination).ConfigureAwait(false);

                var scheduled = new Scheduled();

                await endpoint.Send(scheduled, pipe, performContext.CancellationToken.ShutdownToken).ConfigureAwait(false);

                LogContext.Debug?.Log("Schedule Executed: {JobId}, created at: {CreatedAt}, with range: {StartTime}-{EndTime}", performContext.BackgroundJob.Id,
                    performContext.BackgroundJob.CreatedAt, messageData.StartTime, messageData.EndTime);
            }
            catch (Exception ex)
            {
                LogContext.Error?.Log(ex, "Failed to send scheduled message: {JobId}, created at: {CreatedAt}, destination: {DestinationAddress}",
                    performContext.BackgroundJob.Id, messageData.Destination, performContext.BackgroundJob.CreatedAt);

                throw new JobPerformanceException("Job Execution exception", ex);
            }
        }


        class ForwardScheduledMessagePipe :
            IPipe<SendContext>
        {
            readonly string _body;
            readonly ContentType? _contentType;
            readonly Uri? _destinationAddress;
            readonly HangfireScheduledMessageData _messageData;

            public ForwardScheduledMessagePipe(ContentType? contentType, HangfireScheduledMessageData messageData, string body, Uri? destinationAddress)
            {
                _contentType = contentType;
                _messageData = messageData;
                _body = body;
                _destinationAddress = destinationAddress;
            }

            public Task Send(SendContext context)
            {
                var deserializer = context.Serialization.GetMessageDeserializer(_contentType);

                var messageContext = new MessageDataMessageContext(_messageData, deserializer);

                var body = deserializer.GetMessageBody(_body);

                var serializerContext = deserializer.Deserialize(body, messageContext, _destinationAddress);

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


        class Scheduled
        {
        }
    }
}
