using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

namespace Major.UI {
    public class UI : MonoBehaviour {
        public static UI instance { get; private set; }
        public static Dictionary<string, Menu> menus { get; private set; }
        public static Menu currentMenu;
        public bool fading { get; private set; }

        [HideInInspector] public HUD hud;
        public Debug.Console debugConsole;
        [SerializeField] private Popup popupPrefab;
        private Coroutine fadeCoroutine;
        [SerializeField] private Image fade;

        private void Awake() {
            instance = this;
            menus = new();
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

        public void RegisterMenu(string menuName, Menu menu, bool activate = false) {
            if (menuName == "New Menu") {
                Log2.Warning("'" + menu.name + "' menu name was not set. It has been ignored.", "UI");
                return;
            }

            menus.Add(menuName, menu);
            if (activate) {
                SetMenu(menuName);
            }
        }

        public void SetMenu(string menuName) {
            if (menuName == "Unset") {
                return;
            }

            if (menuName == "New Menu") {
                return;
            }

            if (!menus.TryGetValue(menuName, out var menu)) {
                Log2.Error("Menu '" + menuName + "' has not been registered.", "UI");
                return;
            }
            SetMenu(menu);
        }

        public void SetMenu(Menu menu) {
            if (currentMenu) { currentMenu.Deactivate(); }
            menu.Activate();
            currentMenu = menu;
        }
    }
}