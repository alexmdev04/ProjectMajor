using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using Unity.Logging;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

namespace Major.Levels {
    public class Level : MonoBehaviour {
        // The key used to load this level
        public string key { get; private set; }

        // Database of loaded prefabs that can be spawned by name
        public Dictionary<string, GameObject> prefabs { get; private set; } = new();

        // List of all spawned prefabs
        public List<GameObject> prefabInstances = new();

        // Database of scene addresses for loading during gameplay
        public Dictionary<string, IResourceLocation> sceneAddresses { get; private set; } = new();

        // Database of currently spawned scenes
        public Dictionary<string, SceneInstance> sceneInstances { get; private set; } = new();

        // Reference to the level asset that created this level
        public LevelAsset levelAsset { get; private set; }

        // Indicates successful construction
        public bool isConstructed;

        public event Action<Level> onLevelLoaded = (level) => { Player.instance.transform.position = level.levelAsset.startingPosition; };

        private void Update() {
            if (!isConstructed) {
                return;
            }
        }

        // Constructs and loads a level, loads the first scene present and fills the asset databases
        public void Construct(ConstructData constructData) {
            levelAsset = constructData.levelAsset;
            key = constructData.key;
            sceneAddresses = constructData.sceneAddresses;
            sceneInstances = new();

            for (int i = 0; i < constructData.sceneInstances.Count; i++) {
                var sceneInstance = constructData.sceneInstances[i];
                if (i == 0) {
                    sceneInstance.ActivateAsync();
                    sceneInstances.Add(sceneInstance.Scene.name, sceneInstance);
                    continue;
                }
                Addressables.UnloadSceneAsync(sceneInstance);
            }

            prefabs = new();
            foreach (var prefab in constructData.prefabAssets) {
                prefabs.Add(prefab.name, prefab);
                //Log.Debug("Registered prefab to " + key + " level data: " + prefab.name);
            }

            isConstructed = true;
            onLevelLoaded(this);
        }

        public void Unload() {
            foreach (var prefab in prefabInstances) {
                Addressables.Release(prefab);
            }

            foreach (var sceneInstance in sceneInstances.Keys) {
                UnloadScene(sceneInstance);
            }

            sceneInstances.Clear();
            sceneAddresses.Clear();
        }

        // Loads a scene into the world via its key
        // Only pre-loaded scenes will work
        public async void LoadSceneAsync(string key) {
            // Check if this level has this scene
            if (!sceneAddresses.TryGetValue(key, out var sceneAddress)) {
                Log.Error("[Level] Load Scene failed: scene key '" + key + "' not found in this level.");
                return;
            }

            // Check if the scene is already loaded
            if (sceneInstances.TryGetValue(key, out var sceneInstance)) {
                await sceneInstance.ActivateAsync();
                Log.Debug("[Level] Attempted to load a scene that is already loaded");
                return;
            }

            // Loads the scene additively and activates it
            var loadSceneHandle = Addressables.LoadSceneAsync(
                location: sceneAddress,
                loadMode: LoadSceneMode.Additive,
                releaseMode: SceneReleaseMode.OnlyReleaseSceneOnHandleRelease,
                activateOnLoad: true
            );
            await loadSceneHandle.Task;

            sceneInstances.Add(loadSceneHandle.Result.Scene.name, loadSceneHandle.Result);
        }

        // Unloads a scene via its key, however it will remain cached (by design)
        public void UnloadScene(string key) {
            if (!sceneInstances.TryGetValue(key, out var sceneInstance)) {
                Log.Error("[Level] Unload Scene failed: scene key '" + key + "' not found in scene instances.");
                return;
            }

            UnloadSceneInstance(key, sceneInstance);
        }

        public void UnloadSceneInstance(string key, SceneInstance sceneInstance) {
            if (!sceneInstance.Scene.isLoaded) {
                return;
            }

            Addressables.UnloadSceneAsync(sceneInstance, UnloadSceneOptions.None).Completed += handle => {
                if (handle.Status == AsyncOperationStatus.Failed) {
                    Log.Error("[Level] Unload Scene failed.");
                    Log.Error(handle.OperationException.ToString());
                }
                else {
                    sceneInstances.Remove(key);
                }
            };
        }

        public struct ConstructData {
            public LevelAsset levelAsset;
            public string key;
            public IList<SceneInstance> sceneInstances;
            public Dictionary<string, IResourceLocation> sceneAddresses;
            public IList<GameObject> prefabAssets;
        }
    }
}