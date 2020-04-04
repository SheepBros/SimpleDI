using System;

namespace SimpleDI
{
    /// <summary>
    /// The attribute to indicate the method for dependency injection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class InjectAttribute : Attribute
    {
    }
}