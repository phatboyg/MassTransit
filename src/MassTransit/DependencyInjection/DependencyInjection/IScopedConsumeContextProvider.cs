namespace MassTransit.DependencyInjection
{
    using System;


    public interface IScopedConsumeContextProvider<T>
        where T : class
    {
        ConsumeContext Context { get; }

        bool HasContext { get; }

        IDisposable PushContext(ConsumeContext context);
    }
}
