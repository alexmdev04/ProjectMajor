using UnityEngine;

namespace Major.World {
    public class TriggerableMessage : Triggerable {
        public string triggeredMessage;
        public bool triggeredMessageEnabled = true;

        public string untriggeredMessage;
        public bool untriggeredMessageEnabled;
        private void Awake() {
            if (triggeredMessage == string.Empty) {
                triggeredMessageEnabled = false;
            }
            if (untriggeredMessage == string.Empty) {
                untriggeredMessageEnabled = false;
            }
        }

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            if (!triggeredMessageEnabled) {
                return;
            }
            uiMessage.instance.New(triggeredMessage, "TriggerableMessage");
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            if (!untriggeredMessageEnabled) {
                return;
            }
            uiMessage.instance.New(untriggeredMessage, "TriggerableMessage");
        }
    }
}