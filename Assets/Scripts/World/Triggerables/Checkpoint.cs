using Unity.Logging;
using UnityEngine;

namespace Major.World {
    public class Checkpoint : Triggerable {
        public bool firstCheckpointInLevel = false;
        public bool useObjectTransformAsSpawn = true;
        public bool spawnPlayer = true;
        public bool spawnKevin = true;
        public bool killVelocity = true;
        public bool teleportOnActivation = false;
        public Vector3 playerSpawnLocation = Vector3.zero;
        public Vector3 playerSpawnRotation = Vector3.zero;
        public Vector3 kevinSpawnPosition = Vector3.zero;
        public Vector3 kevinSpawnRotation = Vector3.zero;

        public void Teleport() {
            var targetPlayerPosition = useObjectTransformAsSpawn ? transform.position : playerSpawnLocation;

            if (spawnPlayer) {
                Log.Debug("[Checkpoint] Teleporting Player...");
                var targetPlayerEulerAngles = useObjectTransformAsSpawn ? transform.eulerAngles : playerSpawnRotation;

                if (killVelocity) {
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


            if (spawnKevin) {
                Log.Debug("[Checkpoint] Teleporting Kevin...");
                var targetKevinPosition = useObjectTransformAsSpawn ? targetPlayerPosition + (transform.forward * 1.5f) : kevinSpawnPosition;
                var targetKevinEulerAngles = useObjectTransformAsSpawn ? Vector3.zero : kevinSpawnRotation;

                if (killVelocity) {
                    Kevin.instance.rb.linearVelocity = Vector3.zero;
                    Kevin.instance.rb.angularVelocity = Vector3.zero;
                }

                var targetKevinRotation = Quaternion.Euler(targetKevinEulerAngles);
                Kevin.instance.transform.position = targetKevinPosition;
                Kevin.instance.transform.rotation = targetKevinRotation;
                Kevin.instance.rb.position = targetKevinPosition;
                Kevin.instance.rb.rotation = targetKevinRotation;
            }
        }

        protected override void OnTriggered(Trigger senderTrigger, GameObject sender) {
            Levels.LevelManager.levelCurrent.ActivateCheckpoint(this, teleportOnActivation);
        }

        protected override void OnUntriggered(Trigger senderTrigger, GameObject sender) {
            
        }
    }
}