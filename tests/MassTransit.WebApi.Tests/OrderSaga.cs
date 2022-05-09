namespace MassTransit.WebApi.Tests
{
    using System;


    public class OrderSaga :
        SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
    }
}
