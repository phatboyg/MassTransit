namespace MassTransit.Telemetry.Context.Properties
{
    using System;


    /// <summary>
    /// Stores a single scope data value
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class PropertyValue<TValue> :
        IPropertyValue<TValue>
        where TValue : class
    {
        readonly TValue _value;

        public PropertyValue(TValue value)
        {
            _value = value;
        }

        Type IPropertyValue.ValueType => typeof(TValue);
        TValue IPropertyValue<TValue>.Value => _value;

        bool IPropertyValue.TryGetValue<T>(out T value)
        {
            value = _value as T;

            return value != null;
        }
    }
}