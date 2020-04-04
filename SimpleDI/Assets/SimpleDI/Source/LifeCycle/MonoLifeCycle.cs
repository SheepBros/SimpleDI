using UnityEngine;

namespace SimpleDI
{
    /// <summary>
    /// This executes events of InitializableManager, UpdatableManager, DisposableManager.
    /// It's automatically attached to a gameobject by Context when the context is initialized.
    /// </summary>
    [ExecutionOrder(-8900)]
    public class MonoLifeCycle : MonoBehaviour
    {
        private InitializableManager _initializableManager;

        private UpdatableManager _updatableManager;

        private DisposableManager _disposableManager;

        [Inject]
        public void InitContexts(
            [InjectRange(false)]
            InitializableManager initializableManager,
            [InjectRange(false)]
            UpdatableManager updatableManager,
            [InjectRange(false)]
            DisposableManager disposableManager)
        {
            _initializableManager = initializableManager;
            _updatableManager = updatableManager;
            _disposableManager = disposableManager;
        }

        private void Start()
        {
            _initializableManager.Initialize();
        }

        private void FixedUpdate()
        {
            if (_updatableManager != null)
            {
                _updatableManager.FixedUpdate();
            }
        }

        private void Update()
        {
            if (_updatableManager != null)
            {
                _updatableManager.Update();
            }
        }

        private void LateUpdate()
        {
            if (_updatableManager != null)
            {
                _updatableManager.LateUpdate();
            }
        }

        private void OnDestroy()
        {
            if (_disposableManager != null)
            {
                _disposableManager.Dispose();
            }
        }
    }
}