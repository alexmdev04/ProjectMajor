using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Major.Levels;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Major.World {
    public class PrefabSpawner : Triggerable {

        [SerializeField]
        [AssetReferenceUILabelRestriction(AssetKeys.Labels.prefab)]
        private AssetReferenceGameObject prefabAsset;
        private IResourceLocation prefabAddress;
        [SerializeField] private int maxInstances = 1;
        public Queue<GameObject> instances { get; private set; } = new();

        [SerializeField] private Transform spawnLocation;

        private async void Awake() {
            prefabAddress = (await Addressables.LoadResourceLocationsAsync(prefabAsset).Task)[0];
        }

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            var validInstances = new Queue<GameObject>();
            var validInstancesCount = 0;
            while (instances.TryDequeue(out var instance)) {
                if (instance) {
                    validInstances.Enqueue(instance);
                    validInstancesCount++;
                }
            }

            if (validInstancesCount >= maxInstances) { // if at max instances
                if (validInstances.TryDequeue(out var instance)) { // size check
                    Destroy(instance);
                }
            }

            instances = validInstances;

            if (LevelManager.levelCurrent.SpawnPrefab(prefabAddress, spawnLocation.position, spawnLocation.rotation, out var prefabInstance)) {
                instances.Enqueue(prefabInstance);
            }
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) { }
    }
}