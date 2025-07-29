using System;
using System.Collections;
using Unity.Logging;
using UnityEngine;

namespace Major.Animations {
    public class SlideAnimationRigidbody : ScriptedAnimation {
        [field: SerializeField] public Vector3 animDirection { get; private set; } = Vector3.zero;
        [field: SerializeField] public float animDistance { get; private set; } = 1.0f;
        protected Vector3 animStartPos;
        protected Vector3 animEndPos;
        protected Vector3 targetPos;
        private Rigidbody rb;

        protected void OnEnable() {
            if (!obj) {
                return;
            }
            rb = obj.GetComponent<Rigidbody>();
            animStartPos = obj.transform.localPosition;
            animEndPos = animStartPos + (animDirection * animDistance);
        }

        public override void SetAnimationState(bool state, bool waitForFixedUpdate = false) {
#if UNITY_EDITOR
            if (animDirection == Vector3.zero) {
                Log2.Warning("Animation has zero direction and will not animate.", "SlideAnimation");
                return;
            }
#endif
            base.SetAnimationState(state, waitForFixedUpdate);
        }

        public void OverrideValues(Rigidbody rb = null, Vector3? direction = null, float? distance = null, float? speed = null) {
            if (rb) { this.rb = rb; } // shorter than ternary & GameObjects cant use coalescing
            animDirection = direction ?? animDirection;
            animDistance = distance ?? animDistance;
            animSpeed = speed ?? animSpeed;
            OnEnable();
        }

        public void OverrideAnimPos(Vector3 start, Vector3 end) {
            animStartPos = start;
            animEndPos = end;
        }

        protected override void AnimationStart(bool state) {
            targetPos = state ? animEndPos : animStartPos;
        }

        protected override bool AnimationTick(bool state) {
            if (rb.position != targetPos) {
                rb.position = (Vector3.MoveTowards(rb.position, targetPos, animSpeed * Time.deltaTime));
                return true;
            }
            return false;
        }

        public void OverrideRigidbody(Rigidbody rigidbody) => rb = rigidbody;

        protected override void AnimationEnd(bool state) {
            
        }
    }
}