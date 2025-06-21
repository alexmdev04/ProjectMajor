using UnityEngine;

namespace Major.World {
    public class MultiTriggerable : Triggerable {
        [SerializeField] private Triggerable[] triggerables;
        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            foreach (var triggerable in triggerables) {
                triggerable.Begin(senderTrigger, sender);
            }
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            foreach (var triggerable in triggerables) {
                triggerable.End(senderTrigger, sender);
            }
        }
    }
}