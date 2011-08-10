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
namespace MassTransit.Transports.Msmq
{
	using System;
	using System.Messaging;
	using Context;
	using Exceptions;
	using Magnum.Extensions;
	using log4net;

	public abstract class InboundMsmqTransport :
		IInboundTransport
	{
		readonly IMsmqEndpointAddress _address;
		readonly ConnectionHandler<MessageQueueConnection> _connectionHandler;
		static readonly ILog _log = LogManager.GetLogger(typeof (InboundMsmqTransport));
		static readonly ILog _messageLog = LogManager.GetLogger("MassTransit.Msmq.MessageLog");
		bool _disposed;

		protected InboundMsmqTransport(IMsmqEndpointAddress address, ConnectionHandler<MessageQueueConnection> connectionHandler)
		{
			_address = address;
			_connectionHandler = connectionHandler;
		}

		public IEndpointAddress Address
		{
			get { return _address; }
		}

		public virtual void Receive(Func<IReceiveContext, Action<IReceiveContext>> callback, TimeSpan timeout)
		{
			try
			{
				EnumerateQueue(callback, timeout);
			}
			catch (MessageQueueException ex)
			{
				HandleInboundMessageQueueException(ex);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected bool EnumerateQueue(Func<IReceiveContext, Action<IReceiveContext>> receiver, TimeSpan timeout)
		{
			if (_disposed)
				throw new ObjectDisposedException("The transport has been disposed: '{0}'".FormatWith(Address));

			bool received = false;

			_connectionHandler.Use(connection =>
				{
					using (MessageEnumerator enumerator = connection.Queue.GetMessageEnumerator2())
					{
						// if (_log.IsDebugEnabled)
						// _log.DebugFormat("Enumerating endpoint: {0} ({1}ms)", Address, timeout);

						while (enumerator.MoveNext(timeout))
						{
							if (enumerator.Current == null)
							{
								if (_log.IsDebugEnabled)
									_log.DebugFormat("Current message was null while enumerating endpoint");

								continue;
							}

							Message message = enumerator.Current;

							IReceiveContext context = ReceiveContext.FromBodyStream(message.BodyStream);
							context.SetMessageId(message.Id);
							context.SetInputAddress(_address);

							using (context.CreateScope())
							{
								Action<IReceiveContext> receive;
								using (message)
								{
									byte[] extension = message.Extension;
									if (extension.Length > 0)
									{
										TransportMessageHeaders headers = TransportMessageHeaders.Create(extension);

										context.SetContentType(headers["Content-Type"]);
									}

									receive = receiver(context);
									if (receive == null)
									{
										if (_log.IsDebugEnabled)
											_log.DebugFormat("SKIP:{0}:{1}", Address, message.Id);

										continue;
									}
								}

								ReceiveMessage(enumerator, timeout, receiveCurrent =>
									{
										using (message = receiveCurrent())
										{
											if (message == null)
												throw new TransportException(Address.Uri,
													"Unable to remove message from queue: " + context.MessageId);

											if (message.Id != context.MessageId)
												throw new TransportException(Address.Uri,
													string.Format(
														"Received message does not match current message: ({0} != {1})",
														message.Id, context.MessageId));

											if (_messageLog.IsDebugEnabled)
												_messageLog.DebugFormat("RECV:{0}:{1}:{2}", _address.InboundFormatName, message.Label, message.Id);

											receive(context);

											received = true;
										}
									});
							}
						}
					}
				});

			return received;
		}

		protected virtual void ReceiveMessage(MessageEnumerator enumerator, TimeSpan timeout,
		                                      Action<Func<Message>> receiveAction)
		{
			receiveAction(() => enumerator.RemoveCurrent(timeout, MessageQueueTransactionType.None));
		}

		protected void HandleInboundMessageQueueException(MessageQueueException ex)
		{
			switch (ex.MessageQueueErrorCode)
			{
				case MessageQueueErrorCode.IOTimeout:
					break;

				case MessageQueueErrorCode.ServiceNotAvailable:
					throw new InvalidConnectionException(_address.Uri,"The message queuing service is not available, pausing for timeout period", ex);

				case MessageQueueErrorCode.QueueNotAvailable:
				case MessageQueueErrorCode.AccessDenied:
				case MessageQueueErrorCode.QueueDeleted:
					throw new InvalidConnectionException(_address.Uri, "The message queue was not available", ex);

				case MessageQueueErrorCode.QueueNotFound:
				case MessageQueueErrorCode.IllegalFormatName:
				case MessageQueueErrorCode.MachineNotFound:
					throw new InvalidConnectionException(_address.Uri, "The message queue was not found or is improperly named", ex);

				case MessageQueueErrorCode.MessageAlreadyReceived:
					// we are competing with another consumer, no reason to report an error since
					// the message has already been handled.
					if (_log.IsDebugEnabled)
						_log.Debug(
							"The message was removed from the queue before it could be received. This could be the result of another service reading from the same queue.");
					break;

				case MessageQueueErrorCode.InvalidHandle:
				case MessageQueueErrorCode.StaleHandle:
					throw new InvalidConnectionException(_address.Uri,"The message queue handle is stale or no longer valid due to a restart of the message queuing service", ex);


				default:
					throw new InvalidConnectionException(_address.Uri, "There was a problem communicating with the message queue", ex);
			}
		}

		void Dispose(bool disposing)
		{
			if (_disposed) return;
			if (disposing)
			{
				_connectionHandler.Dispose();
			}

			_disposed = true;
		}

		~InboundMsmqTransport()
		{
			Dispose(false);
		}
	}
}