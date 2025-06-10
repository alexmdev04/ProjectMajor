using UnityEngine;

namespace Major.World {
    public class ItemSlot : Trigger {
        private Item item;

        private void OnTriggerEnter(Collider collider) {
            if (!item && collider.TryGetComponent<Item>(out var collidedItem)) {
                item = collidedItem;
                item.SetSlotted(this, transform.position, transform.rotation);
                Begin(item.gameObject);
            }
        }

        public void Release() {
            End(item.gameObject);
            item = null;
        }

        protected override void OnTriggerBegin(GameObject sender) {

        }

        protected override void OnTriggerEnd(GameObject sender) {
            
        }
    }
}