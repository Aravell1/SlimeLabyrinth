using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] Vector3 movement;
    [SerializeField] MovementState moveState = MovementState.Grounded;
    public const float maxHealth = 20;
    [Range(0f, maxHealth)]
    public float health = 20;
    public float maxSpeed = 10.0f;
    public GameObject slimeBody;
    public CanvasManager canvas;
    Rigidbody rb;
    float slimeXScale;
    float slimeYScale;
    float scaleOffset = 0;
    const float slimeScaleChange = 2.5f;

    [Header("Camera Settings")]
    GameObject cam;
    float maxCamDist = 4;
    public float senX = 30f;

    [Header("Damage Settings")]
    [SerializeField] bool damaged = false;
    const float damageImpulse = 30f;
    float impulseTimer = 0;
    const float impulseDuration = 0.1f;
    Vector3 impactDirection;

    [Header("Ground Settings")]
    public LayerMask groundMask;
    public LayerMask jumpMask;
    RaycastHit hitGround;
    const float checkLeeway = 0.01f;
    const float groundCheckDistance = 0.02f;
    const float checkDistance = 0.05f;


    [Header("Horizontal Movement")]
    [SerializeField] Vector2 hMoveScale;
    bool[] playerStop = new bool[2];
    Vector3[] stopPoint = new Vector3[2];

    //Jump
    [Header("Vertical Movement")]
    public float jumpSpeed = 40.0f;
    public float maxJumpTime = 0.4f;
    public float minJumpTime = 0.2f;
    public float maxHoveringTime = 0.2f;
    public float fallThreshold = 1.5f;
    [SerializeField] float fallTimer = 0;
    [SerializeField] float jumpTime = 0;
    [SerializeField] float hoverTime = 0;
    [SerializeField] bool doubleJump;
    const float doubleJumpScale = 1.2f;
    bool jumpStop = false;

    [SerializeField] float playerGravityFactor = 1;
    const float playerGravityFactorJump = 1;
    const float playerGravityFactorFalling = 2;
    const float playerGravity = -9.8f;

    private enum MovementState
    {
        Grounded,
        Jumping,
        Rising,
        Hovering,
        Falling
    }

    public float Health
    {
        get { return health; }

        set 
        { 
            if (value <= 0)
            {
                SceneManager.LoadScene("GameOver");
            }

            canvas.HealthPercent = value;
            health = value; 
        }
    }

    private MovementState MoveState
    {
        get { return moveState; }

        set 
        { 
            if (moveState == MovementState.Falling)
            {
                if (value == MovementState.Grounded)
                {
                    if (fallTimer > fallThreshold)
                    {
                        Health -= Mathf.Ceil(Mathf.Pow(fallTimer, 2));
                    }
                    fallTimer = 0;
                }
                else if (value != MovementState.Falling)
                {
                    fallTimer = 0;
                }
            }

            switch (value)
            {
                case MovementState.Grounded:
                    if (movement.y < 0)
                        movement.y = 0;
                    doubleJump = false;
                    break;
                case MovementState.Jumping:
                    jumpTime = 0;
                    break; 
                case MovementState.Hovering:
                    hoverTime = 0;
                    movement.y = 0;
                    playerGravityFactor = 0;
                    break;
            }

            moveState = value; 
        }
    }

    private void Start()
    {
        slimeXScale = slimeBody.transform.localScale.x - slimeScaleChange;
        slimeYScale = slimeBody.transform.localScale.z - slimeScaleChange;
        rb = GetComponent<Rigidbody>();
        cam = Camera.main.gameObject;
    }

    void Update()
    {
        AnimateSlime();

        if (IsGrounded())
        {
            if (MoveState != MovementState.Grounded && movement.y < 0)
            {
                if (hitGround.transform && hitGround.collider.gameObject.layer != 7)
                {
                    transform.position = new(transform.position.x, hitGround.transform.position.y + 1 + (transform.localScale.y - 1) / 2 + (hitGround.collider.transform.localScale.y - 1) / 2, transform.position.z);
                }
                MoveState = MovementState.Grounded;
            }
        }
        else if (MoveState == MovementState.Grounded)
        {
            MoveState = MovementState.Falling;
        }

        if (MoveState == MovementState.Falling)
        {
            fallTimer += Time.deltaTime;
        }
        

        if (MoveState == MovementState.Jumping || MoveState == MovementState.Rising)
        {
            jumpStop = JumpCheck();
        }
        else
        {
            jumpStop = false;
        }

        if (!damaged)
        {
            if (Input.GetKeyDown(KeyCode.Space) && MoveState == MovementState.Grounded)
            {
                if (movement.y < 0)
                {
                    if (hitGround.transform)
                        transform.position = new(transform.position.x, hitGround.transform.position.y + 1 + (hitGround.collider.transform.localScale.y - 1) / 2, transform.position.z);
                    movement.y = 0;
                }
                MoveState = MovementState.Jumping;
            }
            else if (Input.GetKeyDown(KeyCode.Space) && (MoveState == MovementState.Falling || MoveState == MovementState.Hovering) && !doubleJump)
            {
                doubleJump = true;
                movement.y = 0;
                MoveState = MovementState.Jumping;
            }
        }

        if (MoveState == MovementState.Jumping)
        {
            jumpTime += Time.deltaTime;
            if ((jumpTime > maxJumpTime 
                || (Input.GetKeyUp(KeyCode.Space) && jumpTime > minJumpTime) 
                || (jumpTime > minJumpTime && !Input.GetKey(KeyCode.Space))) 
                && !doubleJump)
            {
                if (!jumpStop)
                    MoveState = MovementState.Rising;
                else 
                    MoveState = MovementState.Falling;
            }
            else if ((jumpTime > maxJumpTime / doubleJumpScale 
                || (Input.GetKeyUp(KeyCode.Space) && jumpTime > minJumpTime / doubleJumpScale) 
                || (jumpTime > minJumpTime / doubleJumpScale && !Input.GetKey(KeyCode.Space))) 
                && doubleJump)
            {
                if (!jumpStop)
                    MoveState = MovementState.Rising;
                else
                    MoveState = MovementState.Falling;
            }
        }
        else if (MoveState == MovementState.Rising && Mathf.Abs(movement.y) < 0.1f)
        {            
            MoveState = MovementState.Hovering;
        }

    }

    private void LateUpdate()
    {
        playerStop[0] = SideCheck(Vector3.right) || SideCheck(-Vector3.right);
        playerStop[1] = SideCheck(Vector3.forward) || SideCheck(-Vector3.forward);
    }

    void FixedUpdate()
    {
        movement = new(0, movement.y, 0);

        if (impulseTimer > impulseDuration)
        {
            damaged = false;
            impulseTimer = 0;
        }
        
        if (!damaged)
        {
            hMoveScale.x = Input.GetAxis("Horizontal");
            hMoveScale.y = Input.GetAxis("Vertical");
            movement += transform.right * hMoveScale.x;
            movement += transform.forward * hMoveScale.y;
            movement.x *= maxSpeed;
            movement.z *= maxSpeed;
        }
        else
        {
            impulseTimer += Time.fixedDeltaTime;
            hMoveScale.x = 0;
            hMoveScale.y = 0;
            movement.x += impactDirection.x * damageImpulse;
            movement.z += impactDirection.z * damageImpulse;
        }


        #region Jump Logic
        if (MoveState == MovementState.Hovering)
        {
            hoverTime += Time.fixedDeltaTime;
            if (hoverTime >= maxHoveringTime)
            {
                MoveState = MovementState.Falling;
            }
        }
        else if (MoveState == MovementState.Jumping || MoveState == MovementState.Rising)
        {

            if (!jumpStop && MoveState == MovementState.Jumping)
            {
                if (!doubleJump)
                    movement.y += jumpSpeed * Time.fixedDeltaTime;
                else
                    movement.y += jumpSpeed * Time.fixedDeltaTime / doubleJumpScale;
                playerGravityFactor = playerGravityFactorJump;
            }
            else if (jumpStop)
            {
                //Debug.Log("Hit a platform");
                movement.y = 0;
                MoveState = MovementState.Falling;
            }
        }

        if (MoveState == MovementState.Falling)
        {
            playerGravityFactor = playerGravityFactorFalling;
        }
        #endregion

        movement.y += playerGravity * playerGravityFactor * Time.fixedDeltaTime;

        if (MoveState == MovementState.Grounded)
        {
            movement.y = 0;
        }

        for (int i = 0; i < playerStop.Length; i++) 
        {
            if (playerStop[i])
                CounterMovement(i);
        }

        Quaternion rotateX = Quaternion.Euler(0, Input.GetAxis("Mouse X") * senX, 0);
        rb.MoveRotation(rb.rotation * rotateX);
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);

        CheckCameraPosition();
    }

    void CheckCameraPosition()
    {
        Vector3 cameraDirection = cam.transform.position - rb.position;
        float distToCamera = cameraDirection.magnitude;
        cameraDirection.Normalize();
        if (Physics.Raycast(rb.position, cameraDirection, out RaycastHit camHit, maxCamDist, groundMask))
        {
            cam.transform.position = camHit.point;
        }
        else if (distToCamera < maxCamDist)
        {
            cam.transform.position = rb.position + cameraDirection * maxCamDist;
        }
    }

    void CounterMovement(int index)
    {
        Vector3 hitDirection = stopPoint[index];
        hitDirection.y = 0;
        switch (index)
        {
            case 0:
                if ((movement.x > 0 && hitDirection.x > 0) || (movement.x < 0 && hitDirection.x < 0))
                {
                    Vector3 movementDirection = Vector3.Dot(movement, hitDirection) / (hitDirection.magnitude * hitDirection.magnitude) * hitDirection;
                    movement -= movementDirection;
                }
                break;
            case 1:
                if ((movement.z > 0 && hitDirection.z > 0) || (movement.z < 0 && hitDirection.z < 0))
                {
                    Vector3 movementDirection = Vector3.Dot(movement, hitDirection) / (hitDirection.magnitude * hitDirection.magnitude) * hitDirection;
                    movement -= movementDirection;
                }
                break;
        }
        
    }

    private bool IsGrounded()
    {
        Vector3 boxStart = transform.position;
        Vector3 scale = transform.localScale;
        Vector3 halfSize = new(scale.x / 2 - checkDistance, groundCheckDistance, scale.z / 2 - checkDistance);

        return Physics.BoxCast(boxStart, halfSize, -transform.up, out hitGround, transform.rotation, scale.y / 2, groundMask);
    }

    private bool JumpCheck()
    {
        Vector3 boxStart = transform.position;
        Vector3 scale = transform.localScale;
        Vector3 halfSize = new(scale.x / 2 - checkDistance, checkDistance, scale.z / 2 - checkDistance);

        return Physics.BoxCast(boxStart, halfSize, transform.up, transform.rotation, scale.y / 2, jumpMask);
    }

    private bool SideCheck(Vector3 direction)
    {
        Vector3 boxStart = transform.position;
        Vector3 scale = transform.localScale;
        Vector3 halfSize;
        
        if (direction == Vector3.right || direction == -Vector3.right)
            halfSize = new(checkDistance, scale.y / 2 - checkLeeway, scale.z / 2 + checkLeeway);
        else
            halfSize = new(scale.x / 2 + checkLeeway, scale.y / 2 - checkLeeway, checkDistance);

        bool check = Physics.BoxCast(boxStart, halfSize, direction, Quaternion.identity, scale.y / 2, groundMask);
        if (check)
        {
            if (direction == Vector3.right || direction == -Vector3.right)
                stopPoint[0] = direction;
            else
                stopPoint[1] = direction;
        }
        return check;
    }

    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        Vector3 scale = transform.localScale;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pos - new Vector3(0, scale.y / 2, 0), 
            new(scale.x - checkDistance * 2, groundCheckDistance * 2, scale.z - checkDistance * 2));
        Gizmos.DrawWireCube(pos + new Vector3(0, scale.y / 2, 0), 
            new(scale.x - checkDistance * 2, checkDistance * 2, scale.z - checkDistance * 2));
        Gizmos.DrawWireCube(pos + new Vector3(scale.x / 2, 0, 0), 
            new(checkDistance * 2, scale.y - checkLeeway * 2, scale.z + checkDistance * 2));
        Gizmos.DrawWireCube(pos + new Vector3(-scale.x / 2, 0, 0), 
            new(checkDistance * 2, scale.y - checkLeeway * 2, scale.z + checkDistance * 2));
        Gizmos.DrawWireCube(pos + new Vector3(0, 0, scale.z / 2), 
            new(scale.x + checkDistance * 2, scale.y - checkLeeway * 2, checkDistance * 2));
        Gizmos.DrawWireCube(pos + new Vector3(0, 0, -scale.z / 2), 
            new(scale.x + checkDistance * 2, scale.y - checkLeeway * 2, checkDistance * 2));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            canvas.PauseGame(true);
        }
    }

    public void TriggerDamage(Vector3 pointOfImpact, float damage)
    {
        Health -= damage;
        damaged = true;
        Vector3 impactVector = transform.position - pointOfImpact;
        impactVector.y = 0;
        impactDirection = impactVector.normalized;
    }

    void AnimateSlime()
    {
        scaleOffset += Time.deltaTime * 3;
        float xOffset = slimeXScale + Mathf.Sin(scaleOffset) * slimeScaleChange;
        float yOffset = slimeYScale + Mathf.Cos(scaleOffset) * slimeScaleChange;
        slimeBody.transform.localScale = new(xOffset, slimeBody.transform.localScale.y, yOffset);
    }
}
