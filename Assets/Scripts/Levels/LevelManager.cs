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
        public static bool isBusy;

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

        public static async void LoadLevel(string key, bool teleportOnLoad = true, bool homeTransition = false) {
            if (isBusy) {
                Log.Warning("[LevelManager] Busy, load level aborted: " + key);
                return;
            }

            Log.Debug("[LevelManager] Loading level: " + key);
            isBusy = true;

            if (!levelDatabase.TryGetValue(key, out var levelAsset)) {
                Log.Error("[LevelManager] Load Level failed: Key " + key + " does not exist.");
            }

            await LoadLevelAssetAsync(levelAsset, teleportOnLoad, homeTransition);
        }

        public static async void LoadLevel(LevelAsset levelAsset, bool teleportOnLoad = true, bool homeTransition = false) {
            if (isBusy) {
                Log.Warning("[LevelManager] Busy, load level aborted: " + levelAsset.name);
                return;
            }

            Log.Debug("[LevelManager] Loading level: " + levelAsset.name);
            isBusy = true;
            
            await LoadLevelAssetAsync(levelAsset, teleportOnLoad, homeTransition);
        }

        private static async Task LoadLevelAssetAsync(LevelAsset levelAsset, bool teleportOnLoad, bool homeTransition) {
            UnloadLevelCurrent();
            var key = levelAsset.name;
            var levelConstructData = await levelAsset.LoadAsync(true, true);
            levelConstructData.teleportOnLoad = teleportOnLoad;
            levelConstructData.homeTransition = homeTransition;
            var newLevel = levelConstructData.sceneInstance.Scene.GetRootGameObjects()[0].AddComponent<Level>();
            newLevel.Construct(levelConstructData);
            levelCurrent = newLevel;
            GameManager.instance.dbg_noclipEnabled = false;
            Player.instance.moveActive = true;
            Player.instance.rb.detectCollisions = true;
            Player.instance.rb.useGravity = true;
            Player.instance.rb.isKinematic = false;
            Player.instance.autoDropItemsDistance = true;
            isBusy = false;
            Log.Debug("[LevelManager] Loading level " + key + " completed.");
        }

        private static void UnloadLevelCurrent() {
            if (!levelCurrent) {
                return;
            }
            levelCurrent.Unload();
            levelCurrent = null;
        }

        public static void RestartSoft() {
            levelCurrent.GoToCheckpoint();
        }

        public static void RestartHard() {
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