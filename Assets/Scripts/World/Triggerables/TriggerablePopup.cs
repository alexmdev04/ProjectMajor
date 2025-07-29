using UnityEngine;

namespace Major.World {
    public class TriggerablePopup : Triggerable {
        [Header("On Triggered")]
        public string title1;
        public string body1;
        [Header("On Untriggered")]
        public string title2;
        public string body2;
        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            UI.Popup.Create(title1, body1);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            UI.Popup.Create(title2, body2);
        }
    }
}