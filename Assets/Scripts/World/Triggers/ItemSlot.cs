using System.Collections;
using Unity.Logging;
using UnityEngine;

namespace Major.World {
    [RequireComponent(typeof(Animations.SlideAnimation))]
    [RequireComponent(typeof(Animations.RotateAnimation))]
    public class ItemSlot : Trigger {
        private Item item;
        private Animations.SlideAnimation slideAnimation;
        private Animations.RotateAnimation rotateAnimation;
        [SerializeField] private bool passthrough;
        [SerializeField] private bool requireKevin = true;

        private void Awake() {
            rotateAnimation = GetComponent<Animations.RotateAnimation>();
            slideAnimation = GetComponent<Animations.SlideAnimation>();
            slideAnimation.onAnimEnd += (state) => {
                if (state) {
                    item.rb.MovePosition(transform.position);
                    item.transform.position = transform.position;
                    if (passthrough) {
                        Release();
                    }
                }
                else {
                    OnRelease();
                }
            };
        }

        private void OnTriggerEnter(Collider collider) {
            if (item) { return; }
            if (!collider.TryGetComponent<Item>(out var collidedItem)) { return; }
            if (requireKevin && collidedItem != Kevin.instance.item) { return; }
            if (collidedItem.recentlySlotted) { return; }

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
            var dirToPlayer = (Player.instance.transform.position - transform.position).normalized;
            var direction = Vector3.Project(dirToPlayer, transform.forward).normalized;
            var animStartPos = transform.position + (direction * (slideAnimation.animDistance * (passthrough && !takeIn ? -1.0f : 1.0f)));

            if (takeIn) {
                item.rb.MovePosition(animStartPos);
                item.transform.position = animStartPos;
                rotateAnimation.OverrideValues(
                    obj: item.gameObject
                    //speed: Quaternion.Lerp, todo; speed up depending on amount of rotation needed
                );
                rotateAnimation.OverrideAnimRot(item.transform.eulerAngles, MathExt.RoundToVal(item.transform.eulerAngles, 90.0f));
                rotateAnimation.SetAnimationState(true);
            }

            slideAnimation.OverrideObject(item.gameObject);
            slideAnimation.OverrideAnimPos(animStartPos, transform.position);
            slideAnimation.SetAnimationState(takeIn, takeIn);
        }
    }
}