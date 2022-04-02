#nullable enable
namespace MassTransit.Serialization
{
    using System;
    using System.Net.Mime;
    using System.Runtime.Serialization;
    using System.Text.Json;


    public class SystemTextJsonRawMessageSerializer :
        RawMessageSerializer,
        IMessageDeserializer,
        IMessageSerializer
    {
        public static readonly ContentType JsonContentType = new ContentType("application/json");
        readonly IObjectDeserializer _objectDeserializer;
        readonly JsonSerializerOptions _options;

        readonly RawSerializerOptions _rawOptions;

        public SystemTextJsonRawMessageSerializer(RawSerializerOptions rawOptions = RawSerializerOptions.Default)
        {
            _rawOptions = rawOptions;

            _options = SystemTextJsonMessageSerializer.Options;
            _objectDeserializer = new SystemTextJsonObjectDeserializer(_options);
        }

        public SystemTextJsonRawMessageSerializer(JsonSerializerOptions options, RawSerializerOptions rawOptions = RawSerializerOptions.Default)
        {
            _options = options;
            _rawOptions = rawOptions;

            _objectDeserializer = new SystemTextJsonObjectDeserializer(_options);
        }

        public ContentType ContentType => JsonContentType;

        public void Probe(ProbeContext context)
        {
            var scope = context.CreateScope("json");
            scope.Add("contentType", ContentType.MediaType);
            scope.Add("provider", "System.Text.Json");
        }

        public ConsumeContext Deserialize(ReceiveContext receiveContext)
        {
            return new BodyConsumeContext(receiveContext, Deserialize(receiveContext.Body, receiveContext.TransportHeaders, receiveContext.InputAddress));
        }

        public SerializerContext Deserialize(MessageBody body, Headers headers, Uri? destinationAddress = null)
        {
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(body.GetBytes(), _options);

                var messageTypes = headers.GetMessageTypes();

                var messageContext = new RawMessageContext(headers, destinationAddress, _rawOptions);

                var serializerContext = new SystemTextJsonRawSerializerContext(_objectDeserializer, _options, ContentType, messageContext,
                    messageTypes, _rawOptions, jsonElement);

                return serializerContext;
            }
            catch (SerializationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SerializationException("An error occured while deserializing the message enveloper", ex);
            }
        }

        public MessageBody GetMessageBody(string text)
        {
            return new StringMessageBody(text);
        }

        public T? DeserializeObject<T>(object? value, T? defaultValue = default)
            where T : class
        {
            return _objectDeserializer.DeserializeObject(value, defaultValue);
        }

        public T? DeserializeObject<T>(object? value, T? defaultValue = null)
            where T : struct
        {
            return _objectDeserializer.DeserializeObject(value, defaultValue);
        }

        public MessageBody SerializeObject(object? value)
        {
            return _objectDeserializer.SerializeObject(value);
        }

        public MessageBody GetMessageBody<T>(SendContext<T> context)
            where T : class
        {
            if (_rawOptions.HasFlag(RawSerializerOptions.AddTransportHeaders))
                SetRawMessageHeaders<T>(context);

            return new SystemTextJsonRawMessageBody<T>(context, _options);
        }
    }
}
