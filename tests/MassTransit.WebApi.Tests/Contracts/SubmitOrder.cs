namespace MassTransit.WebApi.Tests.Contracts
{
    using System;


    public class SubmitOrder
    {
        public Guid OrderId { get; set; }
        public string? CustomerNumber { get; set; }
    }
}
