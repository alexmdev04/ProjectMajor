using System;
using System.Collections;
using Unity.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Major.UI {
    public abstract class uiHoverColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler {
        [SerializeField] private Color hoverColor;
        [SerializeField] private Color baseColor;
        [SerializeField] private float hoverTime = 0.1f;
        private float colorT;
        private Coroutine hoverCoroutine;
        private bool hoverChanging;

        private void Awake() {
            SetColor(baseColor);
        }

        public void OnPointerEnter(PointerEventData data) {
            if (hoverChanging) {
                StopCoroutine(hoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(HoverCoroutine(true));
        }

        public void OnSelect(BaseEventData eventData) {
            if (hoverChanging) {
                StopCoroutine(hoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(HoverCoroutine(true));
        }

        public void OnPointerExit(PointerEventData data) {
            if (hoverChanging) {
                StopCoroutine(hoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(HoverCoroutine(false));
        }

        public void OnDeselect(BaseEventData eventData) {
            if (hoverChanging) {
                StopCoroutine(hoverCoroutine);
            }
            hoverCoroutine = StartCoroutine(HoverCoroutine(false));
        }

        private IEnumerator HoverCoroutine(bool toHover) {
            hoverChanging = true;
            while (toHover ? (colorT < 1.0f) : (colorT > 0.0f)) {
                colorT += (Time.unscaledDeltaTime / hoverTime) * (toHover ? 1.0f : -1.0f);
                SetColor(Color.Lerp(baseColor, hoverColor, colorT));
                yield return new WaitForEndOfFrame();
            }
            SetColor(toHover ? hoverColor : baseColor);
            colorT = toHover ? 1.0f : 0.0f;
            hoverChanging = false;
        }

        public abstract void SetColor(Color newColor);


    }
}
