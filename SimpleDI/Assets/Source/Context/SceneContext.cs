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
            List<(object, MethodInfo)> injectMethods = new List<(object, MethodInfo)>();
            foreach (GameObject gameObject in rootGameObjects)
            {
                MonoBehaviour[] monoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();
                foreach (MonoBehaviour behaviour in monoBehaviours)
                {
                    GetInjectMethodsFromType(behaviour, injectMethods);
                }
            }

            InjectFromInstances(injectMethods);
        }
    }
}