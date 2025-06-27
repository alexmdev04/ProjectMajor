using Unity.Logging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Major.World {
    public class Checkpoint : Triggerable {
        [Header("General")]
        public bool firstCheckpointInLevel = false;
        public bool useCheckpointTransformAsPosition = true;
        public bool killVelocity = true;
        public bool teleportOnActivation = false;

        [Header("Player")]
        public Vector3 playerSpawnPosition = Vector3.zero;
        public bool playerSpawnPositionIsOffset = false;
        public Vector3 playerSpawnRotation = Vector3.zero;
        
        [Header("Kevin")]
        public Vector3 kevinSpawnPosition = Vector3.zero;
        public bool kevinSpawnLocationIsOffset = false;
        public Vector3 kevinSpawnRotation = Vector3.zero;

        public void Teleport() {
            TeleportPlayer();
            TeleportKevin();
        }

        public void TeleportPlayer() {
            var targetPlayerPosition = useCheckpointTransformAsPosition ? transform.position : playerSpawnPosition;
            var targetPlayerEulerAngles = useCheckpointTransformAsPosition ? transform.eulerAngles : playerSpawnRotation;

            if (playerSpawnPositionIsOffset) {
                targetPlayerPosition += Player.instance.transform.position;
            }

            if (killVelocity && !Player.instance.rb.isKinematic) {
                Player.instance.rb.linearVelocity = Vector3.zero;
                Player.instance.rb.angularVelocity = Vector3.zero;
            }

            Player.instance.transform.position = targetPlayerPosition;
            Player.instance.rb.position = targetPlayerPosition;

            targetPlayerEulerAngles.z = 0.0f; // player has no z rotation
            Player.instance.eulerAngles = targetPlayerEulerAngles;

            targetPlayerEulerAngles.x = 0.0f; // rigidbody only uses y rotation
            Player.instance.rb.rotation = Quaternion.Euler(targetPlayerEulerAngles);
        }

        public void TeleportKevin() {
            var targetKevinPosition = useCheckpointTransformAsPosition ? transform.position + (transform.forward * 1.5f) : kevinSpawnPosition;
            var targetKevinEulerAngles = useCheckpointTransformAsPosition ? Vector3.zero : kevinSpawnRotation;

            if (kevinSpawnLocationIsOffset) {
                targetKevinPosition += Kevin.instance.transform.position;
            }

            if (killVelocity && !Kevin.instance.rb.isKinematic) {
                Kevin.instance.rb.linearVelocity = Vector3.zero;
                Kevin.instance.rb.angularVelocity = Vector3.zero;
            }

            var targetKevinRotation = Quaternion.Euler(targetKevinEulerAngles);
            Kevin.instance.transform.position = targetKevinPosition;
            Kevin.instance.transform.rotation = targetKevinRotation;
            Kevin.instance.rb.position = targetKevinPosition;
            Kevin.instance.rb.rotation = targetKevinRotation;
        }

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            if (Levels.LevelManager.levelCurrent.checkpointCurrent == this) {
                return;
            }
            Levels.LevelManager.levelCurrent.ActivateCheckpoint(this, teleportOnActivation);
            Log.Debug("[Checkpoint] '" + name + "' activated.");
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {

        }
    }
}