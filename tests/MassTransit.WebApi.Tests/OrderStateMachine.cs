namespace MassTransit.WebApi.Tests
{
    using Contracts;


    public class OrderStateMachine :
        MassTransitStateMachine<OrderSaga>
    {
        public OrderStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => Approve, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => Cancel, x => x.CorrelateById(m => m.Message.OrderId));

            Initially(
                When(OrderSubmitted)
                    .TransitionTo(Submitted)
            );

            During(Submitted,
                When(Cancel)
                    .TransitionTo(Canceled)
            );
        }

        //
        // ReSharper disable UnassignedGetOnlyAutoProperty
        public State Submitted { get; }
        public State Canceled { get; }

        public Event<OrderSubmitted> OrderSubmitted { get; }
        public Event<ApproveOrder> Approve { get; }
        public Event<CancelOrder> Cancel { get; }
    }
}
