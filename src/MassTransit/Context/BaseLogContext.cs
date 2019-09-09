namespace MassTransit.Context
{
    using System;
    using GreenPipes;
    using Microsoft.Extensions.Logging;


    public class BaseLogContext :
        BasePipeContext,
        ILogContext
    {
        readonly ILogContext _context;

        public BaseLogContext(ILogContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ILogContext Messages => _context.Messages;

        public IActivityScope BeginActivity(string name, object args = default)
        {
            return _context.BeginActivity(name, args);
        }

        public ILogContext<T> CreateLogContext<T>()
        {
            return _context.CreateLogContext<T>();
        }

        public ILogContext CreateLogContext(string categoryName)
        {
            return _context.CreateLogContext(categoryName);
        }

        public ILogger Logger => _context.Logger;
    }
}
