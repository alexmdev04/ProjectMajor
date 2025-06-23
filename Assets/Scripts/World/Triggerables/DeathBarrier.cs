using UnityEngine;

namespace Major.World {

    [RequireComponent(typeof(TriggerableObjectKiller))]
    public class DeathBarrier : TriggerArea {
        private TriggerableObjectKiller objectKiller;
        private void Awake() {
            objectKiller = GetComponent<TriggerableObjectKiller>();
        }

        protected override void OnTriggerAreaBegin(GameObject sender) {
            objectKiller.obj = sender;
            objectKiller.Begin(this, sender);
        }

        protected override void OnTriggerAreaEnd(GameObject sender) {
            objectKiller.End(this, sender);
        }
    }
}