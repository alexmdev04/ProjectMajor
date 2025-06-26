using System;
using UnityEngine;

namespace Major.World {
    public class ChildInteractable : Interactable {
        public Interactable parent;
        private void Awake() {
            if (parent == null) {
                parent = transform.parent.GetComponent<Interactable>();
            }
        }
        public override void Interact(Player sender, Action callback = null) {
            parent.Interact(sender, callback);
        }

        public override string GetPrompt() {
            return parent.GetPrompt();
        }

    }
}