namespace MassTransit.Context
{
    using Microsoft.Extensions.Logging;


    /// <summary>
    /// Used to provide access to logging and diagnostic services
    /// </summary>
    public interface ILogContext
    {
        ILogger Logger { get; }

        /// <summary>
        /// The log context for all message movement, sent, received, etc.
        /// </summary>
        ILogContext Messages { get; }

        /// <summary>
        /// If enabled, returns a valid source which can be used
        /// </summary>
        /// <param name="name"></param>
        /// <param name="args"></param>
        /// <returns>A valid source, or null</returns>
        IActivityScope BeginActivity(string name, object args = default);

        /// <summary>
        /// Creates a new ILogger instance using the full name of the given type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        ILogContext<T> CreateLogContext<T>();

        /// <summary>
        /// Creates a new <see cref="T:Microsoft.Extensions.Logging.ILogger" /> instance.
        /// </summary>
        /// <param name="categoryName">The category name for messages produced by the logger.</param>
        /// <returns>The <see cref="T:Microsoft.Extensions.Logging.ILogger" />.</returns>
        ILogContext CreateLogContext(string categoryName);
    }


    public interface ILogContext<T> :
        ILogContext
    {
    }
}
