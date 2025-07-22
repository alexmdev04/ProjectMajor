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

        public void Init(string titleText, string bodyText, ButtonConstructor[] buttons = null) {
            title.text = titleText;
            body.text = bodyText;

            if (buttons != null) {
                foreach (var button in buttons) {
                    var newButton = Instantiate(cloneableButton, buttonsParent);
                    newButton.onClick.RemoveAllListeners();
                    newButton.onClick.AddListener(button.onClick);
                    var newButtonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
                    newButtonText.text = button.text;
                    newButtonText.color = button.textColor;
                    newButton.GetComponent<Image>().color = button.bgColor;
                }
            }
            //GetComponent<VerticalLayoutGroup>().SetLayoutVertical();
        }

        public void Destroy() {
            Destroy(gameObject);
        }
        
        public struct ButtonConstructor {
            public string text;
            public UnityAction onClick;
            public Color textColor;
            public Color bgColor;
        }
    }
}
