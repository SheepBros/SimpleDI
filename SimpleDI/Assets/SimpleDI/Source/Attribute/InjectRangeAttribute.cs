using System;

namespace SimpleDI
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class InjectRangeAttribute : Attribute
    {
        /// <summary>
        /// If true, the context will be contained instances of parent containers.
        /// </summary>
        public bool AllowParent;

        public InjectRangeAttribute(bool allowParent = true)
        {
            AllowParent = allowParent;
        }
    }
}