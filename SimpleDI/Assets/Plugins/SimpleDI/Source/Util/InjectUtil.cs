using System;
using System.Reflection;

namespace SimpleDI.Util
{
    public static class InjectUtil
    {
        public static void InjectWithContainer(DiContainer container, object instance)
        {
            if (GetInjectMethod(instance, out MethodInfo methodInfo))
            {
                InvokeInjectMethod(instance, methodInfo, container);
            }
        }

        public static bool GetInjectMethod(object instance, out MethodInfo methodInfo)
        {
            Type type = instance.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttribute<InjectAttribute>() != null)
                {
                    methodInfo = method;
                    return true;
                }
            }

            methodInfo = null;
            return false;
        }

        public static void InvokeInjectMethod(object instance, MethodInfo methodInfo, DiContainer container)
        {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            object[] args = null;
            if (parameters.Length > 0)
            {
                args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; ++i)
                {
                    ParameterInfo parameter = parameters[i];
                    bool allowParentInstace = true;
                    var attributes = parameter.GetCustomAttributes();
                    foreach (Attribute attribute in attributes)
                    {
                        if (attribute is InjectRangeAttribute)
                        {
                            allowParentInstace = ((InjectRangeAttribute)attribute).AllowParent;
                            break;
                        }
                    }

                    Type parameterType = parameter.ParameterType;
                    if (parameterType.IsArray)
                    {
                        Type elementType = parameterType.GetElementType();
                        object[] instances = container.GetInstances(elementType, allowParentInstace);
                        if (instances == null)
                        {
                            continue;
                        }

                        Array convertedInstances = Array.CreateInstance(elementType, instances.Length);
                        Array.Copy(instances, convertedInstances, instances.Length);
                        args[i] = convertedInstances;
                    }
                    else
                    {
                        args[i] = container.GetInstance(parameterType, allowParentInstace);
                    }
                }
            }

            methodInfo.Invoke(instance, args);
        }
    }
}