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
    using Util;


    public class FactBuilder
    {
        /// <summary>
        /// Create a fact, using the type initializer
        /// </summary>
        /// <param name="values">The values to initialize the fact</param>
        /// <typeparam name="T">The fact type</typeparam>
        /// <exception cref="System.ArgumentException"></exception>
        public FactHandle<T> Create<T>(object values)
            where T : class
        {
            if (!TypeMetadataCache<T>.IsValidMessageType)
                throw new ArgumentException($"The Fact is not a valid type: {TypeMetadataCache<T>.ShortName}", nameof(T));

            var fact = TypeMetadataCache<T>.InitializeFromObject(values);

            return Create(fact);
        }

        /// <summary>
        /// Create a fact, using the actual fact object
        /// </summary>
        /// <param name="fact">The fact object</param>
        /// <typeparam name="T">The fact type</typeparam>
        /// <exception cref="System.ArgumentException"></exception>
        public FactHandle<T> Create<T>(T fact)
            where T : class
        {
            if (!TypeMetadataCache<T>.IsValidMessageType)
                throw new ArgumentException($"The Fact is not a valid type: {TypeMetadataCache<T>.ShortName}", nameof(T));

            var id = NewId.NextGuid();

            var handle = new CreatedFactHandle<T>(id, fact);

            return handle;
        }
    }
}