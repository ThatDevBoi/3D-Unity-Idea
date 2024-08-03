using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerCharacter.Mechanics
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Variables")]
        private Rigidbody rb;
        public Vector3 moveDirection;

        public float maxSpeed = 5f;
        [Range(0, 5)]
        public float currentSpeed = 0f;
        public float accelerationTime = 2f;
        public float decelerationTime = 2f;
        private PlayerMoveInputs inputs;
        private float speedVelocity;

        [Header("Jumping Variables")]
        public Transform groundAnchor;
        public LayerMask groundCheckMask;

        public LayerMask verticalChange;

        public float maxSlopeAngle = 45f;
        public float jumpForce = 2;

        public bool ONSLOPE;
        public bool onGround;

        public float ledgeDetectionDistance = 0.5f;
        public float ledgeCheckHeight = 1.0f;

        private void Awake()
        {
            inputs = new PlayerMoveInputs();
        }

        private void OnEnable()
        {
            inputs.Enable();

            inputs.PlayerWalk.Move.performed += OnMove;
            inputs.PlayerWalk.Move.canceled += OnMoveCancel;
            inputs.PlayerWalk.Jump.performed += OnJump;
        }

        private void OnDisable()
        {
            inputs.Disable();

            inputs.PlayerWalk.Move.performed -= OnMove;
            inputs.PlayerWalk.Move.canceled -= OnMoveCancel;
            inputs.PlayerWalk.Jump.performed -= OnJump;
        }

        #region Input Handlers
        private void OnMove(InputAction.CallbackContext context)
        {
            Vector3 inputVector = context.ReadValue<Vector3>();
            moveDirection = new Vector3(inputVector.x, 0, inputVector.z);
        }

        private void OnMoveCancel(InputAction.CallbackContext context)
        {
            moveDirection = Vector3.zero;
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (isGrounded())
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
        #endregion

        void Start()
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true;
            groundAnchor = gameObject.transform.GetChild(1).transform;
        }

        private void Update()
        {

        }

        void FixedUpdate()
        {
            MovePlayer();
        }

        void MovePlayer()
        {
            // Normal ground movement
            if (moveDirection != Vector3.zero)
            {
                // Gradually increase the current speed towards the max speed
                currentSpeed = Mathf.SmoothDamp(currentSpeed, maxSpeed, ref speedVelocity, accelerationTime);
            }
            else
            {
                currentSpeed = 0;
            }
            // Handle Movement of the player
            rb.velocity = new Vector3(moveDirection.x * currentSpeed, rb.velocity.y, moveDirection.z * currentSpeed);

            // Adapted movement
            RaycastHit hit;
            float rayLength = 2.0f; // Adjust as needed

            Debug.DrawRay(transform.position, Vector3.down * rayLength, Color.green);

            if (Physics.Raycast(transform.position, Vector3.down, out hit, rayLength, verticalChange))
            {
                Vector3 groundNormal = hit.normal;
                float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);

                Debug.Log($"Slope Angle: {slopeAngle}, Ground Normal: {groundNormal}");

                if (slopeAngle <= maxSlopeAngle)
                {
                    // Project moveDirection onto the plane defined by groundNormal
                    Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal).normalized;

                    // Calculate target velocity
                    Vector3 targetVelocity = new Vector3(slopeMoveDirection.x * currentSpeed, rb.velocity.y, slopeMoveDirection.z * currentSpeed);

                    if (moveDirection != Vector3.zero)
                    {
                        // Smooth acceleration
                        currentSpeed = Mathf.SmoothDamp(currentSpeed, maxSpeed, ref speedVelocity, accelerationTime);
                    }
                    else
                    {
                        // Smooth deceleration
                        currentSpeed = 0;
                        rb.velocity = Vector3.zero;
                    }

                    // Apply horizontal velocity
                    rb.velocity = new Vector3(slopeMoveDirection.x * currentSpeed, rb.velocity.y, slopeMoveDirection.z * currentSpeed);

                    // Prevent sliding on steep slopes
                    if (ONSLOPE == true & moveDirection == Vector3.zero)
                    {
                        rb.useGravity = false;
                    }
                    else
                        rb.useGravity = true;
                    ONSLOPE = true;
                }
                else
                {
                    // Handle steep slopes
                    rb.velocity = new Vector3(0, rb.velocity.y, 0);
                    ONSLOPE = false;
                }
            }
            else
            {
                // Check for ledges
                if (DetectLedge())
                {
                    Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, Vector3.up).normalized;
                    rb.velocity = new Vector3(slopeMoveDirection.x * currentSpeed, rb.velocity.y, slopeMoveDirection.z * currentSpeed);
                }
                else
                {
                    // No ground detected, stop horizontal movement and handle gravity
                    rb.velocity = new Vector3(0, rb.velocity.y, 0);
                    ONSLOPE = false;
                }
            }
        }

        bool isGrounded()
        {
            RaycastHit hit;
            float sphereRadius = 0.5f;
            float sphereCastDistance = 1.0f;
            Vector3 sphereOrigin = transform.position + Vector3.up * sphereRadius;

            Debug.DrawRay(sphereOrigin, Vector3.down * (sphereRadius + sphereCastDistance), Color.yellow);

            return Physics.SphereCast(sphereOrigin, sphereRadius, Vector3.down, out hit, sphereRadius + sphereCastDistance, groundCheckMask);
        }

        bool DetectLedge()
        {
            RaycastHit hit;
            Vector3 forwardCheckPosition = transform.position + transform.forward * ledgeDetectionDistance;

            if (Physics.Raycast(forwardCheckPosition, Vector3.down, out hit, ledgeCheckHeight, groundCheckMask))
            {
                Debug.Log("Ledge detected.");
                return true;
            }

            return false;
        }
    }
}
