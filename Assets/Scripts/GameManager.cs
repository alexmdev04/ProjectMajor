using System;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

// Per client manager for general Game and UI management
namespace Major {
    public class GameManager : MonoBehaviour {
        public static GameManager instance { get; private set; }
        public bool paused { get; private set; }
        public bool inGame { get; private set; }
        public string playerName { get; private set; } = "Player";
        public event Action OnStartupComplete = () => { Log.Debug("[GameManager] Startup Complete."); };

        // Debug
        private bool dbg_noclipEnabled;
        private float dbg_noclipSpeed = 10.0f;

        private void Awake() {
            if (instance != null) {
                UnityEngine.Debug.LogError("[GameManager] There is already an instance of this singleton");
                return;
            }
            instance = this;
            Application.targetFrameRate = 165;
            QualitySettings.vSyncCount = 1;
            inGame = true;
            Addressables.InitializeAsync();
            SetCursorVisible(false);
        }

        private void Start() {
            Input.Handler.instance.OnPause += SetPause;
        }

        private void Update() {
            if (Keyboard.current.f1Key.wasPressedThisFrame) {
                bool state = !dbg_noclipEnabled;
                bool nState = !state;
                dbg_noclipEnabled = state;
                Player.instance.moveActive = nState;
                Player.instance.rb.detectCollisions = nState;
                Player.instance.rb.useGravity = nState;
                Player.instance.rb.isKinematic = state;
                Player.instance.autoDropFarItems = nState;
            }
            if (dbg_noclipEnabled) {
                Player.instance.transform.position += Player.instance.cam.transform.TransformDirection(Input.Handler.instance.movementDirection) * (dbg_noclipSpeed * Time.deltaTime);
                dbg_noclipSpeed = Mathf.Clamp(dbg_noclipSpeed + Mouse.current.scroll.value.y, 0.0f, 100.0f);
            }
        }

        private void OnDisable() {
            Input.Handler.instance.OnPause -= SetPause;
            PlayerPrefs.Save();
        }

        public void QuitToDesktop() {
            Application.Quit();
        }

        public void SetCursorVisible(bool state) {
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = state;
        }

        public void SetPause(bool state) {
            if (!inGame) { return; }
            SetCursorVisible(state);
            paused = state;
            if (state) {
                // UI.instance.SetMenuState(UI.MenuState.paused);
                Input.Handler.instance.input.Player.Disable();
                Input.Handler.instance.input.Player.Pause.Enable();
            }
            else {
                // UI.instance.SetMenuState(UI.MenuState.inGame);
                Input.Handler.instance.input.Player.Enable();
            }
        }

        public void SetPlayerName(string input) => playerName = input;
    }
}