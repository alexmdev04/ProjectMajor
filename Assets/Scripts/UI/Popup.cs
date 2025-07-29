using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Major.UI {
    public class Popup : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI body;
        [SerializeField] private Button cloneableButton;
        [SerializeField] private RectTransform buttonsParent;
        private Selectable returnSelection;
        private RectTransform rectTransform;
        public static bool resultsVisible { get; private set; }

        private void Awake() {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable() {
            if (EventSystem.current.currentSelectedGameObject) {
                returnSelection = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
            }
        }

        public void Init(string titleText, string bodyText, ButtonConstructor[] buttons = null, int buttonFocus = 0) {
            title.text = titleText;
            body.text = bodyText;

            if (buttons != null) {
                Button prevButton = null;

                for (int i = 0; i < buttons.Length; i++) {
                    ButtonConstructor button = buttons[i];
                    var newButton = Instantiate(cloneableButton, buttonsParent);
                    if (prevButton) {
                        prevButton.navigation = new() {
                            mode = Navigation.Mode.Explicit,
                            selectOnLeft = prevButton.navigation.selectOnLeft,
                            selectOnRight = newButton,
                        };
                    }
                    newButton.navigation = new() {
                        mode = Navigation.Mode.Explicit,
                        selectOnLeft = prevButton,
                    };

                    if (i == buttonFocus) {
                        newButton.Select();
                    }

                    newButton.onClick.RemoveAllListeners();
                    newButton.onClick.AddListener(() => button.onClick(this));
                    var newButtonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
                    newButtonText.text = button.text;
                    newButtonText.color = button.textColor;
                    newButton.GetComponent<Image>().color = button.bgColor;
                    prevButton = newButton;
                }
                Destroy(cloneableButton.gameObject);
            }
            else {
                cloneableButton.onClick.AddListener(() => { Destroy(); });
            }
            rectTransform.ForceUpdateRectTransforms();
        }

        public void Destroy() {
            if (returnSelection) {
                returnSelection.Select();
            }
            Destroy(gameObject);
        }

        private void OnDestroy() {
            Cursor.visible = GameManager.isCursorVisible;
            Cursor.lockState = GameManager.isCursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        public struct ButtonConstructor {
            public string text;
            public UnityAction<Popup> onClick;
            public Color textColor;
            public Color bgColor;

            public ButtonConstructor(string text, UnityAction<Popup> onClick, Color? textColor = null, Color? bgColor = null) {
                this.text = text;
                this.onClick = onClick;
                this.textColor = textColor ?? Color.black;
                this.bgColor = bgColor ?? Color.white;
            }
        }

        public static void Create(string titleText, string bodyText, Popup.ButtonConstructor[] buttons = null, int buttonFocus = 0) {
            Debug.Console.instance.gameObject.SetActive(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Instantiate(UI.instance.popupPrefab, UI.instance.gameObject.transform).Init(titleText, bodyText, buttons);
        }

        public static void Quit() {
            Create(
                "",
                "Are you sure you want to quit?\nProgress will be saved.",
                new ButtonConstructor[] {
                    new("Cancel", (popup) => { popup.Destroy(); }),
                    new("Main Menu", (popup) => { popup.Destroy(); GameManager.ReturnToMainMenu(); }),
                    new("Quit Game", (popup) => { popup.Destroy(); GameManager.QuitGame(); }),
                }
            );
        }

        public static void Credits() {
            Create(
                "Credits",
                "Quadrasylum prototype by Alex Molloy.\n" +
                "Built in Unity 6.0.\n" +
                "All assets except sounds were made by me.\n" +
                "Sounds obtained / modified from freesounds.org\nFull credit on itch.io\n",
                new ButtonConstructor[] {
                    new("itch.io", (popup) => { Application.OpenURL("https://xae0.itch.io/quadrasylum"); }),
                    new("Ok", (popup) => { popup.Destroy(); }),
                }
            );
        }

        public static void Results() {
            resultsVisible = true;
            GUIUtility.systemCopyBuffer = Tester.hwid;
            Application.OpenURL(Tester.GetSurveyLink());
            Tester.SendQuitGame();
            Create(
                "Thanks for playing!",
                "Please fill out the survey.\nYour participant ID is: '" + Tester.hwid + "'",
                new ButtonConstructor[] {
                    new("Main Menu", (popup) => { popup.Destroy(); GameManager.ReturnToMainMenu(); resultsVisible = false; }),
                    new("Copy ID", (popup) => { GUIUtility.systemCopyBuffer = Tester.hwid; }),
                    new("Open Survey", (popup) => { Application.OpenURL(Tester.GetSurveyLink()); }),
                    new("Quit Game", (popup) => { popup.Destroy(); GameManager.QuitToDesktop(); }),
                }
            );
        }
    }
}
