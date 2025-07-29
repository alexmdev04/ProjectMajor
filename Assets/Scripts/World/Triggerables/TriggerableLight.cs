using UnityEngine;

namespace Major.World {
    public class TriggerableLight : Triggerable {
        public new Light light;
        private void Awake() {
            if (!light) {
                light = GetComponent<Light>();
            }
        }

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            light.enabled = true;
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            light.enabled = false;
        }
    }
}