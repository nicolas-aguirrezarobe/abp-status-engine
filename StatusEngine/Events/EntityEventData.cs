using System;
using System.Collections.Generic;
using System.Text;
using Abp.Events.Bus;

namespace WorkflowEngine.Events
{
    public abstract class EntityEventData<TEntity>: EventData
    {
        public TEntity Entity { get; set; }
    }
}
