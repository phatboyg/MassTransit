// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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


    public readonly struct SessionTokenContext<TFact> :
        TokenContext<TFact>
        where TFact : class
    {
        readonly SessionContext _sessionContext;
        readonly ITokenList<TFact> _tokenList;

        public SessionTokenContext(in SessionContext sessionContext, ITokenList<TFact> tokenList)
        {
            _sessionContext = sessionContext;

            _tokenList = tokenList;
        }

        public Guid SessionId => _sessionContext.SessionId;

        public Guid FactId => _tokenList.FactId;
        public Type FactType => _tokenList.FactType;
        public string[] FactTypes => _tokenList.FactTypes;
        public TFact Fact => _tokenList.Fact;
        object FactHandle.Fact => _tokenList.Fact;

        public int TokenCount => _tokenList.Count;

        public bool TryGetFact<T>(int index, out FactContext<T> factContext)
            where T : class
        {
            if (_tokenList.TryGetFact(index, out FactHandle<T> handle))
            {
                factContext = new TokenFactContext<T>(_sessionContext, handle);
                return true;
            }

            factContext = default;
            return false;
        }


        readonly struct TokenFactContext<T> :
            FactContext<T>
            where T : class
        {
            readonly SessionContext _sessionContext;
            readonly FactHandle<T> _fact;

            public TokenFactContext(SessionContext sessionContext, FactHandle<T> fact)
            {
                _sessionContext = sessionContext;
                _fact = fact;
            }

            public Guid SessionId => _sessionContext.SessionId;

            Guid FactHandle.FactId => _fact.FactId;
            Type FactHandle.FactType => _fact.FactType;
            string[] FactHandle.FactTypes => _fact.FactTypes;
            T FactHandle<T>.Fact => _fact.Fact;
            object FactHandle.Fact => ((FactHandle)_fact).Fact;
        }
    }
}