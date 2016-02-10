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
namespace MassTransit.Context
{
    using System;
    using Telemetry;
    using Telemetry.Properties;
    using Util;


    public class PayloadCache
    {
        readonly IPropertyCache _cache;

        public PayloadCache()
        {
            _cache = new PropertyCache();
        }

        public bool HasPayloadType(Type contextType)
        {
            return _cache.HasPropertyType(contextType);
        }

        public bool TryGetPayload<TPayload>(out TPayload context)
            where TPayload : class
        {
            IPropertyValue propertyValue;
            if (_cache.TryGetPropertyValue<TPayload>(out propertyValue))
            {
                if (propertyValue.TryGetValue(out context))
                    return true;
            }

            context = default(TPayload);
            return false;
        }

        public TPayload GetOrAddPayload<TPayload>(PayloadFactory<TPayload> payloadFactory)
            where TPayload : class
        {
            var factory = new Factory<TPayload>(payloadFactory);

            try
            {
                TPayload payload;
                if (_cache.GetOrAddProperty<TPayload>(factory).TryGetValue(out payload))
                    return payload;

                throw new PayloadNotFoundException("The payload was not found: " + TypeMetadataCache<TPayload>.ShortName);
            }
            catch (PayloadNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PayloadFactoryException("The payload factory faulted: " + TypeMetadataCache<TPayload>.ShortName, ex);
            }
        }


        class Factory<TPayload> :
            IContextPropertyFactory
            where TPayload : class
        {
            readonly PayloadFactory<TPayload> _payloadFactory;

            public Factory(PayloadFactory<TPayload> payloadFactory)
            {
                _payloadFactory = payloadFactory;
            }

            public IProperty CreateProperty()
            {
                var payload = _payloadFactory();

                if (payload == default(TPayload))
                    throw new PayloadNotFoundException("The payload was not found: " + TypeMetadataCache<TPayload>.ShortName);

                return new Property<TPayload>(TypeMetadataCache<TPayload>.ShortName, new PropertyValue<TPayload>(payload));
            }
        }
    }
}