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
namespace MassTransit.Authority.Contracts
{
    using System;


    /// <summary>
    /// Insert a fact into a session
    /// </summary>
    public interface InsertFact
    {
        /// <summary>
        /// The Session to which the fact should be added
        /// </summary>
        Guid SessionId { get; }

        /// <summary>
        /// Uniquely identifies the fact (in this session)
        /// </summary>
        Guid FactId { get; }

        /// <summary>
        /// The types supported by the fact
        /// </summary>
        string[] FactTypes { get; }

        /// <summary>
        /// The fact content itself (serialized, obviously to be read out by type)
        /// </summary>
        object Fact { get; }
    }
}