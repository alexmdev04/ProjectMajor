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
        private bool teleportToFirstCheckpoint;

        private void Awake() {
            if (!GameManager.startupComplete) {
                return;
            }
            instance = this;
            levelDatabase = new();
        }

        private void Start() {
            GameManager.onStartupComplete += () => {
                Addressables.LoadAssetsAsync<LevelAsset>(AssetKeys.Labels.level, levelAsset => { levelDatabase.Add(levelAsset.name, levelAsset); }).WaitForCompletion();
                LoadLevel(GameManager.startupSettings.levelKey);
            };
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
                RestartHard();
            }
        }

        public static async void LoadLevel(string key, bool teleportOnLoad = true) {
            Log.Debug("[LevelManager] Loading level: " + key);

            if (!levelDatabase.TryGetValue(key, out var levelAsset)) {
                Log.Error("[LevelManager] Load Level failed: Key " + key + " does not exist.");
            }

            await LoadLevelAssetAsync(levelAsset, teleportOnLoad);
        }

        public static async void LoadLevel(LevelAsset levelAsset, bool teleportOnLoad = true) {
            Log.Debug("[LevelManager] Loading level: " + levelAsset.name);
            await LoadLevelAssetAsync(levelAsset, teleportOnLoad);
        }

        private static async Task LoadLevelAssetAsync(LevelAsset levelAsset, bool teleportOnLoad) {
            var key = levelAsset.name;
            var levelConstructData = await levelAsset.LoadAsync(true, true);
            levelConstructData.teleportOnLoad = teleportOnLoad;
            var newLevel = levelConstructData.sceneInstance.Scene.GetRootGameObjects()[0].AddComponent<Level>();
            newLevel.Construct(levelConstructData);
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

        public void RestartSoft() {
            levelCurrent.GoToCheckpoint();
        }

        public void RestartHard() {
            LoadLevel(levelCurrent.levelAsset);
        }

        private void OnDestroy() {
            if (!GameManager.startupComplete || GameManager.quitting) {
                return;
            }
            Log.Error("[GameManager] Destroyed.");
        }
    }
}