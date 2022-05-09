namespace MassTransit.WebApi
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;


    public interface ISagaMessageActionBuilder
    {
        void Build(ControllerModel controller);
    }
}