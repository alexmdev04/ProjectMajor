using System.Collections.Generic;
using UnityEngine;

namespace Major.World {
    [RequireComponent(typeof(CustomTriggerable))]
    public class ConveyorBelt : MonoBehaviour {
        [SerializeField] private Vector3 direction = Vector3.back;
        [SerializeField] private Vector3 triggeredDirection = Vector3.zero;
        private Vector3 currentDirection = Vector3.zero;
        [SerializeField] private float strength = 1.0f;
        [SerializeField] private HashSet<Rigidbody> targetObjects = new();
        [SerializeField] private ForceMode forceMode = ForceMode.VelocityChange;
        private CustomTriggerable customTriggerable;

        private void Awake() {
            currentDirection = direction;

            if (triggeredDirection == Vector3.zero) {
                triggeredDirection = -direction;
            }

            customTriggerable = GetComponent<CustomTriggerable>();

            customTriggerable.onTriggered += (senderTrigger, sender) => {
                currentDirection = triggeredDirection;
            };

            customTriggerable.onUntriggered += (senderTrigger, sender) => {
                currentDirection = direction;
            };
        }

        private void FixedUpdate() {
            foreach (var obj in targetObjects) {
                obj.AddForce(strength * currentDirection, forceMode);
            }
        }

        private void OnTriggerEnter(Collider sender) {
            if (!sender.attachedRigidbody) {
                return;
            }
            targetObjects.Add(sender.attachedRigidbody);
        }

        private void OnTriggerExit(Collider sender) {
            if (!sender.attachedRigidbody) {
                return;
            }
            targetObjects.Remove(sender.attachedRigidbody);
        }
    }
}