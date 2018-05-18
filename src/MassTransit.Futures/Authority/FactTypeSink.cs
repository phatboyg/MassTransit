namespace MassTransit.Authority
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    public class FactTypeSink :
        IFactSink
    {
        readonly Uri _destinationAddress;
        readonly string _factType;

        public FactTypeSink(string factType, Uri destinationAddress)
        {
            _factType = factType;
            _destinationAddress = destinationAddress;
        }

        public (bool, Uri) Matches(IEnumerable<string> factTypes)
        {
            return (factTypes.Any(x => _factType.Equals(x, StringComparison.OrdinalIgnoreCase)), _destinationAddress);
        }
    }
}