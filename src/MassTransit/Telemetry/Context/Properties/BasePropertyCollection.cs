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


    public abstract class BasePropertyCollection :
        IPropertyCollection
    {
        readonly IReadOnlyPropertyCollection _parent;

        protected BasePropertyCollection(IReadOnlyPropertyCollection parent)
        {
            _parent = parent;
        }

        protected IReadOnlyPropertyCollection Parent => _parent;

        public virtual bool HasPropertyType(Type propertyType)
        {
            return _parent?.HasPropertyType(propertyType) ?? false;
        }

        public virtual bool HasProperty(string name)
        {
            return _parent?.HasProperty(name) ?? false;
        }

        public virtual bool HasProperty<T>(string name) where T : class
        {
            return _parent?.HasProperty<T>(name) ?? false;
        }

        public virtual bool TryGetPropertyValue<T>(out IPropertyValue value) where T : class
        {
            if (_parent != null)
                return _parent.TryGetPropertyValue<T>(out value);

            value = null;
            return false;
        }

        public virtual bool TryGetPropertyValue<T>(string name, out IPropertyValue value) where T : class
        {
            if (_parent != null)
                return _parent.TryGetPropertyValue<T>(name, out value);

            value = null;
            return false;
        }

        public abstract IPropertyCollection Add(IProperty property);

        protected bool MatchesPropertyName(string propertyName, string name)
        {
            return propertyName.Equals(name, StringComparison.OrdinalIgnoreCase);
        }
    }
}