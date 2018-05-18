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
    using System.Threading.Tasks;
    using Util;


    /// <summary>
    /// An alpha memory stores the fact in memory, so that it can be later dispatched to beta
    /// nodes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AlphaMemory<T>
        where T : class
    {
        /// <summary>
        /// Insert a Fact into the alpha memory, should maintain order
        /// </summary>
        /// <param name="context">The fact context</param>
        /// <returns></returns>
        public Task InsertFact(in FactContext<T> context)
        {
            return TaskUtil.Completed;
        }
    }
}