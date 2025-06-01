using UnityEngine;

namespace Major.Interact {
    public abstract class Interactable : MonoBehaviour {
        public abstract void Interact(GameObject sender);
    }
}