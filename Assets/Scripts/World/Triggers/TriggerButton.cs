using UnityEngine;

namespace Major.World {
    public class TriggerButton : TriggerArea {
        [SerializeField] private SlideAnimation anim;
        protected override void OnTriggerAreaBegin(GameObject sender) {
            anim.SetAnimationState(true);
        }
        protected override void OnTriggerAreaEnd(GameObject sender) {
            anim.SetAnimationState(false);
        }
    }
}