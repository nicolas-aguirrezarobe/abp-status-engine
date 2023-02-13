using Abp.Events.Bus;
using System;
using System.Collections.Generic;
using System.Text;
using WorkflowEngine;
using WorkflowEngine.Events;

namespace Example
{
    public class OnOrderShippingEvent: EntityEventData<Order>
    {
        public OnOrderShippingEvent(Order order)
        {
            Order = order;
        }

        public Order Order { get; set; }
    }

    public class OnOrderShippedEvent : EntityEventData<Order>
    {
        public OnOrderShippedEvent()
        {
        }

        public Order Order { get; set; }
    }


}
