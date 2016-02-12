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
namespace MassTransit.Telemetry.Context.Properties
{
    using System;
    using System.Linq;


    public class ArrayPropertyCollection :
        BasePropertyCollection
    {
        readonly IReadOnlyPropertyCollection _parent;
        readonly IProperty[] _properties;

        public ArrayPropertyCollection(IReadOnlyPropertyCollection parent, params IProperty[] properties)
            : base(parent)
        {
            _properties = properties;
            _parent = parent;
        }

        ArrayPropertyCollection(IReadOnlyPropertyCollection parent, IProperty property, IProperty[] properties)
            : base(parent)
        {
            _parent = parent;

            _properties = new IProperty[properties.Length + 1];
            _properties[0] = property;
            Array.Copy(properties, 0, _properties, 1, properties.Length);
        }

        public override bool HasPropertyType(Type propertyType)
        {
            if (_properties.Any(x => propertyType.IsAssignableFrom(x.ValueType)))
                return true;

            return base.HasPropertyType(propertyType);
        }

        public override bool HasProperty(string name)
        {
            if (_properties.Any(x => MatchesPropertyName(x.Name, name)))
                return true;

            return base.HasProperty(name);
        }

        public override bool HasProperty<T>(string name)
        {
            T propertyValue;
            if (_properties.Any(x => MatchesPropertyName(x.Name, name) && x.TryGetValue(out propertyValue)))
                return true;

            return base.HasProperty<T>(name);
        }

        public override bool TryGetPropertyValue<T>(out IPropertyValue value)
        {
            T propertyValue;
            if ((value = _properties.FirstOrDefault(x => x.TryGetValue(out propertyValue))) != null)
            {
                return true;
            }

            return base.TryGetPropertyValue<T>(out value);
        }

        public override bool TryGetPropertyValue<T>(string name, out IPropertyValue value)
        {
            T propertyValue;
            if ((value = _properties.FirstOrDefault(x => MatchesPropertyName(x.Name, name) && x.TryGetValue(out propertyValue))) != null)
            {
                return true;
            }

            return base.TryGetPropertyValue<T>(out value);
        }

        public override IPropertyCollection Add(IProperty property)
        {
            return new ArrayPropertyCollection(_parent, property, _properties);
        }
    }
}