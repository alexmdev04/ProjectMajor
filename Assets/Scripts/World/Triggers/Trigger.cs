using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Logging;
using UnityEngine;

namespace Major.World {
    public abstract class Trigger : MonoBehaviour {
        [SerializeField] private List<Triggerable> triggerables = new();
#if UNITY_EDITOR
        [SerializeField] private bool ignoreWarnings;
#endif

        private void Awake() {
            if (TryGetComponent<DestructionProtection>(out var dp)) {
                dp.onDestroyed += End;
            }
        }

        private void Start() {
#if UNITY_EDITOR
            if (ignoreWarnings) {
                return;
            }
            var logName = transform.parent ? transform.parent.name + "." + name : name;
            string logCreate = "'" + logName + "' has issues;";
            StringBuilder log = new(logCreate);
            if (triggerables.Count == 0) {
                if (TryGetComponent(out Triggerable selfTriggerable)) {
                    log.Append("\n - Self triggerable found, please set this in the editor");
                    triggerables.Add(selfTriggerable);
                }
                else if (this is ItemSlot itemSlot) {
                    if (!itemSlot.passthrough) {
                        log.Append("\n - No triggerables set in scene");
                    }
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
                Log2.Warning(output, "Trigger");
            }
#endif
        }

        public void Begin(GameObject sender) {
            foreach (var triggerable in triggerables) {
                triggerable.Begin(this, sender);
            }
            OnTriggerBegin(sender);
        }

        protected virtual void OnTriggerBegin(GameObject sender) {

        }

        public void End(GameObject sender) {
            foreach (var triggerable in triggerables) {
                triggerable.End(this, sender);
            }
            OnTriggerEnd(sender);
        }

        protected virtual void OnTriggerEnd(GameObject sender) {

        }
    }
}