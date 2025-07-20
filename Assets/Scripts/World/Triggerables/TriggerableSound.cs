using UnityEngine;

namespace Major.World {
    public class TriggerableSound : Triggerable {
        public AudioSource audioSource;
        public bool playOnTriggered = true;
        public bool playOnUntriggered = false;

        private void Awake() {
            if (!audioSource) {
                audioSource = GetComponent<AudioSource>();
            }
        }

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            if (!playOnTriggered) {
                return;
            }
            audioSource.PlayOneShot(audioSource.clip);            
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            if (!playOnUntriggered) {
                return;
            }
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}