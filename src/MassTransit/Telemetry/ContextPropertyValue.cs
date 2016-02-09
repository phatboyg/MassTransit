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
    public static class ContextPropertyValue
    {
        /// <summary>
        /// Create an instance of scope data for the value specified
        /// </summary>
        /// <typeparam name="T">The value type</typeparam>
        /// <param name="value">The value</param>
        /// <returns></returns>
        public static IContextPropertyValue Create<T>(T value)
            where T : class
        {
            return new ContextPropertyValue<T>(value);
        }
    }


    /// <summary>
    /// Stores a single scope data value
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public class ContextPropertyValue<T1> :
        IContextPropertyValue
        where T1 : class
    {
        readonly T1 _value;

        public ContextPropertyValue(T1 value)
        {
            _value = value;
        }

        bool IContextPropertyValue.TryGetValue<T>(out T value)
        {
            value = _value as T;

            return value != null;
        }
    }


    public class ContextPropertyValue<T1, T2> :
        IContextPropertyValue
    {
        readonly T1 _value1;
        readonly T2 _value2;

        public ContextPropertyValue(T1 value1, T2 value2)
        {
            _value1 = value1;
            _value2 = value2;
        }

        bool IContextPropertyValue.TryGetValue<T>(out T value)
        {
            value = _value1 as T;
            if (value != null)
                return true;

            value = _value2 as T;

            return value != null;
        }
    }


    public class ContextPropertyValue<T1, T2, T3> :
        IContextPropertyValue
    {
        readonly T1 _value1;
        readonly T2 _value2;
        readonly T3 _value3;

        public ContextPropertyValue(T1 value1, T2 value2, T3 value3)
        {
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
        }

        bool IContextPropertyValue.TryGetValue<T>(out T value)
        {
            value = _value1 as T;
            if (value != null)
                return true;

            value = _value2 as T;
            if (value != null)
                return true;

            value = _value3 as T;

            return value != null;
        }
    }
}