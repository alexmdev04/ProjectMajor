using System;
using UnityEngine;
using Unity.Logging;
using System.Collections;
using Unity.Mathematics;

namespace Major.Animations {
    public class RotateAnimation : ScriptedAnimation {
        [field: SerializeField] public Vector3 animRotation { get; private set; } = Vector3.up * 90.0f;
        private Quaternion animStartRot;
        private Quaternion animEndRot;
        private Quaternion targetRot;

        private void OnEnable() {
            if (!obj) {
                return;
            }
            animStartRot = obj.transform.localRotation;
            animEndRot = animStartRot * Quaternion.Euler(animRotation);
        }

        public override void SetAnimationState(bool state, bool waitForFixedUpdate = false) {
#if UNITY_EDITOR
            if (animRotation == Vector3.zero) {
                Log.Warning("Animation has zero rotation and will not animate.");
                return;
            }
#endif
            base.SetAnimationState(state, waitForFixedUpdate);
        }

        protected override void AnimationStart(bool state) {
            targetRot = state ? animEndRot : animStartRot;
        }

        protected override bool AnimationTick(bool state) {
            if (obj.transform.localRotation != targetRot) {
                obj.transform.localRotation = Quaternion.RotateTowards(obj.transform.localRotation, targetRot, animSpeed * Time.deltaTime);
                return true;
            }
            return false;
        }

        protected override void AnimationEnd(bool state) {

        }

        public void OverrideValues(GameObject obj = null, Vector3? rotation = null, float? speed = null) {
            if (obj) { this.obj = obj; } // shorter than ternary & GameObjects cant use coalescing
            animRotation = rotation ?? animRotation;
            animSpeed = speed ?? animSpeed;
            OnEnable();
        }

        public void OverrideAnimRot(Vector3 start, Vector3 end) {
            OverrideAnimRot(
                Quaternion.Euler(start),
                Quaternion.Euler(end)
            );
        }

        public void OverrideAnimRot(Quaternion start, Quaternion end) {
            animStartRot = start;
            animEndRot = end;
        }
    }
}
