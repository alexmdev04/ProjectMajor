using System;
using UnityEngine;

namespace Major.Interact {
    public abstract class Interactable : MonoBehaviour {
        public abstract void Interact(Player sender, Action callback = null);
    }
}