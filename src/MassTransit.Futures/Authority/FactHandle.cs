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


    /// <summary>
    /// References a fact that has been added to a session
    /// </summary>
    /// <typeparam name="T">The fact type</typeparam>
    public interface FactHandle<out T> :
        FactHandle
        where T : class
    {
        /// <summary>
        /// That's the Fact, Jack
        /// </summary>
        new T Fact { get; }
    }


    /// <summary>
    /// A handle to a fact, which can be added to a session
    /// </summary>
    public interface FactHandle
    {
        /// <summary>
        /// Uniquely identifies this fact across the universe
        /// </summary>
        Guid FactId { get; }

        /// <summary>
        /// The fact type
        /// </summary>
        Type FactType { get; }

        /// <summary>
        /// The types supported by this fact
        /// </summary>
        string[] FactTypes { get; }

        /// <summary>
        /// The fact object
        /// </summary>
        object Fact { get; }
    }


    public interface ISessionClient
    {
        Task<Response<FactInserted>> InsertFact<T>(FactHandle<T> fact)
            where T : class;
    }


    public class AuthoritySessionClient :
        ISessionClient
    {
        readonly IClientFactory _clientFactory;
        readonly Uri _factAddress;
        readonly IRequestClient<InsertFact> _insertFactClient;
        readonly Guid _sessionId;

        public AuthoritySessionClient(IClientFactory clientFactory, Uri hostAddress, Guid sessionId)
        {
            _clientFactory = clientFactory;
            _sessionId = sessionId;

            _factAddress = new Uri(hostAddress, "authority-facts");
            _insertFactClient = clientFactory.CreateRequestClient<InsertFact>(_factAddress);
        }

        public async Task<Response<FactInserted>> InsertFact<T>(FactHandle<T> fact)
            where T : class
        {
            using (var request = _insertFactClient.Create(new
            {
                SessionId = _sessionId,
                FactId = fact.FactId,
                fact.FactTypes,
                fact.Fact
            }))
            {
                var response = await request.GetResponse<FactInserted>();

                return response;
            }
        }
    }
}