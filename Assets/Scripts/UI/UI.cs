using UnityEngine;
using UnityEngine.InputSystem;

namespace Major.UI {
    public class UI : MonoBehaviour {
        public static UI instance { get; private set; }
        [HideInInspector] public HUD hud;
        public Debug.Console debugConsole;
        [SerializeField] private Popup popupPrefab;

        private void Awake() {
            instance = this;
        }

        private void Update() {
            if (Keyboard.current.backquoteKey.wasPressedThisFrame) {
                debugConsole.gameObject.SetActive(!debugConsole.gameObject.activeSelf);
            }
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

        public void Popup(string titleText, string bodyText, Popup.ButtonConstructor[] buttons = null) {
            Instantiate(popupPrefab, gameObject.transform).Init(titleText, bodyText, buttons);
        }
    }
}