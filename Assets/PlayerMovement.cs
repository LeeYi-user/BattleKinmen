using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private float gravity;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;

    Rigidbody rb;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    bool readyToJump;
    bool grounded;
    RaycastHit slopeHit;
    bool exitingSlope;

    void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        MovePlayer();
        SpeedControl();

        if (grounded)
        {
            rb.drag = groundDrag;

            if (rb.velocity.y < 0 && !OnSlope())
            {
                rb.velocity = new Vector3(rb.velocity.x, -2, rb.velocity.z);
            }
        }
        else
        {
            rb.drag = 0;
        }
    }

    void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 2e3f * Time.deltaTime, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 2e3f * airMultiplier * Time.deltaTime, ForceMode.Force);
        }

        if (!OnSlope())
        {
            rb.velocity += new Vector3(0, gravity * Time.deltaTime, 0);
        }
    }

    void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * Mathf.Sqrt(jumpForce * -2f * gravity), ForceMode.Impulse);
    }

    void ResetJump()
    {
        exitingSlope = false;
        readyToJump = true;
    }

    bool OnSlope()
    {
        if (grounded && Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
