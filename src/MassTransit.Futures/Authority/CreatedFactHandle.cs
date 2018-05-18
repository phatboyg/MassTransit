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


    public class CreatedFactHandle<T> :
        FactHandle<T>
        where T : class
    {
        public CreatedFactHandle(Guid id, T fact)
        {
            FactId = id;
            Fact = fact;
        }

        public Guid FactId { get; }

        public Type FactType => typeof(T);

        public string[] FactTypes => TypeMetadataCache<T>.MessageTypeNames;

        object FactHandle.Fact => Fact;

        public T Fact { get; }
    }
}