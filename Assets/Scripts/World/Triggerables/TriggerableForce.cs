using UnityEngine;
using Unity.Logging;

namespace Major.World {
    public class TriggerableForce : Triggerable {
        public Rigidbody target;
        public bool targetIsSender = true;
        [Tooltip("The base position and forward direction of the force")]
        public Transform forceTransform;
        public Vector3 positionOffset = Vector3.zero;
        public bool positionIsTarget = true;
        public float force = 10.0f;
        public ForceMode forceMode = ForceMode.VelocityChange;
        public bool resetTargetLinearVelocity = true;
        public bool resetTargetAngularVelocity = true;

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            Rigidbody rb = target;

            if (targetIsSender) {
                var senderRb = sender.GetComponentInParent<Rigidbody>();
                if (senderRb) {
                    rb = senderRb;
                }
                else if (!sender.TryGetComponent(out rb)) {
                    Log.Warning("[TriggerableForce] Sender and its parent have no rigidbody.");
                    return;
                }
            }

            if (rb.TryGetComponent(out Item item)) {
                if (item.isCarried) {
                    Player.instance.DropCarriedItem();
                }
            }

            if (rb == Player.instance.rb) {
                Player.instance.OverrideGroundedThisFixedUpdate(false);
            }

            if (resetTargetLinearVelocity) {
                rb.linearVelocity = Vector3.zero;
            }

            if (resetTargetAngularVelocity) {
                rb.angularVelocity = Vector3.zero;
            }

            var targetPos = positionIsTarget ? rb.position : forceTransform.position;

            rb.AddForceAtPosition(
                forceTransform.forward * force,
                targetPos + positionOffset,
                forceMode
            );
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {

        }
    }
}