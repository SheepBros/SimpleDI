using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleDI
{
    public abstract class Context : MonoBehaviour
    {
        public abstract DiContainer Container { get; protected set; }

        protected virtual void OnDestroy()
        {
            Container.UnbindAll();
        }

        protected virtual void InstallInternal() { }

        protected virtual void InjectInternal() { }

        protected void Install()
        {
            Container.BindAs<InitializableManager>();
            Container.BindAs<UpdatableManager>();
            Container.BindAs<DisposableManager>();

            this.gameObject.AddComponent<MonoLifeCycle>();

            InstallInternal();

            InjectAll();
        }

        private void InjectAll()
        {
            InjectBindings();
            InjectInternal();
        }

        protected void GetInjectMethodsFromType(object instance, List<(object, MethodInfo)> injectMethods)
        {
            Type type = instance.GetType();
            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (MethodInfo method in methods)
            {
                if (method.GetCustomAttribute<InjectAttribute>() != null)
                {
                    injectMethods.Add((instance, method));
                }
            }
        }

        protected void InjectFromInstances(List<(object, MethodInfo)> injectMethods)
        {
            foreach (var methodTuple in injectMethods)
            {
                object instance = methodTuple.Item1;
                MethodInfo method = methodTuple.Item2;

                ParameterInfo[] parameters = method.GetParameters();
                object[] args = null;
                if (parameters.Length > 0)
                {
                    args = new object[parameters.Length];
                    for (int i = 0; i < parameters.Length; ++i)
                    {
                        ParameterInfo parameter = parameters[i];
                        Type parameterType = parameter.ParameterType;
                        if (parameterType.IsArray)
                        {
                            args[i] = Container.GetInstances(parameterType);
                        }
                        else
                        {
                            args[i] = Container.GetInstance(parameterType);
                        }
                    }
                }

                method.Invoke(instance, args);
            }
        }

        private void InjectBindings()
        {
            List<(object, MethodInfo)> injectMethods = new List<(object, MethodInfo)>();
            var instances = Container.GetAllInstances();
            while (instances.MoveNext())
            {
                object instance = instances.Current;
                GetInjectMethodsFromType(instance, injectMethods);
            }

            InjectFromInstances(injectMethods);
        }
    }
}