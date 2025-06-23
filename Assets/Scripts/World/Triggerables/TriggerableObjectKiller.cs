using UnityEngine;

namespace Major.World {
    public class TriggerableObjectKiller : Triggerable {
        public GameObject obj;

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            if (obj == Player.instance.gameObject) {
                GameManager.instance.OnPlayerKilled();
                return;                
            }
            if (obj == Kevin.instance.gameObject) {
                GameManager.instance.OnKevinKilled();
                return;
            }
            Destroy(obj);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            
        }
    }
}