using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Major.Levels {
    [CreateAssetMenu(fileName = "New Level", menuName = "Scriptable Objects/Level Asset")]
    public class LevelAsset : ScriptableObject {
        // Scenes that are referenced within this level
        [field: SerializeField]
        [field: AssetReferenceUILabelRestriction(AssetKeys.Labels.scene)]
        public List<AssetReference> sceneReferences { get; private set; }

        // Individual assets referenced 
        [field: SerializeField]
        [field: AssetReferenceUILabelRestriction(AssetKeys.Labels.asset)]
        public List<AssetReference> assetReferences { get; private set; }

        // Prefabs referenced within this level
        [field: SerializeField]
        [field: AssetReferenceUILabelRestriction(AssetKeys.Labels.prefab)]
        public List<AssetReference> prefabReferences { get; private set; }

        public async Task<Level.ConstructData> LoadAsync(string key = "", bool logTasks = false, bool timeTasks = false) {
            // Loads all prefabs in this level
            var prefabs = await CacheAssetsFromReferences<GameObject>(prefabReferences).Debug(GetType(), "Prefab Caching", logTasks, timeTasks);

            // Loads the scenes in this level, it is done after to prevent the game needing to load assets within the scene as well
            var scenes = await CacheScenesFromReferences(sceneReferences).Debug(GetType(), "Scene Caching", logTasks, timeTasks);

            // Typically levels will have at least 1 scene
            if (scenes.Item1.Length <= 0) {
                Log.Warning("Level '" + key + "' contains no scenes");
            }

            // Returns the construction data for levels
            return new() {
                levelAsset = this,
                key = key,
                sceneInstances = scenes.Item1,
                sceneAddresses = scenes.Item2,
                prefabAssets = new List<GameObject>()// assetLoadResults[1]
            };
        }

        // Loads a list of assets from an AssetReference list
        // Gathers their resource location data first then loads the assets using the data
        private static async Task<IList<TAssetType>> CacheAssetsFromReferences<TAssetType>(
            IList<AssetReference> refs,
            Addressables.MergeMode mergeMode = Addressables.MergeMode.None
        ) {
            return await Addressables.LoadAssetsAsync<TAssetType>(await Addressables.LoadResourceLocationsAsync(keys: refs, mode: mergeMode).Task, null).Task;
        }

        // Similar to CacheAssetsFromReferences but for scenes
        // Loads all scenes in the level and stores a database of their names to addresses to be used later
        private static async Task<(SceneInstance[], Dictionary<string, IResourceLocation>)> CacheScenesFromReferences(
            IList<AssetReference> refs,
            Addressables.MergeMode mergeMode = Addressables.MergeMode.None
        ) {
            Dictionary<string, IResourceLocation> sceneAddresses = new();
            List<Task<SceneInstance>> sceneLoadHandles = new();
            foreach (var sceneResourceLocation in await Addressables.LoadResourceLocationsAsync(keys: refs, mode: mergeMode).Task) {
                var handle = Addressables.LoadSceneAsync(
                    location: sceneResourceLocation,
                    loadMode: LoadSceneMode.Additive,
                    releaseMode: SceneReleaseMode.OnlyReleaseSceneOnHandleRelease,
                    activateOnLoad: false
                );
                handle.Completed += (handle) => { sceneAddresses.Add(handle.Result.Scene.name, sceneResourceLocation); };
                sceneLoadHandles.Add(handle.Task);
            }

            var sceneInstances = await Task.WhenAll(sceneLoadHandles);
            return (sceneInstances, sceneAddresses);
        }
    }
}