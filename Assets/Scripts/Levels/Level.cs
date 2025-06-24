using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using Unity.Logging;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceLocations;
using System;

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
        public bool isConstructed;

        public World.Checkpoint checkpointCurrent { get; private set; }

        public event Action<Level> onLevelLoaded = (level) => {
            level.OnLevelLoaded();
        };

        public void OnLevelLoaded() {
            var checkpoints = GetComponentsInChildren<World.Checkpoint>();
            foreach (var checkpoint in checkpoints) {
                if (checkpoint.firstCheckpointInLevel) {
                    checkpointCurrent = checkpoint;
                }
            }

            if (checkpointCurrent) {
                GoToCheckpoint();
            }
            else {
                if (checkpoints.Length > 0) {
                    checkpointCurrent = checkpoints[0];
                    GoToCheckpoint();
                    Log.Warning("[Level] '" + key + "' has no first checkpoint, assumed " + checkpointCurrent.gameObject.name + " is the first.");
                }
                else {
                    Log.Warning("[Level] '" + key + "' has no checkpoints.");
                }
            }

            if (!Player.instance.carriedItem) {
                Kevin.instance.item.SetCarriedState(false);
                Kevin.instance.item.OnUnslotted();
            }
        }

        private void Update() {
            if (!isConstructed) {
                return;
            }
        }

        // Constructs and loads a level, loads the first scene present and fills the asset databases
        public void Construct(ConstructData constructData) {
            levelAsset = constructData.levelAsset;
            key = constructData.key;
            sceneInstance = constructData.sceneInstance;
            prefabs = constructData.cachedPrefabs.prefabs;
            prefabAddresses = constructData.cachedPrefabs.addresses;
            isConstructed = true;
            onLevelLoaded(this);
        }

        public async void Unload() {
            await Addressables.UnloadSceneAsync(sceneInstance, UnloadSceneOptions.None).Task;

            foreach (var prefab in prefabs.Values) {
                Addressables.Release(prefab);
            }
        }

        public bool SpawnPrefab(IResourceLocation prefabAssetLocation, Vector3 position, Quaternion rotation, out GameObject newObj) {
            if (!prefabs.TryGetValue(prefabAssetLocation, out var prefab)) {
                Log.Error("[Level] Prefab spawn failed: '" + prefabAssetLocation.PrimaryKey + "' is not part of this level or doesn't exist.");
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
            ActivateCheckpoint(checkpoint, true);
        }

        public void ActivateCheckpoint(World.Checkpoint checkpoint, bool teleport) {
            checkpointCurrent = checkpoint;
            if (teleport) {
                checkpointCurrent.Teleport();
            }
        }

        public struct ConstructData {
            public LevelAsset levelAsset;
            public string key;
            public CachedPrefabs cachedPrefabs;
            public SceneInstance sceneInstance;

            public ConstructData(
                LevelAsset levelAsset,
                CachedPrefabs cachedPrefabs,
                SceneInstance sceneInstance
            ) {
                this.levelAsset = levelAsset;
                this.key = levelAsset.name;
                this.cachedPrefabs = cachedPrefabs;
                this.sceneInstance = sceneInstance;
            }
        }
    }
}