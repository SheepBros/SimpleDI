using System;
using System.Collections.Generic;
using UnityEngine;
using SimpleDI.Util;
using Debug = System.Diagnostics.Debug;

namespace SimpleDI
{
    /// <summary>
    /// Container class has a reference for instances.
    /// It does inject instances into an object.
    /// </summary>
    public class DiContainer
    {
        private readonly List<DiContainer> _parents = new List<DiContainer>();

        private Dictionary<Type, BindInfo> _instancesByType = new Dictionary<Type, BindInfo>();

        private List<object> _instances = new List<object>();

        private List<Type> _typeOfLifeCycleInterfaces = new List<Type>()
        {
            typeof(IInitializable),
            typeof(IDisposable),
            typeof(IUpdatable),
            typeof(IFixedUpdatable),
            typeof(ILateUpdatable)
        };

        public DiContainer(params DiContainer[] parents)
        {
            _parents.AddRange(parents);
        }

        public void BindAs<T>(bool singleInstance = true, bool bindToParents = false, params object[] args) where T : class
        {
            Bind(typeof(T), singleInstance, bindToParents: bindToParents, args: args);
        }

        public void BindTo<TFrom, TTo>(bool singleInstance = true, bool bindToParents = false, params object[] args)
                where TFrom : class
                where TTo : class
        {
            Type fromType = typeof(TFrom);
            Type toType = typeof(TTo);
            object instance = InstantiateInternal(fromType, args);

            Bind(toType, singleInstance, bindToParents: bindToParents, instance: instance, args: args);
        }

        public void BindFrom<T>(object instance, bool singleInstance = true, bool bindToParents = false) where T : class
        {
            Bind(typeof(T), singleInstance, bindToParents: bindToParents, instance);
        }

        public void BindAllInterfaces<T>(bool bindToParents = false, params object[] args) where T : class
        {
            Type type = typeof(T);
            object instance = InstantiateInternal(type, args);
            BindAllInterfacesFrom<T>(instance, bindToParents);
        }

        public void BindAllInterfacesFrom<T>(object instance, bool bindToParents = false) where T : class
        {
            Type type = typeof(T);
            Type[] interfaceTypes = type.GetInterfaces();

            for (int i = 0; i < interfaceTypes.Length; ++i)
            {
                Type interfaceType = interfaceTypes[i];
                bool canBeSingle = !_typeOfLifeCycleInterfaces.Contains(interfaceType);
                Bind(interfaceType, canBeSingle, bindToParents, instance);
            }
        }

        public void UnbindAll()
        {
            for (int i = 0; i < _parents.Count; ++i)
            {
                foreach (var pair in _instancesByType)
                {
                    _parents[i].Unbind(pair.Key);
                }
            }

            _instancesByType.Clear();
            _instances.Clear();
        }

        public void Unbind<T>() where T : class
        {
            Unbind(typeof(T));
        }

        public void Unbind(Type type)
        {
            if (_instancesByType.TryGetValue(type, out BindInfo bindInfo))
            {
                for (int i = 0; i < bindInfo.Instances.Count; ++i)
                {
                    object instance = bindInfo.Instances[i];
                    _instances.Remove(instance);
                }

                _instancesByType.Remove(type);
            }

            for (int i = 0; i < _parents.Count; ++i)
            {
                _parents[i].Unbind(type);
            }
        }

        public T GetInstance<T>(bool allowParentInstance = true) where T : class
        {
            return GetInstance(typeof(T), allowParentInstance) as T;
        }

        public object GetInstance(Type type, bool allowParentInstance = true)
        {
            if (_instancesByType.TryGetValue(type, out BindInfo bindInfo))
            {
                if (bindInfo.Instances.Count > 0)
                {
                    return bindInfo.Instances[0];
                }
            }

            if (!allowParentInstance)
            {
                return null;
            }

            object instance = null;
            for (int i = 0; i < _parents.Count; ++i)
            {
                DiContainer parent = _parents[i];
                instance = parent.GetInstance(type);
                if (instance != null)
                {
                    break;
                }
            }

            return instance;
        }

        public T[] GetInstances<T>(bool allowParentInstance = true) where T : class
        {
            return GetInstances(typeof(T), allowParentInstance) as T[];
        }

        public object[] GetInstances(Type type, bool allowParentInstance = true)
        {
            if (_instancesByType.TryGetValue(type, out BindInfo bindInfo))
            {
                if (bindInfo.Instances.Count > 0)
                {
                    return bindInfo.Instances.ToArray();
                }
            }

            if (!allowParentInstance)
            {
                return null;
            }

            List<object> instances = new List<object>();
            for (int i = 0; i < _parents.Count; ++i)
            {
                DiContainer parent = _parents[i];
                object[] parentInstances = parent.GetInstances(type);
                if (instances != null)
                {
                    instances.AddRange(parentInstances);
                }
            }

            return instances.ToArray();
        }

        public IEnumerator<object> GetAllInstances()
        {
            return _instances.GetEnumerator();
        }

        public T Instantiate<T>(params object[] args) where T : class
        {
            Type type = typeof(T);
            object instance = InstantiateInternal(type, args);

            InjectUtil.InjectWithContainer(this, instance);
            return instance as T;
        }

        public GameObject InstantiatePrefab(GameObject prefab)
        {
            GameObject instance = GameObject.Instantiate(prefab);
            InjectUtil.InjectWithContainer(this, instance);
            return instance;
        }

        public void Inject(object instance)
        {
            if (instance is GameObject gameObject)
            {
                InjectGameObject(gameObject);
            }
            else
            {
                InjectUtil.InjectWithContainer(this, instance);
            }
        }

        private void InjectGameObject(GameObject gameObject)
        {
            MonoBehaviour[] monoBehaviours = gameObject.GetComponentsInChildren<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in monoBehaviours)
            {
                InjectUtil.InjectWithContainer(this, behaviour);
            }
        }

        private void Bind(Type type, bool singleInstance, bool bindToParents = false, object instance = null, params object[] args)
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
                instance = InstantiateInternal(type, args);
            }

            if (instance != null)
            {
                bindInfo.Instances.Add(instance);

                if (!_instances.Contains(instance))
                {
                    _instances.Add(instance);
                }

                if (bindToParents && singleInstance)
                {
                    for (int i = 0; i < _parents.Count; ++i)
                    {
                        _parents[i].Bind(type, singleInstance, bindToParents, instance, args);
                    }
                }
            }
        }

        private object InstantiateInternal(Type type, params object[] args)
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