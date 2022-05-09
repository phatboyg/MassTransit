namespace MassTransit.WebApi.Tests.Contracts
{
    using System;


    public class OrderSubmitted
    {
        public Guid OrderId { get; set; }
    }
}
