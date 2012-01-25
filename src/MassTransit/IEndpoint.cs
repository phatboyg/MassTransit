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
namespace MassTransit
{
	using System;
	using Serialization;
	using Transports;

	/// <summary>
	/// IEndpoint is implemented by an endpoint. An endpoint is an addressable location on the network.
	/// </summary>
	public interface IEndpoint :
		IDisposable
	{
		/// <summary>
		/// The address of the endpoint
		/// </summary>
		IEndpointAddress Address { get; }

		/// <summary>
		/// The inbound transport for the endpoint
		/// </summary>
		IInboundTransport InboundTransport { get; }

		/// <summary>
		/// The outbound transport for the endpoint
		/// </summary>
		IOutboundTransport OutboundTransport { get; }

		/// <summary>
		/// The transport where faulting messages (poison messages) are sent
		/// </summary>
		IOutboundTransport ErrorTransport { get; }

		/// <summary>
		/// The message serializer being used by the endpoint
		/// </summary>
		IMessageSerializer Serializer { get; }

		/// <summary>
		/// Send to the endpoint
		/// </summary>
		/// <typeparam name="T">The type of the message to send</typeparam>
		/// <param name="context"></param>
		void Send<T>(ISendContext<T> context)
			where T : class;

		/// <summary>
		/// Receive from the endpoint by passing a function that can preview the message and if the caller
		/// chooses to accept it, return a method that will consume the message.
		/// 
		/// Returns after the specified timeout if no message is available.
		/// </summary>
		/// <param name="receiver">The function to preview/consume the message</param>
		/// <param name="timeout">The time to wait for a message to be available</param>
		void Receive(Func<IReceiveContext, Action<IReceiveContext>> receiver, TimeSpan timeout);
	}
}