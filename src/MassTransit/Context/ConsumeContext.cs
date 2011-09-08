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
namespace MassTransit.Context
{
    using System;
    using System.Linq;
    using Magnum.Reflection;
    using Util;
    using log4net;

    public class ConsumeContext<TMessage> :
        IConsumeContext<TMessage>
        where TMessage : class
    {
        static readonly ILog _log = LogManager.GetLogger(typeof (ReceiveContext));

        readonly IReceiveContext _context;
        readonly TMessage _message;
        Uri _responseAddress;

        public ConsumeContext(IReceiveContext context, TMessage message)
        {
            _context = context;
            _message = message;
            _responseAddress = context.ResponseAddress;
        }

        public IMessageHeaders Headers
        {
            get { return _context.Headers; }
        }

        public string RequestId
        {
            get { return _context.RequestId; }
        }

        public string ConversationId
        {
            get { return _context.ConversationId; }
        }

        public string CorrelationId
        {
            get { return _context.CorrelationId; }
        }

        public string MessageId
        {
            get { return _context.MessageId; }
        }

        public string MessageType
        {
            get { return typeof (TMessage).ToMessageName(); }
        }

        public string ContentType
        {
            get { return _context.ContentType; }
        }

        public Uri SourceAddress
        {
            get { return _context.SourceAddress; }
        }

        public Uri DestinationAddress
        {
            get { return _context.DestinationAddress; }
        }

        public Uri ResponseAddress
        {
            get { return _responseAddress; }
        }

        public Uri FaultAddress
        {
            get { return _context.FaultAddress; }
        }

        public string Network
        {
            get { return _context.Network; }
        }

        public DateTime? ExpirationTime
        {
            get { return _context.ExpirationTime; }
        }

        public int RetryCount
        {
            get { return _context.RetryCount; }
        }

        public IServiceBus Bus
        {
            get { return _context.Bus; }
        }

        public IEndpoint Endpoint
        {
            get { return _context.Endpoint; }
        }

        public Uri InputAddress
        {
            get { return _context.InputAddress; }
        }

        public TMessage Message
        {
            get { return _message; }
        }

        public bool TryGetContext<T>(out IConsumeContext<T> context)
            where T : class
        {
            if (typeof (T).IsAssignableFrom(Message.GetType()))
            {
                context = new ConsumeContext<T>(_context, Message as T);
                return true;
            }

            context = null;
            return false;
        }

        public IReceiveContext BaseContext
        {
            get { return _context; }
        }

        public void RetryLater()
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Retrying message of type {0} later", typeof (TMessage));

            Bus.Endpoint.Send(Message, x =>
                {
                    x.SetUsing(this);
                    x.SetRetryCount(RetryCount + 1);
                });
        }

        public void Respond<T>(T message, Action<ISendContext<T>> contextCallback)
            where T : class
        {
            _context.Respond(message, contextCallback);
        }

        public void GenerateFault(Exception ex)
        {
            if (Message == null)
                throw new InvalidOperationException("A fault cannot be generated when no message is present");

            Type correlationType = typeof (TMessage).GetInterfaces()
                .Where(x => x.IsGenericType)
                .Where(x => x.GetGenericTypeDefinition() == typeof (CorrelatedBy<>))
                .Select(x => x.GetGenericArguments()[0])
                .DefaultIfEmpty(null)
                .FirstOrDefault();

            if (correlationType != null)
            {
                this.FastInvoke(new[] {typeof (TMessage), correlationType}, "CreateAndSendCorrelatedFault", Message, ex);
            }
            else
            {
                this.FastInvoke(new[] {typeof (TMessage)}, "CreateAndSendFault", Message, ex);
            }
        }

        public void SetResponseAddress(Uri value)
        {
            _responseAddress = value;
        }

        [UsedImplicitly]
        void CreateAndSendFault<TMessage>(TMessage message, Exception exception)
            where TMessage : class
        {
            var fault = new Fault<TMessage>(message, exception);

            SendFault(fault);
        }

        [UsedImplicitly]
        void CreateAndSendCorrelatedFault<TMessage, TKey>(TMessage message, Exception exception)
            where TMessage : class, CorrelatedBy<TKey>
        {
            var fault = new Fault<TMessage, TKey>(message, exception);

            SendFault(fault);
        }

        void SendFault<T>(T message)
            where T : class
        {
            if (FaultAddress != null)
            {
                Bus.GetEndpoint(FaultAddress).Send(message, context =>
                    {
                        context.SetSourceAddress(Bus.Endpoint.Address.Uri);
                        context.SetRequestId(RequestId);
                    });
            }
            else if (ResponseAddress != null)
            {
                Bus.GetEndpoint(ResponseAddress).Send(message, context =>
                    {
                        context.SetSourceAddress(Bus.Endpoint.Address.Uri);
                        context.SetRequestId(RequestId);
                    });
            }
            else
            {
                Bus.Publish(message, context => context.SetRequestId(RequestId));
            }
        }
    }
}