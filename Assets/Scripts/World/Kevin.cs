using UnityEngine;

namespace Major {
    public class Kevin : MonoBehaviour {
        public static Kevin instance { get; private set; }
        public Rigidbody rb { get; private set; }
        private void Awake() {
            instance = this;
            rb = GetComponent<Rigidbody>(); 
        }
    }
}