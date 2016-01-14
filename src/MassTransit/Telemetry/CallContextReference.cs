// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Telemetry
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.Remoting.Messaging;
    using Util;


    [Serializable]
    public abstract class CallContextReference<T>
    {
        static readonly string Key = $"{TypeMetadataCache<T>.ShortName}_{NewId.NextGuid().ToString("N")}";

        readonly Lazy<ConcurrentBag<CallContextReference<T>>> _children;

        protected CallContextReference()
        {
            ObjectId = NewId.NextGuid();

            _children = new Lazy<ConcurrentBag<CallContextReference<T>>>(() => new ConcurrentBag<CallContextReference<T>>());
        }

        public Guid ObjectId { get; }

        public CallContextReference<T> Parent { get; private set; }

        /// <summary>
        /// Gets the children.
        /// </summary>
        public IEnumerable<CallContextReference<T>> Children => _children.Value;

        /// <summary>
        /// Detaches this instance.
        /// </summary>
        public void Detach()
        {
            if (Parent != null)
            {
                CallContext.LogicalSetData(Key, Parent);
            }
            else
            {
                CallContext.FreeNamedDataSlot(Key);
            }

            Parent = null;
        }

        /// <summary>
        /// Attaches this instance.
        /// </summary>
        public void Attach()
        {
            var existingContext = (CallContextReference<T>)CallContext.LogicalGetData(Key);

            if (existingContext != null)
            {
                Parent = existingContext;
                Parent._children.Value.Add(this);
            }

            CallContext.LogicalSetData(Key, this);
        }

        protected static CallContextReference<T> FromCallContext()
        {
            return (CallContextReference<T>)CallContext.LogicalGetData(Key);
        }
    }
}