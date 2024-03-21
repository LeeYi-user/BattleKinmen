using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private float gravity;

    [Header("Movement")]
    public NetworkVariable<float> moveSpeed = new NetworkVariable<float>(5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] private float groundDrag;
    public NetworkVariable<float> jumpForce = new NetworkVariable<float>(2f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
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

        moveSpeed.OnValueChanged += UpdateRunAnimSpeed;
        spawnPoint = MainSceneManager.Instance.playerSpawn;
        Despawn();
    }

    private void UpdateRunAnimSpeed(float previous, float current)
    {
        animator.SetFloat("runAnimSpeed", 1 + (moveSpeed.Value - 5f) / 1.25f * 0.125f);
    }

    private void Update()
    {
        if (!IsOwner || !live)
        {
            return;
        }

        grounded = Physics.CheckSphere(orientation.position, 0.3f, whatIsGround);

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
            horizontalInput = 0f;
            verticalInput = 0f;
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
        rb.AddForce(transform.up * Mathf.Sqrt(jumpForce.Value * -2f * gravity), ForceMode.Impulse);
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
            if (rb.velocity.magnitude > moveSpeed.Value)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed.Value;
            }
        }
        else
        {
            if (flatVel.magnitude > moveSpeed.Value)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed.Value;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed.Value * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed.Value * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed.Value * 10f * airMultiplier, ForceMode.Force);
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
