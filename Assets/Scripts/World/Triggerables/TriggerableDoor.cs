using UnityEngine;

namespace Major.World {
    public class TriggerableDoor : Triggerable {
        [SerializeField] private Animations.ScriptedAnimation leftDoorAnim;
        [SerializeField] private Animations.ScriptedAnimation rightDoorAnim;

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