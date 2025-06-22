using UnityEngine;
using Unity.Logging;

namespace Major.World {
    [RequireComponent(typeof(CustomTrigger))]
    public class TriggerableTrigger : Triggerable {
        private CustomTrigger trigger;
        private void Awake() {
            trigger = GetComponent<CustomTrigger>();
        }
        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            trigger.Begin(sender);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            trigger.End(sender);
        }
    }
}