using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Major.UI {
    public class UI : MonoBehaviour {
        public static UI instance { get; private set; }
        public bool fading { get; private set; }

        [HideInInspector] public HUD hud;
        public Debug.Console debugConsole;
        [SerializeField] private Popup popupPrefab;
        private Coroutine fadeCoroutine;
        [SerializeField] private Image fade;

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

        /// <param name="fadeOut">To Opaque</param>
        /// <param name="fadeIn">To Transparent</param>
        public void Fade(float fadeOut = 0.0f, float fadeIn = 0.0f) {
            if (fading) {
                StopCoroutine(fadeCoroutine);
            }
            StartCoroutine(FadeCoroutine(fadeOut, fadeIn));
        }

        private IEnumerator FadeCoroutine(float fadeOut, float fadeIn) {
            fading = true;

            if (fadeOut > 0.0f) {
                while (fade.color.a < 1.0f) {
                    fade.color = fade.color.AddAlpha(Time.deltaTime / fadeOut);
                    yield return new WaitForEndOfFrame();
                }
            }
            fade.color = fade.color.WithAlpha(1.0f);

            if (fadeIn > 0.0f) {
                while (fade.color.a > 0.0f) {
                    fade.color = fade.color.SubtractAlpha(Time.deltaTime / fadeIn);
                    yield return new WaitForEndOfFrame();
                }
            }
            fade.color = fade.color.WithAlpha(0.0f);

            fading = false;
        }
    }
}