using UnityEngine;

namespace Major.UI {
    public class UI : MonoBehaviour {
        public static UI instance { get; private set; }
        [HideInInspector] public HUD hud;

        private void Awake() {
            instance = this;
        }

        private void Update() {

        }

        public void ShowInteractPrompt(string text) {
            if (text == string.Empty) {
                HideInteractPrompt();
                return;
            }
            hud.interactText.text = text;
            hud.interactText.enabled = true;
        }

        public void HideInteractPrompt() {
            hud.interactText.enabled = false;
        }
    }
}