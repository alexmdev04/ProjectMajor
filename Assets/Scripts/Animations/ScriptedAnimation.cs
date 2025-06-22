using System;
using System.Collections;
using UnityEngine;

namespace Major.Animations {
    public abstract class ScriptedAnimation : MonoBehaviour {
        public GameObject obj;
        public float animSpeed = 1.0f;
        private Coroutine anim;
        public event Action<bool> onAnimStart = (state) => { };
        public event Action<bool> onAnimEnd = (state) => { };
        private WaitForFixedUpdate _waitForFixedUpdate = new();
        private WaitForEndOfFrame _waitForEndOfFrame = new();

        public virtual void SetAnimationState(bool state, bool waitForFixedUpdate = false) {
            if (anim != null) { StopCoroutine(anim); }
            anim = StartCoroutine(Animate(state, waitForFixedUpdate));
        }

        private IEnumerator Animate(bool state, bool waitForFixedUpdate = false) {
            if (waitForFixedUpdate) {
                yield return _waitForFixedUpdate;
                yield return _waitForFixedUpdate;
            }
            AnimationStart(state);
            onAnimStart(state);
            while (AnimationTick(state)) {
                yield return _waitForEndOfFrame;
            }
            AnimationEnd(state);
            onAnimEnd(state);
        }

        protected abstract void AnimationStart(bool state);

        /// <returns>Should return true while the animation should be playing</returns>
        protected abstract bool AnimationTick(bool state);
        protected abstract void AnimationEnd(bool state);
    }
}