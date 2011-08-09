using System;
using System.Web;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using MassTransit;
using WebRequestReply.Core;
using log4net;
using log4net.Config;

namespace WebRequestReply.UI
{
	public class Global : HttpApplication, IContainerAccessor, IBusAccessor
	{
		public IWindsorContainer Container { get; protected set; }
		public IServiceBus Bus { get { return _uiBus; } }
		private static IServiceBus _coreBus;
		private static IServiceBus _uiBus;

		protected void Application_Start(object sender, EventArgs e)
		{
			BasicConfigurator.Configure();

			Container = new WindsorContainer();
			Container.AddFacility<TypedFactoryFacility>();
			Container.Register(Component.For<IServiceBus>().UsingFactoryMethod((k, c) =>
			{
				return ServiceBusFactory.New(sbc =>
				{
					sbc.UseRabbitMq();
					sbc.UseRabbitMqRouting();
					sbc.ReceiveFrom("rabbitmq://localhost/WebRequestReply.UI");
					sbc.Subscribe(s => s.LoadFrom(Container));
				});
			}).LifeStyle.Singleton);

			var coreContainer = new WindsorContainer();
			coreContainer.AddFacility<TypedFactoryFacility>();
			coreContainer.Register(Component.For<Service>().LifeStyle.Singleton,
				Component.For<IServiceBus>().UsingFactoryMethod((k, c) =>
				{
					return ServiceBusFactory.New(sbc =>
					{
						sbc.UseRabbitMq();
						sbc.UseRabbitMqRouting();
						sbc.ReceiveFrom("rabbitmq://localhost/WebRequestReply.Core.Service");
						sbc.Subscribe(s => s.LoadFrom(coreContainer));
					});
				}).LifeStyle.Singleton);
			
			_coreBus = coreContainer.Resolve<IServiceBus>();
			_uiBus = Container.Resolve<IServiceBus>();
		}

		protected void Application_End(object sender, EventArgs e)
		{
			if (Bus != null)
			{
				Bus.Dispose();
				_uiBus = null;
			}

			if (_coreBus != null)
			{
				_coreBus.Dispose();
				_coreBus = null;
			}

			Container.Dispose();
			Container = null;

			LogManager.Shutdown();
		}

		protected void Session_Start(object sender, EventArgs e)
		{
		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{
		}

		protected void Application_Error(object sender, EventArgs e)
		{
		}

		protected void Session_End(object sender, EventArgs e)
		{
		}
	}
}