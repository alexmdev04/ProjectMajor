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
        public AssetReference sceneReference { get; private set; }

        // // Individual assets referenced 
        // [field: SerializeField]
        // [field: AssetReferenceUILabelRestriction(AssetKeys.Labels.asset)]
        // public List<AssetReference> assetReferences { get; private set; }

        // Prefabs referenced within this level
        [field: SerializeField]
        [field: AssetReferenceUILabelRestriction(AssetKeys.Labels.prefab)]
        public List<AssetReference> prefabReferences { get; private set; }

        [field: SerializeField]
        public Vector3 exitPosition { get; private set; } = Vector3.zero;
        [field: SerializeField]
        public Vector3 exitRotation { get; private set; } = Vector3.zero;

        [field: SerializeField]
        public string nextLevel { get; private set; } = "dev";

        public async Task<Level.ConstructData> LoadAsync(bool logTasks = false, bool timeTasks = false) {
            return new(
                this,
                await CachePrefabsFromReferences(prefabReferences).Debug(GetType(), "Prefab Caching", logTasks, timeTasks),
                await Addressables.LoadSceneAsync(sceneReference, LoadSceneMode.Additive, true).Task.Debug(GetType(), "Loading Scene", logTasks, timeTasks)
            );
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
        private static async Task<CachedPrefabs> CachePrefabsFromReferences(
            IList<AssetReference> refs,
            Addressables.MergeMode mergeMode = Addressables.MergeMode.None
        ) {
            Dictionary<IResourceLocation, GameObject> prefabs = new();
            Dictionary<string, IResourceLocation> prefabAddresses = new();
            List<Task<GameObject>> prefabLoadHandles = new();
            foreach (var prefabResourceLocation in await Addressables.LoadResourceLocationsAsync(keys: refs, mode: mergeMode).Task) {
                var handle = Addressables.LoadAssetAsync<GameObject>(prefabResourceLocation);
                handle.Completed += (handle) => {
                    prefabs.Add(prefabResourceLocation, handle.Result);
                    prefabAddresses.Add(handle.Result.name, prefabResourceLocation);
                };
                prefabLoadHandles.Add(handle.Task);
            }
#if UNITY_WEBGL
            foreach (var handle in prefabLoadHandles) {
                await handle;
            }
#else
            await Task.WhenAll(prefabLoadHandles);
#endif
            return new(prefabs, prefabAddresses, prefabLoadHandles.Count);
        }
    }

    public struct CachedPrefabs {
        public Dictionary<IResourceLocation, GameObject> prefabs;
        public Dictionary<string, IResourceLocation> addresses;
        public int count;
        public CachedPrefabs(
            Dictionary<IResourceLocation, GameObject> prefabs,
            Dictionary<string, IResourceLocation> addresses,
            int count
        ) {
            this.prefabs = prefabs;
            this.addresses = addresses;
            this.count = count;
        }
    }
}