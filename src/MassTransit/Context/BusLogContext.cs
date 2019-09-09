namespace MassTransit.Context
{
    using System.Diagnostics;
    using Logging;
    using Microsoft.Extensions.Logging;


    public class BusLogContext :
        ILogContext
    {
        readonly DiagnosticSource _source;
        readonly ILoggerFactory _loggerFactory;
        readonly ILogContext _messageLogger;

        public BusLogContext(ILoggerFactory loggerFactory, DiagnosticSource source)
        {
            _source = source;
            _loggerFactory = loggerFactory;
            Logger = loggerFactory.CreateLogger(LogCategoryName.MassTransit);

            _messageLogger = new BusLogContext(source, loggerFactory, loggerFactory.CreateLogger("MassTransit.Messages"));
        }

        protected BusLogContext(DiagnosticSource source, ILoggerFactory loggerFactory, ILogContext messageLogger, Microsoft.Extensions.Logging.ILogger logger)
        {
            _source = source;
            _loggerFactory = loggerFactory;
            _messageLogger = messageLogger;
            Logger = logger;
        }

        BusLogContext(DiagnosticSource source, ILoggerFactory loggerFactory, Microsoft.Extensions.Logging.ILogger logger)
        {
            _source = source;
            _loggerFactory = loggerFactory;
            Logger = logger;

            _messageLogger = this;
        }

        ILogContext ILogContext.Messages => _messageLogger;

        public IActivityScope BeginActivity(string name, object args = default)
        {
            return new ActivityScope(_source, name, args);
        }

        public ILogContext<T> CreateLogContext<T>()
        {
            ILogger<T> logger = _loggerFactory.CreateLogger<T>();

            return new BusLogContext<T>(_source, _loggerFactory, _messageLogger, logger);
        }

        public ILogContext CreateLogContext(string categoryName)
        {
            var logger = _loggerFactory.CreateLogger(categoryName);

            return new BusLogContext(_source, _loggerFactory, _messageLogger, logger);
        }

        public ILogger Logger { get; }
    }


    public class BusLogContext<T> :
        BusLogContext,
        ILogContext<T>
    {
        public BusLogContext(DiagnosticSource source, ILoggerFactory loggerFactory, ILogContext messageLogger, ILogger<T> logger)
            : base(source, loggerFactory, messageLogger, logger)
        {
        }
    }
}
