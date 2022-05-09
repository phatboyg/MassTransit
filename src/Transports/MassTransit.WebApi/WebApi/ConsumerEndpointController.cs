namespace MassTransit.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Transports;


    public class ConsumerEndpointController<TConsumer> :
        EndpointController<TConsumer>
        where TConsumer : class
    {
        readonly IReceiveEndpointDispatcher<TConsumer> _dispatcher;
        readonly IServiceProvider _provider;

        public ConsumerEndpointController(IServiceProvider provider, IReceiveEndpointDispatcher<TConsumer> dispatcher)
        {
            _provider = provider;
            _dispatcher = dispatcher;
        }

        [NonAction]
        public async Task<ActionResult<TResponse>> Post<TMessage, TResponse>([FromBody] TMessage message)
            where TMessage : class
        {
            Dictionary<string, object> headers = HttpContext.Request.Headers.ToDictionary(x => x.Key, x => (object)string.Join(";", x.Value));

            await _dispatcher.Dispatch(message, headers, HttpContext.RequestAborted, _provider, new FunctionScope(_provider));

            return Ok(message);
        }

        [NonAction]
        public async Task<ActionResult> Put<TMessage>([FromBody] TMessage message)
            where TMessage : class
        {
            Dictionary<string, object> headers = HttpContext.Request.Headers.ToDictionary(x => x.Key, x => (object)string.Join(";", x.Value));

            await _dispatcher.Dispatch(message, headers, HttpContext.RequestAborted, _provider, new FunctionScope(_provider));

            return Ok();
        }


        class FunctionScope :
            IServiceScope
        {
            public FunctionScope(IServiceProvider serviceProvider)
            {
                ServiceProvider = serviceProvider;
            }

            public void Dispose()
            {
            }

            public IServiceProvider ServiceProvider { get; }
        }
    }
}
