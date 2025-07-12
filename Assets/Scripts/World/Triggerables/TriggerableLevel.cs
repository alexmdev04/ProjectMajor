using Major.Levels;
using UnityEngine;

namespace Major.World {
    public class TriggerableLevel : Triggerable {
        public string level = "dev";
        public bool teleportOnLoad = true;
        public bool seamlessTeleport = false;
        public bool nextLevel = false;

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            if (nextLevel) {
                LevelManager.NextLevel(teleportOnLoad, seamlessTeleport);
                return;
            }
            LevelManager.LoadLevel(level, teleportOnLoad, seamlessTeleport);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {

        }
    }
}