using System.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Major.Levels {
    [CreateAssetMenu(fileName = "New Level", menuName = "Scriptable Objects/Level Asset")]
    public class LevelAsset : ScriptableObject {
        // Scenes that are referenced within this level
        [field: SerializeField]
        [field: AssetReferenceUILabelRestriction("scene")]
        public List<AssetReference> sceneReferences { get; private set; }

        // Individual assets referenced 
        [field: SerializeField]
        [field: AssetReferenceUILabelRestriction("asset")]
        public List<AssetReference> assetReferences { get; private set; }

        // Prefabs referenced within this level, 
        [field: SerializeField]
        [field: AssetReferenceUILabelRestriction("prefab")]
        public List<AssetReference> prefabReferences { get; private set; }


        public async Task<Level.ConstructData> LoadAsync(string key = "", bool logTasks = false, bool timeTasks = false)  {
            Log.Debug("Loading level " + key + "...");

            // Waits for all 
            var assetLoadResults = await WrapTask(Task.WhenAll(
                CacheAssetsFromReferences<GameObject>(assetReferences),
                CacheAssetsFromReferences<GameObject>(prefabReferences)
            ), name: "Asset & Prefab Caching", logged: logTasks, timed: timeTasks);

            var scenes = await WrapTask(
                CacheScenesFromReferences(sceneReferences),
                name: "Scene Caching", logged: logTasks, timed: timeTasks
            );

            if (scenes.Item1.Length <= 0) {
                Log.Warning("Level '" + key + "' contains no scenes");
            }

            return new() {
                levelAsset = this,
                key = key,
                sceneInstances = scenes.Item1,
                sceneAddresses = scenes.Item2,
                prefabAssets = assetLoadResults[1]
            };
        }

        private static async Task<IList<TAssetType>> CacheAssetsFromReferences<TAssetType>(
            IList<AssetReference> refs,
            Addressables.MergeMode mergeMode = Addressables.MergeMode.None
        ) {
            return await Addressables.LoadAssetsAsync<TAssetType>(await Addressables.LoadResourceLocationsAsync(keys: refs, mode: mergeMode).Task, null).Task;
        }

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

        public static async Task WrapTask(Task task, bool logged = false, string name = "Unnamed Task", Action onTaskComplete = null, bool timed = false) {
            if (timed) {
                Stopwatch timer = Stopwatch.StartNew();
                onTaskComplete += () => {
                    timer.Stop();
                    Log.Debug(name + " took " + timer.Elapsed.ToString());
                };
            }

            if (logged) {
                Log.Debug("Beginning " + name + "...");
                onTaskComplete += () => { Log.Debug("Finished " + name + "."); };
            }

            await task;
            onTaskComplete?.Invoke();
        }
        public static async Task<T> WrapTask<T>(Task<T> task, bool logged = false, string name = "Unnamed Task", Action<T> onTaskComplete = null, bool timed = false) {
            if (timed) {
                Stopwatch timer = Stopwatch.StartNew();
                onTaskComplete += (t) => {
                    timer.Stop();
                    Log.Debug(name + " took " + timer.Elapsed.ToString());
                };
            }

            if (logged) {
                Log.Debug("Beginning " + name + "...");
                onTaskComplete += (t) => { Log.Debug("Finished " + name + "."); };
            }            

            var result = await task;
            onTaskComplete?.Invoke(task.Result);
            return result;
        }
    }
}