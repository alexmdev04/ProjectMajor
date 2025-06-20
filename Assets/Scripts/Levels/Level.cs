using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using Unity.Logging;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Threading.Tasks;

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

        public event Action<Level> onLevelLoaded = (level) => {
            Player.instance.rb.StartupTeleport(level.levelAsset.playerStartingPosition);
            World.Kevin.instance.rb.StartupTeleport(level.levelAsset.kevinStartingPosition);
            if (!Player.instance.carriedItem) {
                World.Kevin.instance.item.SetCarriedState(false);
                World.Kevin.instance.item.OnUnslotted();
            }
        };

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