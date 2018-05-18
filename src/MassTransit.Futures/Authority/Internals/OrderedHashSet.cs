// Copyright 2012-2018 Chris Patterson
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
namespace MassTransit.Authority.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;


    public class OrderedHashSet<T>
    {
        readonly IDictionary<T, LinkedListNode<T>> _nodes;
        readonly LinkedList<T> _values;

        public OrderedHashSet()
            : this(EqualityComparer<T>.Default)
        {
        }

        public OrderedHashSet(IEqualityComparer<T> comparer)
        {
            _nodes = new Dictionary<T, LinkedListNode<T>>(comparer);
            _values = new LinkedList<T>();
        }

        public int Count => _nodes.Count;

        public void Clear()
        {
            _values.Clear();
            _nodes.Clear();
        }

        public bool Remove(T item)
        {
            var found = _nodes.TryGetValue(item, out var node);
            if (!found)
                return false;

            _nodes.Remove(item);
            _values.Remove(node);

            return true;
        }

        public bool Contains(T item)
        {
            return _nodes.ContainsKey(item);
        }

        public Task WhenAll(Func<T, Task> callback)
        {
            var valueArray = new T[_nodes.Count];
            _values.CopyTo(valueArray, 0);

            var taskArray = new Task[valueArray.Length];
            for (int i = 0; i < valueArray.Length; i++)
                taskArray[i] = callback(valueArray[i]);

            return Task.WhenAll(taskArray);
        }

        public (bool found, T value) Find(T item)
        {
            if (_nodes.TryGetValue(item, out var node))
                return (true, node.Value);

            return (false, default);
        }

        public bool Add(T item)
        {
            if (_nodes.ContainsKey(item))
                return false;

            var node = _values.AddLast(item);

            _nodes.Add(item, node);

            return true;
        }
    }
}