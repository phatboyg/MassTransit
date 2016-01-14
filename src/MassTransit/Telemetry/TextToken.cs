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
namespace MassTransit.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.IO;


    public class TextToken :
        MessageTemplateToken
    {
        public TextToken(string text, int startIndex = -1)
            : base(startIndex)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            Text = text;
        }

        public override int Length => Text.Length;

        public string Text { get; }

        public override void Render(IReadOnlyDictionary<string, TelemetryLogEventPropertyValue> properties, TextWriter output,
            IFormatProvider formatProvider = null)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));

            output.Write(Text);
        }

        public override bool Equals(object obj)
        {
            var textToken = obj as TextToken;
            return textToken != null && textToken.Text == Text;
        }

        public override int GetHashCode() => Text.GetHashCode();

        public override string ToString() => Text;
    }
}