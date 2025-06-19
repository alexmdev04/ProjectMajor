using UnityEngine;

namespace Major.World {
    public class TriggerableComponent : Triggerable {
        [SerializeField] private MonoBehaviour targetComponent;

        [SerializeField, Tooltip("The component is enabled/disabled based on its current state")]
        private bool toggleOnCurrentState = true;

        [SerializeField, Tooltip("Disables the component when triggered (overriden by toggleOnCurrentState)")]
        private bool flipped = false;
        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            targetComponent.enabled = toggleOnCurrentState ? !targetComponent.enabled : !flipped;
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            targetComponent.enabled = toggleOnCurrentState ? !targetComponent.enabled : flipped;
        }
    }
}