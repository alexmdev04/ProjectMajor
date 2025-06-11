using Major.Levels;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Major.World {
    public class LevelLoader : Triggerable {
        [field: AssetReferenceUILabelRestriction(AssetKeys.Labels.level)]
        [SerializeField] private AssetReference level;
        private LevelAsset levelAsset;

        private async void Start() {
            if (level == null) {
                return;
            }
            levelAsset = await Addressables.LoadAssetAsync<LevelAsset>(level).Task;
        }

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            LevelManager.LoadLevel(levelAsset);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            
        }
    }
}