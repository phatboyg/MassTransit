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


    /// <summary>
    /// A list of facts in the order which they were evaluated from left to right into the network.
    /// </summary>
    /// <typeparam name="TFact">The last fact type in the list</typeparam>
    public readonly struct TokenList<TFact> :
        ITokenList<TFact>
        where TFact : class
    {
        readonly FactHandle[] _facts;
        readonly FactToken[] _tokens;
        readonly FactHandle<TFact> _fact;

        public TokenList(in FactHandle[] facts, in FactToken[] tokens)
        {
            if (facts == null || facts.Length == 0)
                throw new ArgumentNullException(nameof(facts));

            if (tokens == null || tokens.Length == 0)
                throw new ArgumentNullException(nameof(tokens));


            _facts = facts;
            _tokens = tokens;

            _fact = facts[tokens[0].Index] as FactHandle<TFact>;
            if (_fact == null)
                throw new ArgumentException("The first token did not match the fact type", nameof(tokens));
        }

        bool ITokenList.TryGetFact<T>(int index, out FactHandle<T> fact)
        {
            if (index >= _tokens.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var token = _tokens[index];

            if (token.Index >= _facts.Length)
                throw new InvalidOperationException($"The fact index was invalid: {token.Index}");

            if (_facts[token.Index] is FactHandle<T> factHandle)
            {
                fact = factHandle;
                return true;
            }

            fact = default;
            return false;
        }

        int ITokenList.Count => _facts.Length;

        Guid FactHandle.FactId => _fact.FactId;
        Type FactHandle.FactType => _fact.FactType;
        string[] FactHandle.FactTypes => _fact.FactTypes;
        TFact FactHandle<TFact>.Fact => _fact.Fact;
        object FactHandle.Fact => ((FactHandle)_fact).Fact;
    }
}