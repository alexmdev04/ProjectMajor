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


        // Database of currently spawned scenes
        public Dictionary<IResourceLocation, SceneInstance> scenes { get; private set; }

        // Database of scene addresses for loading during gameplay
        public Dictionary<string, IResourceLocation> sceneAddresses { get; private set; }

        //
        public List<SceneInstance> loadedScenes { get; private set; }

        // Reference to the level asset that created this level
        public LevelAsset levelAsset { get; private set; }

        // Indicates successful construction
        public bool isConstructed;

        public event Action<Level> onLevelLoaded = (level) => {
            level.StartupTeleport();
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
            sceneAddresses = constructData.cachedScenes.sceneAddresses;
            scenes = constructData.cachedScenes.scenes;

            foreach (var sceneInstance in constructData.cachedScenes.scenes.Values) {
                sceneInstance.ActivateAsync();
                DespawnSceneAsync(sceneInstance);
            }

            prefabs = constructData.cachedPrefabs.prefabs;
            prefabAddresses = constructData.cachedPrefabs.addresses;
            isConstructed = true;
            onLevelLoaded(this);
        }

        public void Unload() {
            foreach (var prefab in prefabs.Values) { // todo  prefabs isnt instances
                Addressables.Release(prefab);
            }

            foreach (var scene in scenes.Values) {
                Addressables.UnloadSceneAsync(scene, UnloadSceneOptions.None).Completed += handle => {
                    if (handle.Status == AsyncOperationStatus.Failed) {
                        Log.Error("[Level] Unload Scene failed.");
                        Log.Error(handle.OperationException.ToString());
                    }
                };
            }

            scenes.Clear();
            sceneAddresses.Clear();
        }

        // 
        public async void SpawnSceneAsync(string key) {
            // Check if this level has this scene

            if (!TryGetSceneInstance(key, out var sceneInstance)) {
                Log.Error("[Level] Load Scene failed.");
                return;
            }

            await SpawnSceneAsync(sceneInstance);
        }

        // 
        public async Task SpawnSceneAsync(SceneInstance sceneInstance) {
            // Loads the scene additively and activates it
            await SceneManager.LoadSceneAsync(sceneInstance.Scene.name);
        }

        // 
        public async void DespawnSceneAsync(string key) {
            if (!TryGetSceneInstance(key, out var sceneInstance)) {
                Log.Error("[Level] Unload Scene failed.");
                return;
            }

            await DespawnSceneAsync(sceneInstance);
        }

        public async Task DespawnSceneAsync(SceneInstance sceneInstance) {
            if (!sceneInstance.Scene.isLoaded) {
                return;
            }
            foreach (var root in sceneInstance.Scene.GetRootGameObjects()) {
                
            }
            await SceneManager.UnloadSceneAsync(sceneInstance.Scene);
        }

        public void StartupTeleport() {
            Player.instance.rb.position = levelAsset.startingPositionAsOffset ?
                Player.instance.rb.position + levelAsset.startingPosition :
                levelAsset.startingPosition;
        }

        public bool TryGetSceneInstance(string key, out SceneInstance sceneInstance) {
            if (!sceneAddresses.TryGetValue(key, out var sceneAddress)) {
                Log.Error("[Level] '" + key + "' is not a valid scene address key.");
                sceneInstance = default;
                return false;
            }

            if (!scenes.TryGetValue(sceneAddress, out sceneInstance)) {
                Log.Error("[Level] '" + sceneAddress.PrimaryKey + "' is not a valid scene address.");
                return false;
            }

            return true;
        }
        public struct ConstructData {
            public LevelAsset levelAsset;
            public string key;
            public CachedPrefabs cachedPrefabs;
            public CachedScenes cachedScenes;

            public ConstructData(
                LevelAsset levelAsset,
                CachedPrefabs cachedPrefabs,
                CachedScenes cachedScenes
            ) {
                this.levelAsset = levelAsset;
                this.key = levelAsset.name;
                this.cachedPrefabs = cachedPrefabs;
                if (cachedScenes.count <= 0) {
                    Log.Warning("Level '" + key + "' contains no scenes");
                }
                this.cachedScenes = cachedScenes;
            }
        }
    }
}