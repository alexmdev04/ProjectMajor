using UnityEngine;
using TMPro;

namespace Major.UI {
    public class HUD : MonoBehaviour {
        public TextMeshProUGUI interactText;

        private void Awake() {
            UI.hud = this;
        }
    }
}