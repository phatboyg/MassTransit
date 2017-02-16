// Copyright 2007-2017 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Tests
{
    namespace Splitting
    {
        using System;
        using System.Threading.Tasks;
        using Incremental;
        using NUnit.Framework;
        using TestFramework;


        [TestFixture]
        public class Using_an_incremental_consumer :
            InMemoryTestFixture
        {
            [Test]
            public async Task Should_run_until_completed()
            {
            }

            protected override void ConfigureInMemoryReceiveEndpoint(IInMemoryReceiveEndpointConfigurator configurator)
            {
                configurator.IncrementalConsumer<ProcessFileConsumer>();
            }
        }


        public static class IncrementalConsumerExtensions
        {
            public static void IncrementalConsumer<T>(this IReceiveEndpointConfigurator configurator)
                where T : class, IIncrementalConsumer, new()
            {


            }
        }


        public interface ProcessFile
        {
            Guid FileId { get; }
            Uri FileLocation { get; }
        }


        public interface FileProcessed
        {
            Guid FileId { get; }
            Uri FileLocation { get; }
            long BytesProcessed { get; }
        }


        public interface ProcessFileContext
        {
        }


        class ProcessFileConsumer :
            IIncrementalConsumer<ProcessFileContext, ProcessFile>
        {
            public async Task<IncrementalResult<ProcessFileContext>> Initial(InitialContext<ProcessFileContext, ProcessFile> context)
            {
                await Task.Delay(10, context.CancellationToken).ConfigureAwait(false);

                return context.Start(new Context());
            }

            public async Task<IncrementalResult<ProcessFileContext>> Increment(IncrementContext<ProcessFileContext, ProcessFile> context)
            {
                await Task.Delay(10, context.CancellationToken).ConfigureAwait(false);

                if (context.Index == 0)
                    return context.Next(100);

                return context.Last();
            }

            public Task Final(FinalContext<ProcessFileContext, ProcessFile> context)
            {
                return context.Publish<FileProcessed>(new
                {
                    context.Message.FileId,
                    context.Message.FileLocation,
                    BytesProcessed = context.Offset
                });
            }


            class Context :
                ProcessFileContext
            {
            }
        }
    }
}