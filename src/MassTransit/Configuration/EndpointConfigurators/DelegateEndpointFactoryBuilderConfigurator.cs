﻿// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.EndpointConfigurators
{
	using System;
	using System.Collections.Generic;
	using Builders;
	using Configurators;

	public class DelegateEndpointFactoryBuilderConfigurator :
		EndpointFactoryBuilderConfigurator
	{
		readonly Action<EndpointFactoryBuilder> _builderCallback;

		public DelegateEndpointFactoryBuilderConfigurator(Action<EndpointFactoryBuilder> builderCallback)
		{
			_builderCallback = builderCallback;
		}

		public IEnumerable<ValidationResult> Validate()
		{
			if (_builderCallback == null)
				yield return this.Failure("BuilderCallback", "was not configured. This should have been passed in via the ctor.");
		}

		public EndpointFactoryBuilder Configure(EndpointFactoryBuilder builder)
		{
			_builderCallback(builder);

			return builder;
		}
	}
}