using Unity.Logging;
using UnityEngine;

namespace Major.World {
    public class ItemSlot : Trigger {
        private Item item;
        private Animations.SlideAnimation slideAnimation;

        private void Awake() {
            slideAnimation = GetComponent<Animations.SlideAnimation>();
            slideAnimation.onAnimEnd += (state) => {
                if (!state) {
                    OnRelease();
                }
            };
        }

        private void OnTriggerEnter(Collider collider) {
            if (item) {
                return;
            }

            if (!collider.TryGetComponent<Item>(out var collidedItem)) {
                return;
            }

            if (collidedItem.recentlySlotted) {
                return;
            }

            item = collidedItem;
            item.OnSlotted(this);
            Animate(true);
            Begin(item.gameObject);
        }

        public void Release(bool animate = true) {
            End(item.gameObject);
            if (!animate) {
                OnRelease();
                return;
            }
            Animate(false);
        }

        private void OnRelease() {
            item.OnUnslotted();
            item = null;
        }

        private void Animate(bool takeIn) {
            // is the direction to the player positve or negative on the local forward axis
            float isPlayerForward = Mathf.Sign(transform.TransformDirection((Player.instance.transform.position - transform.position).normalized).z);
            var direction = transform.forward * isPlayerForward;
            var animStartPos = transform.position + (direction * slideAnimation.animDistance);
            if (takeIn) {
                item.rb.MovePosition(animStartPos);
                item.transform.position = animStartPos;
                item.rb.MoveRotation(Quaternion.identity);
                item.transform.rotation = Quaternion.identity;
            }

            slideAnimation.OverrideValues(
                obj: item.gameObject,
                direction: Vector3.one
            );

            slideAnimation.OverrideAnimPos(
                start: animStartPos,
                end: transform.position
            );

            slideAnimation.SetAnimationState(takeIn, takeIn);
        }

        protected override void OnTriggerBegin(GameObject sender) {

        }

        protected override void OnTriggerEnd(GameObject sender) {

        }
    }
}