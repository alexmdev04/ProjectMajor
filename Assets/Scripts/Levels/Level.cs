using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using Unity.Logging;
using UnityEngine.SceneManagement;
using Unity.Android.Types;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Threading.Tasks;

namespace Major.Levels {
    public class Level : MonoBehaviour {
        public string key { get; private set; }
        public Dictionary<string, GameObject> prefabs { get; private set; }
        public List<GameObject> prefabInstances;
        public Dictionary<string, IResourceLocation> sceneAddresses { get; private set; }
        public Dictionary<string, SceneInstance> sceneInstances { get; private set; }
        public LevelAsset levelAsset { get; private set; }
        public bool isConstructed;

        private void Update() {
            if (!isConstructed) {
                return;
            }
        }

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
        }

        public void Unload() {
            
        }

        public async void LoadSceneAsync(string key) {
            // Check if this level has this scene
            if (!sceneAddresses.TryGetValue(key, out var sceneAddress)) {
                return;
            }

            // Check if the scene is already loaded
            if (sceneInstances.TryGetValue(key, out var sceneInstance)) {
                await sceneInstance.ActivateAsync();
                return;
            }

            var loadSceneHandle = Addressables.LoadSceneAsync(
                location: sceneAddress,
                loadMode: LoadSceneMode.Additive,
                releaseMode: SceneReleaseMode.OnlyReleaseSceneOnHandleRelease,
                activateOnLoad: true
            );
            await loadSceneHandle.Task;

            sceneInstances.Add(loadSceneHandle.Result.Scene.name, loadSceneHandle.Result);
        }

        public void UnloadScene(string key) {
            if (sceneInstances.TryGetValue(key, out var sceneInstance)) {
                UnloadScene(sceneInstance);
            }
        }

        public async void UnloadScene(SceneInstance sceneInstance) {
            await Addressables.UnloadSceneAsync(sceneInstance, UnloadSceneOptions.None).Task;
            sceneInstances.Remove(key);
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