using UnityEngine;

namespace Major.World {
    public class TriggerableScriptedAnimation : Triggerable {
        [SerializeField] private Animations.ScriptedAnimation scriptedAnimation;
        [SerializeField] private bool waitForFixedUpdate;
        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            scriptedAnimation.SetAnimationState(true, waitForFixedUpdate);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            scriptedAnimation.SetAnimationState(false, waitForFixedUpdate);
        }
    }
}