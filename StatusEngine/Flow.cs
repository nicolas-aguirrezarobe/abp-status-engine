using System;
using System.Collections.Generic;
using System.Text;

namespace WorkflowEngine
{
    public class Flow<TStep>
    {
        public TStep From { get; set; }
        public List<TStep> To { get; set; }
    }
}
