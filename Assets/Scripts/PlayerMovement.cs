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
    [SerializeField] private KeyCode jumpKey;

    [Header("Ground Check")]
    [SerializeField] private LayerMask whatIsGround;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    [Header("Body")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform model;

    private Transform spawnPoint;
    private bool grounded;
    private float horizontalInput;
    private float verticalInput;
    private bool readyToJump = true;
    private bool exitingSlope;
    private Vector3 moveDirection;
    private RaycastHit slopeHit;
    private bool live;

    private void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        spawnPoint = MainSceneManager.Instance.playerSpawn;
        Despawn();
    }

    private void Update()
    {
        if (!IsOwner || !live)
        {
            return;
        }

        MyInput();
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

        model.rotation = orientation.rotation;
    }

    private void FixedUpdate()
    {
        if (!IsOwner || !live)
        {
            return;
        }

        MovePlayer();
    }

    private void MyInput()
    {
        if (Cursor.lockState == CursorLockMode.None)
        {
            return;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * Mathf.Sqrt(jumpForce * -2f * gravity), ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        animator.SetBool("isRunning", flatVel.magnitude > 1);

        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void MovePlayer()
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
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        if (!OnSlope())
        {
            rb.velocity += new Vector3(0, gravity * Time.fixedDeltaTime, 0);
        }
    }

    private bool OnSlope()
    {
        //Debug.DrawRay(orientation.position + Vector3.up * 0.3f, Vector3.forward * 0.6f, Color.green);
        //Debug.DrawRay(orientation.position + Vector3.up * 0.3f, Vector3.back * 0.6f, Color.green);
        //Debug.DrawRay(orientation.position + Vector3.up * 0.3f, Vector3.left * 0.6f, Color.green);
        //Debug.DrawRay(orientation.position + Vector3.up * 0.3f, Vector3.right * 0.6f, Color.green);
        //Debug.DrawRay(orientation.position, Vector3.down * 0.3f, Color.green);

        if (grounded && (Physics.Raycast(orientation.position + Vector3.up * 0.3f, Vector3.forward, out slopeHit, 0.6f) ||
                         Physics.Raycast(orientation.position + Vector3.up * 0.3f, Vector3.back, out slopeHit, 0.6f) ||
                         Physics.Raycast(orientation.position + Vector3.up * 0.3f, Vector3.left, out slopeHit, 0.6f) ||
                         Physics.Raycast(orientation.position + Vector3.up * 0.3f, Vector3.right, out slopeHit, 0.6f) ||
                         Physics.Raycast(orientation.position, Vector3.down, out slopeHit, 0.3f)))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Ground")
        {
            grounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Ground")
        {
            grounded = false;
        }
    }

    public void Despawn()
    {
        live = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void Respawn()
    {
        live = true;
        grounded = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.MovePosition(spawnPoint.position + new Vector3(Random.Range(-4f, 4f), 0f, Random.Range(-4f, 4f)));
    }
}
