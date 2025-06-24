using System.Collections.Generic;
using UnityEngine;

namespace Major.World {
    public class ConveyorBelt : MonoBehaviour {
        [SerializeField] private Vector3 direction = Vector3.back;
        [SerializeField] private float strength = 1.0f;
        [SerializeField] private HashSet<Rigidbody> targetObjects = new();
        [SerializeField] private ForceMode forceMode = ForceMode.VelocityChange;

        private void FixedUpdate() {
            foreach (var obj in targetObjects) {
                obj.AddForce(strength * direction, forceMode);
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