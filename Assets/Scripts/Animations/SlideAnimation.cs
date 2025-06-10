using System.Collections;
using Unity.Logging;
using UnityEngine;

namespace Major {
    public class SlideAnimation : MonoBehaviour {
        [SerializeField] private GameObject obj;
        [SerializeField] private Vector3 animDirection = Vector3.zero;
        [SerializeField] private float animDistance = 1.6f;
        [SerializeField] private float animSpeed = 5.0f;
        private Vector3 animStartPos;
        private Vector3 animEndPos;
        private Coroutine anim;

        private void OnEnable() {
            if (animDirection == Vector3.zero) {
                Log.Warning("Animation has zero direction and will not animate.");
                return;
            }

            animStartPos = obj.transform.position;
            animEndPos = animStartPos + (animDirection * animDistance);
        }

        public void SetAnimationState(bool state) {
            if (anim != null) { StopCoroutine(anim); }
            anim = StartCoroutine(Animate(state));
        }

        private IEnumerator Animate(bool state) {
            var targetPos = state ? animEndPos : animStartPos;
            while (obj.transform.position != targetPos) {
                obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPos, animSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}