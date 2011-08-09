using System.Web;

namespace WebRequestReply.UI
{
	using System;
	using System.Web.UI;
	using Core;
	using MassTransit;

    public partial class _Default :
		Page, IRequestReplyView
	{
		private readonly RequestReplyController _controller;

		public _Default()
		{
			var ba = (IBusAccessor)HttpContext.Current.ApplicationInstance;
			var bus = ba.Bus;

			_controller = new RequestReplyController(this, 
				bus, 
				bus.GetEndpoint(new Uri("loopback://localhost/WebRequestReply.Core.Service")));
		}

		public string RequestText
		{
			get { return requestText.Text; }
		}

		public string ResponseText
		{
			set { responseBox.Text = value; }
		}

		protected void Button1_Click(object sender, EventArgs e)
		{
			_controller.SendRequest();
		}

		protected void Button2_Click(object sender, EventArgs e)
		{
			RegisterAsyncTask(new PageAsyncTask(_controller.BeginRequest, _controller.EndRequest, x => { }, this));
		}
	}
}