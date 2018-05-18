// Copyright 2012-2018 Chris Patterson
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
namespace MassTransit.Authority
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using Courier.Serialization;
    using Internals;
    using Logging;
    using Newtonsoft.Json.Linq;
    using Util;


    public class InsertFactConsumer :
        IConsumer<InsertFact>,
        IConsumer<ConnectFactTypeSink>
    {
        static readonly ILog _log = Logger.Get<InsertFactConsumer>();

        readonly ConnectionList<IFactSink> _factSinks;
        readonly IClientFactory _clientFactory;

        public InsertFactConsumer(ConnectionList<IFactSink> factSinks, IClientFactory clientFactory)
        {
            _factSinks = factSinks;
            _clientFactory = clientFactory;
        }

        public Task Consume(ConsumeContext<ConnectFactTypeSink> context)
        {
            _factSinks.Connect(new FactTypeSink(context.Message.FactType, context.Message.DestinationAddress));

            return TaskUtil.Completed;
        }

        public async Task Consume(ConsumeContext<InsertFact> context)
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Insert Fact: {0} ({1})", context.Message.FactId, string.Join(", ", context.Message.FactTypes));

            await _factSinks.All(async sink =>
            {
                var (matches,destination) = sink.Matches(context.Message.FactTypes);
                if (matches)
                {
                    using (var request = _clientFactory.CreateRequest<InsertFact>(context, destination, context.Message))
                    {
                        await request.GetResponse<FactInserted>();
                    }
                }
            });

            await context.RespondAsync<FactInserted>(context.Message);
        }
    }


    /// <summary>
    /// Connects to the fact, and subscribes to facts that are added with a matching fact type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InsertFactTypeConsumer<T> :
        IConsumer<InsertFact>
        where T : class
    {
        static readonly ILog _log = Logger.Get<InsertFactTypeConsumer<T>>();

        public async Task Consume(ConsumeContext<InsertFact> context)
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat("Insert Fact: {0} ({1})", context.Message.FactId, TypeMetadataCache<T>.ShortName);

            var factHandle = context.GetFactHandle<T>(context.Message.FactId, "fact");

            await context.RespondAsync<FactInserted>(context.Message);
        }
    }


    public static class FactHandleExtensions
    {
        public static FactHandle<T> GetFactHandle<T>(this ConsumeContext context, Guid factId, string propertyName)
            where T : class
        {
            if (!context.TryGetMessage(out ConsumeContext<JToken> messageTokenContext))
                throw new MessageException(typeof(InsertFact), "Unable to retrieve JSON token");

            var messageToken = messageTokenContext.Message;

            var converter = new DefaultJsonTypeConverter<T>();

            var factToken = messageToken[propertyName] ?? new JObject();

            var fact = converter.Convert(factToken);

            var factHandle = new CreatedFactHandle<T>(factId, fact);

            return factHandle;
        }
    }


    public class DummyTupleConsumer<T> :
        IConsumer<ActivateFact>
        where T : class
    {
        readonly IConnectionList<ITupleSink> _sinks;
        readonly IClientFactory _clientFactory;

        public DummyTupleConsumer(IClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _sinks = new ConnectionList<ITupleSink>();
        }

        public async Task Consume(ConsumeContext<ActivateFact> context)
        {
            var factHandle = context.GetFactHandle<T>(context.Message.FactId, "fact");

            var builder = new TupleBuilder();
            builder.Add(factHandle);

            var tuple = builder.Build();
            var tupleId = NewId.NextGuid();

            await _sinks.All(async sink =>
            {
                {
                    IRequestClient<InsertTuple> requestClient = _clientFactory.CreateRequestClient<InsertTuple>(context, sink.DestinationAddress);

                    using (var request = requestClient.Create(new
                    {
                        context.Message.SessionId,
                        TupleId = tupleId,
                        Right = tuple,
                    }))
                    {
                        await request.GetResponse<TupleInserted>();
                    }
                }
            });
        }
    }


    public interface ITupleSink
    {
        Uri DestinationAddress { get; }
    }


    public class TupleBuilder
    {
        Contracts.Token _right;

        public void Add<T>(FactHandle<T> fact)
            where T : class
        {
            _right = new TokenFact<T>(fact.FactId, fact.Fact, _right);
        }


        class TokenFact<T> :
            Contracts.Token
        {
            public TokenFact(Guid factId, T fact, Contracts.Token left = null)
            {
                FactId = factId;
                Fact = fact;
                Left = left;
            }

            public Guid FactId { get; }
            public string[] FactTypes => TypeMetadataCache<T>.MessageTypeNames;
            public object Fact { get; }
            public Contracts.Token Left { get; }
        }


        public Contracts.Token Build()
        {
            return _right;
        }
    }
}