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
        private int overrideGrounded = 0;
        public bool grounded { get; private set; }


        [Header("General")]
        public bool lookActive = true;
        [SerializeField] private LayerMask groundedCheckLayer;
        [SerializeField] private Vector3 groundedCheckBoxSize = new(0.1f, 0.01f, 0.1f);
        [SerializeField]
        private float
            toCrouchSpeed = 3.5f,
            playerHeight = 1.0f,
            playerCrouchHeight = 0.6f,
            cameraHeight = 0.85f;
        public Vector3 eulerAngles {
            get {
                return new(
                    cam.transform.localEulerAngles.x,
                    body.transform.localEulerAngles.y,
                    0.0f
                );
            }
            set {
                body.transform.localEulerAngles = new(0.0f, value.y, 0.0f);
                cam.transform.localEulerAngles = new(value.x, 0.0f, 0.0f);
            }
        }


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
        [SerializeField]
        private float
            interactDistance = 3.0f,
            itemDistanceForMaxSpeed = 10.0f,
            itemTravelSpeed = 50.0f,
            itemMaxHeldDistance = 5.0f,
            itemMaxHeldPlayerSpeed = 10.0f;
        public bool autoDropItemsDistance = true;
        public bool autoDropItemsPlayerSpeed = false;
        public bool autoDropItemsBehindWall = false;

        // Privates
        private bool respawning = false;
        private float maxVelocity;
        public bool decelerateToMaxVelocity = false;

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
            Input.Handler.OnJump += Jump;
            Input.Handler.OnInteract += Interact;
        }

        private void Update() {
            if (transform.position.y < -100.0f) {
                GameManager.instance.OnPlayerKilled();
            }
            UpdateCrouch();
        }

        private void FixedUpdate() {
            GroundedCheck();
            if (moveActive) { UpdateMove(); }
            UpdateCarriedItem();
            UpdateInteract();
        }

        private void LateUpdate() {
            if (lookActive) { Look(); }
        }

        private void OnDestroy() {
            if (!GameManager.startupComplete || GameManager.isQuitting) {
                return;
            }
            Input.Handler.OnJump -= Jump;
            Input.Handler.OnInteract -= Interact;
            GameManager.instance.OnPlayerDestroyed();
        }

        public static void OverrideInstance(Player newInstance) => instance = newInstance;

        private void Look() {
            Vector2 lookDelta = Input.Handler.sensitivity * Input.Handler.lookDelta;
            body.transform.localRotation *= Quaternion.AngleAxis(lookDelta.x, Vector3.up);
            cam.transform.localEulerAngles = new Vector3(
                Mathf.Clamp(((cam.transform.localEulerAngles.x + 180.0f) % 360.0f - 180.0f) - lookDelta.y, -90.0f, 90.0f),
                0.0f,
                0.0f
            );
        }

        private void UpdateMove() {
            float accel = acceleration;
            Vector3 movementDirectionGlobal = body.transform.TransformDirection(Input.Handler.movementDirection);

            if (grounded) {
                maxVelocity = Input.Handler.crouched ? crouchVelocity : Input.Handler.sprinting ? sprintVelocity : walkVelocity;

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

            rb.linearVelocity += decelerateToMaxVelocity ?
                movementDirectionGlobal * accelVel :
                movementDirectionGlobal * Math.Max(accelVel, 0.0f);
        }

        private void GroundedCheck() {
            // Prevents remaining grounded for a single fixed update after gaining the necessary force
            if (overrideGrounded > 0) {
                overrideGrounded--;
                return;
            }
            grounded = Physics.OverlapBox(center: rb.position, halfExtents: groundedCheckBoxSize, orientation: Quaternion.identity, layerMask: groundedCheckLayer).Length > 0;
        }

        private void Jump() {
            if (!grounded) { return; }
            rb.AddForce(jumpForce * Vector3.up, ForceMode.VelocityChange);
        }

        private void UpdateCrouch() {
            // overkill early exit
            var bodyTransform = body.transform;
            var currentHeight = bodyTransform.localPosition.y;
            var crouched = Input.Handler.crouched;

            if (!crouched && currentHeight >= playerHeight) {
                return;
            }

            // depending on input move between playerHeight and crouchHeight by crouchSpeed
            var height = Mathf.Clamp(
                currentHeight + (toCrouchSpeed * Time.deltaTime * (crouched ? -1.0f : 1.0f)),
                playerCrouchHeight,
                playerHeight
            );

            var offset = Vector3.up * height;
            bodyTransform.localPosition = offset;
            var bodyScale = bodyTransform.localScale;
            bodyScale.y = height;
            bodyTransform.localScale = bodyScale;
        }

        private void UpdateInteract() {
            var isFacingInteractable = TryGetFacingInteractable(out var interactable, out var hit, out var isFacingWall);

            if (carriedItem) {
                UI.UI.ShowInteractPrompt("Drop");
                if (autoDropItemsBehindWall && isFacingWall && hit.distance < Vector3.Distance(transform.position, carriedItem.transform.position)) {
                    Log2.Debug("The object was dropped because it's behind a wall!", "Player", true);
                    DropCarriedItem();
                }
            }
            else if (isFacingInteractable) {
                UI.UI.ShowInteractPrompt(interactable.GetPrompt());
            }
            else {
                UI.UI.HideInteractPrompt();
            }
        }

        private bool TryGetFacingInteractable(out World.Interactable interactable, out RaycastHit hit, out bool facingWall) {
            if (!Physics.Raycast(
                new Ray(cam.transform.position, cam.transform.forward),
                out hit,
                interactDistance,
                interactLayerMask
            )) {
                interactable = null;
                hit = new();
                facingWall = false;
                return false;
            }

            if (facingInteractable) {
                if (hit.collider.gameObject == facingInteractable.gameObject) {
                    interactable = facingInteractable;
                    facingWall = false;
                    return true;
                }
            }

            var isFacingInteractable = hit.collider.TryGetComponent(out interactable);
            facingWall = !isFacingInteractable;
            return isFacingInteractable;
        }

        private void Interact() {
            // Check if the player has an item and drop it
            if (DropCarriedItem()) {
                return;
            }

            if (!TryGetFacingInteractable(out var interactable, out var hit, out _)) {
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
                Log2.Debug("The object was dropped because the player is too fast!", "Player", true);
                DropCarriedItem();
                return;
            }

            var objPos = carriedItem.rb.position;
            var target = cam.transform.position + (cam.transform.forward * 2.5f);
            var distance = Vector3.Distance(objPos, target);
            UnityEngine.Debug.DrawLine(start: objPos, end: target, color: Color.Lerp(Color.green, Color.red, Mathf.InverseLerp(0.0f, itemMaxHeldDistance, distance)), depthTest: false, duration: 0.0f);
            if (autoDropItemsDistance && distance > itemMaxHeldDistance) {
                DropCarriedItem();
                Log2.Debug("The object was dropped for being too far away!", "Player", true);
                return;
            }

            carriedItem.rb.linearVelocity =
                itemTravelSpeed *
                Mathf.Min(Mathf.InverseLerp(0.0f, itemDistanceForMaxSpeed, distance), 1.0f) *
                (target - objPos).normalized;
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

        public void OverrideGrounded(bool state, int frames = 1) {
            overrideGrounded += frames;
            grounded = state;
        }
    }
}