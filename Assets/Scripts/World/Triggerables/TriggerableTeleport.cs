using UnityEngine;

namespace Major.World {
    public class TriggerableTeleport : Triggerable {
        public GameObject target;
        public bool targetIsSender;
        public Transform position;
        public bool positionIsTargetOffset;
        public bool resetTargetLinearVelocity = true;
        public bool resetTargetAngularVelocity = true;

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

            _target.transform.position = positionIsTargetOffset ? _target.transform.position + position.position : position.position;
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            
        }
    }
}