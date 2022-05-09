namespace MassTransit.WebApi.Tests.Contracts
{
    using System;


    public class CancelOrder
    {
        public Guid OrderId { get; set; }
    }
}
