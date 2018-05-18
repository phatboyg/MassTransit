// Copyright 2007-2018 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Tests.Authority
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using MassTransit.Authority;
    using MassTransit.Authority.Contracts;
    using MassTransit.Authority.Internals;
    using NUnit.Framework;
    using Subjects;
    using Testing;
    using Util;


    [TestFixture]
    public class Inserting_a_fact
    {
        [Test]
        public async Task Should_publish_an_event_after_the_fact_is_added()
        {
            var harness = new InMemoryTestHarness();

            var sinks = new ConnectionList<IFactSink>();

            InsertFactConsumer InsertFactConsumerFactory()
            {
                var clientFactory = harness.Bus.CreateClientFactory();

                return new InsertFactConsumer(sinks, clientFactory);
            }

            ConsumerTestHarness<InsertFactConsumer> insertFactHarness = harness.Consumer(InsertFactConsumerFactory, "authority-facts");

            InsertFactTypeConsumer<Customer> InsertCustomerConsumerFactory()
            {
                return new InsertFactTypeConsumer<Customer>();
            }

            ConsumerTestHarness<InsertFactTypeConsumer<Customer>> insertCustomerHarness = harness.Consumer(InsertCustomerConsumerFactory, "authority-fact-customer");

            await harness.Start();

            var destinationAddress = new Uri(harness.BaseAddress, "authority-fact-customer");
            var factType = TypeMetadataCache<Customer>.MessageTypeNames.First();

            var endpoint = await harness.Bus.GetSendEndpoint(new Uri(harness.BaseAddress, "authority-facts"));
            await endpoint.Send<ConnectFactTypeSink>(new
            {
                destinationAddress,
                factType
            });

            await Task.Delay(1000);

            try
            {
                var builder = new FactBuilder();

                FactHandle<Customer> customerHandle = builder.Create<Customer>(new
                {
                    Id = "877123",
                    Name = "Frank's Taco Stand",
                    EstablishedOn = new DateTime(2003, 7, 3, 0, 0, 0, DateTimeKind.Utc)
                });

                var clientFactory = harness.Bus.CreateClientFactory(harness.TestTimeout);

                var sessionId = NewId.NextGuid();

                var sessionClient = new AuthoritySessionClient(clientFactory, harness.BaseAddress, sessionId);

                await sessionClient.InsertFact(customerHandle);
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
}