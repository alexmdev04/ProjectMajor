using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
            if (!levelDatabase.TryGetValue(key, out var levelAsset)) {
                Log2.Error("Load level failed: Key " + key + " does not exist.", "LevelManager");
                return;
            }

            if (isBusy) {
                Log2.Warning("Already loading level, load level '" + key + " ' aborted.", "LevelManager");
                return;
            }

            Log2.Debug("Loading level: " + key, "LevelManager");
            isBusy = true;
            await LoadLevelAssetAsync(levelAsset, teleportOnLoad, seamlessTeleport);
        }

        public static async void LoadLevel(LevelAsset levelAsset, bool teleportOnLoad = true, bool seamlessTeleport = false) {
            if (!levelAsset) {
                Log2.Error("Load level failed: Null level asset.", "LevelManager");
                return;
            }

            if (isBusy) {
                Log2.Warning("Already loading level, load level '" + levelAsset.name + " ' aborted.", "LevelManager");
                return;
            }

            Log2.Debug("Loading level: " + levelAsset.name, "LevelManager");
            isBusy = true;
            await LoadLevelAssetAsync(levelAsset, teleportOnLoad, seamlessTeleport);
        }

        private static async Task LoadLevelAssetAsync(LevelAsset levelAsset, bool teleportOnLoad, bool seamlessTeleport) {
            if (levelAsset.sceneReference == null) {
                Log2.Error("Load level failed: Level asset '" + levelAsset.name + "' has no scene reference .", "LevelManager");
                isBusy = false;
                return;
            }

            if (levelCurrent) {
                if (teleportOnLoad && seamlessTeleport) {
                    var playerTp = ExitTransform(Player.instance.rb.position, Player.instance.eulerAngles);
                    Player.instance.rb.position = playerTp.Item1;
                    Player.instance.eulerAngles = playerTp.Item2;

                    ExitTransform(Kevin.instance.rb);
                }
            }

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
            GameManager.instance.SetNoclipActive(false);
            Player.instance.moveActive = true;
            Player.instance.rb.detectCollisions = true;
            Player.instance.rb.useGravity = true;
            Player.instance.rb.isKinematic = false;
            Player.instance.autoDropItemsDistance = true;
            isBusy = false;
            Log2.Debug("Loading level " + key + " completed.", "LevelManager");
        }

        private static (Vector3, Vector3) ExitTransform(Vector3 position, Vector3 eulerAngles) {
            var levelAsset = levelCurrent.levelAsset;
            var exitEul = levelAsset.exitRotation;
            var entrancePos = new Vector3(4.5f, 0.0f, -17.0f);
            var posOffset = position - levelAsset.exitPosition;

            return (
                entrancePos + (Quaternion.Inverse(Quaternion.Euler(exitEul)) * posOffset),
                eulerAngles - exitEul
            );
        }

        private static void ExitTransform(Rigidbody rb) {
            var tp = ExitTransform(rb.position, rb.rotation.eulerAngles);
            rb.position = tp.Item1;
            rb.rotation = Quaternion.Euler(tp.Item2);
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
                Log2.Warning("'" + levelCurrent.levelAsset.name + "' does not have a specified next level.", "LevelManager");
                return false;
            }
            LoadLevel(levelCurrent.levelAsset.nextLevel, teleportOnLoad, seamlessTeleport);
            return true;
        }

        private void OnDestroy() {
            if (!GameManager.startupComplete || GameManager.isQuitting) {
                return;
            }
            Log2.Error("Destroyed.", "LevelManager");
        }
    }
}