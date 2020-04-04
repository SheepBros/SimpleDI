using System;

namespace SimpleDI
{
    /// <summary>
    /// The attribute class to control the script execution order.
    /// This is only working for classes in SimpleDi namespace.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExecutionOrderAttribute : Attribute
    {
        /// <summary>
        /// If true, The order is fixed.
        /// </summary>
        public bool FixedOrder { get; set; } = true;

        /// <summary>
        /// Execution order.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="order">Execution order.</param>
        public ExecutionOrderAttribute(int order)
        {
            Order = order;
        }
    }
}