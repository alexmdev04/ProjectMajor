using Unity.Logging;
using UnityEngine;

namespace Major.World {
    public class TriggerableTeleport : Triggerable {
        public GameObject target;
        public bool targetIsSender;
        public Transform position;
        public bool positionIsTargetOffset;
        public bool resetTargetLinearVelocity = true;
        public bool resetTargetAngularVelocity = true;
        public bool requireRigidbody = true;

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            var _target = targetIsSender ? sender : target;
            Rigidbody targetRb;
            if (!_target.TryGetComponent(out targetRb)) {
                targetRb = _target.GetComponentInParent<Rigidbody>();
            }

            if (targetRb) {
                if (resetTargetLinearVelocity) {
                    targetRb.linearVelocity = Vector3.zero;
                }

                if (resetTargetAngularVelocity) {
                    targetRb.angularVelocity = Vector3.zero;
                }

                targetRb.position = positionIsTargetOffset ? targetRb.position + position.position : position.position;
            }
            else if (requireRigidbody) {
                Log.Warning("[TriggerableForce] '" + name + "' found no rigidbody on '" + _target.name + "' or it's parent, but requireRigidbody is enabled.");
            }
            else {
                _target.transform.position = positionIsTargetOffset ? _target.transform.position + position.position : position.position;
            }
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            
        }
    }
}