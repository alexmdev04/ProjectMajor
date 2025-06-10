using System;
using UnityEngine;

namespace Major.World {
    public abstract class Interactable : MonoBehaviour {
        public abstract void Interact(Player sender, Action callback = null);
    }
}