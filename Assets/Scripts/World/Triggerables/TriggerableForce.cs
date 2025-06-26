using UnityEngine;
using Unity.Logging;

namespace Major.World {
    public class TriggerableForce : Triggerable {
        public Rigidbody target;
        public bool targetIsSender = true;
        public Vector3 position = Vector3.zero;
        public bool positionIsOffset = true;
        public bool positionIsTarget = true;
        public Vector3 direction = Vector3.zero;
        public float force = 10.0f;
        public ForceMode forceMode = ForceMode.VelocityChange;

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            Rigidbody rb = target;

            if (targetIsSender) {
                var senderRb = sender.GetComponentInParent<Rigidbody>();
                if (senderRb) {
                    rb = senderRb;
                }
                else if (!sender.TryGetComponent(out rb)) {
                    Log.Warning("[TriggerableForce] Sender and its parent has no rigidbody.");
                    return;
                }
            }

            var targetPos = positionIsTarget ? rb.position : position;

            rb.AddForceAtPosition(
                direction * force,
                positionIsOffset ? transform.position + targetPos : targetPos,
                forceMode
            );
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {

        }
    }
}