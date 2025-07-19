using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Major.Levels {
    public class Manager : MonoBehaviour {
        public static Manager instance { get; private set; }
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
                Log2.Error("Load level failed: Key " + key + " does not exist.", "Levels.Manager");
                return;
            }

            if (isBusy) {
                Log2.Warning("Already loading level, load level '" + key + " ' aborted.", "Levels.Manager");
                return;
            }

            Log2.Debug("Loading level: " + key, "Levels.Manager");
            isBusy = true;
            await LoadLevelAssetAsync(levelAsset, teleportOnLoad, seamlessTeleport);
        }

        public static async void LoadLevel(LevelAsset levelAsset, bool teleportOnLoad = true, bool seamlessTeleport = false) {
            if (!levelAsset) {
                Log2.Error("Load level failed: Null level asset.", "Levels.Manager");
                return;
            }

            if (isBusy) {
                Log2.Warning("Already loading level, load level '" + levelAsset.name + " ' aborted.", "Levels.Manager");
                return;
            }

            Log2.Debug("Loading level: " + levelAsset.name, "Levels.Manager");
            isBusy = true;
            await LoadLevelAssetAsync(levelAsset, teleportOnLoad, seamlessTeleport);
        }

        private static async Task LoadLevelAssetAsync(LevelAsset levelAsset, bool teleportOnLoad, bool seamlessTeleport) {
            if (levelAsset.sceneReference == null) {
                Log2.Error("Load level failed: Level asset '" + levelAsset.name + "' has no scene reference .", "Levels.Manager");
                isBusy = false;
                return;
            }

            if (levelCurrent) {
                if (teleportOnLoad && seamlessTeleport) {
                    (Player.instance.rb.position, Player.instance.eulerAngles, Player.instance.rb.linearVelocity) =
                        ExitTransform(Player.instance.rb.position, Player.instance.eulerAngles, Player.instance.rb.linearVelocity);
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
            Log2.Debug("Loading level " + key + " completed.", "Levels.Manager");
        }

        private static (Vector3 position, Vector3 eulerAngles, Vector3 linearVelocity) ExitTransform(Vector3 position, Vector3 eulerAngles, Vector3 linearVelocity) {
            var levelAsset = levelCurrent.levelAsset;
            var exitEul = levelAsset.exitRotation;
            var entrancePos = new Vector3(4.5f, 0.0f, -17.0f);
            var posOffset = position - levelAsset.exitPosition;
            var exitEulInverse = Quaternion.Inverse(Quaternion.Euler(exitEul));

            return (
                entrancePos + (exitEulInverse * posOffset),
                eulerAngles - exitEul,
                exitEulInverse * linearVelocity
            );
        }

        private static void ExitTransform(Rigidbody rb) {
            Vector3 eul;
            (rb.position, eul, rb.linearVelocity) = ExitTransform(rb.position, rb.rotation.eulerAngles, rb.linearVelocity);
            rb.rotation = Quaternion.Euler(eul);
        }

        private static void UnloadLevelCurrent() {
            if (!levelCurrent) {
                return;
            }
            levelCurrent.Unload();
            levelCurrent = null;
        }

        public static void RestartSoft() {
            if (!levelCurrent) {
                return;
            }
            levelCurrent.GoToCheckpoint();
        }

        public static void RestartHard() {
            if (!levelCurrent) {
                return;
            }
            LoadLevel(levelCurrent.levelAsset);
        }

        public static bool NextLevel(bool teleportOnLoad = true, bool seamlessTeleport = false) {
            if (levelCurrent.levelAsset.nextLevel == string.Empty) {
                Log2.Warning("'" + levelCurrent.levelAsset.name + "' does not have a specified next level.", "Levels.Manager");
                return false;
            }
            LoadLevel(levelCurrent.levelAsset.nextLevel, teleportOnLoad, seamlessTeleport);
            return true;
        }

        private void OnDestroy() {
            if (!GameManager.startupComplete || GameManager.isQuitting) {
                return;
            }
            Log2.Error("Destroyed.", "Levels.Manager");
        }
    }
}