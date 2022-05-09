namespace MassTransit.WebApi
{
    using System;
    using System.Reflection;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.ModelBinding;


    public class ParameterModelBuilder<TParameter> :
        IParameterModelBuilder
    {
        readonly ParameterInfo _parameterInfo;

        public ParameterModelBuilder(ParameterInfo parameterInfo)
        {
            if (parameterInfo == null)
                throw new ArgumentNullException(nameof(parameterInfo));
            _parameterInfo = parameterInfo;
        }

        public ParameterModel Create(ActionModel actionModel)
        {
            var attributes = _parameterInfo.GetCustomAttributes(true);

            var bindingInfo = BindingInfo.GetBindingInfo(attributes);

            var parameterModel = new ParameterModel(_parameterInfo, attributes)
            {
                ParameterName = _parameterInfo.Name,
                BindingInfo = bindingInfo,
                Action = actionModel,
            };

            return parameterModel;
        }
    }
}