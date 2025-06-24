using Major.Levels;
using UnityEngine;

namespace Major.World {
    public class TriggerableLevel : Triggerable {
        [SerializeField] private string level = "dev";
        [SerializeField] private bool teleportOnLoad = true;

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            LevelManager.LoadLevel(level, teleportOnLoad);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {

        }
    }
}