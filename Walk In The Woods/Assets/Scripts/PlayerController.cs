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
        // Ground Control
        public Transform groundAnchor;
        public LayerMask groundCheckMask;

        public float jumpForce = 2;

        private void Awake()
        {
            inputs = new PlayerMoveInputs();
        }

        private void OnEnable()
        {
            inputs.Enable();  // Enable the input actions

            // Subscribe methods to input action events
            inputs.PlayerWalk.Move.performed += OnMove;
            inputs.PlayerWalk.Move.canceled += OnMoveCancel;
            inputs.PlayerWalk.Jump.performed += OnJump;
        }

        private void OnDisable()
        {
            inputs.Disable();  // Disable the input actions

            // Unsubscribe methods from input action events
            inputs.PlayerWalk.Move.performed -= OnMove;
            inputs.PlayerWalk.Move.canceled -= OnMoveCancel;
            inputs.PlayerWalk.Jump.performed += OnJump;
        }

        #region Input Handlers
        private void OnMove(InputAction.CallbackContext context)
        {
            Vector3 inputVector = context.ReadValue<Vector3>();
            moveDirection = new Vector3(inputVector.x, 0, inputVector.z);
            Debug.Log($"Move input: {moveDirection}");
        }

        private void OnMoveCancel(InputAction.CallbackContext context)
        {
            moveDirection = Vector3.zero;
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if(isGrounded())
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            // Make Rigidbody and set up
            rb = gameObject.AddComponent<Rigidbody>();
            rb.freezeRotation = true;

            // Find Components
            groundAnchor = gameObject.transform.GetChild(1).transform;

            //Debug.Log(inputs);
        }

        private void Update()
        {
            Debug.Log(isGrounded());
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            MovePlayer();
        }

        void MovePlayer()
        {
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
        }

        bool isGrounded()
        {
            RaycastHit hit;
            // Draaws ray to be seen in scene where it'll draw down 0.025f
            Debug.DrawRay(groundAnchor.position, Vector3.down * 0.025f, Color.red);
            return Physics.Raycast(groundAnchor.position, Vector3.down, out hit, 0.025f, groundCheckMask);  // returns check if it hits a collider with layer set
        }
    }
}
