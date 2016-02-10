// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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


    /// <summary>
    /// Supports the reading of the property cache
    /// </summary>
    public interface IReadOnlyPropertyCollection
    {
        /// <summary>
        /// Checks if the property exists in the cache
        /// </summary>
        /// <param name="propertyType">The property type</param>
        /// <returns>True if the property exists in the cache, otherwise false</returns>
        bool HasPropertyType(Type propertyType);

        /// <summary>
        /// Checks if the property exists in the cache
        /// </summary>
        /// <param name="name">The property name</param>
        /// <returns>True if the property exists in the cache, otherwise false</returns>
        bool HasProperty(string name);

        /// <summary>
        /// Checks if the property exists in the cache
        /// </summary>
        /// <typeparam name="T">The property type</typeparam>
        /// <param name="name">The property name</param>
        /// <returns>True if the property exists in the cache, otherwise false</returns>
        bool HasProperty<T>(string name)
            where T : class;

        /// <summary>
        /// Returns the value of the property if it exists in the cache
        /// </summary>
        /// <typeparam name="T">The property type</typeparam>
        /// <param name="value">The property value</param>
        /// <returns>True if the value was returned, otherwise false</returns>
        bool TryGetPropertyValue<T>(out IPropertyValue value)
            where T : class;

        /// <summary>
        /// Returns the value of the property if it exists in the cache
        /// </summary>
        /// <typeparam name="T">The property type</typeparam>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        /// <returns>True if the value was returned, otherwise false</returns>
        bool TryGetPropertyValue<T>(string name, out IPropertyValue value)
            where T : class;
    }
}