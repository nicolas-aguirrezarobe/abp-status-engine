using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using WorkflowEngine;

namespace Example
{
    public class Order
    {
        public Order()
        {
            HistoricLogisticStatus = new List<LogisticStatus>();
        }
        public OverallStatus OverallStatus { get; set; }
        public LogisticStatus LogisticStatus { get; set; }
        public List<LogisticStatus> HistoricLogisticStatus { get; set; }
        public bool Approved { get; set; }
    }

    public abstract class OrderStatus<T>: Entity, IStep<T>
    {
        public T Value { get; set; }
        public string Reason { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class OverallStatus: OrderStatus<OverallStatuses>
    {

    }
    public class LogisticStatus : OrderStatus<LogisticStatuses>
    {

    }

    public enum OverallStatuses
    {
        Active,
        Finished,
        Cancelled
    }

    public enum LogisticStatuses
    {
        ReadyToHandle,
        Handling,
        ReadyToShip,
        Shipped,
        Received,
        NotDelivered
    }
}
