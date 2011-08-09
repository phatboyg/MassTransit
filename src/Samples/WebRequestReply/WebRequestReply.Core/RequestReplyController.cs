namespace WebRequestReply.Core
{
	using System;
	using MassTransit;

	public class RequestReplyController 
	{
		private readonly IServiceBus _serviceBus;
		private readonly IEndpoint _targetService;
		private readonly IRequestReplyView _view;

		public RequestReplyController(IRequestReplyView view, IServiceBus serviceBus, IEndpoint targetService)
		{
			if (view == null) throw new ArgumentNullException("view");
			if (serviceBus == null) throw new ArgumentNullException("serviceBus");
			if (targetService == null) throw new ArgumentNullException("targetService");
			_view = view;
			_serviceBus = serviceBus;
			_targetService = targetService;
		}

		public void SendRequest()
		{
			Guid requestId = Guid.NewGuid();

			_targetService.SendRequest(new RequestMessage(requestId, _view.RequestText),
				_serviceBus, rc =>
				{
					rc.HandleTimeout(TimeSpan.FromMilliseconds(100), () =>
					{
						_view.ResponseText = "Async Task Timeout";
					});
					rc.Handle<ResponseMessage>(rm =>
					{
						_view.ResponseText = rm.Text;
					});
				});
		}

		public IAsyncResult BeginRequest(object sender, EventArgs e, AsyncCallback callback, object state)
		{
			Guid requestId = Guid.NewGuid();

			return _targetService.BeginSendRequest(_serviceBus, new RequestMessage(requestId, _view.RequestText),
				callback, state, rc =>
				{
					rc.HandleTimeout(TimeSpan.FromMilliseconds(100), () =>
					{
						_view.ResponseText = "Async Task Timeout";
					});
					rc.Handle<ResponseMessage>(rm =>
					{
						_view.ResponseText = rm.Text;
					});
				});
		}

		public void EndRequest(IAsyncResult ar)
		{
		}
	}
}