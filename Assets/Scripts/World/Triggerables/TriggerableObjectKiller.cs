using Unity.Logging;
using UnityEngine;

namespace Major.World {
    public class TriggerableObjectKiller : Triggerable {
        public GameObject target;
        [SerializeField] private bool targetIsSender;

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            var _target = targetIsSender ? sender : target;

            if (_target == Player.instance.gameObject) {
                GameManager.instance.OnPlayerKilled();
                return;
            }
            else if (_target.GetComponentInParent<Player>()) {
                GameManager.instance.OnPlayerKilled();
                return;                
            }

            if (_target == Kevin.instance.gameObject) {
                GameManager.instance.OnKevinKilled();
                return;
            }
            else if (_target.GetComponentInParent<Kevin>()) {
                GameManager.instance.OnKevinKilled();
                return;
            }

#if UNITY_EDITOR
            if (!_target) {
                Log.Warning("[TriggerableObjectKiller] No target.");
            }
#endif
            Destroy(_target);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {

        }
    }
}