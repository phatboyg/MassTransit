namespace MassTransit.Authority.Contracts
{
    using System;


    /// <summary>
    /// Published when a fact is inserted
    /// </summary>
    public interface FactInserted
    {
        /// <summary>
        /// The Session to which the fact should be added
        /// </summary>
        Guid SessionId { get; }

        /// <summary>
        /// Uniquely identifies the fact (in this session)
        /// </summary>
        Guid FactId { get; }

        /// <summary>
        /// The types supported by the fact
        /// </summary>
        string[] FactTypes { get; }
    }


    public interface TupleInserted
    {
        /// <summary>
        /// The Session to which the fact should be added
        /// </summary>
        Guid SessionId { get; }

        Guid TupleId { get; }
    }
}