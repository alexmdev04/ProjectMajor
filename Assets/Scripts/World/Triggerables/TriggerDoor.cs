using UnityEngine;

namespace Major.World {
    public class TriggerDoor : Triggerable {
        [SerializeField] private SlideAnimation leftDoorAnim;
        [SerializeField] private SlideAnimation rightDoorAnim;

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            leftDoorAnim.SetAnimationState(true);
            rightDoorAnim.SetAnimationState(true);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            leftDoorAnim.SetAnimationState(false);
            rightDoorAnim.SetAnimationState(false);
        }
    }
}