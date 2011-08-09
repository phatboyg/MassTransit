using MassTransit;

namespace WebRequestReply.Core
{
	public class Service : Consumes<RequestMessage>.All
	{
		private readonly IServiceBus _Bus;

		public Service(IServiceBus bus)
		{
			_Bus = bus;
		}

		public void Consume(RequestMessage message)
		{
			_Bus.Context(cc => cc.Respond(new ResponseMessage(message.CorrelationId, "Request: " + message.Text)));
		}
	}
}