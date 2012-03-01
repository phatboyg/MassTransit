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
namespace MassTransit.Pipeline.Configuration
{
    using System;
    using Exceptions;
    using Sinks;
    using Util;

	/// <summary>
	/// A configurator that takes in the constructor, the outbound pipeline (bus.OutboundPipeline),
	/// and then allows consumers to call Create with their own message interceptor; this interceptor
	/// is then inserted after the last outbound message sink.
	/// </summary>
    public class OutboundMessageInterceptorConfigurator
    {
        readonly IPipelineSink<ISendContext> _sink;

		/// <summary>
		/// Create a new configurator based on a pipeline sink that takes the send context,
		/// i.e. the outbound pipeline.
		/// </summary>
		/// <param name="sink">The outbound pipeline/pipeline sinks taking send contexts.</param>
        public OutboundMessageInterceptorConfigurator([NotNull] IPipelineSink<ISendContext> sink)
		{
			if (sink == null) throw new ArgumentNullException("sink");
			_sink = sink;
		}

		/// <summary>
		/// Create the new interceptor as well as thread it into the pipeline. The pipeline insertion is done with 
		/// the <see cref="OutboundMessageInterceptor"/>.
		/// </summary>
		public OutboundMessageInterceptor Create([NotNull] IOutboundMessageInterceptor messageInterceptor)
        {
			if (messageInterceptor == null) throw new ArgumentNullException("messageInterceptor");

			var scope = new OutboundMessageInterceptorConfiguratorScope();
            _sink.Inspect(scope);

            return ConfigureInterceptor(scope.InsertAfter, messageInterceptor);
        }

        static OutboundMessageInterceptor ConfigureInterceptor(
            Func<IPipelineSink<ISendContext>, IPipelineSink<ISendContext>> insertAfter,
            IOutboundMessageInterceptor messageInterceptor)
        {
            if (insertAfter == null)
                throw new PipelineException("Unable to insert filter into pipeline for message type " +
                                            typeof (object).FullName);

            var interceptor = new OutboundMessageInterceptor(insertAfter, messageInterceptor);

            return interceptor;
        }
    }
}