using System;
using System.Collections;
using UnityEngine;

namespace Major.World {
    [RequireComponent(typeof(CustomTrigger))]
    public class InteractableTrigger : Interactable {
        [SerializeField] private float timer = 1.0f;
        [SerializeField] private bool toggleable = false;
        private CustomTrigger customTrigger;
        private bool active;
        [SerializeField] private string prompt = "Interact";
        private Coroutine timerCoroutine;

        private void Awake() {
            customTrigger = GetComponent<CustomTrigger>();
        }

        public override void Interact(Player sender, Action callback = null) {
            if (active) {
                if (toggleable) {
                    End(sender.gameObject);
                }
                else {
                    ResetTimer(sender.gameObject);
                }
            }
            else {
                Begin(sender.gameObject);
            }
        }

        private void Begin(GameObject sender) {
            customTrigger.Begin(sender);
            active = true;
            if (timer != 0.0f) {
                timerCoroutine = StartCoroutine(Timer(sender));
            }

            customTrigger.Begin(sender);
            active = true;
        }

        private void ResetTimer(GameObject sender) {
            if (timer != 0.0f) {
                StopCoroutine(timerCoroutine);
                timerCoroutine = StartCoroutine(Timer(sender));
            }
        } 

        private void End(GameObject sender) {
            customTrigger.End(sender);
            active = false;
        }

        private IEnumerator Timer(GameObject sender) {
            yield return new WaitForSeconds(timer);

            End(sender);
        }

        public override string GetPrompt() {
            return prompt;
        }
    }
}