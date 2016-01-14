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
    using System.Linq;


    public class MessageTemplate
    {
        readonly MessageTemplateToken[] _tokens;

        public MessageTemplate(IEnumerable<MessageTemplateToken> tokens)
            : this(string.Join("", tokens), tokens)
        {
        }

        public MessageTemplate(string text, IEnumerable<MessageTemplateToken> tokens)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (tokens == null)
                throw new ArgumentNullException(nameof(tokens));

            Text = text;

            _tokens = tokens.ToArray();

            var propertyTokens = _tokens.OfType<PropertyToken>().ToArray();
            if (propertyTokens.Length != 0)
            {
                var allPositional = true;
                var anyPositional = false;
                foreach (var propertyToken in propertyTokens)
                {
                    if (propertyToken.IsPositional)
                        anyPositional = true;
                    else
                        allPositional = false;
                }

                if (allPositional)
                {
                    PositionalProperties = propertyTokens;
                }
                else
                {
                    if (anyPositional)
                        TelemetryContext.CurrentOrDefault.Warning("Message template is malformed: {0}", text);

                    NamedProperties = propertyTokens;
                }
            }
        }

        public string Text { get; }

        public IEnumerable<MessageTemplateToken> Tokens => _tokens;

        internal PropertyToken[] NamedProperties { get; }

        internal PropertyToken[] PositionalProperties { get; }

        public override string ToString()
        {
            return Text;
        }

        public string Render(IReadOnlyDictionary<string,TelemetryLogEventPropertyValue> properties, IFormatProvider formatProvider = null)
        {
            var writer = new StringWriter(formatProvider);
            Render(properties, writer, formatProvider);
            return writer.ToString();
        }

        /// <summary>
        /// Convert the message template into a textual message, given the
        /// properties matching the tokens in the message template.
        /// </summary>
        /// <param name="properties">Properties matching template tokens.</param>
        /// <param name="output">The message created from the template and properties. If the
        /// properties are mismatched with the template, the template will be
        /// returned with incomplete substitution.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        public void Render(IReadOnlyDictionary<string, TelemetryLogEventPropertyValue> properties, TextWriter output, IFormatProvider formatProvider = null)
        {
            foreach (var token in _tokens)
            {
                token.Render(properties, output, formatProvider);
            }
        }
    }
}