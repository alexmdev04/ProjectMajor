using Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Major.UI {
    public class uiHoverColorImage : uiHoverColor {
        [SerializeField] private Image target;

        public override void SetColor(Color newColor) {
            target.color = newColor;
        }
    }
}