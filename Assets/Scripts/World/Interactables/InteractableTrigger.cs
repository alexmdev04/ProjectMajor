using System;
using System.Collections;
using UnityEngine;

namespace Major.World {
    [RequireComponent(typeof(CustomTrigger))]
    public class InteractableTrigger : Interactable {
        [SerializeField] private float timer = 1.0f;
        private CustomTrigger customTrigger;
        private bool active;
        [SerializeField] private string prompt = "Interact";

        private void Awake() {
            customTrigger = GetComponent<CustomTrigger>();
            customTrigger.onBegin += (s) => {
                active = true;
            };
            customTrigger.onEnd += (s) => {
                active = false;
            };
        }

        public override void Interact(Player sender, Action callback = null) {
            if (!active) {
                StartCoroutine(Timer(sender.gameObject));
            }
        }

        private IEnumerator Timer(GameObject sender) {
            customTrigger.Begin(sender);

            yield return new WaitForSeconds(timer);

            customTrigger.End(sender);
        }

        public override string GetPrompt() {
            return prompt;
        }
    }
}