using UnityEngine;

// Per client manager for general Game and UI management
namespace Major {
    public class GameManager : MonoBehaviour {
        public static GameManager instance { get; private set; }
        [SerializeField] public GameObject ctsRoot;
        [SerializeField] public GameObject testCube;
        public bool paused { get; private set; }
        public bool inGame { get; private set; }
        public string playerName { get; private set; } = "Player";

        private void Awake() {
            if (instance != null) {
                UnityEngine.Debug.LogError("[GameManager] There is already an instance of this singleton");
                return;
            }
            instance = this;
            Application.targetFrameRate = 165;
            QualitySettings.vSyncCount = 1;
            inGame = true;
        }

        private void Start() {    
            Input.Handler.instance.OnPause += SetPause;
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