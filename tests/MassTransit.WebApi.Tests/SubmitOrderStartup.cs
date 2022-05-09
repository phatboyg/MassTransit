namespace MassTransit.WebApi.Tests
{
    using DependencyInjection;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;


    public class SubmitOrderStartup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOpenApiDocument(cfg => cfg.PostProcess = d => d.Info.Title = "MassTransit API Endpoint Routing");

            services.AddMassTransit(x =>
            {
                x.AddConsumer<SubmitOrderConsumer>();

                x.AddSagaStateMachine<OrderStateMachine, OrderSaga>()
                    .InMemoryRepository();

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });

            // IBus is just temporary, this would be the receive endpoint dispatcher (akin to mediator style)
            services.TryAddSingleton(provider => Bind<IBus, SubmitOrderConsumer>.Create(provider.GetRequiredService<IBusControl>()));

            services.AddControllers();

            services.AddMassTransitWebApi();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
