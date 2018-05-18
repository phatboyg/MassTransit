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
    /// <summary>
    /// Used to carry context between beta nodes, and includes the FactPath through the beta
    /// network.
    /// </summary>
    /// <typeparam name="TFact">The fact type</typeparam>
    public interface TokenContext<out TFact> :
        FactHandle<TFact>,
        SessionContext
        where TFact : class
    {
        int TokenCount { get; }

        /// <summary>
        /// Return the Fact at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="factContext"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool TryGetFact<T>(int index, out FactContext<T> factContext)
            where T : class;
    }
}