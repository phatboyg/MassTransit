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
namespace MassTransit.Transports
{
	using System;
	using System.Diagnostics;
	using System.Runtime.Serialization;
	using Context;
	using Exceptions;
	using Serialization;
	using Util;
	using log4net;

	[DebuggerDisplay("{Address}")]
	public class Endpoint :
		IEndpoint
	{
		static readonly ILog _log = LogManager.GetLogger(typeof (Endpoint));
		readonly IEndpointAddress _address;
		readonly IMessageSerializer _serializer;
		readonly MessageRetryTracker _tracker;
		bool _disposed;
		string _disposedMessage;
		IOutboundTransport _errorTransport;
		IDuplexTransport _transport;

		public Endpoint(IEndpointAddress address, IMessageSerializer serializer, IDuplexTransport transport,
		                IOutboundTransport errorTransport)
		{
			_address = address;
			_transport = transport;
			_errorTransport = errorTransport;
			_serializer = serializer;

			_tracker = new MessageRetryTracker(5);

			SetDisposedMessage();
		}

		public IOutboundTransport ErrorTransport
		{
			get { return _errorTransport; }
		}

		public IMessageSerializer Serializer
		{
			get { return _serializer; }
		}

		public IEndpointAddress Address
		{
			get { return _address; }
		}

		public IInboundTransport InboundTransport
		{
			get { return _transport.InboundTransport; }
		}

		public IOutboundTransport OutboundTransport
		{
			get { return _transport.OutboundTransport; }
		}

		public void Send<T>(ISendContext<T> context)
			where T : class
		{
			if (_disposed)
				throw new ObjectDisposedException(_disposedMessage);

			try
			{
				context.SetDestinationAddress(Address.Uri);
				context.SetBodyWriter(stream => _serializer.Serialize(stream, context));

				_transport.Send(context);

				context.NotifySend(_address);

				if (SpecialLoggers.Messages.IsInfoEnabled)
					SpecialLoggers.Messages.InfoFormat("SEND:{0}:{1}:{2}", Address, typeof (T).Name, context.MessageId);
			}
			catch (Exception ex)
			{
				throw new SendException(typeof (T), _address.Uri, "An exception was thrown during Send", ex);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Receive(Func<IReceiveContext, Action<IReceiveContext>> receiver, TimeSpan timeout)
		{
			if (_disposed)
				throw new ObjectDisposedException(_disposedMessage);

			_transport.Receive(acceptContext =>
				{
					if (_tracker.IsRetryLimitExceeded(acceptContext.MessageId))
					{
						if (_log.IsErrorEnabled)
							_log.ErrorFormat("Message retry limit exceeded {0}:{1}", Address, acceptContext.MessageId);

						return MoveMessageToErrorTransport;
					}

					Action<IReceiveContext> receive;
					try
					{
						_serializer.Deserialize(acceptContext);
						acceptContext.SetEndpoint(this);

						receive = receiver(acceptContext);
						if (receive == null)
							return null;
					}
					catch (SerializationException sex)
					{
						if (_log.IsErrorEnabled)
							_log.Error("Unrecognized message " + Address + ":" + acceptContext.MessageId, sex);

						_tracker.IncrementRetryCount(acceptContext.MessageId);
						return MoveMessageToErrorTransport;
					}
					catch (Exception ex)
					{
						if (_log.IsErrorEnabled)
							_log.Error("An exception was thrown preparing the message consumers", ex);

						_tracker.IncrementRetryCount(acceptContext.MessageId);
						return null;
					}

					return receiveContext =>
						{
							try
							{
								receive(receiveContext);

								_tracker.MessageWasReceivedSuccessfully(receiveContext.MessageId);
							}
							catch (Exception ex)
							{
								if (_log.IsErrorEnabled)
									_log.Error("An exception was thrown by a message consumer", ex);

								_tracker.IncrementRetryCount(receiveContext.MessageId);
								MoveMessageToErrorTransport(receiveContext);
							}
						};
				}, timeout);
		}

		void SetDisposedMessage()
		{
			_disposedMessage = "The endpoint has already been disposed: " + _address;
		}

		void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				_tracker.Dispose();

				_transport.Dispose();
				_transport = null;

				_errorTransport.Dispose();
				_errorTransport = null;
			}

			_disposed = true;
		}

		void MoveMessageToErrorTransport(IReceiveContext context)
		{
			var moveContext = new MoveMessageSendContext(context);

			_errorTransport.Send(moveContext);

			if (_log.IsDebugEnabled)
				_log.DebugFormat("MOVE:{0}:{1}:{2}", Address, _errorTransport.Address, context.MessageId);

			if (SpecialLoggers.Messages.IsInfoEnabled)
				SpecialLoggers.Messages.InfoFormat("MOVE:{0}:{1}:{2}", Address, _errorTransport.Address, context.MessageId);
		}

		~Endpoint()
		{
			Dispose(false);
		}
	}
}