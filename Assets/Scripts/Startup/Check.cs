using UnityEngine;

// shit workaround for checking if the game was started with the startup scene loaded
// it might be a bit overkill but it works

namespace Major.Startup {
    public class Check : MonoBehaviour {
        [SerializeField] private string sceneName;
        [SerializeField] private bool shouldBeLoaded = true;
        private bool logged;

        // not sure of the reliability of the execution order, this is destroyed during an Awake AsyncOperation
        private void LateUpdate() {
            if (logged) { return; }
            Log();
            logged = true;
        }

        public void DestroySelf() {
            if (!shouldBeLoaded) {
                return;
            }
            Destroy(gameObject);
        }

        private void Log() {
            UnityEngine.Debug.LogError($"[{sceneName}] Startup Error. Make sure the startup scene is open.");
        }
    }
}