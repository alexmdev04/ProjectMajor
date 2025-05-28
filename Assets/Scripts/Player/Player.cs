using System;
using UnityEngine;

namespace Major {
    public class Player : MonoBehaviour {
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

        [SerializeField, Range(0f, 0.99f)] private float 
            friction = 0.85f;
        [SerializeField, Range(0f, 0.1f)] private float 
            forceToApplyFriction = 0.1f,
            flatvelMin = 0.1f;
        [SerializeField] private float
            walkSpeed = 4f,
            sprintSpeed = 6.5f,
            cameraHeight = 0.825f,
            movementAcceleration = 0.1f,
            movementDecceleration = 0.05f,
            jumpForce = 5f,
            playerHeight = 180f, // in cm
            playerCrouchHeight = 100f, // in cm
            groundedRayDistance = 1f,
            movementRampTime,
            heldObjectDistance = 6.0f,
            crouchLerpSpeed = 15.0f;
        [SerializeField] private GameObject 
            body,
            heldObject;
        [SerializeField] private Camera 
            cam;
        public Vector2 playerEulerAngles { get; private set; } = Vector2.zero;
        
        [Header("Interaction")]
        [SerializeField] private float interactDistance = 100.0f;
        [SerializeField] private LayerMask interactLayerMask = int.MaxValue;

        private bool grounded => MathF.Round(rb.linearVelocity.y, 3) == 0;

        private void Awake() {
            rb = GetComponent<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Start() {
            Input.Handler.instance.OnJump += Jump;
            Input.Handler.instance.OnInteract += Interact;
            cam = Camera.main;
            cam!.transform.SetParent(body.transform);
            cam.transform.localPosition = new Vector3(0.0f, cameraHeight, 0.0f);
        }

        private void OnDisable() {
            Input.Handler.instance.OnJump -= Jump;
            Input.Handler.instance.OnInteract -= Interact;
        }

        private void Update() {
            if (moveActive && !moveFixedUpdate) { Move(); }
            heldObjectDistance = Math.Clamp(heldObjectDistance + Input.Handler.instance.scrollDelta.y, 1.0f, 15.0f);
            Crouch();
        }
        
        private void FixedUpdate() {
            if (moveActive && moveFixedUpdate) { Move(); }
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

            body.transform.localEulerAngles = new Vector3(0f, playerEulerAngles.y, 0f);
            cam.transform.localEulerAngles = new Vector3(playerEulerAngles.x, 0f, 0f);
        }
        
        private void Move() {
            float maxVelocity = Input.Handler.instance.sprinting ? sprintSpeed : walkSpeed;
            Vector3 movementDirectionGlobal = body.transform.TransformDirection(Input.Handler.instance.movementDirection);
            
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
        
        private void Crouch() {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                new Vector3(
                    transform.localScale.x,
                    (Input.Handler.instance.crouched ? playerCrouchHeight : playerHeight) / 200,
                    transform.localScale.z
                ),
                crouchLerpSpeed * Time.deltaTime
            );
        }
        
        private void Interact() {
            // if - func: drop if held
            
            // get the hit
            if (!Physics.Raycast(
                new Ray(cam.transform.position, cam.transform.forward),
                out var hit,
                interactDistance, 
                interactLayerMask)) {
                return;
            }

            // func: check if object is interactble besides layer/tag
            
            // func: pickup object
        }
    }
}