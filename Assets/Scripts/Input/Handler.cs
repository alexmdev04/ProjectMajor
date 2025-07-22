using System;
using UnityEngine;

namespace Major.Input {
    public class Handler : MonoBehaviour {
        public static Actions input { get; private set; }
        public static bool sprinting { get; private set; }
        public static bool crouched { get; private set; }
        public static Vector2 lookDelta { get; private set; }
        public static Vector2 scrollDelta { get; private set; }
        public static Vector3 movementDirection { get; private set; }
        public static float sensitivity;
        public static event Action OnJump = () => { };
        public static event Action OnInteract = () => { };
        public static event Action<bool> OnPause = paused => { };
        private const string sensPrefKey = "sensitivity";

        private void Awake() {
            if (!GameManager.startupComplete) {
                return;
            }
            input = new();
        }

        private void Start() {
            if (PlayerPrefs.HasKey(sensPrefKey)) {
                sensitivity = PlayerPrefs.GetFloat(sensPrefKey);
            }
            else {
                sensitivity = 1.0f;
            }
        }

        private void Update() {
            // mouse vector
            lookDelta = input.Player.Look.ReadValue<Vector2>();

            // movement vector
            var vec = input.Player.Movement.ReadValue<Vector2>();
            movementDirection = new Vector3(vec.x, 0.0f, vec.y);

            // scroll vector
            scrollDelta = input.Player.Scroll.ReadValue<Vector2>();
        }

        private void OnEnable() {
            if (!GameManager.startupComplete) {
                return;
            }
            input.Player.Enable();
            input.Player.Sprint.started += (ctx) => sprinting = true;
            input.Player.Sprint.canceled += (ctx) => sprinting = false;
            input.Player.Crouch.started += (ctx) => crouched = true;
            input.Player.Crouch.canceled += (ctx) => crouched = false;
            input.Player.Jump.performed += (ctx) => OnJump();
            input.Player.Interact.performed += (ctx) => OnInteract();
            input.Player.Pause.performed += (ctx) => OnPause(!GameManager.isPaused);
            //input.Player.RestartLevel.performed += (ctx) => Levels.Manager.RestartHard();
            //input.Player.RestartCheckpoint.performed += (ctx) => Levels.Manager.RestartSoft();
            //input.Player.RestartCheckpointKevin.performed += (ctx) => GameManager.instance.OnKevinKilled();
            input.Player.Console.performed += (ctx) => Debug.Console.Toggle();
        }

        private void OnDisable() {
            if (!GameManager.startupComplete) {
                return;
            }
            input.Player.Disable();
            input.Player.Sprint.started -= (ctx) => sprinting = true;
            input.Player.Sprint.canceled -= (ctx) => sprinting = false;
            input.Player.Crouch.started -= (ctx) => crouched = true;
            input.Player.Crouch.canceled -= (ctx) => crouched = false;
            input.Player.Jump.performed -= (ctx) => OnJump();
            input.Player.Interact.performed -= (ctx) => OnInteract();
            input.Player.Pause.performed -= (ctx) => OnPause(!GameManager.isPaused);
            //input.Player.RestartLevel.performed -= (ctx) => Levels.Manager.RestartHard();
            //input.Player.RestartCheckpoint.performed -= (ctx) => Levels.Manager.RestartSoft();
            //input.Player.RestartCheckpointKevin.performed -= (ctx) => GameManager.instance.OnKevinKilled();
            input.Player.Console.performed -= (ctx) => Debug.Console.Toggle();
        }

        private void OnApplicationQuit() {
            PlayerPrefs.SetFloat(sensPrefKey, sensitivity);
        }
    }
}