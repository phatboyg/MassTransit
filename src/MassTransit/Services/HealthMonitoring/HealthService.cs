// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Services.HealthMonitoring
{
	using System;
	using log4net;
	using Magnum.Extensions;
	using Messages;
	using Saga;
	using Server;

	/// <summary>
	/// The health service attempts to track all known endpoints and monitor their status for up/down. 
	/// </summary>
	public class HealthService :
		Consumes<HealthUpdateRequest>.All,
		IDisposable
	{
		static readonly ILog _log = LogManager.GetLogger(typeof (HealthService));
		readonly IServiceBus _bus;
		readonly ISagaRepository<HealthSaga> _healthSagas;
		UnsubscribeAction _unsubscribeToken = () => false;

		public HealthService(IServiceBus bus, ISagaRepository<HealthSaga> healthSagas)
		{
			_bus = bus;
			_healthSagas = healthSagas;
		}

		public void Consume(HealthUpdateRequest message)
		{
			UpdateSubscribers();
		}

		public void Dispose()
		{
			_bus.Dispose();
		}

		public void Start()
		{
			_log.Info("Health Service Starting");

			_unsubscribeToken += _bus.SubscribeInstance(this);
			_unsubscribeToken += _bus.SubscribeSaga(_healthSagas);

			_log.Info("Health Service Started");
		}

		public void Stop()
		{
			_log.Info("Health Service Stopping");

			_unsubscribeToken();

			_log.Info("Health Service Stopped");
		}

		void UpdateSubscribers()
		{
			var message = new HealthUpdate();

			_healthSagas
				.Select(x => new HealthInformation(x.CorrelationId, x.ControlUri, x.DataUri, x.LastHeartbeat, x.CurrentState.Name))
				.Each(x => message.Information.Add(x));

			_log.Debug("Publishing HealthUpdate");
			_bus.Publish(message);
		}
	}
}