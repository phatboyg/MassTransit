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
    using System.Collections.Concurrent;
    using System.Collections.Generic;





    /// <summary>
    /// Stores telemetry data
    /// </summary>
    public class TelemetryData :
        ITelemetryData
    {
        readonly object _lock = new object();
         List<ITelemetryDataItem> _items;
         Dictionary<string, int> _keys; 

        public TelemetryData()
        {
        }

        void ITelemetryData.Add<T>(string key, T data)
        {
            GetOrAdd(key, () => new ObjectTelemetryDataItem<T>(data));
        }

        void ITelemetryData.Set<T>(string key, T data)
        {
            GetOrSet(key, () => new ObjectTelemetryDataItem<T>(data));
        }

        void GetOrAdd(string key, Func<ITelemetryDataItem> itemFactory)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            lock (_lock)
            {
                if (_items == null)
                {
                    _items = new List<ITelemetryDataItem>();
                    _keys = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                    _items.Add(itemFactory());
                    _keys[key] = 0;
                }
                else
                {
                    int index;
                    if (_keys.TryGetValue(key, out index))
                    {
                        var dataItem = _items[index] as MultipleTelemetryDataItem;

                        var item = itemFactory();
                        if (dataItem != null)
                            dataItem.Add(item);
                        else
                            _items[index] = new MultipleTelemetryDataItem(_items[index], item);
                    }
                    else
                    {
                        _keys[key] = _items.Count;

                        _items.Add(itemFactory());
                    }
                }
            }
        }

        void GetOrSet(string key, Func<ITelemetryDataItem> itemFactory)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            lock (_lock)
            {
                if (_items == null)
                {
                    _items = new List<ITelemetryDataItem>();
                    _keys = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                    _items.Add(itemFactory());
                    _keys[key] = 0;
                }
                else
                {
                    int index;
                    if (_keys.TryGetValue(key, out index))
                    {
                        _items[index] = itemFactory();
                    }
                    else
                    {
                        _keys[key] = _items.Count;

                        _items.Add(itemFactory());
                    }
                }
            }
        }
    }
}