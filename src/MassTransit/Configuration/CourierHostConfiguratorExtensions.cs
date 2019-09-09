﻿namespace MassTransit
{
    using System;
    using Context;
    using Courier;
    using Courier.Factories;
    using PipeConfigurators;
    using Util;


    public static class CourierHostConfiguratorExtensions
    {
        public static void ExecuteActivityHost<TActivity, TArguments>(this IReceiveEndpointConfigurator configurator,
            Action<IExecuteActivityConfigurator<TActivity, TArguments>> configure = null)
            where TActivity : class, IExecuteActivity<TArguments>, new()
            where TArguments : class
        {
            ExecuteActivityHost(configurator, DefaultConstructorExecuteActivityFactory<TActivity, TArguments>.ExecuteFactory, configure);
        }

        public static void ExecuteActivityHost<TActivity, TArguments>(this IReceiveEndpointConfigurator configurator,
            Uri compensateAddress, Action<IExecuteActivityConfigurator<TActivity, TArguments>> configure = null)
            where TActivity : class, IExecuteActivity<TArguments>, new()
            where TArguments : class
        {
            ExecuteActivityHost(configurator, compensateAddress, DefaultConstructorExecuteActivityFactory<TActivity, TArguments>.ExecuteFactory, configure);
        }

        public static void ExecuteActivityHost<TActivity, TArguments>(this IReceiveEndpointConfigurator configurator,
            Uri compensateAddress, Func<TActivity> activityFactory, Action<IExecuteActivityConfigurator<TActivity, TArguments>> configure = null)
            where TActivity : class, IExecuteActivity<TArguments>
            where TArguments : class
        {
            ExecuteActivityHost(configurator, compensateAddress, _ => activityFactory(), configure);
        }

        public static void ExecuteActivityHost<TActivity, TArguments>(this IReceiveEndpointConfigurator configurator,
            Func<TActivity> activityFactory, Action<IExecuteActivityConfigurator<TActivity, TArguments>> configure = null)
            where TActivity : class, IExecuteActivity<TArguments>
            where TArguments : class
        {
            ExecuteActivityHost(configurator, _ => activityFactory(), configure);
        }

        public static void ExecuteActivityHost<TActivity, TArguments>(this IReceiveEndpointConfigurator configurator,
            Uri compensateAddress, Func<TArguments, TActivity> activityFactory,
            Action<IExecuteActivityConfigurator<TActivity, TArguments>> configure = null)
            where TActivity : class, IExecuteActivity<TArguments>
            where TArguments : class
        {
            if (activityFactory == null)
                throw new ArgumentNullException(nameof(activityFactory));

            var factory = new FactoryMethodExecuteActivityFactory<TActivity, TArguments>(activityFactory);

            ExecuteActivityHost(configurator, compensateAddress, factory, configure);
        }

        public static void ExecuteActivityHost<TActivity, TArguments>(this IReceiveEndpointConfigurator configurator,
            Func<TArguments, TActivity> activityFactory,
            Action<IExecuteActivityConfigurator<TActivity, TArguments>> configure = null)
            where TActivity : class, IExecuteActivity<TArguments>
            where TArguments : class
        {
            if (activityFactory == null)
                throw new ArgumentNullException(nameof(activityFactory));

            var factory = new FactoryMethodExecuteActivityFactory<TActivity, TArguments>(activityFactory);

            ExecuteActivityHost(configurator, factory, configure);
        }

        public static void ExecuteActivityHost<TActivity, TArguments>(this IReceiveEndpointConfigurator configurator, Uri compensateAddress,
            IExecuteActivityFactory<TActivity, TArguments> factory, Action<IExecuteActivityConfigurator<TActivity, TArguments>> configure = null)
            where TActivity : class, IExecuteActivity<TArguments>
            where TArguments : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (compensateAddress == null)
                throw new ArgumentNullException(nameof(compensateAddress));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            LogContext.LogDebug("Configuring Execute Activity: {ActivityType}, {ArgumentType}", TypeMetadataCache<TActivity>.ShortName,
                TypeMetadataCache<TArguments>.ShortName);

            var specification = new ExecuteActivityHostSpecification<TActivity, TArguments>(factory, compensateAddress);

            configure?.Invoke(specification);

            configurator.AddEndpointSpecification(specification);
        }

        public static void ExecuteActivityHost<TActivity, TArguments>(this IReceiveEndpointConfigurator configurator,
            IExecuteActivityFactory<TActivity, TArguments> factory, Action<IExecuteActivityConfigurator<TActivity, TArguments>> configure = null)
            where TActivity : class, IExecuteActivity<TArguments>
            where TArguments : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            LogContext.LogDebug("Configuring Execute Activity: {ActivityType}, {ArgumentType}", TypeMetadataCache<TActivity>.ShortName,
                TypeMetadataCache<TArguments>.ShortName);

            var specification = new ExecuteActivityHostSpecification<TActivity, TArguments>(factory);

            configure?.Invoke(specification);

            configurator.AddEndpointSpecification(specification);
        }

        public static void CompensateActivityHost<TActivity, TLog>(this IReceiveEndpointConfigurator configurator,
            Action<ICompensateActivityConfigurator<TActivity, TLog>> configure = null)
            where TActivity : class, ICompensateActivity<TLog>, new()
            where TLog : class
        {
            CompensateActivityHost(configurator, DefaultConstructorCompensateActivityFactory<TActivity, TLog>.CompensateFactory, configure);
        }

        public static void CompensateActivityHost<TActivity, TLog>(this IReceiveEndpointConfigurator configurator, Func<TActivity> activityFactory,
            Action<ICompensateActivityConfigurator<TActivity, TLog>> configure = null)
            where TActivity : class, ICompensateActivity<TLog>
            where TLog : class
        {
            if (activityFactory == null)
                throw new ArgumentNullException(nameof(activityFactory));

            CompensateActivityHost(configurator, _ => activityFactory(), configure);
        }

        public static void CompensateActivityHost<TActivity, TLog>(this IReceiveEndpointConfigurator configurator, Func<TLog, TActivity> activityFactory,
            Action<ICompensateActivityConfigurator<TActivity, TLog>> configure = null)
            where TActivity : class, ICompensateActivity<TLog>
            where TLog : class
        {
            if (activityFactory == null)
                throw new ArgumentNullException(nameof(activityFactory));

            var factory = new FactoryMethodCompensateActivityFactory<TActivity, TLog>(activityFactory);

            CompensateActivityHost(configurator, factory, configure);
        }

        public static void CompensateActivityHost<TActivity, TLog>(this IReceiveEndpointConfigurator configurator,
            ICompensateActivityFactory<TActivity, TLog> factory, Action<ICompensateActivityConfigurator<TActivity, TLog>> configure = null)
            where TActivity : class, ICompensateActivity<TLog>
            where TLog : class
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            LogContext.LogDebug("Configuring Compensate Activity: {ActivityType}, {LogType}", TypeMetadataCache<TActivity>.ShortName,
                TypeMetadataCache<TLog>.ShortName);

            var specification = new CompensateActivityHostSpecification<TActivity, TLog>(factory);

            configure?.Invoke(specification);

            configurator.AddEndpointSpecification(specification);
        }
    }
}
