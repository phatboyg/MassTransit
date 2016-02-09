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


    public class ContextPropertyCache
    {
        
    }


    public interface IContextPropertyCache
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
        bool TryGetPropertyValue<T>(out IContextPropertyValue value)
            where T : class;

        /// <summary>
        /// Returns the value of the property if it exists in the cache
        /// </summary>
        /// <typeparam name="T">The property type</typeparam>
        /// <param name="name">The property name</param>
        /// <param name="value">The property value</param>
        /// <returns>True if the value was returned, otherwise false</returns>
        bool TryGetPropertyValue<T>(string name, out IContextPropertyValue value)
            where T : class;

        /// <summary>
        /// Returns an existing payload or creates the payload using the factory method provided
        /// </summary>
        /// <typeparam name="TPayload">The payload type</typeparam>
        /// <param name="payloadFactory">The payload factory is the payload is not present</param>
        /// <returns>The payload</returns>
        TPayload GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory)

    }

    /// <summary>
    /// A property within the context
    /// </summary>
    public class ContextProperty :
        IContextProperty
    {
        readonly IContextPropertyValue _value;

        public ContextProperty(string name, IContextPropertyValue value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (!IsValidPropertyName(name))
                throw new ArgumentException("Property name is not valid.");

            Name = name;
            _value = value;
        }

        public string Name { get; }

        public Type ValueType => _value.ValueType;

        public bool TryGetValue<T>(out T value) where T : class
        {
            return _value.TryGetValue(out value);
        }

        public static bool IsValidPropertyName(string name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }
    }
}