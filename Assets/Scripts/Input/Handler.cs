using System;
using UnityEngine;

namespace Major.Input {
    public class Handler : MonoBehaviour {
        public static Handler instance { get; private set; }
        public Actions input { get; private set; }
        public bool sprinting { get; private set; }
        public bool crouched { get; private set; }
        public Vector2 mouseDelta { get; private set; }
        public Vector2 scrollDelta { get; private set; }
        public Vector3 movementDirection;// { get; private set; }
        // public float sensitivity {
        //     get => _sens;
        //     set {
        //         PlayerPrefs.SetFloat(sensPrefKey, value);
        //         _sens = value;
        //     }
        // }
        public float sensitivity = 1.0f;
        private float _sens = 1.0f;
        public event Action OnJump = () => { };
        public event Action OnInteract = () => { };
        public event Action<bool> OnPause = paused => { };
        private const string sensPrefKey = "sensitivity";

        private void Awake() {
            instance = this;
            input = new();
            input.Player.Enable();
        }

        private void Start() {
            // if (PlayerPrefs.HasKey(sensPrefKey)) {
            //     UI.instance.SetSensitivity(PlayerPrefs.GetFloat(sensPrefKey).ToString());
            // }
        }

        private void Update() {
            // mouse vector
            mouseDelta = input.Player.Look.ReadValue<Vector2>();

            // movement vector
            var vec = input.Player.Movement.ReadValue<Vector2>();
            movementDirection = new Vector3(vec.x, 0.0f, vec.y);

            // scroll vector
            scrollDelta = input.Player.Scroll.ReadValue<Vector2>();

            // sprint
            sprinting = input.Player.Sprint.IsPressed();

            // crouch
            crouched = input.Player.Crouch.IsPressed();

            // jump
            if (input.Player.Jump.WasPressedThisFrame()) {
                OnJump();
            }

            // interact
            if (input.Player.Interact.WasPressedThisFrame()) {
                OnInteract();
            }

            // pause
            if (input.Player.Pause.WasPressedThisFrame()) {
                OnPause(!GameManager.instance.paused);
            }
        }

        private void OnDisable() {
            input.Player.Disable();
        }
    }
}