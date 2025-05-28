using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Major.Levels {
    public class LevelManager : MonoBehaviour {
        public static LevelManager instance { get; private set; }
        public static List<Level> levelsLoaded { get; private set; }
        
        private void Awake() {
            instance = this;
        }

        public async Task LoadLevel(string name) {
            
        }

        public async Task UnloadLevel(Level level) {
            
        }
    }
}