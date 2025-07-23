using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Major.UI {
    public class Menu : MonoBehaviour {
        private RectTransform rectTransform;
        private Coroutine menuAnimCoroutine;
        private bool animating;
        public string menuName = "New Menu";
        public string previousMenu = "Unset";
        [SerializeField] private bool activateOnRegistered = false;
        [SerializeField] private float animDistance = 100.0f;
        [SerializeField] private float animTime = 1.0f;
        [SerializeField] private Vector2 animDirection = Vector2.up;
        [SerializeField] private Selectable selectOnActivate;
        private Vector2 basePos;

        private void Awake() {
            rectTransform = GetComponent<RectTransform>();
            basePos = rectTransform.anchoredPosition;
            gameObject.SetActive(false);
            UI.instance.RegisterMenu(menuName, this, activateOnRegistered);
        }

        public void Activate(bool animDirection = true) {
            gameObject.SetActive(true);
            if (animating) {
                StopCoroutine(menuAnimCoroutine);
            }
            menuAnimCoroutine = StartCoroutine(MenuAnim(true, animDirection));
            if (selectOnActivate) {
                selectOnActivate.Select();
            }
        }

        public void Deactivate(bool animDirection = true) {
            if (animating) {
                StopCoroutine(menuAnimCoroutine);
            }
            menuAnimCoroutine = StartCoroutine(MenuAnim(false, animDirection));
        }

        private IEnumerator MenuAnim(bool activating, bool direction) {
            animating = true;

            var deltaPos = animDirection * animDistance;
            var startPos = activating ? basePos + deltaPos : basePos;
            var endPos = activating ? basePos : basePos + deltaPos;
            var time = animTime * (direction ? 1.0f : -1.0f);
            var animT = 0.0f;

            while (direction ? (animT < 1.0f) : (animT > 0.0f)) {
                animT += Time.unscaledDeltaTime / time;
                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, animT);
                yield return new WaitForEndOfFrame();
            }

            gameObject.SetActive(activating);
            animating = false;
        }

        public event Action OnActivated;
        public event Action OnDeactivated;
    }
}