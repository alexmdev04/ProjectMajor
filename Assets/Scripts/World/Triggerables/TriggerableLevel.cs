using Major.Levels;
using UnityEngine;

namespace Major.World {
    public class TriggerableLevel : Triggerable {
        [SerializeField] private string level = "dev";
        [SerializeField] private bool teleportOnLoad = true;
        [SerializeField] private bool homeTransition = false;

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            LevelManager.LoadLevel(level, teleportOnLoad, homeTransition);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {

        }
    }
}