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
        public static GameObject exitHallway;
        [SerializeField] private GameObject exitHallwayPrefab;

        private void Awake() {
            if (!GameManager.startupComplete) {
                return;
            }
            instance = this;
            levelDatabase = new();
        }

        private void Start() {
            GameManager.onStartupComplete += async () => {
                exitHallway = Instantiate(exitHallwayPrefab);
                await Addressables.LoadAssetsAsync<LevelAsset>(AssetKeys.Labels.level, levelAsset => { levelDatabase.Add(levelAsset.name, levelAsset); }).Task;//.WaitForCompletion();
                LoadLevel(GameManager.startupSettings.levelKey, false, false);
            };
        }

        public static async void LoadLevel(string key, bool teleportOnLoad = true, bool seamlessTeleport = false) {
            if (isBusy) {
                Log.Warning("[LevelManager] Busy, load level aborted: " + key);
                return;
            }

            Log.Debug("[LevelManager] Loading level: " + key);
            isBusy = true;

            if (!levelDatabase.TryGetValue(key, out var levelAsset)) {
                Log.Error("[LevelManager] Load Level failed: Key " + key + " does not exist.");
            }

            await LoadLevelAssetAsync(levelAsset, teleportOnLoad, seamlessTeleport);
        }

        public static async void LoadLevel(LevelAsset levelAsset, bool teleportOnLoad = true, bool seamlessTeleport = false) {
            if (isBusy) {
                Log.Warning("[LevelManager] Busy, load level aborted: " + levelAsset.name);
                return;
            }

            Log.Debug("[LevelManager] Loading level: " + levelAsset.name);
            isBusy = true;
            
            await LoadLevelAssetAsync(levelAsset, teleportOnLoad, seamlessTeleport);
        }

        private static async Task LoadLevelAssetAsync(LevelAsset levelAsset, bool teleportOnLoad, bool seamlessTeleport) {
            UnloadLevelCurrent();
            var key = levelAsset.name;
            var levelConstructData = await levelAsset.LoadAsync(true, true);
            levelConstructData.teleportOnLoad = teleportOnLoad;
            levelConstructData.seamlessTeleport = seamlessTeleport;
            var newLevel = levelConstructData.sceneInstance.Scene.GetRootGameObjects()[0].AddComponent<Level>();
            newLevel.Construct(levelConstructData);
            levelCurrent = newLevel;
            exitHallway.transform.position = levelAsset.exitPosition;
            exitHallway.transform.eulerAngles = levelAsset.exitRotation;
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

        public static bool NextLevel(bool teleportOnLoad = true, bool seamlessTeleport = false) {
            if (levelCurrent.levelAsset.nextLevel == string.Empty) {
                Log.Warning("[LevelManager] '" + levelCurrent.levelAsset.name + "' does not have a specified next level.");
                return false;
            }
            LoadLevel(levelCurrent.levelAsset.nextLevel, teleportOnLoad, seamlessTeleport);
            return true;
        }

        private void OnDestroy() {
            if (!GameManager.startupComplete || GameManager.quitting) {
                return;
            }
            Log.Error("[GameManager] Destroyed.");
        }
    }
}