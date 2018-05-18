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
    using System.Linq;
    using System.Threading.Tasks;
    using GreenPipes;
    using Util;


    /// <summary>
    /// Maintains an ordered list of connections
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConnectionList<T> :
        IConnectionList<T>
        where T : class
    {
        readonly LinkedList<T> _connected;
        readonly Dictionary<T, LinkedListNode<T>> _connections;

        public ConnectionList()
        {
            _connections = new Dictionary<T, LinkedListNode<T>>();
            _connected = new LinkedList<T>();
        }

        /// <summary>
        /// The number of connections
        /// </summary>
        public int Count
        {
            get
            {
                lock (_connections)
                {
                    return _connections.Count;
                }
            }
        }

        /// <summary>
        /// Connect a connectable type
        /// </summary>
        /// <param name="connection">The connection to add</param>
        /// <returns>The connection handle</returns>
        public ConnectHandle Connect(T connection)
        {
            if (connection == null)
                throw new ArgumentNullException(nameof(connection));

            LinkedListNode<T> node;
            lock (_connections)
            {
                if (_connections.ContainsKey(connection))
                    throw new ArgumentException("The connection is already connected", nameof(connection));

                node = _connected.AddLast(connection);

                _connections.Add(connection, node);
            }

            return new Handle(node, Disconnect);
        }

        /// <summary>
        /// Enumerate the connections invoking the callback for each connection
        /// </summary>
        /// <param name="callback">The callback</param>
        /// <returns>An awaitable Task for the operation</returns>
        public Task All(Func<T, Task> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            T[] connected;
            lock (_connections)
            {
                if (_connections.Count == 0)
                    return TaskUtil.Completed;

                connected = _connected.ToArray();
            }

            if (connected.Length == 1)
                return callback(connected[0]);

            return Task.WhenAll(connected.Select(callback));
        }

        public bool All(Func<T, bool> callback)
        {
            T[] connected;
            lock (_connections)
            {
                if (_connections.Count == 0)
                    return true;

                connected = _connected.ToArray();
            }

            if (connected.Length == 1)
                return callback(connected[0]);

            return connected.All(callback);
        }

        void Disconnect(LinkedListNode<T> connection)
        {
            lock (_connections)
            {
                _connections.Remove(connection.Value);
                _connected.Remove(connection);
            }
        }


        delegate void OnDisconnect(LinkedListNode<T> connection);


        class Handle :
            ConnectHandle
        {
            readonly LinkedListNode<T> _connection;
            readonly OnDisconnect _disconnect;

            public Handle(LinkedListNode<T> connection, OnDisconnect disconnect)
            {
                _connection = connection;
                _disconnect = disconnect;
            }

            public T Connection => _connection.Value;

            public void Disconnect()
            {
                _disconnect(_connection);
            }

            public void Dispose()
            {
                Disconnect();
            }
        }
    }
}