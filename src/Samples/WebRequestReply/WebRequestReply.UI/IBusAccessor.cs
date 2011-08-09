using MassTransit;

namespace WebRequestReply.UI
{
	public interface IBusAccessor
	{
		IServiceBus Bus { get; }
	}
}