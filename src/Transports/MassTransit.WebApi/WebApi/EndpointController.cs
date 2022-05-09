namespace MassTransit.WebApi
{
    using Microsoft.AspNetCore.Mvc;


    [Route("endpoint/[controller]")]
    public class EndpointController<T> :
        ControllerBase
        where T : class
    {
        public EndpointController()
        {
        }

        // [HttpGet]
        // public IEnumerable<T> Get()
        // {
        //     return _storage.GetAll();
        // }
        //
        // [HttpGet("{id}")]
        // public T Get(Guid id)
        // {
        //     return _storage.GetById(id);
        // }
        //
        // [HttpPost("{id}")]
        // public void Post(Guid id, [FromBody] T value)
        // {
        //     _storage.Add(id, value);
        // }
    }
}