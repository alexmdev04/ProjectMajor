using System.Collections.Generic;
using UnityEngine;

namespace Major.World {
    public abstract class Trigger : MonoBehaviour {
        [SerializeField] private List<Triggerable> triggerables = new();
        public void Begin(GameObject sender) {
            foreach (var triggerable in triggerables) {
                triggerable.Begin(this, sender);            
            }
            OnTriggerBegin(sender);
        }
        protected abstract void OnTriggerBegin(GameObject sender);

        public void End(GameObject sender) {
            foreach (var triggerable in triggerables) {
                triggerable.End(this, sender);            
            }
            OnTriggerEnd(sender);
        }
        protected abstract void OnTriggerEnd(GameObject sender);
    }
}