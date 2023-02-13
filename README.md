# abp-status-engine

A simple status-flow engine based on Abp.ZeroCore that help you define a flow of statuses with validations and events.
Built on top of Abp.Events.Bus to distribute events raised because of status changes.

## Features

- Use your own business entities
- Automatically generates an history of changes
- Build your workflow using a fluent builder

## Usage example

### Define your business entities

Define the enum with the possible statuses

```csharp
    public enum LogisticStatuses
    {
        ReadyToHandle,
        Handling,
        ReadyToShip,
        Shipped,
        Received,
        NotDelivered
    }
```

Define the class that will represent the status at a given time

```csharp
public abstract class LogisticStatus: Entity, IStep<LogisticStatuses>
    {
        public LogisticStatuses Value { get; set; }
        public string Reason { get; set; }
    }
```

Define the entity that will travel through the workflow.

```csharp
    public class Order
    {
        public LogisticStatus LogisticStatus { get; set; }
        public List<LogisticStatus> HistoricLogisticStatus { get; set; }
        public bool Approved { get; set; }
    }
```

### Build the workflow

```csharp
    var workflow = WorkflowBuilder<Order, LogisticStatus, LogisticStatuses>.Init()
        .For(LogisticStatuses.ReadyToHandle, step =>
        {
            step.AddNextSteps(LogisticStatuses.Handling);
            step.AddEntryValidation(x => !x.Approved, "Order already approved");
            step.AddExitValidation(x => x.Approved, "Order not approved");
        })
        .For(LogisticStatuses.Handling, step =>
        {
            step.AddNextSteps(LogisticStatuses.ReadyToShip);
            step.AddEventAfterEntering(new EventDataBuilder<Order>(new OnOrderHandlingEvent()));
        })
        .Build();
```

This will build a 2 step workflow with an explicit flow, some entry and exit validations on the first step and an event that will be fired after entering the second step.

An example of a full workflow for an order manager flow

```csharp
         var workflow = WorkflowBuilder<Order, LogisticStatus, LogisticStatuses>.Init()
            .For(LogisticStatuses.ReadyToHandle, step =>
            {
                step.AddNextSteps(LogisticStatuses.Handling);
                step.AddEntryValidation(x => !x.Approved, "Order already approved");
                step.AddExitValidation(x => x.Approved, "Order not approved");
            })
            .For(LogisticStatuses.Handling, step =>
            {
                step.AddNextSteps(LogisticStatuses.ReadyToShip);
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
```

Once you have the workflow created you can call `.GetFlow()` to watch the full flow so you can share it.

### Excecute the workflow

To use the workflow you need to call:

```csharp
await workflow.ChangeStep(order,
    x => x.LogisticStatus,
    x => x.HistoricLogisticStatus,
    LogisticStatuses.Handling,
    "Jhon is preparing this order");
```

Where: 
- `order` is the main entity 
- `x => x.LogisticStatus` is the projection to the status property 
- `x => x.HistoricLogisticStatus` is the projection to the list of historic statuses 
- `LogisticStatuses.Handling` is the new status 
- `"Jhon is preparing this order"` is the reason

