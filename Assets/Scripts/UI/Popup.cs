using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

namespace Major.UI {
    public class Popup : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI body;
        [SerializeField] private Button cloneableButton;
        [SerializeField] private RectTransform buttonsParent;

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
        }

        public void Destroy() {
            Destroy(gameObject);
        }

        public struct ButtonConstructor {
            public string text;
            public UnityAction<Popup> onClick;
            public Color textColor;
            public Color bgColor;
        }
        
        public static void Quit() {
            UI.instance.Popup(
                "",
                "Are you sure you want to quit?",
                new ButtonConstructor[] {
                    new() {
                        text = "Cancel",
                        onClick = (popup) => { popup.Destroy(); },
                        textColor = Color.black,
                        bgColor = Color.white
                    },
                    new() {
                        text = "Main Menu",
                        onClick = (popup) => { GameManager.ReturnToMainMenu(); popup.Destroy(); },
                        textColor = Color.black,
                        bgColor = Color.white
                    },
                    new() {
                        text = "Quit Game",
                        onClick = (popup) => { GameManager.QuitToDesktop(); },
                        textColor = Color.black,
                        bgColor = Color.white
                    },
                }
            );
        }
    }
}
