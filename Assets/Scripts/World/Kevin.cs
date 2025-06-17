using UnityEngine;

namespace Major.World {
    public class Kevin : MonoBehaviour {
        public static Kevin instance { get; private set; }
        public Rigidbody rb { get; private set; }
        public Item item { get; private set; }
        private void Awake() {
            instance = this;
            rb = GetComponent<Rigidbody>(); 
            item = GetComponent<Item>();
        }
    }
}