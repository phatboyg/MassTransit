// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
#if NET40
namespace MassTransit
{
    using System;
    using System.Reflection;
    using RequestResponse.Configurators;

    public static class DynamicRequestResponseExtensions
    {
        private static readonly MethodInfo GenericPublishRequestMethod = typeof(RequestResponseExtensions).GetMethod("PublishRequest");
        public static bool PublishRequest(this IServiceBus bus,
                                            Type type,
                                            dynamic message,
                                            Action<InlineRequestConfigurator<dynamic>> configureCallback)
        {
            var publishRequestMethod = GenericPublishRequestMethod.MakeGenericMethod(type);
            return (bool)publishRequestMethod.Invoke(null, new[] { bus, message, configureCallback });
        }

        private static readonly MethodInfo GenericBeginPublishRequestMethod = typeof(RequestResponseExtensions).GetMethod("BeginPublishRequest");
        public static IAsyncResult BeginPublishRequest(this IServiceBus bus,
                                                                 Type type,
                                                                 dynamic message,
                                                                 AsyncCallback callback,
                                                                 object state,
                                                                 Action<InlineRequestConfigurator<dynamic>> configureCallback)
        {
            var publishRequestMethod = GenericBeginPublishRequestMethod.MakeGenericMethod(type);
            return (IAsyncResult)publishRequestMethod.Invoke(null, new[] { bus, message, callback, state, configureCallback });
        }

        private static readonly MethodInfo GenericSendRequestMethod = typeof(RequestResponseExtensions).GetMethod("SendRequest");
        public static bool SendRequest(this IEndpoint endpoint,
                                            Type type,
                                            dynamic message,
                                            IServiceBus bus,
                                            Action<InlineRequestConfigurator<dynamic>> configureCallback)
        {
            var publishRequestMethod = GenericSendRequestMethod.MakeGenericMethod(type);
            return (bool)publishRequestMethod.Invoke(null, new[] { endpoint, message, bus, configureCallback });
        }

        private static readonly MethodInfo GenericBeginSendRequestMethod = typeof(RequestResponseExtensions).GetMethod("BeginSendRequest");
        public static IAsyncResult BeginSendRequest(this IEndpoint endpoint,
                                                        Type type,
                                                        dynamic message,
                                                        IServiceBus bus,
                                                        AsyncCallback callback,
                                                        object state,
                                                        Action<InlineRequestConfigurator<dynamic>> configureCallback)
        {
            var publishRequestMethod = GenericBeginSendRequestMethod.MakeGenericMethod(type);
            return (IAsyncResult)publishRequestMethod.Invoke(null, new[] { endpoint, message, bus, callback, state, configureCallback });
        }
    }
}

#endif