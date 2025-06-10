using System;
using System.Collections;
using UnityEngine;

namespace Major.World {
    public class Door : Interactable {
        public float openAngle = 90.0f;
        public float closeAngle = 0.0f;
        public Transform parent;
        public float doorOpenSpeed = 150.0f;
        public bool openState;

        public void Awake() {
            if (parent == null) {
                parent = transform.parent;
            }
        }

        public override void Interact(Player sender, Action callback = null) {
            SetState(!openState);
            callback?.Invoke();
        }

        public void SetState(bool state) {
            StopAllCoroutines();
            StartCoroutine(Animate(state));
            openState = state;
        }

        IEnumerator Animate(bool state) {
            var targetAngle = state ? openAngle : closeAngle;
            var targetEuler = new Vector3(
                parent.localEulerAngles.x,
                targetAngle,
                parent.localEulerAngles.z
            );
            var targetRotation = Quaternion.Euler(targetEuler);

            do {
                parent.localRotation = Quaternion.RotateTowards(
                    parent.localRotation,
                    targetRotation,
                    Time.deltaTime * doorOpenSpeed
                );
                yield return new WaitForEndOfFrame();
            } while (MathF.Round(parent.localEulerAngles.y, 3) != targetAngle);

            yield break;
        }
    }
}