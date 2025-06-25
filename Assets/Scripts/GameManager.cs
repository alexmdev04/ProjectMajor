using System;
using System.Collections;
using Major.Levels;
using Unity.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Per client manager for general Game and UI management
namespace Major {
    public class GameManager : MonoBehaviour {
        public static GameManager instance { get; private set; }
        public bool paused { get; private set; }
        public bool inGame { get; private set; }
        public string playerName { get; private set; } = "Player";

        public static bool startupComplete { get; private set; }
        public static bool quitting { get; private set; }
        public static Startup.Settings startupSettings { get; private set; }
        public static event Action onStartupComplete;

        // Debug
        [HideInInspector] public bool dbg_noclipEnabled;
        [HideInInspector] public float dbg_noclipSpeed = 10.0f;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject kevinPrefab;

        private void Awake() {
            if (instance != null) {
                UnityEngine.Debug.LogError("[GameManager] There is already an instance of this singleton");
                return;
            }
            if (!startupComplete) {
                return;
            }
            instance = this;
        }

        public void Start() {
            if (!startupComplete) {
                return;
            }
            Application.targetFrameRate = 165;
            QualitySettings.vSyncCount = 1;
            inGame = true;
            Addressables.InitializeAsync();
            SetCursorVisible(false);
            Input.Handler.instance.OnPause += SetPause;
            StartCoroutine(Startup());
        }

        public static void OnStartupComplete(Startup.Settings newStartupSettings) {
            startupSettings = newStartupSettings;
            startupComplete = true;
            onStartupComplete = () => {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
                Log.Debug("[GameManager] Startup Complete.");
            };
        }

        private IEnumerator Startup() {
            yield return new WaitForEndOfFrame();
            onStartupComplete();
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

            if (Keyboard.current.f6Key.wasPressedThisFrame) {
                OnPlayerKilled();
            }

            if (Keyboard.current.f7Key.wasPressedThisFrame) {
                OnKevinKilled();
            }

            if (dbg_noclipEnabled) {
                Player.instance.transform.position += Player.instance.cam.transform.TransformDirection(Input.Handler.instance.movementDirection) * (dbg_noclipSpeed * Time.deltaTime);
                dbg_noclipSpeed = Mathf.Clamp(dbg_noclipSpeed + Mouse.current.scroll.value.y, 0.0f, 100.0f);
            }
        }

        private void OnDisable() {
            if (!startupComplete) {
                return;
            }
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

        public void OnPlayerKilled() {
            LevelManager.levelCurrent.GoToCheckpoint();
        }

        public void OnKevinKilled() {
            LevelManager.levelCurrent.checkpointCurrent.TeleportKevin();
            if (Player.instance.carriedItem == Kevin.instance.item) {
                Player.instance.DropCarriedItem();
            }
        }

        public void OnPlayerDestroyed() {
            Log.Error("[GameManager] Player object was destroyed, do not do this. Respawning and restarting level.");
            var newPlayer = Instantiate(playerPrefab).GetComponent<Player>();
            newPlayer.OnRespawn();
            LevelManager.instance.RestartHard();
        }

        public void OnKevinDestroyed() {
            Log.Error("[GameManager] Kevin object was destroyed, do not do this. Respawning and restarting level.");
            var newKevin = Instantiate(kevinPrefab).GetComponent<Kevin>();
            newKevin.OnRespawn();
            LevelManager.instance.RestartHard();
        }

        private void OnDestroy() {
            if (!startupComplete || quitting) {
                return;
            }
            Log.Error("[GameManager] Destroyed.");
        }

        private void OnApplicationQuit() {
            quitting = true;
            Log.Debug("[GameManager] Quitting.");
        }
    }
}