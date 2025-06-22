using System.Collections.Generic;
using UnityEngine;

namespace Major.World {
    public abstract class Triggerable : MonoBehaviour {
        private bool triggered => triggeredBy.Count >= triggersRequired;
        [SerializeField] private uint triggersRequired = 1;
        public HashSet<Trigger> triggeredBy = new HashSet<Trigger>();
        public void Begin(Trigger senderTrigger, GameObject sender) {
            if (triggeredBy.Contains(senderTrigger)) {
                return;
            }

            triggeredBy.Add(senderTrigger);

            if (triggered) {
                OnTriggered(senderTrigger, sender);
            }
        }
        protected abstract void OnTriggered(Trigger senderTrigger, GameObject sender);
        public void End(Trigger senderTrigger, GameObject sender) {
            if (!triggeredBy.Contains(senderTrigger)) {
                return;
            }

            if (triggeredBy.Count == triggersRequired) {
                OnUntriggered(senderTrigger, sender);
            }

            triggeredBy.Remove(senderTrigger);
        }
        protected abstract void OnUntriggered(Trigger senderTrigger, GameObject sender);
    }
}