using Major.Levels;
using UnityEngine;
using Unity.Logging;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System;

namespace Major.Startup {
    public class Startup : MonoBehaviour {
        [SerializeField] private Settings startupSettings;

        private void Awake() {
#if UNITY_EDITOR  
            Log.Debug("[Startup] Verifying start up...");
            if (SceneManager.sceneCount != 1 || SceneManager.GetSceneAt(0).buildIndex != 0) {
                SceneManager.LoadScene(0);
                return;
            }
#endif
            Log.Debug("[Startup] Starting up...");
            SceneManager.LoadScene("Persistence", LoadSceneMode.Additive);
            SceneManager.LoadScene("Game", LoadSceneMode.Additive);
            SceneManager.UnloadSceneAsync(gameObject.scene);
            GameManager.OnStartupComplete(startupSettings);
        }
    }
}