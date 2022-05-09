namespace MassTransit.WebApi
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;


    public interface IConsumerMessageActionBuilder
    {
        void Build(ControllerModel controller);
    }
}