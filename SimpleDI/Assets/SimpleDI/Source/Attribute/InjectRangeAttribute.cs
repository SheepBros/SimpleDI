using System;

namespace SimpleDI
{
    /// <summary>
    /// The attribute that is used to block including instances from parent containers in injection.
    /// SimpleDI injects all instances including parent containers even without this attribute by default.
    /// </summary>
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