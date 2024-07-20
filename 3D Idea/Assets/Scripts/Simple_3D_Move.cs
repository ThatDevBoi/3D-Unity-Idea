using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerCharcter.Movement
{
    public class Simple_3D_Move : MonoBehaviour
    {
        // Variables
        // The Direction we are currently moving in
        public Vector3 moveDirection;
        // The Physics Component needed to move
        private Rigidbody rb;
        // The Speed that we move at
        public float moveSpeed = 5f;

        // Input System Call
        public InputAction playerMove;


        private void OnEnable()
        {
            playerMove.Enable();
        }

        private void OnDisable()
        {
            playerMove.Disable();
        }

        // Start is called before the first frame update
        void Start()
        {
            // Make a Rigibody Physics Component in the Unity Inspector During Runtime
            rb = gameObject.AddComponent<Rigidbody>();  
            // Turn Off the Gravity Of The Component
            rb.useGravity = false;
            // Freeze all rotation
            rb.freezeRotation = true;
        }

        // Update is called once per frame
        void Update()
        {
            // Store the Horizontal Axis In The Legacy Input System (A, D Left, Right)
            //float Hor = Input.GetAxis("Horizontal");
            // Store the Vertical Axis In The Legacy Input System (W, S Forward, Backward)
            //float ver = Input.GetAxis("Vertical");

            // Add the Input Values from the two floats to the Vector3 Axis on the X, Z axis. Leave Y Blank (Jump Axis)
            //moveDirection = new Vector3(Hor, 0, ver).normalized;

            moveDirection = playerMove.ReadValue<Vector3>();

        }
        // Call all Physics Updates On Fixed Update
        private void FixedUpdate()
        {
            // Move Our Rigibody Component with it's velocity property on the Axis we declared via Input. 
            rb.velocity = new Vector3(moveDirection.x * moveSpeed, 0 ,moveDirection.z * moveSpeed);
        }
    }
}


