using System;
using Unity.Logging;
using UnityEngine;

namespace Major {
    public class DestructionProtection : MonoBehaviour {
        public event Action<GameObject> onDestroyed = (go) => { };
        public event Action<Collider> onColliderDestroyed = (c) => { };
        [HideInInspector] public new Collider collider;
        private bool hasCollider;

        private void Awake() {
            hasCollider = TryGetComponent(out collider);
        }

        private void Start() {
            if (!Levels.Manager.levelCurrent) {
                return;
            }
            Levels.Manager.levelCurrent.onLevelUnloaded += ClearCallbacks;
        }

        private void OnDestroy() {
            onDestroyed(gameObject);
            if (hasCollider) {
                onColliderDestroyed(collider);
            }
        }

        private void OnApplicationQuit() {
            // Destruction protection is not needed if the app is quitting
            ClearCallbacks(Levels.Manager.levelCurrent);
        }

        public void ClearCallbacks(Levels.Level level) {
            onDestroyed = (go) => { };
            onColliderDestroyed = (c) => { };            
        }
    }
}