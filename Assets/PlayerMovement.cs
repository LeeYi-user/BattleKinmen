using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    // 該檔案是用來控制玩家移動的, 請把它放在玩家物件之下
    // 這個檔案真的有點難解釋.. 我也是直接抄教學的, 建議你們也去看一看
    // https://www.youtube.com/watch?v=f473C43s8nE (第一種方法)
    // https://www.youtube.com/watch?v=_QajrabyTJc (第二種方法)
    // https://youtu.be/xCxSjgYTw9c?t=226 (斜坡移動)
    // 簡單來說, 我融合了兩種方法, 然後再加上 slope movement, 就搞定了
    // 有空再打更詳細的註解 (如果你們需要的話)

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
    [SerializeField] private LayerMask whatIsGround;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    Rigidbody rb;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    bool readyToJump;
    bool grounded;
    RaycastHit slopeHit;
    bool exitingSlope;
    bool live;

    void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        live = true;
    }

    void Update()
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

        animator.SetBool("isRunning", horizontalInput != 0 || verticalInput != 0);

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
        if (grounded && Physics.Raycast(orientation.position, Vector3.down, out slopeHit, 0.3f))
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

    public void Despawn()
    {
        live = false;
        rb.velocity = Vector3.zero;
    }

    public void Respawn()
    {
        live = true;
        gameObject.transform.position = new Vector3(0f, 0f, 0f);
    }
}
