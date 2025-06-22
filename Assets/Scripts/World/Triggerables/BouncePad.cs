using UnityEngine;

namespace Major.World {
    public class BouncePad : Triggerable {
        [SerializeField] private float strength = 100.0f;
        [SerializeField] private Vector3 direction = new Vector3(0.0f, 1.0f, 1.0f);
        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            if (sender.TryGetComponent<Collider>(out var senderCollider)) {
                var senderRb = senderCollider.attachedRigidbody;
                if (senderRb) {
                    senderRb.linearVelocity += strength * direction;
                }
            }
        }
        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {

        }
    }
}