using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    // 元件用途: 操控玩家移動
    // 元件位置: 玩家物件(player prefab)之下

    [SerializeField] private Transform orientation;
    [SerializeField] private float gravity;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey; // space

    [Header("Ground Check")]
    [SerializeField] private LayerMask whatIsGround;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    private Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private bool readyToJump;
    private bool grounded;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    private Transform spawnPoint;
    private bool live;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        rb = GetComponent<Rigidbody>();
        readyToJump = true;
        spawnPoint = GameObject.Find("Player Spawner").transform;
        Despawn();
    }

    // Update is called once per frame
    void Update()
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
    }

    void FixedUpdate()
    {
        if (!IsOwner || !live)
        {
            return;
        }

        MovePlayer();
    }

    void MyInput()
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

    void SpeedControl()
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

    void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * Mathf.Sqrt(jumpForce * -2f * gravity), ForceMode.Impulse);
    }

    void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    bool OnSlope()
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

    Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Untagged") && LayerMask.LayerToName(collision.gameObject.layer) == "Hittable")
        {
            grounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Untagged") && LayerMask.LayerToName(collision.gameObject.layer) == "Hittable")
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
        rb.MovePosition(spawnPoint.position + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f)));
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
