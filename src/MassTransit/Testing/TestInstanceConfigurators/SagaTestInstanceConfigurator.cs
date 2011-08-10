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
namespace MassTransit.Testing.TestInstanceConfigurators
{
	using System;
	using BuilderConfigurators;
	using Builders;
	using Saga;
	using Scenarios;

	public interface SagaTestInstanceConfigurator<TScenario, TSaga> :
		TestInstanceConfigurator<TScenario>
		where TSaga : class, ISaga
		where TScenario : TestScenario
	{
		void UseBuilder(Func<TScenario, SagaTestBuilder<TScenario, TSaga>> builderFactory);
		void AddConfigurator(SagaTestBuilderConfigurator<TScenario, TSaga> configurator);

		void UseSagaRepository(ISagaRepository<TSaga> sagaRepository);
	}
}