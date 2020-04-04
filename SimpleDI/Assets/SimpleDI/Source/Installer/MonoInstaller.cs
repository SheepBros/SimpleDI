using UnityEngine;

namespace SimpleDI
{
    /// <summary>
    /// MonoInstaller is an installer for binding instances to the container in a context.
    /// It should be hooked up to a context manually as a monobehaviour component.
    /// </summary>
    public abstract class MonoInstaller : MonoBehaviour
    {
        protected DiContainer _container { get; private set; }

        public void Initialize(DiContainer container)
        {
            _container = container;
        }

        public abstract void InstallBindings();
    }
}