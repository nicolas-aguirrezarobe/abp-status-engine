using System;
using System.Collections.Generic;

namespace WorkflowEngine
{
    public class ValidationsException: Exception
    {
        public ValidationsException(string message, List<ValidationResult> validationResults):base(message)
        {
            ValidationResults = validationResults;
        }

        public List<ValidationResult> ValidationResults { get; private set; }
    }


}
