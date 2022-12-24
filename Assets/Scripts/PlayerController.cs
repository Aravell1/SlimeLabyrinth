using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public LayerMask groundMask;
    public LayerMask jumpMask;
    public float playerGravityFactor = 1;
    public float playerGravityFactorJump = 1;
    public float playerGravityFactorFalling = 2;
    public float groundCheckDistance = 0.51f;

    private const float playerGravity = -9.8f;

    [SerializeField] private bool isGrounded;

    [Header("Horizontal Movement")]
    public bool isLeft;
    public bool isRight;

    public float maxSpeed = 10.0f;
    public float hSpeed = 0;
    public float hAccelRate = 4f;
    public float hDecelRate = 1f;

    //Jump
    [Header("Vertical Movement")]
    public float jumpSpeed = 14.0f;
    public float maxJumpTime = 0.5f;
    public float jumpTime = 0;
    public bool isJumping;
    public bool isJumpingUp;
    public bool isFalling;

    private Rigidbody rb;
    private RaycastHit hitGround;

    private Vector3 playerInput;
    private Vector3 playerVelocity;


    void OnValidate()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        isLeft = Input.GetKey(KeyCode.A);
        isRight = Input.GetKey(KeyCode.D);

        if (isLeft)
        {
            playerInput.x = -1.0f;
        }
        if (isRight)
        {
            playerInput.x = 1.0f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isGrounded = false;
            isJumping = true;
            isJumpingUp = true;

            jumpTime = 0;
        }

        if (isJumping)
        {
            jumpTime += Time.deltaTime;
            if (jumpTime > maxJumpTime || Input.GetKeyUp(KeyCode.Space))
            {
                isJumping = false;
            }
        }
    }

    void FixedUpdate()
    {
        if (isLeft || isRight)
        {
            hSpeed = Mathf.Lerp(hSpeed, playerInput.x * maxSpeed, hAccelRate * Time.fixedDeltaTime);
        }
        else
        {
            if (hSpeed > 0.1f || hSpeed < -0.1f)
                hSpeed = Mathf.Lerp(hSpeed, 0, hDecelRate * Time.fixedDeltaTime);
            else 
                hSpeed = 0;
        }

        playerVelocity.x = hSpeed;

        //Jump Logic
        #region
        if (isJumping)
        {
            playerVelocity.y += jumpSpeed * Time.fixedDeltaTime;
            playerGravityFactor = playerGravityFactorJump;
        }

        if (isFalling)
        {
            playerGravityFactor = playerGravityFactorFalling;
        }
        #endregion

        playerVelocity.y += playerGravity * playerGravityFactor * Time.fixedDeltaTime;

        if (isFalling)
            isGrounded = IsGrounded();

        //Ground Logic
        #region
        if (isGrounded)
        {
            transform.position = new(transform.position.x, hitGround.transform.position.y + 1.0f, transform.position.z);
            playerVelocity.y = 0;

            isFalling = false;
            isJumpingUp = false;
        }
        else
        {
            if (playerVelocity.y < 0)
            {
                isFalling = true;
                isJumpingUp = false;
            }
            else if (playerVelocity.y > 0)
            {
                isFalling = false;
                isJumpingUp = true;
            }
            else
            {
                isFalling = false;
                isJumpingUp = false;
            }
        }
        #endregion

        rb.MovePosition(rb.position + playerVelocity * Time.fixedDeltaTime);
    }

    private bool IsGrounded()
    {
        Vector3 lineStart = transform.position;
        Vector3 lineEnd = new(lineStart.x, lineStart.y - groundCheckDistance,lineStart.z);

        return Physics.Linecast(lineStart, lineEnd, out hitGround, groundMask);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, -transform.up * groundCheckDistance);
    }
}
