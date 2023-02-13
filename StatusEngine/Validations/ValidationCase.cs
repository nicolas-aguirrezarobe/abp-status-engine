using System;
using System.Linq.Expressions;

namespace WorkflowEngine
{
    internal class ValidationCase<TEntity>
    {
        internal ValidationCase(Expression<Func<TEntity, bool>> expression, string inavlidMessage)
        {
            Expression = expression;
            InavlidMessage = inavlidMessage;
        }

        internal Expression<Func<TEntity, bool>> Expression { get; set; }
        internal string InavlidMessage { get; set; }
    }


}
