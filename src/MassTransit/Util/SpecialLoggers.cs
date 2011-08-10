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
namespace MassTransit.Util
{
	using log4net;

	public class SpecialLoggers
	{
		static readonly ILog _diagnostics = LogManager.GetLogger("MassTransit.Diagnostics");
		static readonly ILog _ironLogger = LogManager.GetLogger("MassTransit.Iron");
		static readonly ILog _messages = LogManager.GetLogger("MassTransit.Messages");

		public static ILog Messages
		{
			get { return _messages; }
		}

		public static ILog Diagnostics
		{
			get { return _diagnostics; }
		}

		public static ILog Iron
		{
			get { return _ironLogger; }
		}
	}
}