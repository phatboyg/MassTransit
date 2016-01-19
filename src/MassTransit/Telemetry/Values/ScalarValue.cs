// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.Telemetry.Values
{
    using System;
    using System.Globalization;
    using System.IO;


    public class ScalarValue :
        TelemetryLogEventPropertyValue
    {
        public ScalarValue(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public override void Render(TextWriter output, string format = null, IFormatProvider formatProvider = null)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            if (Value == null)
            {
                output.Write("null");
            }
            else
            {
                var s = Value as string;
                if (s != null)
                {
                    if (format != "l")
                    {
                        output.Write("\"");
                        output.Write(s.Replace("\"", "\\\""));
                        output.Write("\"");
                    }
                    else
                    {
                        output.Write(s);
                    }
                }
                else
                {
                    var value = Value as IFormattable;

                    output.Write(value?.ToString(format, formatProvider ?? CultureInfo.InvariantCulture) ?? Value.ToString());
                }
            }
        }
    }
}