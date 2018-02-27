using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtility.Game.Helper {

    public static class BehaviorEnhance_ext {
        public static void RunOnMainThread(this MonoBehaviour traget, Action action, float delay = 0.0f) {
            BehaviorEnhance.RunOnMainThread(action, delay);
        }

        public static void RunAsync(this MonoBehaviour traget, Action action, float delay = 0.0f) {
            BehaviorEnhance.RunAsync(action, delay);
        }
    }
    
    public class BehaviorEnhance : MonoBehaviour {
        static BehaviorEnhance _Instance;
        public static BehaviorEnhance Instance {
            get {
                if (_Instance == null) {
                    _Instance = GameObject.FindObjectOfType<BehaviorEnhance>();
                    if (_Instance == null) {
                        _Instance = (new GameObject("BehaviorEnhance")).AddComponent<BehaviorEnhance>();
                    }
                }
                return _Instance;
            }
        }

        struct DelayedAction {
            public float time;
            public Action action;
        }

        private List<Action> actions = new List<Action>();
        private List<Action> current_actions = new List<Action>();
        private List<DelayedAction> delayed = new List<DelayedAction>();
        private List<DelayedAction> current_delayed = new List<DelayedAction>();

        private int maxThreads = 16;
        private int numThreads = 0;

        #region Awake
        private void Awake() {
            DontDestroyOnLoad(_Instance.gameObject);
        }
        #endregion
        #region 主线程中执行
        public static void RunOnMainThread(Action action, float delay) {
            BehaviorEnhance.Instance._RunOnMainThread(action, delay);
        }
        #endregion

        #region 主线程中执行
        public static void RunAsync(Action action, float delay) {
            BehaviorEnhance.Instance._RunAsync(action, delay);
        }
        #endregion

        private void _RunOnMainThread(Action action, float delay) {
            if (delay > 0.0001f) {
                lock(delayed) {
                    delayed.Add(new DelayedAction() { time = Time.time + delay, action = action});
                }
            } else {
                lock(actions) {
                    actions.Add(action);
                }
            }
        }

        private void _RunAsync(Action action, float delay) {
            while (numThreads >= maxThreads) Thread.Sleep(1);
            Interlocked.Increment(ref numThreads);
            Thread.Sleep((int)(delay * 1000));
            ThreadPool.QueueUserWorkItem(RunAction, action);
        }

        void RunAction(object action) {
            try {
                ((Action)action)();
            } catch {
                
            } finally {
                Interlocked.Decrement(ref numThreads);
            }
        }

        private void Update() {
            RunActions();
            RunDelayed();
        }

        void RunActions() {
            lock (actions) {
                current_actions.Clear();
                current_actions.AddRange(actions);
                actions.Clear();
            }
            foreach(var a in current_actions) {
                RunAction(a);
            }
            current_actions.Clear();
        }
        void RunDelayed() {
            lock (actions) {
                current_delayed.Clear();
                current_delayed.AddRange(delayed.Where(d=>d.time <= Time.time));
                var tmp = new List<DelayedAction>();
                tmp.AddRange(delayed.Where(d => d.time > Time.time));
                delayed = tmp;
            }
            foreach (var a in current_delayed) {
                RunAction(a.action);
            }
            current_delayed.Clear();
        }
    }
}
