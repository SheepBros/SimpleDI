using System.Diagnostics;
using System.Collections.Generic;

namespace SimpleDI
{
    /// <summary>
    /// This is a manager class that notifies Initialize event.
    /// It's dependent to context and There is one InitializableManager for each context.
    /// The initialize event is notified only one time after the context is initialized.
    /// It never notifies the event again.
    /// </summary>
    public class InitializableManager
    {
        private List<IInitializable> _initializables = new List<IInitializable>();

        private bool _initialized = false;

        [Inject]
        public void InitContexts(
            [InjectRange(false)]
            IInitializable[] initializables)
        {
            if (initializables != null)
            {
                for (int i = 0; i < initializables.Length; ++i)
                {
                    if (initializables[i] != null)
                    {
                        _initializables.Add(initializables[i]);
                    }
                }
            }
        }

        public void Initialize()
        {
            Debug.Assert(!_initialized, "InitializableManager.Initialize is called twice.");
            _initialized = true;

            for (int i = 0; i < _initializables.Count; ++i)
            {
                _initializables[i].Initialize();
            }
        }
    }
}