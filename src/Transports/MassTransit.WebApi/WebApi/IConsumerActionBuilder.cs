namespace MassTransit.WebApi
{
    using System;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;


    public interface IConsumerActionBuilder
    {
        Type ConsumerType { get; }
        void Build(ControllerModel controller);
    }

    public interface ISagaActionBuilder
    {
        Type SagaType { get; }
        void Build(ControllerModel controller);
    }
}
