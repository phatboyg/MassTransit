#nullable enable
namespace MassTransit.Serialization
{
    using System;
    using System.Net.Mime;
    using System.Runtime.Serialization;
    using System.Text.Encodings.Web;
    using System.Text.Json;
    using JsonConverters;


    public class SystemTextJsonMessageSerializer :
        IMessageDeserializer,
        IMessageSerializer
    {
        public static readonly ContentType JsonContentType = new ContentType("application/vnd.masstransit+json");

        public static JsonSerializerOptions Options;

        public static readonly SystemTextJsonMessageSerializer Instance = new SystemTextJsonMessageSerializer();
        readonly IObjectDeserializer _objectDeserializer;

        static SystemTextJsonMessageSerializer()
        {
            GlobalTopology.MarkMessageTypeNotConsumable(typeof(JsonElement));

            Options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReadCommentHandling = JsonCommentHandling.Skip,
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };

            Options.Converters.Add(new SystemTextJsonMessageDataConverter());
            Options.Converters.Add(new SystemTextJsonConverterFactory());
        }

        public SystemTextJsonMessageSerializer(ContentType? contentType = null)
        {
            ContentType = contentType ?? JsonContentType;

            _objectDeserializer = new SystemTextJsonObjectDeserializer(Options);
        }

        public ContentType ContentType { get; }

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
                var envelope = JsonSerializer.Deserialize<MessageEnvelope>(body.GetBytes(), Options);
                if (envelope == null)
                    throw new SerializationException("Message envelope not found");

                var messageContext = new EnvelopeMessageContext(envelope, this);

                var messageTypes = envelope.MessageType ?? Array.Empty<string>();

                var serializerContext = new SystemTextJsonSerializerContext(this, Options, ContentType, messageContext, messageTypes, envelope);

                return serializerContext;
            }
            catch (SerializationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new SerializationException("An error occured while deserializing the message envelope", ex);
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
            return new SystemTextJsonMessageBody<T>(context, Options);
        }
    }
}
