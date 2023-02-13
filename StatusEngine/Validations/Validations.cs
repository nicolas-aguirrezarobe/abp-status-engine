using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace WorkflowEngine
{
    public class Validations<TEntity>
    {
        private readonly List<ValidationCase<TEntity>> _list = new List<ValidationCase<TEntity>>();

        internal void AddValidation(Expression<Func<TEntity, bool>> expression, string inavlidMessage)
        {
            _list.Add(new ValidationCase<TEntity>(expression, inavlidMessage));
        }

        internal List<ValidationResult> Validate(TEntity entity)
        {
            var results = new List<ValidationResult>();
            _list.ForEach(x => {
                results.Add(new ValidationResult(x.Expression.Compile().Invoke(entity), x.InavlidMessage));
            });
            return results;
        }
    }
}
