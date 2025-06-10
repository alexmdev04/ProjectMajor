using UnityEngine;

namespace Major.World {
    public abstract class Trigger : MonoBehaviour {
        [SerializeField] private Triggerable triggerable;
        public void Begin(GameObject sender) {
            triggerable.Begin(this, sender);
            OnTriggerBegin(sender);
        }
        protected abstract void OnTriggerBegin(GameObject sender);

        public void End(GameObject sender) {
            triggerable.End(this, sender);
            OnTriggerEnd(sender);
        }
        protected abstract void OnTriggerEnd(GameObject sender);
    }
}