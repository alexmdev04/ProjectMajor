using UnityEngine;

namespace Major.World {
    public class TriggerableGameObject : Triggerable {
        [SerializeField] private GameObject targetGameObject;

        [SerializeField, Tooltip("The game object is enabled/disabled based on its current state")]
        private bool toggleOnCurrentState = true;

        [SerializeField, Tooltip("Disables the game object when triggered (overriden by toggleOnCurrentState)")]
        private bool flipped = false;
        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            targetGameObject.SetActive(toggleOnCurrentState ? !targetGameObject.activeSelf : !flipped);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            targetGameObject.SetActive(toggleOnCurrentState ? !targetGameObject.activeSelf : flipped);
        }
    }
}