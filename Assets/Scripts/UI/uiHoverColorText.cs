using TMPro;
using Unity.Logging;
using UnityEngine;

namespace Major.UI {
    public class uiHoverColorText : uiHoverColor {
        [SerializeField] private TextMeshProUGUI target;

        public override void SetColor(Color newColor) {
            target.color = newColor;
        }
    }
}