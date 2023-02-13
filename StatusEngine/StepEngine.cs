using Abp.Events.Bus;
using  WorkflowEngine.Events;
using System.Collections.Generic;

namespace WorkflowEngine
{
    public class StepEngine<TEntity, TStep>
    {
        public StepEngine()
        {
            EntryValidations = new Validations<TEntity>();
            ExitValidations = new Validations<TEntity>();
            PostEntryValidations = new Validations<TEntity>();
            PostExitValidations = new Validations<TEntity>();
            NextSteps = new List<TStep>();
            BeforeEnteringEvents = new List<EventDataBuilder<TEntity>>();
            AfterEnteringEvents = new List<EventDataBuilder<TEntity>>();
            BeforeLeavingEvents = new List<EventDataBuilder<TEntity>>();
            AfterLeavingEvents = new List<EventDataBuilder<TEntity>>();
        }
        public Validations<TEntity> EntryValidations { get; protected set; }
        public Validations<TEntity> ExitValidations { get; protected set; }
        public Validations<TEntity> PostEntryValidations { get; protected set; }
        public Validations<TEntity> PostExitValidations { get; protected set; }
        public List<EventDataBuilder<TEntity>> BeforeEnteringEvents { get; protected set; }
        public List<EventDataBuilder<TEntity>> AfterEnteringEvents { get; protected set; }
        public List<EventDataBuilder<TEntity>> BeforeLeavingEvents { get; protected set; }
        public List<EventDataBuilder<TEntity>> AfterLeavingEvents { get; protected set; }


        public List<TStep> NextSteps { get; protected set; }
    }


}
