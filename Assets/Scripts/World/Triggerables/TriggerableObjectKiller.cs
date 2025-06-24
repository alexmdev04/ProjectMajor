using UnityEngine;

namespace Major.World {
    public class TriggerableObjectKiller : Triggerable {
        public GameObject obj;
        [SerializeField] private bool killSender;

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            var target = killSender ? sender : obj;

            if (target == Player.instance.gameObject) {
                GameManager.instance.OnPlayerKilled();
                return;
            }

            if (target == Kevin.instance.gameObject) {
                GameManager.instance.OnKevinKilled();
                return;
            }
            
            Destroy(target);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {

        }
    }
}