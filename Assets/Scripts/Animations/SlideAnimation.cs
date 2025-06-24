using System;
using System.Collections;
using Unity.Logging;
using UnityEngine;

namespace Major.Animations {
    public class SlideAnimation : ScriptedAnimation {
        [field: SerializeField] public Vector3 animDirection { get; private set; } = Vector3.zero;
        [field: SerializeField] public float animDistance { get; private set; } = 1.0f;
        private Vector3 animStartPos;
        private Vector3 animEndPos;
        private Vector3 targetPos;

        private void OnEnable() {
            if (!obj) {
                return;
            }
            animStartPos = obj.transform.localPosition;
            animEndPos = animStartPos + (animDirection * animDistance);
        }

        public override void SetAnimationState(bool state, bool waitForFixedUpdate = false) {
            if (animDirection == Vector3.zero) {
                Log.Warning("Animation has zero direction and will not animate.");
                return;
            }
            base.SetAnimationState(state, waitForFixedUpdate);
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

        protected override void AnimationStart(bool state) {
            targetPos = state ? animEndPos : animStartPos;
        }

        protected override bool AnimationTick(bool state) {
            obj.transform.localPosition = Vector3.MoveTowards(obj.transform.localPosition, targetPos, animSpeed * Time.deltaTime);
            return obj.transform.localPosition != targetPos;
        }

        protected override void AnimationEnd(bool state) {

        }
    }
}