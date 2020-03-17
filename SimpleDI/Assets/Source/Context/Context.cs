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

        private void InjectBindings()
        {
            List<(object, MethodInfo)> injectMethods = new List<(object, MethodInfo)>();
            var instances = Container.GetAllInstances();
            while (instances.MoveNext())
            {
                object instance = instances.Current;
                Type behaviourType = instance.GetType();
                MethodInfo[] methods = behaviourType.GetMethods(BindingFlags.Public);

                foreach (MethodInfo method in methods)
                {
                    if (method.GetCustomAttribute<InjectAttribute>() != null)
                    {
                        injectMethods.Add((instance, method));
                    }
                }
            }

            foreach (var methodTuple in injectMethods)
            {
                object instance = methodTuple.Item1;
                MethodInfo method = methodTuple.Item2;

                ParameterInfo[] parameters = method.GetParameters();
                object[] args = new object[parameters.Length];
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

                method.Invoke(instance, args);
            }
        }
    }
}