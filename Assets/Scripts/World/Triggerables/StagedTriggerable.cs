using System.Collections.Generic;
using UnityEngine;

namespace Major.World {
    public class StagedTriggerable : Triggerable {
        [SerializeField] private List<Triggerable> triggerables = new();

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            Trigger(true, senderTrigger, sender);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            Trigger(false, senderTrigger, sender);
        }

        private void Trigger(bool state, Trigger senderTrigger, GameObject sender) {
            var currentTriggerable = triggerables[triggeredBy.Count - 1];
            if (!currentTriggerable) {
                return;
            }
            if (state) {
                currentTriggerable.Begin(senderTrigger, sender);
            }
            else {
                currentTriggerable.End(senderTrigger, sender);
            }
        }
    }
}