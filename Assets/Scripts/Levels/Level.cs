using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using Unity.Logging;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceLocations;
using System;
using System.Diagnostics;

namespace Major.Levels {
    public class Level : MonoBehaviour {
        // The key used to load this level
        public string key { get; private set; }

        // Database of loaded prefabs that can be spawned by resource location
        public Dictionary<IResourceLocation, GameObject> prefabs { get; private set; }

        // Database of prefab addresses
        public Dictionary<string, IResourceLocation> prefabAddresses { get; private set; }

        // 
        public SceneInstance sceneInstance { get; private set; }

        // Reference to the level asset that created this level
        public LevelAsset levelAsset { get; private set; }

        // Indicates successful construction
        public bool isConstructed { get; private set; }
        public ConstructData constructData { get; private set; }

        public World.Checkpoint checkpointCurrent { get; private set; }
        private World.Checkpoint firstCheckpoint;

        public event Action<Level> onLevelUnloaded = (l) => { };

        public int restarts { get; private set; }
        public int playerDeaths { get; private set; }
        public int kevinDeaths { get; private set; }
        public Stopwatch stopwatch { get; private set; }

        // Constructs and loads a level, loads the first scene present and fills the asset databases
        public void Construct(ConstructData constructData) {
            this.constructData = constructData;
            levelAsset = constructData.levelAsset;
            key = constructData.key;
            sceneInstance = constructData.sceneInstance;
            prefabs = constructData.cachedPrefabs.prefabs;
            prefabAddresses = constructData.cachedPrefabs.addresses;
            isConstructed = true;

            var checkpoints = GetComponentsInChildren<World.Checkpoint>();
            foreach (var checkpoint in checkpoints) {
                if (checkpoint.firstCheckpointInLevel) {
                    firstCheckpoint = checkpoint;
                }
            }

            if (firstCheckpoint) {
                ActivateCheckpoint(firstCheckpoint, constructData.teleportOnLoad && !constructData.seamlessTeleport);
            }
            else {
                if (checkpoints.Length > 0) {
                    firstCheckpoint = checkpoints[0];
                    ActivateCheckpoint(firstCheckpoint, constructData.teleportOnLoad && !constructData.seamlessTeleport);
                    Log2.Warning("'" + key + "' has no first checkpoint, assumed " + checkpointCurrent.gameObject.name + " is the first.", "Level");
                }
                else {
                    Log2.Warning("'" + key + "' has no checkpoints.", "Level");
                }
            }

            if (!Player.instance.carriedItem) {
                Kevin.instance.item.SetCarriedState(false);
                Kevin.instance.item.OnUnslotted();
            }

            stopwatch.Start();
        }

        public async void Unload(bool wasLevelComplete) {
            stopwatch.Stop();
            if (wasLevelComplete) {
                if (key == GameManager.startupSettings.finalLevel) {
                    GameManager.OnGameCompleted();
                }
            }

            onLevelUnloaded(this);

            await Addressables.UnloadSceneAsync(sceneInstance, UnloadSceneOptions.None).Task;

            foreach (var prefab in prefabs.Values) {
                Addressables.Release(prefab);
            }
        }

        public bool SpawnPrefab(IResourceLocation prefabAssetLocation, Vector3 position, Quaternion rotation, out GameObject newObj) {
            if (!prefabs.TryGetValue(prefabAssetLocation, out var prefab)) {
                Log2.Error("Prefab spawn failed: '" + prefabAssetLocation.PrimaryKey + "' is not part of this level or doesn't exist.", "Level");
                newObj = null;
                return false;
            }
            newObj = Instantiate(prefab, position, rotation);
            SceneManager.MoveGameObjectToScene(newObj, sceneInstance.Scene);
            return true;
        }

        public void GoToCheckpoint() {
            GoToCheckpoint(checkpointCurrent);
        }

        public void GoToCheckpoint(World.Checkpoint checkpoint) {
            restarts++;
            ActivateCheckpoint(checkpoint, true);
        }

        public void ActivateCheckpoint(World.Checkpoint checkpoint, bool teleport) {
            checkpointCurrent = checkpoint;
            if (teleport) {
                checkpointCurrent.Teleport();
            }
        }

        public void IncrementDeaths() => playerDeaths++;

        public struct ConstructData {
            public LevelAsset levelAsset;
            public string key;
            public CachedPrefabs cachedPrefabs;
            public SceneInstance sceneInstance;
            public bool teleportOnLoad;
            public bool seamlessTeleport;

            public ConstructData(
                LevelAsset levelAsset,
                CachedPrefabs cachedPrefabs,
                SceneInstance sceneInstance,
                bool teleportOnLoad = true,
                bool seamlessTeleport = false
            ) {
                this.levelAsset = levelAsset;
                this.key = levelAsset.name;
                this.cachedPrefabs = cachedPrefabs;
                this.sceneInstance = sceneInstance;
                this.teleportOnLoad = teleportOnLoad;
                this.seamlessTeleport = seamlessTeleport;
            }
        }
    }
}