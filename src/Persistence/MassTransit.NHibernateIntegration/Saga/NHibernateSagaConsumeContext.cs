﻿namespace MassTransit.NHibernateIntegration.Saga
{
    using System;
    using System.Threading.Tasks;
    using Context;
    using MassTransit.Saga;
    using NHibernate;
    using Util;


    public class NHibernateSagaConsumeContext<TSaga, TMessage> :
        ConsumeContextProxyScope<TMessage>,
        SagaConsumeContext<TSaga, TMessage>
        where TMessage : class
        where TSaga : class, ISaga
    {
        readonly ISession _session;

        public NHibernateSagaConsumeContext(ISession session, ConsumeContext<TMessage> context, TSaga instance)
            : base(context)
        {
            Saga = instance;
            _session = session;
        }

        Guid? MessageContext.CorrelationId => Saga.CorrelationId;

        async Task SagaConsumeContext<TSaga>.SetCompleted()
        {
            await _session.DeleteAsync(Saga).ConfigureAwait(false);
            IsCompleted = true;

            LogContext.Debug?.Log("SAGA:{SagaType}:{CorrelationId} Removed {MessageType}", TypeMetadataCache<TSaga>.ShortName,
                Saga.CorrelationId, TypeMetadataCache<TMessage>.ShortName);
        }

        public bool IsCompleted { get; private set; }
        public TSaga Saga { get; }
    }
}
