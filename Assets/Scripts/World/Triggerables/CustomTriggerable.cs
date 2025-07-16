using System;
using UnityEngine;

namespace Major.World {
    public class CustomTriggerable : Triggerable {
        public event Action<Trigger, GameObject> onTriggered = (senderTrigger, sender) => { };
        public event Action<Trigger, GameObject> onUntriggered = (senderTrigger, sender) => { };

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            onTriggered(senderTrigger, sender);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            onUntriggered(senderTrigger, sender);
        }
    }
}