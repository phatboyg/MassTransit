#nullable enable
namespace MassTransit.Serialization
{
    using System;
    using System.Reflection;
    using System.Text.Json;
    using Initializers;
    using Initializers.TypeConverters;
    using Metadata;


    public class SystemTextJsonObjectDeserializer :
        IObjectDeserializer
    {
        readonly JsonSerializerOptions _options;

        public SystemTextJsonObjectDeserializer(JsonSerializerOptions options)
        {
            _options = options;
        }

        public T? DeserializeObject<T>(object? value, T? defaultValue = default)
            where T : class
        {
            switch (value)
            {
                case null:
                    return defaultValue;
                case T returnValue:
                    return returnValue;
                case string text:
                    if (TypeConverterCache.TryGetTypeConverter(out ITypeConverter<T, string>? typeConverter) && typeConverter.TryConvert(text, out var result))
                        return result;
                    return GetObject<T>(JsonSerializer.Deserialize<JsonElement>(text));
                case JsonElement jsonElement:
                    return GetObject<T>(jsonElement);
            }

            var element = JsonSerializer.SerializeToElement(value, _options);

            return element.ValueKind == JsonValueKind.Null
                ? defaultValue
                : GetObject<T>(element);
        }

        public T? DeserializeObject<T>(object? value, T? defaultValue = null)
            where T : struct
        {
            switch (value)
            {
                case null:
                    return defaultValue;
                case T returnValue:
                    return returnValue;
                case string text:
                    if (TypeConverterCache.TryGetTypeConverter(out ITypeConverter<T, string>? typeConverter) && typeConverter.TryConvert(text, out var result))
                        return result;
                    return JsonSerializer.Deserialize<T>(text, _options);
                case JsonElement jsonElement:
                    return jsonElement.Deserialize<T>(_options);
            }

            var element = JsonSerializer.SerializeToElement(value, _options);

            return element.ValueKind == JsonValueKind.Null
                ? defaultValue
                : element.Deserialize<T>(_options);
        }

        public MessageBody SerializeObject(object? value)
        {
            return new BytesMessageBody(JsonSerializer.SerializeToUtf8Bytes(value, _options));
        }

        T? GetObject<T>(JsonElement jsonElement)
            where T : class
        {
            if (typeof(T).GetTypeInfo().IsInterface && MessageTypeCache<T>.IsValidMessageType)
            {
                var messageType = TypeMetadataCache<T>.ImplementationType;

                if (jsonElement.Deserialize(messageType, _options) is T obj)
                    return obj;
            }

            return jsonElement.Deserialize<T>(_options);
        }
    }
}
