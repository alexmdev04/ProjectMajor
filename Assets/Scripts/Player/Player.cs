using System;
using Unity.Logging;
using Unity.Mathematics;
using UnityEngine;

namespace Major {
    public class Player : MonoBehaviour {
        public static Player instance { get; private set; }
        public Rigidbody rb { get; private set; }
        public GameObject body { get; private set; }

        public float
            acceleration = 15f,
            accelerationAir = 15f;

        public bool
            lookActive = true,
            moveActive = true;

        [SerializeField] private float friction = 0.85f;
        [SerializeField, Range(0f, 0.1f)] private float flatvelMin = 0.1f;

        [SerializeField]
        private float
            walkVelocity = 4f,
            crouchVelocity = 2f,
            sprintVelocity = 6.5f,
            //maxVelocityAir = 6.5f, 
            cameraHeight = 0.825f,
            // movementAcceleration = 0.1f,
            // movementDecceleration = 0.05f,
            jumpForce = 5f,
            crouchSpeed = 3.6576f;
        [SerializeField] private Vector3 groundedCheckBoxSize = Vector3.one;
        [SerializeField] private LayerMask groundedCheckLayer;
        [SerializeField] private World.Item _carriedItem;
        public World.Item carriedItem => _carriedItem;
        [field: SerializeField] public Camera cam { get; private set; }
        public Vector3 eulerAngles;
        public float playerHeightCm = 185.42f; // in cm
        public float playerCrouchHeightCm = 93.98f; // in cm
        private float playerHeight;
        private float playerCrouchHeight;
        private float maxVelocity;

        [Header("Interaction")]
        [SerializeField] private float interactDistance = 100.0f;
        [SerializeField] private LayerMask interactLayerMask = int.MaxValue;
        [SerializeField] private float maxDistance = 10.0f;
        [SerializeField] private float travelSpeed = 15.0f;
        [SerializeField] private float maxItemHoldDistance = 5.0f;
        [SerializeField] private float maxItemHoldSpeed = 5.0f;
        public bool autoDropFarItems = true;

        public bool grounded { get; private set; }
        // => MathF.Round(rb.linearVelocity.y, 3) == 0.0f;

        private bool respawning = false;

        private void Awake() {
            if (!GameManager.startupComplete) {
                return;
            }
            instance = this;
            body = transform.GetChild(0).gameObject;
            rb = GetComponent<Rigidbody>();
            cam = Camera.main;
            cam.transform.localPosition = new Vector3(0.0f, cameraHeight, 0.0f);
            playerHeight = playerHeightCm / 200.0f;
            playerCrouchHeight = playerCrouchHeightCm / 200.0f;
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
            //UpdateCrouch();
            UpdateCarriedItem();
        }

        private void FixedUpdate() {
            GroundedCheck();
            if (moveActive) { UpdateMove(); }
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

        // private void UpdateCrouch() {
        //     // overkill early exit
        //     // if (!crouched && currentHeight >= playerHeight) {
        //     //     return;
        //     // }

        //     var bodyTransform = _body.transform;

        //     // depending on input move between playerHeight and crouchHeight by crouchSpeed
        //     var height = Mathf.Clamp(
        //         bodyTransform.localPosition.y + (crouchSpeed * Time.deltaTime * (Input.Handler.instance.crouched ? -1.0f : 1.0f)),
        //         playerCrouchHeight,
        //         playerHeight
        //     );

        //     // == Vector3(0,h,0)
        //     var offset = Vector3.up * height;

        //     // edit the mesh as a child of the collider
        //     bodyTransform.localPosition = offset;

        //     var bodyScale = bodyTransform.localScale;
        //     bodyScale.y = height;
        //     bodyTransform.localScale = bodyScale;

        //     // edit the collider
        //     _capsuleCollider.center = offset;
        //     _capsuleCollider.height = height * 2.0f;
        // }

        private void Interact() {
            // Check if the player has an item and drop it
            if (DropCarriedItem()) {
                return;
            }

            // get the hit
            if (!Physics.Raycast(
                new Ray(cam.transform.position, cam.transform.forward),
                out var hit,
                interactDistance,
                interactLayerMask)) {
                return;
            }

            if (!hit.transform.TryGetComponent(out World.Interactable interactable)) {
                return;
            }

            interactable.Interact(this);
        }

        private void UpdateCarriedItem() {
            if (!_carriedItem) {
                return;
            }

            if (_carriedItem.rb.isKinematic) {
                return;
            }

            if (rb.linearVelocity.magnitude > maxItemHoldSpeed) {
                DropCarriedItem();
                return;
            }

            var objPos = _carriedItem.rb.position;
            var target = cam.transform.position + (cam.transform.forward * 2.5f);
            var distance = Vector3.Distance(objPos, target);
            UnityEngine.Debug.DrawLine(start: objPos, end: target, color: Color.Lerp(Color.green, Color.red, Mathf.InverseLerp(0.0f, maxItemHoldDistance, distance)), depthTest: false, duration: 0.0f);
            if (autoDropFarItems && distance > maxItemHoldDistance) {
                DropCarriedItem();
                return;
            }

            _carriedItem.rb.linearVelocity =
                (target - objPos).normalized * // direction
                Mathf.Min(Mathf.InverseLerp(0.0f, maxDistance, distance), 1.0f) * // speed
                travelSpeed;
        }

        public void SetCarriedItem(World.Item item) {
            DropCarriedItem();
            item.SetCarriedState(true);
            _carriedItem = item;
        }

        public bool DropCarriedItem() {
            if (_carriedItem) {
                _carriedItem.SetCarriedState(false);
                _carriedItem = null;
                return true;
            }
            return false;
        }
    }
}