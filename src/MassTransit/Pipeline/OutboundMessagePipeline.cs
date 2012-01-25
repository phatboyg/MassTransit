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
namespace MassTransit.Pipeline
{
    using System;
    using System.Collections.Generic;
    using Magnum.Concurrency;

    public class OutboundMessagePipeline :
        IOutboundMessagePipeline
    {
        readonly Atomic<IPipelineSink<ISendContext>> _output;

        public OutboundMessagePipeline(IPipelineSink<ISendContext> output)
        {
            _output = Atomic.Create(output);
        }

        public IEnumerable<Action<ISendContext>> Enumerate(ISendContext context)
        {
            return _output.Value.Enumerate(context);
        }

        public bool Inspect(IPipelineInspector inspector)
        {
            return inspector.Inspect(this, () => _output.Value.Inspect(inspector));
        }

        public IPipelineSink<ISendContext> ReplaceOutputSink(IPipelineSink<ISendContext> sink)
        {
            return _output.Set(output => sink);
        }
    }
}