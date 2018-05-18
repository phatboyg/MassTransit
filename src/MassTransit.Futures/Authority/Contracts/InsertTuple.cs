namespace MassTransit.Authority.Contracts
{
    using System;


    /// <summary>
    /// Insert a tuple into the node
    /// </summary>
    public interface InsertTuple
    {
        /// <summary>
        /// The sessionId for this tuple
        /// </summary>
        Guid SessionId { get; }

        Guid TupleId { get; }

        /// <summary>
        /// The tuple, which includes the right-activated fact at the front
        /// </summary>
        Token Right { get; }
    }


    /// <summary>
    /// When a fact is activated, it is different than being inserted, it's expected that the fact
    /// will be combined with the existing tuples and inserted as tuples into the destination
    /// </summary>
    public interface ActivateFact
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

        /// <summary>
        /// The fact content itself (serialized, obviously to be read out by type)
        /// </summary>
        object Fact { get; }
    }


    public interface ActivateTuple
    {
        /// <summary>
        /// The session in which the tuple was activated
        /// </summary>
        Guid SessionId { get; }

        /// <summary>
        /// Identifies the tuple, which remains the same regardless of how many facts are added
        /// </summary>
        Guid TupleId { get; }

        /// <summary>
        /// The tuple being activated
        /// </summary>
        Token Right { get; }
    }
}