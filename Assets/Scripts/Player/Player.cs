using System;
using UnityEngine;
using Unity.Logging;
using System.Threading;
using UnityEngine.UIElements;
using System.Runtime.InteropServices;

namespace Major {
    public class Player : MonoBehaviour {
        public static Player instance { get; private set; }

        public bool
            frictionType;
        public float
            // groundedAccelerate = 15f,
            // airAccelerate = 15f,
            // maxVelocityGrounded = 6.5f,
            // maxVelocityAir = 6.5f,
            acceleration = 15f;

        [Range(0.01f, 1f)] public float friction1 = 0.7f;
        [Range(1f, 5f)] public float friction2 = 1f;

        [Space] public bool moveFixedUpdate;
        public bool
            lookActive = true,
            moveActive = true;
        public Rigidbody rb { get; private set; }

        [SerializeField, Range(0f, 0.99f)]
        private float
            friction = 0.85f;
        [SerializeField, Range(0f, 0.1f)]
        private float
            forceToApplyFriction = 0.1f,
            flatvelMin = 0.1f;
        [SerializeField]
        private float
            walkSpeed = 4f,
            sprintSpeed = 6.5f,
            cameraHeight = 0.825f,
            movementAcceleration = 0.1f,
            movementDecceleration = 0.05f,
            jumpForce = 5f,
            groundedRayDistance = 1f,
            movementRampTime,
            heldObjectDistance = 6.0f,
            crouchSpeed = 3.6576f;
        [SerializeField] private GameObject _body;
        public GameObject body => _body;
        [SerializeField] private CapsuleCollider _capsuleCollider;
        public CapsuleCollider capsuleCollider => _capsuleCollider;
        [SerializeField] private Interact.Item _carriedItem;
        public Interact.Item carriedItem => _carriedItem;
        [SerializeField] private Camera _cam;
        public Camera cam => _cam;
        public Vector2 playerEulerAngles { get; private set; } = Vector2.zero;
        public Vector3 combinedFacingEulerAngles { get; private set; }
        public float playerHeightCm = 185.42f; // in cm
        public float playerCrouchHeightCm = 93.98f; // in cm
        private float playerHeight;
        private float playerCrouchHeight;


        [Header("Interaction")]
        [SerializeField] private float interactDistance = 100.0f;
        [SerializeField] private LayerMask interactLayerMask = int.MaxValue;
        private bool grounded => MathF.Round(rb.linearVelocity.y, 3) == 0;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
            playerHeight = playerHeightCm / 200.0f;
            playerCrouchHeight = playerCrouchHeightCm / 200.0f;
        }

        private void Start() {
            Input.Handler.instance.OnJump += Jump;
            Input.Handler.instance.OnInteract += Interact;
            _cam.transform.localPosition = new Vector3(0.0f, cameraHeight, 0.0f);
        }

        private void OnDisable() {
            Input.Handler.instance.OnJump -= Jump;
            Input.Handler.instance.OnInteract -= Interact;
        }

        private void Update() {
            if (moveActive && !moveFixedUpdate) { UpdateMove(); }
            heldObjectDistance = Math.Clamp(heldObjectDistance + Input.Handler.instance.scrollDelta.y, 1.0f, 15.0f);
            UpdateCrouch();
            UpdateCarriedItem();
        }

        private void FixedUpdate() {
            if (moveActive && moveFixedUpdate) { UpdateMove(); }
        }

        private void LateUpdate() {
            if (lookActive) { Look(); }
        }

        private void Look() {
            Vector2 mouseDeltaMult = Input.Handler.instance.mouseDelta * Input.Handler.instance.sensitivity;

            playerEulerAngles = new Vector2(
                Math.Clamp(
                    playerEulerAngles.x - mouseDeltaMult.y,
                    -90f,
                    90f
                ),
                playerEulerAngles.y + mouseDeltaMult.x
            );

            _capsuleCollider.transform.localEulerAngles = new Vector3(0f, playerEulerAngles.y, 0f);
            _cam.transform.localEulerAngles = new Vector3(playerEulerAngles.x, 0f, 0f);

            // Combines rotation data so that it can be used in external calculations
            combinedFacingEulerAngles = new Vector3(_cam.transform.localEulerAngles.x, _capsuleCollider.transform.localEulerAngles.y, 0.0f);
        }

        private void UpdateMove() {
            float maxVelocity = Input.Handler.instance.sprinting ? sprintSpeed : walkSpeed;
            Vector3 movementDirectionGlobal = _capsuleCollider.transform.TransformDirection(Input.Handler.instance.movementDirection);

            if (grounded) {
                // apply friction
                float speed = rb.linearVelocity.magnitude;

                if (speed <= flatvelMin) {
                    rb.linearVelocity = Vector3.zero;
                }

                if (frictionType) {
                    if (speed > 0) // Scale the velocity based on friction.
                    {
                        rb.linearVelocity *= (speed - (speed * friction2 * Time.fixedDeltaTime)) / speed;
                    }

                }
                else {
                    rb.linearVelocity *= friction1;
                }
            }
            else {
                //maxVelocity = maxVelocityAir;
                //acceleration = airAccelerate;
            }

            float projVel = Vector3.Dot(rb.linearVelocity, movementDirectionGlobal); // Vector projection of Current velocity onto accelDir.
            float accelVel = acceleration * Time.fixedDeltaTime; // Accelerated velocity in direction of movment

            // If necessary, truncate the accelerated velocity so the vector projection does not exceed max_velocity
            if (projVel + accelVel > maxVelocity) {
                accelVel = maxVelocity - projVel;
            }

            rb.linearVelocity += movementDirectionGlobal * accelVel;
        }

        private void Jump() {
            if (!grounded) { return; }
            rb.AddForce(jumpForce * Vector3.up, ForceMode.VelocityChange);
        }

        private void UpdateCrouch() {
            // overkill early exit
            // if (!crouched && currentHeight >= playerHeight) {
            //     return;
            // }

            var bodyTransform = _body.transform;

            // depending on input move between playerHeight and crouchHeight by crouchSpeed
            var height = Mathf.Clamp(
                bodyTransform.localPosition.y + (crouchSpeed * Time.deltaTime * (Input.Handler.instance.crouched ? -1.0f : 1.0f)),
                playerCrouchHeight,
                playerHeight
            );

            // == Vector3(0,h,0)
            var offset = Vector3.up * height;

            // edit the mesh as a child of the collider
            bodyTransform.localPosition = offset;

            var bodyScale = bodyTransform.localScale;
            bodyScale.y = height;
            bodyTransform.localScale = bodyScale;

            // edit the collider
            _capsuleCollider.center = offset;
            _capsuleCollider.height = height * 2.0f;
        }

        private void Interact() {
            if (_carriedItem) {
                _carriedItem.SetCarriedState(false);
                _carriedItem = null;
                return;
            }

            // get the hit
            if (!Physics.Raycast(
                new Ray(_cam.transform.position, _cam.transform.forward),
                out var hit,
                interactDistance,
                interactLayerMask)) {
                return;
            }

            if (!hit.transform.TryGetComponent(out Interact.Interactable interactable)) {
                return;
            }

            interactable.Interact(this);
        }

        private void UpdateCarriedItem() {
            if (!_carriedItem) {
                return;
            }

            _carriedItem.position = Vector3.Lerp(
                _carriedItem.position,
                _cam.transform.position + Quaternion.Euler(combinedFacingEulerAngles) * Vector3.forward * 2.5f, // distance
                15.0f * Time.deltaTime // lerp speed
            );
        }

        public void SetCarriedItem(Interact.Item item) {
            if (_carriedItem) {
                _carriedItem.SetCarriedState(false);
                _carriedItem = null;
            }

            item.SetCarriedState(true);
            _carriedItem = item;
        }
    }
}