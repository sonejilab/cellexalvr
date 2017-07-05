//----------------------------------------------
// SQLiter
// Copyright © 2014 OuijaPaw Games LLC
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using Action = System.Action;

namespace SQLiter
{
    /// <summary>
    /// The LoomManager will moves things to another thread to process, then bring them back to main thread.
    /// Very useful for SQLite so you can send a save/whatever to another thread without blocking the main
    /// Unity thread.  This is a modified version of the Unity Gems Loom.
    /// </summary>
    public class LoomManager : MonoBehaviour
    {
        public interface ILoom
        {
            void QueueOnMainThread(Action action);
        }

        private static NullLoom _nullLoom = new NullLoom();
        private static LoomDispatcher _loom;

        public static ILoom Loom
        {
            get
            {
                if (_loom != null)
                {
                    return _loom as ILoom;
                }
                return _nullLoom as ILoom;
            }
        }

        void Awake()
        {
            _loom = new LoomDispatcher();
        }

        void OnDestroy()
        {
            _loom = null;
        }

        void Update()
        {
            if (Application.isPlaying)
            {
                _loom.Update();
            }
        }

        private class NullLoom : ILoom
        {
            public void QueueOnMainThread(Action action) { }
        }

        private class LoomDispatcher : ILoom
        {
            private readonly List<Action> actions = new List<Action>();
            public void QueueOnMainThread(Action action)
            {
                lock (actions)
                {
                    actions.Add(action);
                }
            }
            public void Update()
            {
                // Pop the actions from the synchronized list
                Action[] actionsToRun = null;
                lock (actions)
                {
                    actionsToRun = actions.ToArray();
                    actions.Clear();
                }
                // Run each action
                foreach (Action action in actionsToRun)
                {
                    action();
                }
            }
        }
    }

}
