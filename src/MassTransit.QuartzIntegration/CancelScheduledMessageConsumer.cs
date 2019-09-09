namespace MassTransit.QuartzIntegration
{
    using System.Threading.Tasks;
    using Context;
    using Quartz;
    using Scheduling;


    public class CancelScheduledMessageConsumer :
        IConsumer<CancelScheduledMessage>,
        IConsumer<CancelScheduledRecurringMessage>
    {
        readonly IScheduler _scheduler;

        public CancelScheduledMessageConsumer(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public async Task Consume(ConsumeContext<CancelScheduledMessage> context)
        {
            var correlationId = context.Message.TokenId.ToString("N");

            var jobKey = new JobKey(correlationId);
            var deletedJob = await _scheduler.DeleteJob(jobKey, context.CancellationToken).ConfigureAwait(false);

            if (deletedJob)
                LogContext.LogDebug("Cancelled Scheduled Message: {Id} at {Timestamp}", jobKey, context.Message.Timestamp);
            else
                LogContext.LogDebug("CancelScheduledMessage: no message found for {Id}", jobKey);
        }

        public async Task Consume(ConsumeContext<CancelScheduledRecurringMessage> context)
        {
            const string prependedValue = "Recurring.Trigger.";

            var scheduleId = context.Message.ScheduleId;

            if (!scheduleId.StartsWith(prependedValue))
                scheduleId = string.Concat(prependedValue, scheduleId);

            var unscheduledJob = await _scheduler.UnscheduleJob(new TriggerKey(scheduleId, context.Message.ScheduleGroup), context.CancellationToken)
                .ConfigureAwait(false);

            if (unscheduledJob)
            {
                LogContext.LogDebug("CancelRecurringScheduledMessage: {ScheduleId}/{ScheduleGroup} at {Timestamp}", context.Message.ScheduleId,
                    context.Message.ScheduleGroup, context.Message.Timestamp);
            }
            else
            {
                LogContext.LogDebug("CancelRecurringScheduledMessage: no message found {ScheduleId}/{ScheduleGroup}", context.Message.ScheduleId,
                    context.Message.ScheduleGroup);
            }
        }
    }
}
