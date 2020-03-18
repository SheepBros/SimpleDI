using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace SimpleDI
{
    public class DiContainer
    {
        private readonly List<DiContainer> _parents = new List<DiContainer>();

        private Dictionary<Type, BindInfo> _instancesByType = new Dictionary<Type, BindInfo>();

        private Dictionary<Type, object> _instancesByOriginalType = new Dictionary<Type, object>();

        private List<object> _instances = new List<object>();

        public DiContainer(params DiContainer[] parents)
        {
            _parents.AddRange(parents);

            BindFrom<DiContainer>(this);
        }

        public void BindAs<T>(bool singleInstance = true, params object[] args) where T : class
        {
            Bind(typeof(T), singleInstance, args: args);
        }

        public void BindTo<TFrom, TTo>(bool singleInstance = true, params object[] args)
                where TFrom : class
                where TTo : class
        {
            Type fromType = typeof(TFrom);
            Type toType = typeof(TTo);
            object instance = Instantiate(fromType, args);

            Bind(toType, singleInstance, instance, args: args);
        }

        public void BindFrom<T>(object instance, bool singleInstance = true) where T : class
        {
            Type type = typeof(T);

            _instancesByOriginalType.Add(type, instance);
            Bind(type, singleInstance, instance);
        }

        public void BindAllInterfaces<T>(params object[] args) where T : class
        {
            Type type = typeof(T);
            Type[] interfaceTypes = type.GetInterfaces();
            object instance = Instantiate(type, args);

            for (int i = 0; i < interfaceTypes.Length; ++i)
            {
                Bind(interfaceTypes[i], false, instance);
            }
        }

        public void UnbindAll()
        {
            _instancesByType.Clear();
            _instancesByOriginalType.Clear();
            _instances.Clear();
        }

        public T GetInstance<T>() where T : class
        {
            return GetInstance(typeof(T)) as T;
        }

        public object GetInstance(Type type)
        {
            if (_instancesByType.TryGetValue(type, out BindInfo bindInfo))
            {
                if (bindInfo.Instances.Count > 0)
                {
                    return bindInfo.Instances[0];
                }
            }

            object instance = null;
            foreach (DiContainer parent in _parents)
            {
                instance = parent.GetInstance(type);
                if (instance != null)
                {
                    break;
                }
            }

            return instance;
        }

        public T[] GetInstances<T>() where T : class
        {
            return GetInstances(typeof(T)) as T[];
        }

        public object[] GetInstances(Type type)
        {
            if (_instancesByType.TryGetValue(type, out BindInfo bindInfo))
            {
                if (bindInfo.Instances.Count > 0)
                {
                    return bindInfo.Instances.ToArray();
                }
            }

            object[] instances = null;
            foreach (DiContainer parent in _parents)
            {
                instances = parent.GetInstances(type);
                if (instances != null)
                {
                    break;
                }
            }

            return instances;
        }

        public IEnumerator<object> GetAllInstances()
        {
            return _instances.GetEnumerator();
        }

        private void Bind(Type type, bool singleInstance, object instance = null, params object[] args)
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

            if (instance == null)
            {
                instance = Instantiate(type, args);
            }

            if (instance != null)
            {
                bindInfo.Instances.Add(instance);
                _instances.Add(instance);
            }
        }

        private object Instantiate(Type type, params object[] args)
        {
            object instance = Activator.CreateInstance(type, args);
            Debug.Assert(instance != null, $"Failed to instantiate {type}.");
            _instancesByOriginalType.Add(type, instance);
            return instance;
        }

        private class BindInfo
        {
            public List<object> Instances = new List<object>();

            public bool Single;
        }
    }
}