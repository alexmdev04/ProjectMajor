using System;
using System.Collections;
using Unity.Logging;
using UnityEngine;

namespace Major {
    public class SlideAnimation : MonoBehaviour {
        [field: SerializeField] public GameObject obj { get; private set; }
        [field: SerializeField] public Vector3 animDirection { get; private set; } = Vector3.zero;
        [field: SerializeField] public float animDistance { get; private set; } = 1.0f;
        [field: SerializeField] public float animSpeed { get; private set; } = 5.0f;
        private Vector3 animStartPos;
        private Vector3 animEndPos;
        public Coroutine anim;
        public event Action<bool> onAnimStart = (state) => { };
        public event Action<bool> onAnimEnd = (state) => { };

        private void OnEnable() {
            if (!obj) {
                return;
            }
            animStartPos = obj.transform.position;
            animEndPos = animStartPos + (animDirection * animDistance);
        }

        public void SetAnimationState(bool state, bool waitForFixedUpdate = false) {
            if (animDirection == Vector3.zero) {
                Log.Warning("Animation has zero direction and will not animate.");
                return;
            }
            if (anim != null) { StopCoroutine(anim); }
            anim = StartCoroutine(Animate(state, waitForFixedUpdate));
        }

        private IEnumerator Animate(bool state, bool waitForFixedUpdate = false) {
            if (waitForFixedUpdate) {
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
            }
            onAnimStart(state);
            var targetPos = state ? animEndPos : animStartPos;
            while (obj.transform.position != targetPos) {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPos, animSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
            onAnimEnd(state);
        }

        public void OverrideValues(GameObject obj = null, Vector3? direction = null, float? distance = null, float? speed = null) {
            if (obj) { this.obj = obj; } // shorter than ternary & GameObjects cant use coalescing
            animDirection = direction ?? animDirection;
            animDistance = distance ?? animDistance;
            animSpeed = speed ?? animSpeed;
            OnEnable();
        }

        public void OverrideAnimPos(Vector3 start, Vector3 end) {
            animStartPos = start;
            animEndPos = end;
        }
    }
}