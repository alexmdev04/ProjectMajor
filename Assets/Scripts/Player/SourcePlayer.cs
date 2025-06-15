using System;
using System.Runtime.InteropServices;
using Major;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class SourcePlayer : MonoBehaviour {
    public static SourcePlayer instance { get; private set; }
    public Vector3 groundedCastOffset = Vector3.zero;
    public float groundedCastDistance = 0.15f;
    public GameObject groundObject;
    public Vector3 groundNormal = Vector3.up;
    public MovementConfig config = new();
    public bool
        jumping,
        crouching,
        sprinting;

    public float
        speed,
        maxVelocity = 50.0f,
        frictionMult = 1.0f,
        gravity = 20.0f,
        surfaceFriction,
        gravityFactor = 1.0f;

    public LayerMask groundedlayerMask;
    public Vector3 velocity;
    private Vector3 movementDirectionGlobal;
    public float verticalAxis => movementDirectionGlobal.z;
    public float horizontalAxis => movementDirectionGlobal.x;
    public float forwardMove => verticalAxis * config.acceleration; // movementDirection.z
    public float sideMove => horizontalAxis * config.acceleration; // movementDirection.x
    public bool wishJump => Keyboard.current.spaceKey.wasPressedThisFrame;

    private void Awake() {
        instance = this;

    }
    private void Start() {

    }
    private void Update() {
        GroundedUpdate();
    }
    private void FixedUpdate() {
        movementDirectionGlobal = Player.instance.capsuleCollider.transform.TransformDirection(Major.Input.Handler.instance.movementDirection);
        velocity = Player.instance.rb.linearVelocity;
        ProcessMovement();
        Player.instance.rb.linearVelocity = velocity;
    }

    private void ProcessMovement() {
        // not underwater, not climbing ladder
        // if (velocity.y < 0.0f) {
        //     jumping = false;
        // }

        // apply gravity
        if (groundObject == null) {
            velocity.y -= gravityFactor * gravity * Time.fixedDeltaTime;
        }

        CheckGrounded();
        CalculateMovementVelocity();

        // var yVel = velocity.y;
        // velocity.y = 0.0f;
        // velocity = Vector3.ClampMagnitude(velocity, maxVelocity);
        // speed = velocity.magnitude;
        // velocity.y = yVel;

        speed = Vector3.ClampMagnitude(
            new Vector3(velocity.x, 0.0f, velocity.z),
            maxVelocity
        ).magnitude;

        // if (velocity.sqrMagnitude == 0.0f) {
        //     // do collisions while standing still
        //     // ResolveCollisions 1f
        // }
        // else {
        //     var maxDistPerFrame = 0.2f;
        //     var velocityThisFrame = velocity * Time.fixedDeltaTime;
        //     var velocityDistLeft = velocityThisFrame.magnitude;
        //     var initialVel = velocityDistLeft;
        //     while (velocityDistLeft > 0.0f) {
        //         var amountThisLoop = Mathf.Min(maxDistPerFrame, velocityDistLeft);
        //         velocityDistLeft -= amountThisLoop;

        //         var velThisLoop = velocityThisFrame * (amountThisLoop / initialVel);
        //         origin += velThisLoop;

        //         // dont penetrate walls
        //         // ResolveCollisions amountThisLoop / initialVel
        //     }
        // }
        // groundedTemp = grounded;
    }

    public bool grounded;
    public RaycastHit groundedHit;
    public void GroundedUpdate() {
        var rayStart = Player.instance.transform.position + groundedCastOffset;
        UnityEngine.Debug.DrawLine(rayStart, rayStart + (groundedCastDistance * Vector3.down), Color.green);
        grounded = Physics.Raycast(
            ray: new Ray(origin: rayStart, direction: Vector3.down),
            maxDistance: groundedCastDistance,
            hitInfo: out groundedHit,
            layerMask: groundedlayerMask
        );
    }
    private bool CheckGrounded() {
        surfaceFriction = 1.0f;
        var movingUp = velocity.y > 0.0f;
        // var hit = Physics.Raycast(
        //     ray: new Ray(origin: Player.instance.transform.position + groundedCastOffset, direction: Vector3.down),
        //     maxDistance: groundedCastDistance,
        //     hitInfo: out RaycastHit ray,
        //     layerMask: groundedlayerMask
        // );

        var groundSteepness = Vector3.Angle(Vector3.up, groundedHit.normal);

        if (!grounded || groundSteepness > config.slopeLimit /*|| (jumping && velocity.y > 0.0f)*/) {
            groundObject = null;
            if (movingUp) {
                surfaceFriction = config.airFriction;
            }
            return false;
        }
        else {
            groundNormal = groundedHit.normal;
            groundObject = groundedHit.collider.gameObject;
            velocity.y = 0.0f;
            return true;
        }
    }

    private void CalculateMovementVelocity() {
        if (!grounded) {
            velocity += AirInputMovement();
            //Reflect(ref velocity, collider, origin);
        }
        else {
            var fric = crouching ? config.crouchFriction : config.friction;
            var accel = crouching ? config.crouchAcceleration : config.acceleration;
            var decel = crouching ? config.crouchDeceleration : config.deceleration;

            var forward = Vector3.Cross(groundNormal, -Player.instance.transform.right);
            var right = Vector3.Cross(groundNormal, forward);
            var speed = sprinting ? config.sprintSpeed : config.walkSpeed;
            if (crouching) {
                speed = config.crouchSpeed;
            }

            Vector3 wishDir;

            if (wishJump) {
                ApplyFriction(0.0f, true);
                velocity.y += config.jumpForce;
                //jumping = true;
                return;
            }
            else {
                ApplyFriction(1.0f * frictionMult, true);
            }

            var forwardMove = verticalAxis;
            var rightMove = horizontalAxis;

            wishDir = forwardMove * forward + rightMove * right;
            wishDir.Normalize();
            var moveDirNorm = wishDir;

            var forwardVelocity = Vector3.Cross(
                groundNormal,
                Quaternion.AngleAxis(-90.0f, Vector3.up) *
                    new Vector3(
                        velocity.x,
                        0.0f,
                        velocity.z
                    )
            );

            // target speed
            var wishSpeed = wishDir.magnitude;
            wishSpeed *= speed;

            var yVelPreAccel = velocity.y;
            // Accelerate
            // Accelerate (_wishDir, _wishSpeed, accel * Mathf.Min (frictionMult, 1f), false);
            // public static Vector3 Accelerate (Vector3 currentVelocity, Vector3 wishdir, float wishspeed, float accel, float deltaTime, float surfaceFriction)
            {
                var acceleration = accel * Mathf.Min(frictionMult, 1.0f);
                var yMovement = false;
                var currentSpeed = Vector3.Dot(velocity, wishDir);
                var addSpeed = wishSpeed - currentSpeed;

                if (addSpeed <= 0.0f) {
                    return;
                }

                var accelSpeed = Mathf.Min(acceleration * Time.fixedDeltaTime * wishSpeed * surfaceFriction, addSpeed);
                velocity.x += accelSpeed * wishDir.x;
                if (yMovement) { velocity.y += accelSpeed * wishDir.y; }
                velocity.z += accelSpeed * wishDir.z;
            }

            var maxVelocityMagnitude = maxVelocity;
            velocity = Vector3.ClampMagnitude(
                new Vector3(
                    velocity.x,
                    0.0f,
                    velocity.z
                ),
                maxVelocityMagnitude
            );
            velocity.y = yVelPreAccel;

            var yVelocityNew = forwardVelocity.normalized.y * new Vector3(velocity.x, 0.0f, velocity.z).magnitude;
            velocity.y = yVelocityNew * (wishDir.y < 0.0f ? 1.2f : 1.0f);
            var removeableYVelocity = velocity.y - yVelocityNew;
        }
    }

    private void ApplyFriction(float t, bool yAffected) {
        var speed = new Vector3(velocity.x, 0.0f, velocity.z).magnitude;
        var drop = 0.0f;

        var fric = crouching ? config.crouchFriction : config.friction;
        var accel = crouching ? config.crouchAcceleration : config.acceleration;
        var decel = crouching ? config.crouchDeceleration : config.deceleration;

        if (grounded) {
            var control = speed < decel ? decel : speed;
            drop = control * fric * Time.fixedDeltaTime * t;
        }

        var newSpeed = Mathf.Max(speed - drop, 0.0f);
        if (speed > 0.0f) {
            newSpeed /= speed;
        }

        velocity.x *= newSpeed;
        if (yAffected) { velocity.y *= newSpeed; }
        velocity.z *= newSpeed;
    }

    private Vector3 AirInputMovement() {
        // GetWishValues
        Vector3 wishVel = Vector3.zero;
        Vector3 wishDir = Vector3.zero;
        float wishSpeed = 0.0f;

        Vector3 forward = Vector3.zero;// = _surfer.forward;
        Vector3 right = Vector3.zero;// = _surfer.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        for (var i = 0; i < 3; i++) {
            wishVel[i] = forward[i] * forwardMove + right[i] * sideMove;
        }
        wishVel[1] = 0;
        wishSpeed = wishVel.magnitude;
        wishDir = wishVel.normalized;
        // GetWishValues


        if (config.clampAirSpeed && (wishSpeed != 0.0f && (wishSpeed > config.maxSpeed))) {
            wishVel *= config.maxSpeed / wishSpeed;
            wishSpeed = config.maxSpeed;
        }

        // AirAccelerate
        wishSpeed = Mathf.Min(wishSpeed, config.airCap);
        var currentSpeed = Vector3.Dot(velocity, wishDir);
        var addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0.0f) {
            return Vector3.zero;
        }

        var accelSpeed = config.airAcceleration * wishSpeed * Time.fixedDeltaTime;
        accelSpeed = Mathf.Min(accelSpeed, addSpeed);

        var result = Vector3.zero;
        for (var i = 0; i < 3; i++) {
            result[i] += accelSpeed * wishDir[i];
        }
        return result;
        // AirAccelerate
    }
}

// public struct MoveData {
//     public Transform playerTransform => Player.instance.transform;
//     // public Transform viewTransform => Player.instance.cam.transform; // camera transform

//     // public Vector3 origin => Player.instance.transform.position; // transform.position
//     // public Vector3 viewAngles => Player.instance.playerEulerAngles; // eulerAngles
//     public Vector3 velocity;
//     public float sideMove(float accel) => horizontalAxis * accel; // movementDirection.x
//     public float forwardMove(float accel) => verticalAxis * accel; // movementDirection.z
//     public float surfaceFriction;
//     public float gravityFactor => 1.0f;
//     // public float walkFactor => 1.0f;
//     public float verticalAxis => movementDirectionGlobal.z;
//     public float horizontalAxis => movementDirectionGlobal.x;
//     private Vector3 movementDirectionGlobal => Player.instance.capsuleCollider.transform.TransformDirection(Major.Input.Handler.instance.movementDirection);
//     public bool wishJump => Keyboard.current.spaceKey.wasPressedThisFrame;
//     // public bool crouching;
//     // public bool sprinting;

//     // public float rigidbodyPushForce;// = 1f;

//     // public float defaultHeight;// = 2f;
//     // public float crouchingHeight;// = 1f;
//     // public float crouchingSpeed;// = 10f;
//     // public bool toggleCrouch;

//     // public bool slidingEnabled;

//     // public bool grounded;
//     // public bool groundedTemp;
//     // public float fallingVelocity;

//     // public bool useStepOffset;
//     // public float stepOffset;
// }

public class MovementConfig {
    // [Header("Jumping and gravity")]
    public bool autoBhop = true;
    public float gravity = 20f;
    public float jumpForce = 6.5f;

    // [Header("General physics")]
    public float friction = 6f;
    public float maxSpeed = 6f;
    public float maxVelocity = 50f;
    // [Range(30f, 75f)] 
    public float slopeLimit = 45f;

    // [Header("Air movement")]
    public bool clampAirSpeed = true;
    public float airCap = 0.4f;
    public float airAcceleration = 12f;
    public float airFriction = 0.4f;

    // [Header("Ground movement")]
    public float walkSpeed = 7f;
    public float sprintSpeed = 12f;
    public float acceleration = 14f;
    public float deceleration = 10f;

    // [Header("Crouch movement")]
    public float crouchSpeed = 4f;
    public float crouchAcceleration = 8f;
    public float crouchDeceleration = 4f;
    public float crouchFriction = 3f;
}