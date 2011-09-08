﻿// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit
{
    using System;
    using Magnum.Reflection;

    public static class PublishExtensions
    {
        public static void Publish<T>(this IServiceBus bus, T message)
            where T : class
        {
            bus.Publish(message, x => { });
        }

        public static void Publish<T>(this IServiceBus bus, object values)
            where T : class
        {
            var message = InterfaceImplementationExtensions.InitializeProxy<T>(values);

            bus.Publish(message, x => { });
        }

        public static void Publish<T>(this IServiceBus bus, object values, Action<IPublishContext<T>> contextCallback)
            where T : class
        {
            var message = InterfaceImplementationExtensions.InitializeProxy<T>(values);

            bus.Publish(message, contextCallback);
        }
    }
}