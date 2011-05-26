// Copyright 2007-2011 The Apache Software Foundation.
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
namespace MassTransit.Transports
{
	using System;
	using Context;

	public interface IInboundTransport :
		ITransport
	{
		/// <summary>
		/// Implementors should create a receive context (e.g. a <see cref="ConsumeContext"/>)
		/// and call the getConsumers function with it, in order to get a handler; the action result,
		/// or otherwise to get null, if there are no handlers available.
		/// </summary>
		/// <param name="getConsumers">A function taking a receive context and returning the handlers for it. 
		/// Return null if there 
		/// are no handlers for it.</param>
		/// <param name="dequeueTimeout">A 'setting' parameter, specifying how long the listening thread
		/// should wait before 'doing another loop'.</param>
		void Receive(Func<IReceiveContext, Action<IReceiveContext>> getConsumers, TimeSpan dequeueTimeout);
	}
}