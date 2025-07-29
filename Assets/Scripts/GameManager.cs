using System;
using System.Collections;
using System.Collections.Generic;
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
        public static int baseFrameRateLimit { get; private set; } = 0;
        public static int baseVsyncCount { get; private set; } = 0;
        public static Dictionary<string, float> skillRatingData { get; private set; } = new();
        public static Startup.Settings startupSettings { get; private set; }
        public static event Action onStartupComplete;
        public static event Action onStartGame = () => {};
        public static event Action onPlayerKilled = () => {};
        public static event Action onKevinKilled = () => {};

        // Debug
        [HideInInspector] public bool dbg_noclipEnabled { get; private set; }
        [HideInInspector] public float dbg_noclipSpeed = 15.0f;
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject kevinPrefab;
        public static bool gameCompleted { get; private set; }

        private void Awake() {
            if (instance != null) {
                Log2.Error("There is already an instance of this singleton", "GameManager");
                return;
            }
            if (!startupComplete) {
                return;
            }
            instance = this;
            QualitySettings.vSyncCount = 0;
#if !UNITY_EDITOR
            Application.wantsToQuit += () => {
                if (UI.Popup.resultsVisible || !Tester.consent) {
                    return true;
                }
                QuitGame();
                return false;
            };
#endif
        }

        public void Start() {
            if (!startupComplete) {
                return;
            }
            if (!PlayerPrefs.HasKey("timeplayed")) {
                PlayerPrefs.SetFloat("timeplayed", 0.0f);
            }
            // if (!PlayerPrefs.HasKey("gamecompleted")) {
            //     PlayerPrefs.SetInt("gamecompleted", 0);
            // }
            Addressables.InitializeAsync();
            Input.Handler.OnPause += SetPause;
            StartCoroutine(Startup());
        }

        public static void SetFrameRate(int value) {
            baseFrameRateLimit = value;
            if (!Tester.instance.enabled) {
                Application.targetFrameRate = baseFrameRateLimit;
            }
            else {
                Log2.Warning("FPS limits will be ignored while testing is enabled.", "DebugConsole");                
            }
        }

        public static void SetVsyncCount(int value) {
            baseVsyncCount = value;
            if (!Tester.instance.enabled) {
                QualitySettings.vSyncCount = baseVsyncCount;
            }
            else {
                Log2.Warning("Vsync count will be ignored while testing is enabled.", "DebugConsole");
            }
        }

        public static void OnStartupComplete(Startup.Settings newStartupSettings) {
            startupSettings = newStartupSettings;
            startupComplete = true;
            skillRatingData.Add(startupSettings.firstLevel, 0.5f);
            onStartupComplete = () => {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
                if (startupSettings.startupMessage) {
                    UI.Popup.Create(
                        startupSettings.startupMessageTitle,
                        startupSettings.startupMessageBody,
                        new UI.Popup.ButtonConstructor[] {
                            new("Quit", (popup) => { QuitToDesktop(); }),
                            new("itch.io", (popup) => { Application.OpenURL("https://xae0.itch.io/quadrasylum"); }),
                            new("Agree", (popup) => { popup.Destroy(); UI.UI.Fade(fadeIn: 1.0f); }),
                        }
                    );
#if UNITY_ANDROID
                    UI.Popup.Create(
                        "Notice",
                        "This game does not have on-screen controls.\n" +
                        "You will need to connect a controller.",
                        new UI.Popup.ButtonConstructor[] {
                            new("Ok", (popup) => { popup.Destroy(); }),
                        }
                    );                    
#endif
                }
                else {
                    UI.UI.Fade(fadeIn: 1.0f);
                }
                Levels.Manager.onNextLevelLoaded += (level) => {
                    OnMainMenuStart();
                };
                Log2.Debug("Startup Complete.", "GameManager");
            };
        }

        private static void OnMainMenuStart() {
            var mainMenu = UI.UI.menus["main"];
            Player.instance.DropCarriedItem();
            Player.instance.moveActive = false;
            Player.instance.lookActive = false;
            Player.instance.rb.isKinematic = true;
            SetCursorVisible(true);
        }

        public static void ReturnToMainMenu(bool loadLevel = true) {
            if (UI.UI.currentMenuString == "main") {
                return;
            }

            if (loadLevel) {
                Levels.Manager.LoadLevel("mainmenu");
                Levels.Manager.onNextLevelLoaded += (level) => {
                    OnMainMenuStart();
                };
            }
            SetPause(false);
            UI.UI.SetMenu("main");

            isInGame = false;
        }

        private IEnumerator Startup() {
            yield return new WaitForEndOfFrame();
            onStartupComplete();
        }

        private void Update() {
            if (dbg_noclipEnabled) {
                Player.instance.rb.position += Player.instance.cam.transform.TransformDirection(Input.Handler.movementDirection) * (dbg_noclipSpeed * Time.deltaTime);
                dbg_noclipSpeed = Mathf.Clamp(dbg_noclipSpeed + Mouse.current.scroll.value.y, 0.0f, 100.0f);
            }
        }

        private void OnDisable() {
            if (!startupComplete) {
                return;
            }
            Input.Handler.OnPause -= SetPause;
            PlayerPrefs.Save();
        }

        public static void StartGame(bool loadRecentLevel = false) {
            Levels.Manager.LoadLevel(
                loadRecentLevel ?
                    PlayerPrefs.HasKey("recentlevel") ? PlayerPrefs.GetString("recentlevel") : startupSettings.firstLevel :
                    startupSettings.firstLevel
            );
            SetCursorVisible(false);
            Player.instance.moveActive = true;
            Player.instance.lookActive = true;
            Player.instance.rb.isKinematic = false;
            isInGame = true;
            onStartGame();
        }

        public static void QuitGame() {
            UI.UI.SetMenu("none");
            UI.Popup.Results();
        }

        public static void QuitToDesktop() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        public static void SetCursorVisible(bool state) {
            isCursorVisible = state;
            Cursor.lockState = isCursorVisible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isCursorVisible;
        }

        public static void SetPause(bool state) {
            if (Debug.Console.instance.gameObject.activeSelf) {
                Debug.Console.instance.gameObject.SetActive(false);
                return;
            }
            if (!isInGame) { return; }

            SetCursorVisible(state);
            isPaused = state;
            if (state) {
                UI.UI.SetMenu("pause");
                Time.timeScale = 0.0f;
                Input.Handler.input.Player.Disable();
                Input.Handler.input.Player.Pause.Enable();
            }
            else {
                UI.UI.SetMenu("hud");
                Time.timeScale = 1.0f;
                Input.Handler.input.Player.Enable();
            }
        }

        public void SetPlayerName(string input) => playerName = input;

        public void OnPlayerKilled() {
            Log2.Debug("You died.", "DebugConsole", true);

            if (Player.instance.carriedItem) {
                Player.instance.DropCarriedItem();
            }
            OnKevinKilled(true);
            Levels.Manager.levelCurrent.checkpointCurrent.TeleportPlayer();
            onPlayerKilled();
        }

        public void OnKevinKilled(bool killedWithPlayer = false) {
            Log2.Debug("The cube was destroyed.", "DebugConsole", true);

            if (Player.instance.carriedItem == Kevin.instance.item) {
                Player.instance.DropCarriedItem();
            }
            if (Kevin.instance.item.itemSlot) {
                Kevin.instance.item.itemSlot.Release(false);
            }
            Levels.Manager.levelCurrent.checkpointCurrent.TeleportKevin();
            if (!killedWithPlayer) { onKevinKilled(); }
        }

        public void OnPlayerDestroyed() {
            Log2.Error("Player object was destroyed, do not do this. Respawning and restarting level.", "GameManager");
            var newPlayer = Instantiate(playerPrefab).GetComponent<Player>();
            newPlayer.OnRespawn();
            Levels.Manager.RestartHard();
        }

        public void OnKevinDestroyed() {
            Log2.Error("Kevin object was destroyed, do not do this. Respawning and restarting level.", "GameManager");
            var newKevin = Instantiate(kevinPrefab).GetComponent<Kevin>();
            newKevin.OnRespawn();
            Levels.Manager.RestartHard();
        }

        private void OnDestroy() {
            if (!startupComplete || isQuitting) {
                return;
            }
            Log2.Error("Destroyed.", "GameManager");
        }

        private void OnApplicationQuit() {
            isQuitting = true;
            PlayerPrefs.SetFloat("timeplayed", PlayerPrefs.GetFloat("timeplayed") + Time.time);
            Log2.Debug("Quitting.", "GameManager");
        }

        public void Dbg_ToggleNoclip() {
            Dbg_SetNoclipActive(!dbg_noclipEnabled);
        }

        public void Dbg_SetNoclipActive(bool state) {
            dbg_noclipEnabled = state;
            Player.instance.moveActive = !state;
            Player.instance.rb.detectCollisions = !state;
            Player.instance.rb.useGravity = !state;
            Player.instance.rb.isKinematic = state;
            Player.instance.autoDropItemsDistance = !state;
        }

        public static void OnGameCompleted() {
            gameCompleted = true;
            // PlayerPrefs.SetInt("gamecompleted", 1);
            ReturnToMainMenu(false);
            Levels.Manager.onNextLevelLoaded += (level) => {
                OnMainMenuStart();
                level.GoToCheckpoint();
            };
            UI.UI.SetMenu("none");
            UI.Popup.Results();
            Tester.SendGameComplete();
        }
    }
}