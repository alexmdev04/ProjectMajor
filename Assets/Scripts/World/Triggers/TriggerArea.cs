using Unity.Logging;
using UnityEngine;

namespace Major.World {
    public abstract class TriggerArea : Trigger {
        private int triggerEntities;
        [SerializeField, Tooltip("Overriden by 'triggered by every entity'")]
        private int minimumEntities = 1;

        [SerializeField, Tooltip("If an entity enters the area immediately begin and end the trigger, useful for one time triggerables like bounce pads")]
        private bool triggeredByEveryEntity = false;

        private void OnTriggerEnter(Collider collision) {
            if (triggeredByEveryEntity) {
                OnBegin(collision.gameObject);
                OnEnd(collision.gameObject);
                return;
            }

            if (collision.TryGetComponent<DestructionProtection>(out var collisionDP)) {
                collisionDP.onColliderDestroyed += OnTriggerExit;
            }

            triggerEntities++;
            if (triggerEntities == minimumEntities) {
                OnBegin(collision.gameObject);
            }
        }

        private void OnBegin(GameObject sender) {
            Begin(sender);
            OnTriggerAreaBegin(sender);
        }

        private void OnTriggerExit(Collider collision) {
            if (triggeredByEveryEntity) {
                return;
            }

            if (collision.TryGetComponent<DestructionProtection>(out var collisionDP)) {
                collisionDP.onColliderDestroyed -= OnTriggerExit;
            }

            triggerEntities--;
            if (triggerEntities < minimumEntities) {
                OnEnd(collision.gameObject);
            }
        }

        private void OnEnd(GameObject sender) {
            End(sender);
            OnTriggerAreaEnd(sender);
        }

        protected abstract void OnTriggerAreaBegin(GameObject sender);

        protected abstract void OnTriggerAreaEnd(GameObject sender);
    }
}