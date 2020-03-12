using System;

namespace SimpleDI
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class InjectAttribute : Attribute
    {
    }
}