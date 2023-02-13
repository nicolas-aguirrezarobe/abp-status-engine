namespace WorkflowEngine
{
    public class ValidationResult
    {
        public ValidationResult(bool succes, string description)
        {
            Success = succes;
            if(!succes)
                Description = description;
        }
        public bool Success { get; private set; }
        public string Description { get; private set; }
    }


}
