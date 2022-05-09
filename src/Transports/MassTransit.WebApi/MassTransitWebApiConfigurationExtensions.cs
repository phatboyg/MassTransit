namespace MassTransit
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;


    public static class MassTransitWebApiConfigurationExtensions
    {
        public static IServiceCollection AddMassTransitWebApi(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<MvcOptions>, MassTransitWebApiConfigureOptions>());

            return services;
        }
    }
}
