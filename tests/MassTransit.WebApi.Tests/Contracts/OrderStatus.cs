namespace MassTransit.WebApi.Tests.Contracts
{
    using System;


    public class OrderStatus
    {
        public Guid OrderId { get; set; }

        public string? Status { get; set; }
    }
}
