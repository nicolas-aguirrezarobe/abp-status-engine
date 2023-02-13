using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Abp.Events.Bus;
using Abp.Domain.Entities;
using System.Threading.Tasks;
using Abp.Domain.Entities.Auditing;

namespace WorkflowEngine
{
    public class Workflow<TEntity, TStep, TStepValue>
        where TStep : IStep<TStepValue>
    {
        public Dictionary<TStepValue, StepEngine<TEntity, TStepValue>> Steps { get; internal set; }

        public List<Flow<TStepValue>> GetFlow()
        {
            var flows = new List<Flow<TStepValue>>();
            foreach (var step in Steps)
            {
                flows.Add(new Flow<TStepValue> { 
                    From = step.Key,
                    To = step.Value.NextSteps
                });
            }
            return flows;
        }

        public async Task ChangeStep(TEntity entity, Expression<Func<TEntity, TStep>> currentStepExpression, TStepValue nextStep, string reason = null)
        {
            await ChangeStep(entity, currentStepExpression, null, nextStep, reason);
        }

        public async Task ChangeStep(TEntity entity, Expression<Func<TEntity, TStep>> currentStepExpression, Expression<Func<TEntity, List<TStep>>> historicStepsExpression, TStepValue nextStep, string reason = null)
        {
            var currentStep = GetCurrentStep(entity, currentStepExpression).Value;
            if (IsSameStep(currentStep, nextStep))
                return;
            var currentStepEngine = GetStepEngine(currentStep);
            var nextStepEngine = GetStepEngine(nextStep);
            currentStepEngine.ExitValidations.AddValidation(_ => currentStepEngine.NextSteps.Contains(nextStep), $"[Wrong flow changing]: [{currentStep}] -> [{nextStep}]");

            var validations = new List<ValidationResult>();
            validations.AddRange(currentStepEngine.ExitValidations.Validate(entity));
            validations.AddRange(nextStepEngine.EntryValidations.Validate(entity));

            if (validations.Any(x => !x.Success))
                throw new ValidationsException("Some validations did not succeed.", validations.Where(x => !x.Success).ToList());

            foreach (var e in nextStepEngine.BeforeEnteringEvents){
                await EventBus.Default.TriggerAsync(e.Build(entity));
            }

            foreach (var e in currentStepEngine.BeforeLeavingEvents){
                await EventBus.Default.TriggerAsync(e.Build(entity));
            }

            nextStepEngine.BeforeEnteringEvents.ForEach(x => {  });
            currentStepEngine.BeforeLeavingEvents.ForEach(x => {  });

            if (historicStepsExpression != null)
                AddHistoric(entity, currentStepExpression, historicStepsExpression);
            SetValue(entity, currentStepExpression, nextStep, reason);

            validations = new List<ValidationResult>();
            validations.AddRange(currentStepEngine.PostExitValidations.Validate(entity));
            validations.AddRange(nextStepEngine.PostEntryValidations.Validate(entity));

            if (validations.Any(x => !x.Success))
                throw new ValidationsException("Some post validations did not succeed.", validations.Where(x => !x.Success).ToList());

            foreach (var e in nextStepEngine.AfterEnteringEvents){
                await EventBus.Default.TriggerAsync(e.Build(entity));
            }
            foreach (var e in currentStepEngine.AfterLeavingEvents){
                await EventBus.Default.TriggerAsync(e.Build(entity));
            }
        }

        private void SetValue(TEntity entity, Expression<Func<TEntity, TStep>> memberLamda, TStepValue nextStep, string reason)
        {
            var current = GetCurrentStep(entity, memberLamda);
            var next = (TStep)current.Clone();
            next.Value = nextStep;
            next.Reason = reason;
            if (next is IEntity)
                (next as IEntity).Id = 0;
            if (next is IHasCreationTime)
                (next as IHasCreationTime).CreationTime = default;
            GetProperty(entity, memberLamda).SetValue(entity, next);
        }

        private void AddHistoric(TEntity entity, Expression<Func<TEntity, TStep>> currentStepExpression, Expression<Func<TEntity, List<TStep>>> historicStepsExpression)
        {
            var current = GetCurrentStep(entity, currentStepExpression);

            ((List<TStep>)GetProperty(entity, historicStepsExpression).GetValue(entity)).Add(current);
        }

        private TStep GetCurrentStep(TEntity entity, Expression<Func<TEntity, TStep>> memberLamda)
        {
            return (TStep)GetProperty(entity, memberLamda).GetValue(entity);
        }

        private PropertyInfo GetProperty<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> memberLamda)
        {
            if (memberLamda.Body is MemberExpression memberSelectorExpression)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    return property;
                }
            }
            throw new Exception("There is no such property in this class");
        }

        private StepEngine<TEntity, TStepValue> GetStepEngine(TStepValue step)
        {
            if (Steps.TryGetValue(step, out StepEngine<TEntity, TStepValue> stepEngine))
                return stepEngine;
            else
                throw new Exception($"Invalid step {step.ToString()}");
        }

        private bool IsSameStep(TStepValue stepValue1, TStepValue stepValue2)
        {
            return stepValue1.Equals(stepValue2);
        }

    }
}
