// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Transports.Stomp.Tests
{
    using System;
    using BusConfigurators;
    using Magnum.Extensions;
    using Magnum.TestFramework;
    using TestFramework;

    [Scenario]
    public class When_a_message_is_published :
        Given_a_stomp_bus
    {
        private Future<A> _received;

        protected override void ConfigureServiceBus(Uri uri, ServiceBusConfigurator configurator)
        {
            base.ConfigureServiceBus(uri, configurator);

            _received = new Future<A>();

            configurator.Subscribe(s => s.Handler<A>(message => _received.Complete(message)));
        }

        [When]
        public void A_message_is_published()
        {
            LocalBus.Publish(new A
                                 {
                                     StringA = "ValueA",
                                 });
        }

        [Then]
        public void Should_be_received_by_the_queue()
        {
            _received.WaitUntilCompleted(8.Seconds()).ShouldBeTrue();
            _received.Value.StringA.ShouldEqual("ValueA");
        }

        private class A 
        {
            public string StringA { get; set; }
        }

    }
}