using UnityEngine;

namespace Major {
    public class Kevin : MonoBehaviour {
        public static Kevin instance { get; private set; }
        public Rigidbody rb { get; private set; }
        public World.Item item { get; private set; }
        private void Awake() {
            if (!GameManager.startupComplete) {
                return;
            }
            instance = this;
            rb = GetComponent<Rigidbody>();
            item = GetComponent<World.Item>();
        }
    }
}