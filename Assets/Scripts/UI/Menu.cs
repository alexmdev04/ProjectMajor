using System;
using UnityEngine;

namespace Major.UI {
    public class Menu : MonoBehaviour {
        public enum MenuState {
            none,
            mainMenu,
            pauseMenu,
            levelSelect,
            settings,
            results,
            inGame
        }

        public MenuState menu;
        public float activationDelay = 0.0f;
        public float deactivationDelay = 0.0f;
        public event Action OnActivated;
        public event Action OnActivatedPreDelay;
        public event Action OnDeactivated;
        public event Action OnDeactivatedPreDelay;
    }
}