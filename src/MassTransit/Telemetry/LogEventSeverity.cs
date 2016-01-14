namespace MassTransit.Telemetry
{
    public enum LogEventSeverity
    {
        /// <summary>
        /// All the things!
        /// </summary>
        Verbose,

        /// <summary>
        /// Enough to know what is happening inside
        /// </summary>
        Debug,

        /// <summary>
        /// Enough to know what is happening outside
        /// </summary>
        Information,

        /// <summary>
        /// Things are in danger of going sideway quickly
        /// </summary>
        Warning,

        /// <summary>
        /// Okay, this is a bad thing - we aren't dead yet but it's coming
        /// </summary>
        Error,

        /// <summary>
        /// Boom, is there a doctor in house?
        /// </summary>
        Fatal
    }
}