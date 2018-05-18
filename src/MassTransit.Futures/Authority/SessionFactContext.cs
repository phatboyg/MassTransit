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
    /// Created within a session to store the fact context
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct SessionFactContext<T> :
        FactContext<T>
        where T : class
    {
        readonly SessionContext _sessionContext;
        readonly FactHandle<T> _fact;

        public SessionFactContext(SessionContext sessionContext, FactHandle<T> fact)
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