using System.Collections.Generic;

namespace Assets.Scripts
{
    public interface IUpdateable
    {
        void Update();
    }

    public interface ILateUpdateable
    {
        void LateUpdate();
    }

    public interface IFixedUpdateable
    {
        void FixedUpdate();
    }

    public class UpdateManager
    {
        private static UpdateManager _instance;

        private List<IUpdateable> _updateables;
        private List<ILateUpdateable> _lateUpdateables;
        private List<IFixedUpdateable> _fixedUpdateables;

        public static UpdateManager Instance
        {
            get { return _instance ?? (_instance = new UpdateManager()); }
        }

        private UpdateManager()
        {
            _updateables = new List<IUpdateable>();
            _lateUpdateables = new List<ILateUpdateable>();
            _fixedUpdateables = new List<IFixedUpdateable>();
        }

        public void Update()
        {
            // Iterate in reverse order in case an updateable unregisters itself during an update call.
            for (int i = _updateables.Count - 1; i >= 0; --i)
            {
                _updateables[i].Update();
            }
        }

        public void LateUpdate()
        {
            for (int i = _lateUpdateables.Count - 1; i >= 0; --i)
            {
                _lateUpdateables[i].LateUpdate();
            }
        }

        public void FixedUpdate()
        {
            for (int i = _fixedUpdateables.Count - 1; i >= 0; --i)
            {
                _fixedUpdateables[i].FixedUpdate();
            }
        }

        public void Destroy()
        {
            _updateables.Clear();
            _lateUpdateables.Clear();
            _fixedUpdateables.Clear();
            _updateables = null;
            _lateUpdateables = null;
            _fixedUpdateables = null;
            _instance = null;
        }

        public void AddUpdateable(IUpdateable updateable)
        {
            _updateables.Add(updateable);
        }

        public void RemoveUpdateable(IUpdateable updateable)
        {
            _updateables.Remove(updateable);
        }

        public void AddLateUpdateable(ILateUpdateable lateUpdateable)
        {
            _lateUpdateables.Add(lateUpdateable);
        }

        public void RemoveLateUpdateable(ILateUpdateable lateUpdateable)
        {
            _lateUpdateables.Remove(lateUpdateable);
        }

        public void AddFixedUpdateable(IFixedUpdateable fixedUpdateable)
        {
            _fixedUpdateables.Add(fixedUpdateable);
        }

        public void RemoveFixedUpdateable(IFixedUpdateable fixedUpdateable)
        {
            _fixedUpdateables.Remove(fixedUpdateable);
        }
    }
}
