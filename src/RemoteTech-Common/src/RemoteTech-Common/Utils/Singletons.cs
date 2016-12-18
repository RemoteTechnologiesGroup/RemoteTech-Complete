using UnityEngine;

namespace RemoteTech.Common.Utils
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public abstract class SimpleMonoBehaviorSingleton<T> : MonoBehaviour
        where T : class
    {
        public static T Instance { get; private set; }

        protected virtual T GetInstance()
        {
            return Instance;
        }

        public virtual void Awake()
        {
            if (Instance != null && Instance != this as T)
                Destroy(gameObject);
            else
                Instance = this as T;
        }
    }
}