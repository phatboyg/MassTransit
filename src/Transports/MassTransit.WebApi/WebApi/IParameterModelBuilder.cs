namespace MassTransit.WebApi
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;


    public interface IParameterModelBuilder
    {
        ParameterModel Create(ActionModel actionModel);
    }
}