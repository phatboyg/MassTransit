namespace MassTransit.KafkaIntegration.Registration
{
    using System;
    using Confluent.Kafka;
    using Transport;


    public interface IKafkaProducerRegistrationConfigurator<TKey, TValue>
        where TValue : class
    {
        /// <summary>Set the serializer to use to serialize keys.</summary>
        /// <remarks>
        /// If your key serializer throws an exception, this will be
        /// wrapped in a ConsumeException with ErrorCode
        /// Local_KeyDeserialization and thrown by the initiating call to
        /// Consume.
        /// </remarks>
        IKafkaProducerRegistrationConfigurator<TKey, TValue> SetKeySerializer(ISerializer<TKey> serializer);

        /// <summary>
        /// Set the serializer to use to serialize values.
        /// </summary>
        /// <remarks>
        /// If your value serializer throws an exception, this will be
        /// wrapped in a ConsumeException with ErrorCode
        /// Local_ValueDeserialization and thrown by the initiating call to
        /// Consume.
        /// </remarks>
        IKafkaProducerRegistrationConfigurator<TKey, TValue> SetValueSerializer(ISerializer<TValue> serializer);

        IKafkaProducerRegistrationConfigurator<TKey, TValue> Configure(Action<IKafkaProducerConfigurator> configure);

        /// <summary>
        /// Connect a publish observer to the bus and produce a message for each published message matching the
        /// producer message type.
        /// </summary>
        /// <returns></returns>
        IKafkaProducerRegistrationConfigurator<TKey, TValue> RoutePublishedMessages(KafkaKeyResolver<TKey, TValue> resolver);
    }
}
