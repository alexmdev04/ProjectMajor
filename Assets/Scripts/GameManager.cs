using System;
using System.Collections;
using Major.Levels;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Major {
    public class GameManager : MonoBehaviour {
        public static GameManager instance { get; private set; }
        public string playerName { get; private set; } = "Player";

        public static bool isPaused { get; private set; }
        public static bool isInGame { get; private set; }
        public static bool isCursorVisible { get; private set; }
        public static bool startupComplete { get; private set; }
        public static bool isQuitting { get; private set; }
        public static Startup.Settings startupSettings { get; private set; }
        public static event Action onStartupComplete;

        // Debug
        [HideInInspector] public bool dbg_noclipEnabled;
        [HideInInspector] public float dbg_noclipSpeed = 10.0f;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject kevinPrefab;

        private void Awake() {
            if (instance != null) {
                Log2.Error("There is already an instance of this singleton", "GameManager");
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
            //Application.targetFrameRate = 165;
            QualitySettings.vSyncCount = 1;
            isInGame = true;
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
                Log2.Debug("Startup Complete.", "GameManager");
            };
        }

        private IEnumerator Startup() {
            yield return new WaitForEndOfFrame();
            onStartupComplete();
        }

        private void Update() {
            // Debug Keys
            if (Keyboard.current.tabKey.isPressed) { // using ctrl has issues with browsers
                if (Keyboard.current.f1Key.wasPressedThisFrame) {
                    LevelManager.LoadLevel(AssetKeys.Levels.home, true);
                }

                if (Keyboard.current.f2Key.wasPressedThisFrame) {
                    LevelManager.LoadLevel(AssetKeys.Levels.tutorial, true);
                }

                if (Keyboard.current.f3Key.wasPressedThisFrame) {
                    LevelManager.LoadLevel(AssetKeys.Levels.level1, true);
                }

                if (Keyboard.current.f4Key.wasPressedThisFrame) {
                    LevelManager.LoadLevel(AssetKeys.Levels.doorway, true);
                }

                if (Keyboard.current.f5Key.wasPressedThisFrame) {
                    LevelManager.LoadLevel(AssetKeys.Levels.pitJump, true);
                }

                if (Keyboard.current.f6Key.wasPressedThisFrame) {
                    LevelManager.LoadLevel(AssetKeys.Levels.external1, true);
                }
            }
            else {
                if (Keyboard.current.f1Key.wasPressedThisFrame) {
                    bool state = !dbg_noclipEnabled;
                    dbg_noclipEnabled = state;
                    Player.instance.moveActive = !state;
                    Player.instance.rb.detectCollisions = !state;
                    Player.instance.rb.useGravity = !state;
                    Player.instance.rb.isKinematic = state;
                    Player.instance.autoDropItemsDistance = !state;
                }

                if (Keyboard.current.f5Key.wasPressedThisFrame && LevelManager.levelCurrent) {
                    LevelManager.RestartHard();
                }

                if (Keyboard.current.f6Key.wasPressedThisFrame) {
                    OnPlayerKilled();
                }

                if (Keyboard.current.f7Key.wasPressedThisFrame) {
                    OnKevinKilled();
                }

                if (Keyboard.current.f9Key.wasPressedThisFrame) {
                    QualitySettings.vSyncCount = QualitySettings.vSyncCount == 1 ? 0 : 1;
                }

                if (Keyboard.current.equalsKey.wasPressedThisFrame) {
                    Input.Handler.instance.sensitivity = Mathf.Clamp(Input.Handler.instance.sensitivity + 0.1f, 0.0f, float.MaxValue);
                }

                if (Keyboard.current.minusKey.wasPressedThisFrame) {
                    Input.Handler.instance.sensitivity = Mathf.Clamp(Input.Handler.instance.sensitivity - 0.1f, 0.0f, float.MaxValue);
                }
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
            isCursorVisible = state;
            Cursor.lockState = isCursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isCursorVisible;
        }

        public void SetPause(bool state) {
            if (!isInGame) { return; }
            SetCursorVisible(state);
            isPaused = state;
            if (state) {
                // UI.instance.SetMenuState(UI.MenuState.paused);
                Time.timeScale = 0.0f;
                Input.Handler.instance.input.Player.Disable();
                Input.Handler.instance.input.Player.Pause.Enable();
            }
            else {
                // UI.instance.SetMenuState(UI.MenuState.inGame);
                Time.timeScale = 1.0f;
                Input.Handler.instance.input.Player.Enable();
            }
        }

        public void SetPlayerName(string input) => playerName = input;

        public void OnPlayerKilled() {
            if (Player.instance.carriedItem) {
                Player.instance.DropCarriedItem();
            }
            OnKevinKilled();
            LevelManager.levelCurrent.checkpointCurrent.TeleportPlayer();
        }

        public void OnKevinKilled() {
            if (Player.instance.carriedItem == Kevin.instance.item) {
                Player.instance.DropCarriedItem();
            }
            if (Kevin.instance.item.itemSlot) {
                Kevin.instance.item.itemSlot.Release(false);
            }
            LevelManager.levelCurrent.checkpointCurrent.TeleportKevin();
        }

        public void OnPlayerDestroyed() {
            Log2.Error("Player object was destroyed, do not do this. Respawning and restarting level.", "GameManager");
            var newPlayer = Instantiate(playerPrefab).GetComponent<Player>();
            newPlayer.OnRespawn();
            LevelManager.RestartHard();
        }

        public void OnKevinDestroyed() {
            Log2.Error("Kevin object was destroyed, do not do this. Respawning and restarting level.", "GameManager");
            var newKevin = Instantiate(kevinPrefab).GetComponent<Kevin>();
            newKevin.OnRespawn();
            LevelManager.RestartHard();
        }

        private void OnDestroy() {
            if (!startupComplete || isQuitting) {
                return;
            }
            Log2.Error("Destroyed.", "GameManager");
        }

        private void OnApplicationQuit() {
            isQuitting = true;
            Log2.Debug("Quitting.", "GameManager");
        }
    }
}