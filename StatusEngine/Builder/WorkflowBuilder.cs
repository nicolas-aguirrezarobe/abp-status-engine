using WorkflowEngine.Events;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace WorkflowEngine
{
    public class WorkflowBuilder<TEntity, TStep, TStepValue>: IWorkflowBuilder<TEntity, TStep, TStepValue>
        where TStep : IStep<TStepValue>
    {
        private static Workflow<TEntity, TStep, TStepValue> _workflow;

        private WorkflowBuilder()
        {
            _workflow = new Workflow<TEntity, TStep, TStepValue>();
            _workflow.Steps = new Dictionary<TStepValue, StepEngine<TEntity, TStepValue>>();
        }

        public static IWorkflowBuilder<TEntity, TStep, TStepValue> Init()
        {
            return new WorkflowBuilder<TEntity, TStep, TStepValue>();
        }

        public Workflow<TEntity, TStep, TStepValue> Build()
        {
            return _workflow;
        }


        IWorkflowBuilder<TEntity, TStep, TStepValue> IWorkflowBuilder<TEntity, TStep, TStepValue>.For(TStepValue step, Action<IStepBuilder<TEntity, TStepValue>> expression)
        {
            var builder = new StepBuilder<TEntity, TStepValue>();
            expression(builder);
            var engine = builder.Build();
            _workflow.Steps.Add(step, engine);
            return this;
        }
    }

    public class StepBuilder<TEntity, TStepValue> : IStepBuilder<TEntity, TStepValue>
    {
        private StepEngine<TEntity, TStepValue> _stepEngine;

        public StepBuilder()
        {
            _stepEngine = new StepEngine<TEntity, TStepValue>();
        }

        public IStepBuilder<TEntity, TStepValue> AddPostEntryValidation(Expression<Func<TEntity, bool>> expression, string inavlidMessage)
        {
            _stepEngine.PostEntryValidations.AddValidation(expression, inavlidMessage);
            return this;
        }

        public IStepBuilder<TEntity, TStepValue> AddEntryValidation(Expression<Func<TEntity, bool>> expression, string inavlidMessage)
        {
            _stepEngine.EntryValidations.AddValidation(expression, inavlidMessage);
            return this;
        }

        public IStepBuilder<TEntity, TStepValue> AddPostExitValidation(Expression<Func<TEntity, bool>> expression, string inavlidMessage)
        {
            _stepEngine.PostExitValidations.AddValidation(expression, inavlidMessage);
            return this;
        }

        public IStepBuilder<TEntity, TStepValue> AddExitValidation(Expression<Func<TEntity, bool>> expression, string inavlidMessage)
        {
            _stepEngine.ExitValidations.AddValidation(expression, inavlidMessage);
            return this;
        }

        public IStepBuilder<TEntity, TStepValue> AddNextSteps(params TStepValue[] steps)
        {
            _stepEngine.NextSteps.AddRange(steps);
            return this;
        }

        internal StepEngine<TEntity, TStepValue> Build()
        {
            return _stepEngine;
        }

        public IStepBuilder<TEntity, TStepValue> AddEventBeforeEntering<TEventBuilder>(TEventBuilder eventBuilder) 
            where TEventBuilder : EventDataBuilder<TEntity>
        {
            _stepEngine.BeforeEnteringEvents.Add(eventBuilder);
            return this;
        }

        public IStepBuilder<TEntity, TStepValue> AddEventBeforeLeaving<TEventBuilder>(TEventBuilder eventBuilder) 
            where TEventBuilder : EventDataBuilder<TEntity>
        {
            _stepEngine.BeforeLeavingEvents.Add(eventBuilder);
            return this;
        }

        public IStepBuilder<TEntity, TStepValue> AddEventAfterEntering<TEventBuilder>(TEventBuilder eventBuilder) 
            where TEventBuilder : EventDataBuilder<TEntity>
        {
            _stepEngine.AfterEnteringEvents.Add(eventBuilder);
            return this;
        }

        public IStepBuilder<TEntity, TStepValue> AddEventAfterLeaving<TEventBuilder>(TEventBuilder eventBuilder) 
            where TEventBuilder : EventDataBuilder<TEntity>
        {
            _stepEngine.AfterLeavingEvents.Add(eventBuilder);
            return this;
        }
    }

    public interface IBuilder<TEntity, TStepValue>
    {
        StepEngine<TEntity, TStepValue> Build(TStepValue step);
    }

    public interface IWorkflowBuilder<TEntity, TStep, TStepValue>
        where TStep : IStep<TStepValue>
    {
        IWorkflowBuilder<TEntity, TStep, TStepValue> For(TStepValue step, Action<IStepBuilder<TEntity, TStepValue>> expression);
        Workflow<TEntity, TStep, TStepValue> Build();
    }

    public interface IStepBuilder<TEntity, TStep>
    {

        /// <summary>
        /// Condition that should be validated by the step that is entering.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="inavlidMessage"></param>
        /// <returns></returns>
        IStepBuilder<TEntity, TStep> AddEntryValidation(Expression<Func<TEntity, bool>> expression, string inavlidMessage);

        /// <summary>
        /// Condition that should be validated before the current step is changed.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="inavlidMessage"></param>
        /// <returns></returns>
        IStepBuilder<TEntity, TStep> AddExitValidation(Expression<Func<TEntity, bool>> expression, string inavlidMessage);

        /// <summary>
        /// Condition that should be validated after switching to the new step.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="inavlidMessage"></param>
        /// <returns></returns>
        IStepBuilder<TEntity, TStep> AddPostEntryValidation(Expression<Func<TEntity, bool>> expression, string inavlidMessage);

        /// <summary>
        /// Condition that should be validated after the previous step was changed.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="inavlidMessage"></param>
        /// <returns></returns>
        IStepBuilder<TEntity, TStep> AddPostExitValidation(Expression<Func<TEntity, bool>> expression, string inavlidMessage);
        IStepBuilder<TEntity, TStep> AddNextSteps(params TStep[] step);

        IStepBuilder<TEntity, TStep> AddEventBeforeEntering<TEventBuilder>(TEventBuilder eventBuilder) where TEventBuilder : EventDataBuilder<TEntity>;
        IStepBuilder<TEntity, TStep> AddEventBeforeLeaving<TEventBuilder>(TEventBuilder eventBuilder) where TEventBuilder : EventDataBuilder<TEntity>;
        IStepBuilder<TEntity, TStep> AddEventAfterEntering<TEventBuilder>(TEventBuilder eventBuilder) where TEventBuilder : EventDataBuilder<TEntity>;
        IStepBuilder<TEntity, TStep> AddEventAfterLeaving<TEventBuilder>(TEventBuilder eventBuilder) where TEventBuilder : EventDataBuilder<TEntity>;
    }
}
