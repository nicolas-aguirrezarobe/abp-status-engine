using System.Threading.Tasks;
using System;
using System.Linq;
using WorkflowEngine;
using WorkflowEngine.Events;

namespace Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var order = new Order()
            {
                OverallStatus = new OverallStatus { Value = OverallStatuses.Active },
                LogisticStatus = new LogisticStatus { Value = LogisticStatuses.ReadyToHandle }
            };

            var workflow = WorkflowBuilder<Order, LogisticStatus, LogisticStatuses>.Init()
               .For(LogisticStatuses.ReadyToHandle, step =>
               {
                   step.AddNextSteps(LogisticStatuses.Handling);
                   step.AddEntryValidation(x => x.OverallStatus.Value == OverallStatuses.Active, "Order not active");
                   step.AddEntryValidation(x => !x.Approved, "Order already approved");
                   step.AddExitValidation(x => x.Approved, "Order not approved");
               })
               .For(LogisticStatuses.Handling, step =>
               {
                   step.AddNextSteps(LogisticStatuses.ReadyToShip);
                   step.AddEntryValidation(x => x.OverallStatus.Value == OverallStatuses.Active, "Order not active");
               })
               .For(LogisticStatuses.ReadyToShip, step =>
               {
                   step.AddNextSteps(LogisticStatuses.Shipped);
               })
               .For(LogisticStatuses.Shipped, step =>
               {
                   step.AddNextSteps(LogisticStatuses.Received, LogisticStatuses.NotDelivered);
                   step.AddEventBeforeEntering(new EventDataBuilder<Order>(new OnOrderShippingEvent(order)));
               })
               .For(LogisticStatuses.Received, step =>
               {
                   step.AddNextSteps(LogisticStatuses.Received);
                   step.AddEventAfterEntering(new EventDataBuilder<Order>(new OnOrderShippedEvent()));
               })
               .For(LogisticStatuses.NotDelivered, step =>
               {
                   step.AddPostEntryValidation(x => !string.IsNullOrEmpty(x.LogisticStatus.Reason), "You must enter a reason");
               })
               .Build();

            var flow = workflow.GetFlow();

            order.Approved = true;

            Console.WriteLine(order.LogisticStatus.Value.ToString());

            await workflow.ChangeStep(order,
                x => x.LogisticStatus,
                x => x.HistoricLogisticStatus,
                LogisticStatuses.ReadyToHandle,
                "Same status");

            Console.WriteLine("Current status: " + order.LogisticStatus.Value.ToString());
            Console.WriteLine("Historic status: " + string.Join(", ", order.HistoricLogisticStatus.Select(x => x.Value.ToString())));

            await workflow.ChangeStep(order,
                x => x.LogisticStatus,
                x => x.HistoricLogisticStatus,
                LogisticStatuses.Handling,
                "Testing");

            Console.WriteLine("Current status: " + order.LogisticStatus.Value.ToString());
            Console.WriteLine("Historic status: " + string.Join(", ", order.HistoricLogisticStatus.Select(x => x.Value.ToString())));

            await workflow.ChangeStep(order,
                x => x.LogisticStatus,
                x => x.HistoricLogisticStatus,
                LogisticStatuses.ReadyToShip);

            Console.WriteLine("Current status: " + order.LogisticStatus.Value.ToString());
            Console.WriteLine("Historic status: " + string.Join(", ", order.HistoricLogisticStatus.Select(x => x.Value.ToString())));

            await workflow.ChangeStep(order,
                x => x.LogisticStatus,
                x => x.HistoricLogisticStatus,
                LogisticStatuses.Shipped);

            Console.WriteLine("Current status: " + order.LogisticStatus.Value.ToString());
            Console.WriteLine("Historic status: " + string.Join(", ", order.HistoricLogisticStatus.Select(x => x.Value.ToString())));

            await workflow.ChangeStep(order,
                x => x.LogisticStatus,
                x => x.HistoricLogisticStatus,
                LogisticStatuses.Received);

            Console.WriteLine("Current status: " + order.LogisticStatus.Value.ToString());
            Console.WriteLine("Historic status: " + string.Join(", ", order.HistoricLogisticStatus.Select(x => x.Value.ToString())));

            try
            {
            await workflow.ChangeStep(order,
                x => x.LogisticStatus,
                x => x.HistoricLogisticStatus,
                LogisticStatuses.NotDelivered,
                "No one at home");
            }
            catch(ValidationsException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine("Validations: " + string.Join(", ", ex.ValidationResults.Select(x => x.Description)));
            }

        }
    }
}
