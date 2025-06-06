using UnityEngine;
using UnityEngine.SceneManagement;

namespace Major.Startup {
    public class Startup : MonoBehaviour {
        private void Awake() {
            UnityEngine.Debug.Log("[Startup] Starting up...");
            LoadScene("Persistence").completed += operation1 => {
                LoadScene("Game", LoadSceneMode.Additive, true).completed += operation2 => {
                    ClearStartupChecks();

                    if (LevelManager.instance) {
                        LevelManager.instance.LoadLevel(testScene);
                    }
                };
            };
        }

        private AsyncOperation LoadScene(string scene, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool setActive = false) {
            UnityEngine.Debug.Log($"[Startup] Loading '{scene}' scene...");

            var returnOperation = SceneManager.LoadSceneAsync(scene, loadSceneMode)!;

            returnOperation.completed += operation => {
                UnityEngine.Debug.Log($"[Startup] Loading '{scene}' scene completed.");
                if (setActive) { SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene)); }
            };

            return returnOperation;
        }

        // im not sure how slow this is, refer to StartupCheck for reason
        public static void ClearStartupChecks() {
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                GameObject[] rootObjs = scene.GetRootGameObjects();
                foreach (GameObject rootObj in rootObjs) {
                    if (rootObj.TryGetComponent(out Check startupCheck)) {
                        startupCheck.DestroySelf();
                    }
                }
            }
        }
    }
}