using System;
using Unity.Logging;
using Unity.Mathematics;
using UnityEngine;

namespace Major {
    public class Player : MonoBehaviour {
        public static Player instance { get; private set; }
        public Rigidbody rb { get; private set; }
        public GameObject body { get; private set; }
        public World.Item carriedItem { get; private set; }
        public World.Interactable facingInteractable { get; private set; }
        [field: SerializeField] public Camera cam { get; private set; }
        public bool grounded { get; private set; }


        [Header("General")]
        public bool lookActive = true;
        [SerializeField] private LayerMask groundedCheckLayer;
        [SerializeField] private Vector3 groundedCheckBoxSize = new (0.1f, 0.01f, 0.1f);
        [SerializeField] private float
            toCrouchSpeed = 3.5f,
            playerHeight = 1.0f,
            playerCrouchHeight = 0.6f,
            cameraHeight = 0.85f;
        public Vector3 eulerAngles = Vector3.zero;


        [Header("Movement")]
        public bool moveActive = true;
        [SerializeField] private float acceleration = 50.0f;
        [SerializeField] private float accelerationAir = 20.0f;
        [SerializeField] private float friction = 5.0f;
        [SerializeField, Range(0f, 0.1f)] private float flatvelMin = 0.1f;
        [SerializeField] private float walkVelocity = 4.0f;
        [SerializeField] private float crouchVelocity = 2.0f;
        [SerializeField] private float sprintVelocity = 5.0f;
        // [SerializeField] private float maxVelocityAir = 6.5f;
        // [SerializeField] private float movementAcceleration = 0.1f;
        // [SerializeField] private float movementDecceleration = 0.05f;
        [SerializeField] private float jumpForce = 5f;


        [Header("Interaction")]
        [SerializeField] private LayerMask interactLayerMask = int.MaxValue;
        [SerializeField] private float
            interactDistance = 3.0f,
            itemDistanceForMaxSpeed = 10.0f,
            itemTravelSpeed = 50.0f,
            itemMaxHeldDistance = 5.0f,
            itemMaxHeldPlayerSpeed = 10.0f;
        public bool autoDropItemsDistance = true;
        public bool autoDropItemsPlayerSpeed = true;

        // Privates
        private bool respawning = false;
        private float maxVelocity;


        private void Awake() {
            if (!GameManager.startupComplete) {
                return;
            }
            instance = this;
            body = transform.GetChild(0).gameObject;
            rb = GetComponent<Rigidbody>();
            cam = Camera.main;
            cam.transform.localPosition = new Vector3(0.0f, cameraHeight, 0.0f);
        }

        public void OnRespawn() {
            respawning = true;
        }

        private void Start() {
            if (respawning) {
                OnStartupComplete();
                respawning = false;
                return;
            }

            GameManager.onStartupComplete += () => {
                OnStartupComplete();
            };
        }

        private void OnStartupComplete() {
            Input.Handler.instance.OnJump += Jump;
            Input.Handler.instance.OnInteract += Interact;
        }

        private void Update() {
            if (transform.position.y < -10.0f) {
                GameManager.instance.OnPlayerKilled();
            }
            UpdateCrouch();
            UpdateCarriedItem();
        }

        private void FixedUpdate() {
            GroundedCheck();
            if (moveActive) { UpdateMove(); }
            if (carriedItem) {
                UI.UI.instance.ShowInteractPrompt("Drop");
            }
            else {
                UpdateInteractPrompt();
            }
        }

        private void LateUpdate() {
            if (lookActive) { Look(); }
        }

        private void OnDestroy() {
            if (!GameManager.startupComplete || GameManager.quitting) {
                return;
            }
            Input.Handler.instance.OnJump -= Jump;
            Input.Handler.instance.OnInteract -= Interact;
            GameManager.instance.OnPlayerDestroyed();
        }

        public static void OverrideInstance(Player newInstance) => instance = newInstance;

        private void Look() {
            Vector2 mouseDeltaMult = Input.Handler.instance.mouseDelta * Input.Handler.instance.sensitivity;

            eulerAngles = new Vector2(
                Math.Clamp(
                    eulerAngles.x - mouseDeltaMult.y,
                    -90f,
                    90f
                ),
                eulerAngles.y + mouseDeltaMult.x
            );

            body.transform.eulerAngles = new Vector3(0f, eulerAngles.y, 0f);
            cam.transform.localEulerAngles = new Vector3(eulerAngles.x, 0f, 0f);
        }

        private void UpdateMove() {
            float accel = acceleration;
            Vector3 movementDirectionGlobal = body.transform.TransformDirection(Input.Handler.instance.movementDirection);

            if (grounded) {
                maxVelocity = Input.Handler.instance.crouched ? crouchVelocity : Input.Handler.instance.sprinting ? sprintVelocity : walkVelocity;

                // apply friction
                float speed = rb.linearVelocity.magnitude;
                rb.linearVelocity = speed > flatvelMin ?
                    rb.linearVelocity * ((speed - (speed * friction * Time.fixedDeltaTime)) / speed) :
                    Vector3.zero;
            }
            else {
                //maxVelocity = maxVelocityAir;
                accel = accelerationAir;
            }

            float projVel = Vector3.Dot(rb.linearVelocity, movementDirectionGlobal); // Projection of current velocity onto movement dir
            float accelVel = accel * Time.fixedDeltaTime;

            // Clamp acceleration
            if (projVel + accelVel > maxVelocity) {
                accelVel = maxVelocity - projVel;
            }

            rb.linearVelocity += movementDirectionGlobal * accelVel;
        }

        private void GroundedCheck() {
            grounded = Physics.OverlapBox(center: rb.position, halfExtents: groundedCheckBoxSize, orientation: Quaternion.identity, layerMask: groundedCheckLayer).Length > 0;
        }

        private void Jump() {
            if (!grounded) { return; }
            rb.AddForce(jumpForce * Vector3.up, ForceMode.VelocityChange);
        }

        private void UpdateCrouch() {
            // overkill early exit
            // if (!Input.Handler.instance.crouched && currentHeight >= playerHeight) {
            //     return;
            // }

            var bodyTransform = body.transform;

            // depending on input move between playerHeight and crouchHeight by crouchSpeed
            var height = Mathf.Clamp(
                bodyTransform.localPosition.y + (toCrouchSpeed * Time.deltaTime * (Input.Handler.instance.crouched ? -1.0f : 1.0f)),
                playerCrouchHeight,
                playerHeight
            );

            var offset = Vector3.up * height;
            bodyTransform.localPosition = offset;
            var bodyScale = bodyTransform.localScale;
            bodyScale.y = height;
            bodyTransform.localScale = bodyScale;
        }

        private void UpdateInteractPrompt() {
            if (TryGetFacingInteractable(out var interactable, out var hit)) {
                UI.UI.instance.ShowInteractPrompt(interactable.GetPrompt());
            }
            else {
                UI.UI.instance.HideInteractPrompt();
            }
        }

        private bool TryGetFacingInteractable(out World.Interactable interactable, out RaycastHit hit) {
            if (!Physics.Raycast(
                new Ray(cam.transform.position, cam.transform.forward),
                out hit,
                interactDistance,
                interactLayerMask
            )) {
                interactable = null;
                hit = new();
                return false;
            }

            if (facingInteractable) {
                if (hit.collider.gameObject == facingInteractable.gameObject) {
                    interactable = facingInteractable;
                    return true;
                }
            }

            return hit.collider.TryGetComponent(out interactable);
        }

        private void Interact() {
            // Check if the player has an item and drop it
            if (DropCarriedItem()) {
                return;
            }

            if (!TryGetFacingInteractable(out var interactable, out var hit)) {
                return;
            }

            interactable.Interact(this);
        }

        private void UpdateCarriedItem() {
            if (!carriedItem) {
                return;
            }

            if (carriedItem.rb.isKinematic) {
                return;
            }

            if (autoDropItemsPlayerSpeed && rb.linearVelocity.magnitude > itemMaxHeldPlayerSpeed) {
                DropCarriedItem();
                return;
            }

            var objPos = carriedItem.rb.position;
            var target = cam.transform.position + (cam.transform.forward * 2.5f);
            var distance = Vector3.Distance(objPos, target);
            UnityEngine.Debug.DrawLine(start: objPos, end: target, color: Color.Lerp(Color.green, Color.red, Mathf.InverseLerp(0.0f, itemMaxHeldDistance, distance)), depthTest: false, duration: 0.0f);
            if (autoDropItemsDistance && distance > itemMaxHeldDistance) {
                DropCarriedItem();
                return;
            }

            carriedItem.rb.linearVelocity =
                (target - objPos).normalized * // direction
                Mathf.Min(Mathf.InverseLerp(0.0f, itemDistanceForMaxSpeed, distance), 1.0f) * // speed
                itemTravelSpeed;
        }

        public void SetCarriedItem(World.Item item) {
            DropCarriedItem();
            item.SetCarriedState(true);
            carriedItem = item;
        }

        public bool DropCarriedItem() {
            if (carriedItem) {
                carriedItem.SetCarriedState(false);
                carriedItem = null;
                return true;
            }
            return false;
        }
    }
}