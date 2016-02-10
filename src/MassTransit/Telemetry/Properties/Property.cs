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
namespace MassTransit.Telemetry.Properties
{
    using System;


    /// <summary>
    /// A property within the context
    /// </summary>
    public class Property<TValue> :
        IProperty<TValue>
        where TValue : class
    {
        readonly IPropertyValue<TValue> _value;

        public Property(string name, IPropertyValue<TValue> value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (!IsValidPropertyName(name))
                throw new ArgumentException("Property name is not valid.");

            Name = name;
            _value = value;
        }

        public string Name { get; }

        Type IPropertyValue.ValueType => _value.ValueType;
        TValue IPropertyValue<TValue>.Value => _value.Value;

        bool IPropertyValue.TryGetValue<T>(out T value)
        {
            return _value.TryGetValue(out value);
        }

        public static bool IsValidPropertyName(string name)
        {
            return !string.IsNullOrWhiteSpace(name);
        }
    }
}