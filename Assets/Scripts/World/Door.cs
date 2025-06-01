using System;
using System.Collections;
using UnityEngine;

namespace Major.Interact {
    public class Door : Interactable {
        public float openAngle = 90.0f;
        public float closeAngle = 0.0f;
        public GameObject handle;
        public float doorOpenSpeed = 150.0f;
        public bool openState;
        public override void Interact(GameObject sender) {
            SetState(!openState);
        }

        public void SetState(bool state) {
            StopAllCoroutines();
            StartCoroutine(DoorMoveCoroutine(state));
            openState = state;
        }

        IEnumerator DoorMoveCoroutine(bool state) {
            var targetAngle = state ? openAngle : closeAngle;
            var targetEuler = new Vector3(
                transform.localEulerAngles.x,
                targetAngle,
                transform.localEulerAngles.z
            );
            var targetRotation = Quaternion.Euler(targetEuler);

            do {
                transform.localRotation = Quaternion.RotateTowards(
                    transform.localRotation,
                    targetRotation,
                    Time.deltaTime * doorOpenSpeed
                );
                yield return new WaitForEndOfFrame();
            } while (MathF.Round(transform.localEulerAngles.y, 3) != targetAngle);

            yield break;
        }
    }
}