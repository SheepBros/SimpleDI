using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace SimpleDI
{
    public class DiContainer
    {
        private Dictionary<Type, BindInfo> _instancesByType = new Dictionary<Type, BindInfo>();

        public void BindAs<T>(bool singleInstance = false, params object[] args) where T : class
        {
            Bind(typeof(Type), singleInstance, args);
        }

        public void BindAllInterfaces<T>() where T : class
        {
            Type type = typeof(T);
            Type[] interfaceTypes = type.GetInterfaces();

            for (int i = 0; i < interfaceTypes.Length; ++i)
            {
                Bind(interfaceTypes[i], false);
            }
        }

        public T GetInstance<T>() where T : class
        {
            Type type = typeof(T);
            if (_instancesByType.TryGetValue(type, out BindInfo bindInfo))
            {
                if (bindInfo.Instances.Count > 0)
                {
                    return bindInfo.Instances[0] as T;
                }
            }

            return null;
        }

        public T[] GetInstances<T>() where T : class
        {
            Type type = typeof(T);
            if (_instancesByType.TryGetValue(type, out BindInfo bindInfo))
            {
                if (bindInfo.Instances.Count > 0)
                {
                    return bindInfo.Instances.ToArray() as T[];
                }
            }

            return null;
        }

        private void Bind(Type type, bool singleInstance, params object[] args)
        {
            if (!_instancesByType.TryGetValue(type, out BindInfo bindInfo))
            {

                bindInfo = new BindInfo()
                {
                    Single = singleInstance
                };

                _instancesByType.Add(type, bindInfo);
            }
            else if (singleInstance)
            {
                Debug.Assert(!singleInstance, $"Trying to instantiate a single instance({type}). But, the instance is already instantiated.");
                return;
            }

            object instance = Instantiate(type, args);
            if (instance != null)
            {
                bindInfo.Instances.Add(instance);
            }
        }

        private object Instantiate(Type type, params object[] args)
        {
            object instance = Activator.CreateInstance(type, args);
            Debug.Assert(instance != null, $"Failed to instantiate {type}.");
            return instance;
        }

        private class BindInfo
        {
            public List<object> Instances = new List<object>();

            public bool Single;
        }
    }
}