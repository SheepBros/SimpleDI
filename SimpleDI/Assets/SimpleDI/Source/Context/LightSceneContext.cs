using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace SimpleDI
{
    public class LightSceneContext : SceneContext
    {
        [ReadOnly]
        [SerializeField]
        private List<MonoBehaviour> _initialInjectInstances = new List<MonoBehaviour>();
        
        protected override void InjectInternal()
        {
            foreach (MonoBehaviour instance in _initialInjectInstances)
            {
                Container.Inject(instance);
            }
        }

        /// <summary>
        /// NOTE: Don't call this in runtime.
        /// </summary>
        public void RegisterAllInjectInstancesInScene()
        {
            _initialInjectInstances.Clear();
            
            GameObject[] rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                MonoBehaviour[] monoBehaviours = rootGameObject.GetComponentsInChildren<MonoBehaviour>();
                foreach (MonoBehaviour monoBehaviour in monoBehaviours)
                {
                    if (monoBehaviour is IInjectable)
                    {
                        _initialInjectInstances.Add(monoBehaviour);
                    }
                }
            }
        }
    }
}