namespace MassTransit.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Transports;


    public class SagaEndpointController<TSaga> :
        EndpointController<TSaga>
        where TSaga : class, ISaga
    {
        readonly IReceiveEndpointDispatcher<TSaga> _dispatcher;
        readonly IServiceProvider _provider;

        public SagaEndpointController(IServiceProvider provider, IReceiveEndpointDispatcher<TSaga> dispatcher)
        {
            _provider = provider;
            _dispatcher = dispatcher;
        }

        [NonAction]
        public async Task<ActionResult<TSaga>> Get([FromQuery] Guid id, [FromServices] ISagaRepository<TSaga> repository)
        {
            ILoadSagaRepository<TSaga> loadSagaRepository = repository as ILoadSagaRepository<TSaga> ?? throw new InvalidOperationException("No load saga");

            var saga = await loadSagaRepository.Load(id);

            return Ok(saga);
        }

        [NonAction]
        public async Task<ActionResult<TSaga>> Post<TMessage>([FromBody] TMessage message)
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
