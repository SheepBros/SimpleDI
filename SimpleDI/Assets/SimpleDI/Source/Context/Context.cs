using UnityEngine;

namespace SimpleDI
{
    /// <summary>
    /// Context is a class that holds a container and gives interfaces to bind instances into the container.
    /// </summary>
    public abstract class Context : MonoBehaviour
    {
        public abstract DiContainer Container { get; protected set; }

        protected virtual void OnDestroy()
        {
            if (Container != null)
            {
                Container.UnbindAll();
            }
        }

        protected virtual void InstallInternal() { }

        protected virtual void InjectInternal() { }

        protected void Install()
        {
            Container.BindFrom<DiContainer>(Container);
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
            var instances = Container.GetAllInstances();
            while (instances.MoveNext())
            {
                Container.Inject(instances.Current);
            }
        }
    }
}