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
namespace MassTransit.Tests.Authority
{
    using System;
    using MassTransit.Authority;
    using NUnit.Framework;
    using Subjects;


    [TestFixture]
    public class Adding_a_fact_to_the_session
    {
        [Test]
        public void Should_allow_the_builder_to_add()
        {
            FactBuilder builder = new FactBuilder();

            var customerHandle = builder.Create<Customer>(new
            {
                Id = "877123",
                Name = "Frank's Taco Stand",
                EstablishedOn = new DateTime(2003, 7, 3, 0, 0, 0, DateTimeKind.Utc)
            });
        }
    }
}