﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Sample.Contracts;

namespace Sample.Components.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderStatusRequest, x =>
            {
                x.CorrelateById(m => m.Message.OrderId);
                x.OnMissingInstance(m => m.ExecuteAsync(async context =>
                {
                    if (context.RequestId.HasValue)
                    {
                        await context.RespondAsync<OrderNotFound>(new {context.Message.OrderId});
                    }
                }));
            });


            InstanceState(x => x.CurrentState);

            Initially(
                When(OrderSubmitted)
                .Then(context =>
                {
                    context.Instance.SubmitDate = context.Data.Timestamp;
                    context.Instance.CustomerNumber = context.Data.CustomerNumber;
                    context.Instance.Updated = DateTime.UtcNow;
                })
                .TransitionTo(Submitted));

            During(Submitted,
                Ignore(OrderSubmitted));

            DuringAny(
                When(OrderStatusRequest)
                .RespondAsync(x => x.Init<OrderStatus>(new
                {
                    OrderId = x.Instance.CorrelationId,
                    State = x.Instance.CurrentState
                }))
            );

            DuringAny(
                When(OrderSubmitted)
                .Then(context =>
                {
                    context.Instance.SubmitDate ??= context.Data.Timestamp;
                    context.Instance.CustomerNumber ??= context.Data.CustomerNumber;
                })
            );
        }

        public State Submitted { get; private set; }

        public Event<OrderSubmitedEvent> OrderSubmitted { get; private set; }
        public Event<CheckOrder> OrderStatusRequest { get; private set; }
    }
}
