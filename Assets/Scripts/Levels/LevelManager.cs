using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using Unity.Logging;
using UnityEngine.SceneManagement;

namespace Major.Levels {
    public class LevelManager : MonoBehaviour {
        public static LevelManager instance { get; private set; }
        public static Level levelCurrent { get; private set; }
        public static Dictionary<string, LevelAsset> levelDatabase { get; private set; }

        private void Awake() {
            instance = this;
        }

        private void Start() {
            // Cache all level data so it can be used to load levels later
            levelDatabase = new();
            Addressables.LoadAssetsAsync<LevelAsset>(AssetKeys.Labels.level, levelAsset => { levelDatabase.Add(levelAsset.name, levelAsset); }).WaitForCompletion();
        }

        private void Update() {
            // Debug Keys
            if (Keyboard.current.f1Key.wasPressedThisFrame) {
                LoadLevel(AssetKeys.Levels.intro);
            }

            if (Keyboard.current.f2Key.wasPressedThisFrame) {
                levelCurrent.UnloadScene(AssetKeys.Scenes.intro.home);
            }

            if (Keyboard.current.f3Key.wasPressedThisFrame) {
                levelCurrent.LoadSceneAsync(AssetKeys.Scenes.intro.home);
            }
        }

        public async void LoadLevel(string key) {
            Log.Debug("[LevelManager] Loading level: " + key);

            // Loads a new level via its name/key and attaches it to an empty game object
            var levelAssetLoadHandle = Addressables.LoadAssetAsync<LevelAsset>(key);
            var levelAsset = await levelAssetLoadHandle.Task;
            if (levelAssetLoadHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Failed) {
                Log.Error("[LevelManager] Load Level failed");
                Log.Error(levelAssetLoadHandle.OperationException.ToString());
                return;
            }
            var newObj = new GameObject(key);
            var newLevel = newObj.AddComponent<Level>();
            newLevel.Construct(await levelAsset.LoadAsync(key, true, true));
            levelCurrent = newLevel;
            Log.Debug("[LevelManager] Loading level " + key + " completed.");
        }
    }
}