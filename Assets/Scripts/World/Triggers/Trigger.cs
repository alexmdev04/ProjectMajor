using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Logging;
using UnityEngine;

namespace Major.World {
    public abstract class Trigger : MonoBehaviour {
        [SerializeField] private List<Triggerable> triggerables = new();

        private void Start() {
#if UNITY_EDITOR
            var logName = transform.parent != transform ? transform.parent.name + "." + name : name;
            string logCreate = "[Trigger] '" + logName + "' has issues;";
            StringBuilder log = new(logCreate);
            if (triggerables.Count == 0) {
                if (TryGetComponent(out Triggerable selfTriggerable)) {
                    log.Append("\n - Self triggerable found, please set this in the editor");
                    triggerables.Add(selfTriggerable);
                }
                else {
                    log.Append("\n - No triggerables set in scene");
                }
            }
            else {
                bool hasNull = false;
                foreach (var triggerable in triggerables) {
                    if (!triggerable && !hasNull) {
                        hasNull = true;
                    }
                }
                if (hasNull) {
                    log.Append("\n - Has null triggerables");
                    triggerables = triggerables.Where(t => t).ToList();
                }
            }

            var output = log.ToString();
            if (logCreate != output) {
                Log.Warning(output);
            }
#endif
        }

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