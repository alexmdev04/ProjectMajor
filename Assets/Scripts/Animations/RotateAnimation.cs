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
            animStartRot = obj.transform.rotation;
            animEndRot = animStartRot * Quaternion.Euler(animRotation);
        }

        public override void SetAnimationState(bool state, bool waitForFixedUpdate = false) {
            if (animRotation == Vector3.zero) {
                Log.Warning("Animation has zero rotation and will not animate.");
                return;
            }
            base.SetAnimationState(state, waitForFixedUpdate);
        }

        protected override void AnimationStart(bool state) {
            targetRot = state ? animEndRot : animStartRot;
        }
        
        protected override bool AnimationTick(bool state) {
            obj.transform.rotation = Quaternion.RotateTowards(obj.transform.rotation, targetRot, animSpeed * Time.deltaTime);
            return obj.transform.rotation != targetRot;
        }

        protected override void AnimationEnd(bool state) {
            
        }
    }
}
