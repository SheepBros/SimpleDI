using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleDI
{
    public class SceneContext : Context
    {
        public override DiContainer Container { get; protected set; }

        [SerializeField]
        public List<MonoInstaller> _monoInstallers;

        private void Awake()
        {
            PersistentContext.Instance.MakeSureItsReady();
            Container = new DiContainer(PersistentContext.Instance.Container);

            Install();
        }

        protected override void InstallInternal()
        {
            foreach (MonoInstaller installer in _monoInstallers)
            {
                installer.Initialize(Container);
                installer.InstallBindings();
            }
        }

        protected override void InjectInternal()
        {
            InjectMonoBehaviours();
        }

        private void InjectMonoBehaviours()
        {
            GameObject[] rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            List<(MonoBehaviour, MethodInfo)> injectMethods = new List<(MonoBehaviour, MethodInfo)>();
            foreach (GameObject gameObject in rootGameObjects)
            {
                MonoBehaviour[] monoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();
                foreach (MonoBehaviour behaviour in monoBehaviours)
                {
                    Type behaviourType = behaviour.GetType();
                    MethodInfo[] methods = behaviourType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                    foreach (MethodInfo method in methods)
                    {
                        if (method.GetCustomAttribute<InjectAttribute>() != null)
                        {
                            injectMethods.Add((behaviour, method));
                        }
                    }
                }
            }

            foreach (var methodTuple in injectMethods)
            {
                MonoBehaviour instance = methodTuple.Item1;
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