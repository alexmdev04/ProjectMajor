using System;
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
        private void OnDestroy() {
            onDestroyed(gameObject);
            if (hasCollider) {
                onColliderDestroyed(collider);
            }
        }
    }
}