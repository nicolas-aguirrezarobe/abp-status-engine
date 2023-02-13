using System;

namespace WorkflowEngine
{
    public interface IStep<T>: ICloneable
    {
        T Value { get; set; }
        string Reason { get; set; }
    }


}
