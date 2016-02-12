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
    using System.Threading;
    using Context.Properties;


    public class PropertyCache :
        IPropertyCache
    {
        IPropertyCollection _collection;

        public PropertyCache()
        {
            _collection = new EmptyPropertyCollection();
        }

        PropertyCache(IPropertyCollection collection)
        {
            _collection = new EmptyPropertyCollection(collection);
        }

        bool IReadOnlyPropertyCollection.HasPropertyType(Type propertyType)
        {
            return _collection.HasPropertyType(propertyType);
        }

        bool IReadOnlyPropertyCollection.HasProperty(string name)
        {
            return _collection.HasProperty(name);
        }

        bool IReadOnlyPropertyCollection.HasProperty<T>(string name)
        {
            return _collection.HasProperty<T>(name);
        }

        bool IReadOnlyPropertyCollection.TryGetPropertyValue<T>(out IPropertyValue value)
        {
            return _collection.TryGetPropertyValue<T>(out value);
        }

        bool IReadOnlyPropertyCollection.TryGetPropertyValue<T>(string name, out IPropertyValue value)
        {
            return _collection.TryGetPropertyValue<T>(name, out value);
        }

        IPropertyValue IPropertyCache.GetOrAddProperty<T>(IContextPropertyFactory propertyFactory)
        {
            IProperty property = null;

            IPropertyCollection currentCollection;
            do
            {
                IPropertyValue existingValue;
                if (_collection.TryGetPropertyValue<T>(out existingValue))
                    return existingValue;

                var contextProperty = property ?? (property = propertyFactory.CreateProperty());

                currentCollection = Volatile.Read(ref _collection);

                Interlocked.CompareExchange(ref _collection, currentCollection.Add(contextProperty), currentCollection);
            }
            while (currentCollection == Volatile.Read(ref _collection));

            return property;
        }

        IPropertyValue IPropertyCache.GetOrAddProperty<T>(string name, IContextPropertyFactory propertyFactory)
        {
            IProperty property = null;

            IPropertyCollection currentCollection;
            do
            {
                IPropertyValue existingValue;
                if (_collection.TryGetPropertyValue<T>(name, out existingValue))
                    return existingValue;

                var contextProperty = property ?? (property = propertyFactory.CreateProperty());

                currentCollection = Volatile.Read(ref _collection);

                Interlocked.CompareExchange(ref _collection, currentCollection.Add(contextProperty), currentCollection);
            }
            while (currentCollection == Volatile.Read(ref _collection));

            return property;
        }

        public IPropertyCache CreateChildCache()
        {
            return new PropertyCache(_collection);
        }
    }
}