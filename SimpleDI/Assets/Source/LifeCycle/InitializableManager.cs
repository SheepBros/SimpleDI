using System.Diagnostics;
using System.Collections.Generic;

namespace SimpleDI
{
    public class InitializableManager
    {
        private List<IInitializable> _initializables = new List<IInitializable>();

        private bool _initialized = false;

        [Inject]
        public void InitInjections(IInitializable[] initializables)
        {
            _initializables.AddRange(initializables);
        }

        public void Initialize()
        {
            Debug.Assert(!_initialized, "InitializableManager.Initialize is called twice.");
            _initialized = true;

            for (int i = 0; i < _initializables.Count; ++i)
            {
                _initializables[i].Initlaize();
            }
        }
    }
}