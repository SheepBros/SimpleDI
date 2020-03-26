using System;
using System.Reflection;
using System.Collections.Generic;

namespace SimpleDI.Util
{
    public static class InjectUtil
    {
        public static void InjectWithContainer(DiContainer container, object instance)
        {
            if (GetInjectMethod(instance, out MethodInfo[] methodInfoArray))
            {
                InvokeInjectMethodArray(instance, methodInfoArray, container);
            }
        }

        public static bool GetInjectMethod(object instance, out MethodInfo[] methodInfoArray)
        {
            Type type = instance.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            List<MethodInfo> methodList = new List<MethodInfo>();
            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttribute<InjectAttribute>() != null)
                {
                    methodList.Add(method);
                }
            }

            methodInfoArray = methodList.ToArray();
            return methodInfoArray.Length > 0;
        }

        private static void InvokeInjectMethodArray(object instance, MethodInfo[] methodInfoArray, DiContainer container)
        {
            for (int i = 0; i < methodInfoArray.Length; ++i)
            {
                InvokeInjectMethod(instance, methodInfoArray[i], container);
            }
        }

        private static void InvokeInjectMethod(object instance, MethodInfo methodInfo, DiContainer container)
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
                        //UnityEngine.Debug.LogError($"{instance.GetType()} Bind {parameterType} {args[i] == null}");
                    }
                }
            }

            methodInfo.Invoke(instance, args);
        }
    }
}