namespace MassTransit.Authority.Contracts
{
    using System;


    /// <summary>
    /// Connect a Fact sink, for the specified FactType
    /// </summary>
    public interface ConnectFactTypeSink
    {
        string FactType { get; }

        Uri DestinationAddress { get; }
    }
}