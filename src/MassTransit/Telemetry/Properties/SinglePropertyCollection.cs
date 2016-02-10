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


    public class SinglePropertyCollection :
        BasePropertyCollection
    {
        readonly IReadOnlyPropertyCollection _parent;
        readonly IProperty _property;

        public SinglePropertyCollection(IProperty property, IReadOnlyPropertyCollection parent = null)
            : base(parent)
        {
            _property = property;
            _parent = parent;
        }

        public override bool HasPropertyType(Type propertyType)
        {
            if (propertyType.IsAssignableFrom(_property.ValueType))
                return true;

            return base.HasPropertyType(propertyType);
        }

        public override bool HasProperty(string name)
        {
            if (MatchesPropertyName(_property.Name, name))
                return true;

            return base.HasProperty(name);
        }

        public override bool HasProperty<T>(string name)
        {
            T propertyValue;
            if (MatchesPropertyName(_property.Name, name) && _property.TryGetValue(out propertyValue))
                return true;

            return base.HasProperty<T>(name);
        }

        public override bool TryGetPropertyValue<T>(out IPropertyValue value)
        {
            T propertyValue;
            if (_property.TryGetValue(out propertyValue))
            {
                value = _property;
                return true;
            }

            return base.TryGetPropertyValue<T>(out value);
        }

        public override bool TryGetPropertyValue<T>(string name, out IPropertyValue value)
        {
            T propertyValue;
            if (MatchesPropertyName(_property.Name, name) && _property.TryGetValue(out propertyValue))
            {
                value = _property;
                return true;
            }

            return base.TryGetPropertyValue<T>(out value);
        }

        public override IPropertyCollection Add(IProperty property)
        {
            return new ArrayPropertyCollection(_parent, property, _property);
        }
    }
}