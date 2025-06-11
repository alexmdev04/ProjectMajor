using Major.Levels;
using UnityEngine;
using Unity.Logging;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;

namespace Major.Startup {
    public class Startup : MonoBehaviour {
        [field: SerializeField]
        [field: AssetReferenceUILabelRestriction(AssetKeys.Labels.level)]
        public AssetReference startupLevel;
        private async void Awake() {
            Log.Debug("[Startup] Starting up...");

            await LoadScene("Persistence");
            await LoadScene("Game", LoadSceneMode.Additive, true);
            ClearStartupChecks();
            LevelManager.instance.SetStartupLevel(startupLevel);
        }

        private AsyncOperation LoadScene(string scene, LoadSceneMode loadSceneMode = LoadSceneMode.Single, bool setActive = false) {
            Log.Debug($"[Startup] Loading '{scene}' scene...");

            var returnOperation = SceneManager.LoadSceneAsync(scene, loadSceneMode)!;

            returnOperation.completed += operation => {
                Log.Debug($"[Startup] Loading '{scene}' scene completed.");
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