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
        private AssetReference startupLevel;
        public void SetStartupLevel(AssetReference levelAsset) => startupLevel = levelAsset;

        private void Awake() {
            instance = this;
        }

        private async void Start() {
            // Cache all level data so it can be used to load levels later
            levelDatabase = new();
            Addressables.LoadAssetsAsync<LevelAsset>(AssetKeys.Labels.level, levelAsset => { levelDatabase.Add(levelAsset.name, levelAsset); }).WaitForCompletion();
            LoadLevel(await Addressables.LoadAssetAsync<LevelAsset>(startupLevel).Task);
        }

        private void Update() {
            // Debug Keys
            if (Keyboard.current.f3Key.wasPressedThisFrame) {
                LoadLevel(AssetKeys.Levels.home);
            }

            if (Keyboard.current.f4Key.wasPressedThisFrame) {
                LoadLevel(AssetKeys.Levels.tutorial);
            }

            if (Keyboard.current.f5Key.wasPressedThisFrame) {
                LoadLevel(levelCurrent.levelAsset);
            }
        }

        public static async void LoadLevel(string key) {
            Log.Debug("[LevelManager] Loading level: " + key);

            if (!levelDatabase.TryGetValue(key, out var levelAsset)) {
                Log.Error("[LevelManager] Load Level failed: Key " + key + " does not exist.");
            }

            await LoadLevelAssetAsync(levelAsset);
        }

        public static async void LoadLevel(LevelAsset levelAsset) {
            Log.Debug("[LevelManager] Loading level: " + levelAsset.name);
            await LoadLevelAssetAsync(levelAsset);
        }

        private static async Task LoadLevelAssetAsync(LevelAsset levelAsset) {
            var key = levelAsset.name;
            var newObj = new GameObject(key);
            var newLevel = newObj.AddComponent<Level>();

            newLevel.Construct(await levelAsset.LoadAsync(true, true));
            UnloadLevelCurrent();
            levelCurrent = newLevel;
            Log.Debug("[LevelManager] Loading level " + key + " completed.");
        }

        private static void UnloadLevelCurrent() {
            if (!levelCurrent) {
                return;
            }
            levelCurrent.Unload();
            levelCurrent = null;
        }
    }
}