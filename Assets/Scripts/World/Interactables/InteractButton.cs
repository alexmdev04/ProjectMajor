using System;
using System.Collections;
using UnityEngine;

namespace Major.World {
    [RequireComponent(typeof(CustomTrigger))]
    public class InteractButton : Interactable {
        [SerializeField] private float buttonTimer = 1.0f;
        private CustomTrigger customTrigger;
        private bool pressed;

        private void Awake() {
            customTrigger = GetComponent<CustomTrigger>();
            customTrigger.onBegin += (s) => {
                pressed = true;
            };
            customTrigger.onEnd += (s) => {
                pressed = false;
            };
        }

        public override void Interact(Player sender, Action callback = null) {
            if (!pressed) {
                StartCoroutine(ButtonTimer(sender.gameObject));
            }
        }

        private IEnumerator ButtonTimer(GameObject sender) {
            customTrigger.Begin(sender);

            yield return new WaitForSeconds(buttonTimer);

            customTrigger.End(sender);
        }
    }
}