using UnityEngine;

namespace Major.World {
    public abstract class TriggerArea : Trigger {
        private int triggerEntities;
        [SerializeField] private int minimumEntities = 1;
        private void OnTriggerEnter(Collider collision) {
            if (triggerEntities < minimumEntities) {
                Begin(collision.gameObject);
                OnTriggerAreaBegin(collision.gameObject);
            }
            triggerEntities++;
        }
        private void OnTriggerExit(Collider collision) {
            triggerEntities--;
            if (triggerEntities < minimumEntities) {
                End(collision.gameObject);
                OnTriggerAreaEnd(collision.gameObject);
            }
        }

        protected abstract void OnTriggerAreaBegin(GameObject sender);

        protected abstract void OnTriggerAreaEnd(GameObject sender);
    }
}