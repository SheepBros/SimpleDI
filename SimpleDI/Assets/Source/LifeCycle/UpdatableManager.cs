using System.Collections.Generic;

namespace SimpleDI
{
    public class UpdatableManager
    {
        private List<IFixedUpdatable> _fixedUpdatables = new List<IFixedUpdatable>();

        private List<IUpdatable> _updatables = new List<IUpdatable>();

        private List<ILateUpdatable> _lateUpdatables = new List<ILateUpdatable>();

        [Inject]
        public void InitInjections(IFixedUpdatable[] fixedUpdatables, IUpdatable[] updatables, ILateUpdatable[] lateUpdatables)
        {
            _fixedUpdatables.AddRange(fixedUpdatables);
            _updatables.AddRange(updatables);
            _lateUpdatables.AddRange(lateUpdatables);
        }

        public void FixedUpdate()
        {
            for (int i = 0; i < _fixedUpdatables.Count; ++i)
            {
                _fixedUpdatables[i].FixedUpdate();
            }
        }

        public void Update()
        {
            for (int i = 0; i < _updatables.Count; ++i)
            {
                _updatables[i].Update();
            }
        }

        public void LateUpdate()
        {
            for (int i = 0; i < _lateUpdatables.Count; ++i)
            {
                _lateUpdatables[i].LateUpdate();
            }
        }
    }
}