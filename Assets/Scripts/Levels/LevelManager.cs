using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using Unity.Logging;
using UnityEngine.SceneManagement;

namespace Major.Levels {
    public class LevelManager : MonoBehaviour {
        public static LevelManager instance { get; private set; }
        public static Level levelCurrent { get; private set; }
        public static Dictionary<string, LevelAsset> levelDatabase { get; private set; }
        public const string levelKey_Intro = "intro";

        private void Awake() {
            instance = this;
        }

        private void Start() {
            // Cache all level data so it can be used to load levels later
            levelDatabase = new();
            foreach (LevelAsset levelAsset in Addressables.LoadAssetsAsync<LevelAsset>("level").WaitForCompletion()) {
                levelDatabase.Add(levelAsset.name, levelAsset);
            }
        }

        private void Update() {
            // Debug Keys
            if (Keyboard.current.f1Key.wasPressedThisFrame) {
                LoadLevel(levelKey_Intro);
            }

            if (Keyboard.current.f2Key.wasPressedThisFrame) {
                levelCurrent.UnloadScene(levelKey_Intro);
            }

            if (Keyboard.current.f3Key.wasPressedThisFrame) {
                levelCurrent.LoadSceneAsync(levelKey_Intro);
            }
        }

        public async void LoadLevel(string key) {
            // Loads a new level via its name/key and attaches it to an empty game object
            var levelAsset = await Addressables.LoadAssetAsync<LevelAsset>(key).Task;
            var newObj = new GameObject(key);
            var newLevel = newObj.AddComponent<Level>();
            newLevel.Construct(await levelAsset.LoadAsync(key, true, true));
            levelCurrent = newLevel;
        }
    }
}