using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    public GameObject targetObject;
    protected GameObject playerRef;
    public float maxSpeed = 3.0f;
    protected bool isGrounded;
    protected float velocityY = 0;
    protected float gravity = -9.8f;
    protected Rigidbody rb;
    public float attackDistance;
    public float damage = 2;
    protected RaycastHit hit;

    RaycastHit hitGround;
    const float groundCheckDistance = 0.02f;
    const float checkDistance = 0.05f;
    const float checkLeeway = 0.01f;
    public LayerMask groundMask;

    protected bool impulse = false;
    const float damageImpulse = 30f;
    protected float impulseTimer = 0;
    const float impulseDuration = 0.1f;
    Vector3 impactDirection;

    // Start is called before the first frame update
    public virtual void Start()
    {
        playerRef = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody>();
        attackDistance = transform.localScale.z + 0.1f;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        isGrounded = IsGrounded();
    }

    public virtual void FixedUpdate()
    {
        if (!isGrounded)
        {
            velocityY += gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
            transform.position += new Vector3(0, velocityY, 0);
        }
        else if (velocityY < 0) 
        {
            velocityY = 0;
            transform.position = new(transform.position.x, hitGround.transform.position.y + 1 + (transform.localScale.y - 1) / 2 + (hitGround.collider.transform.localScale.y - 1) / 2, transform.position.z);
        }

        if (impulse)
        {
            impulseTimer += Time.fixedDeltaTime;
            transform.position += Time.fixedDeltaTime * new Vector3(impactDirection.x * damageImpulse, 0, impactDirection.z * damageImpulse);
            if (impulseTimer > impulseDuration)
            {
                impulse = false;
                impulseTimer = 0;
            }
        }
    }

    public bool IsGrounded()
    {
        Vector3 boxStart = transform.position;
        Vector3 scale = transform.localScale;
        Vector3 halfSize = new(scale.x / 2 - checkDistance, groundCheckDistance, scale.z / 2 - checkDistance);

        return Physics.BoxCast(boxStart, halfSize, -transform.up, out hitGround, transform.rotation, scale.y / 2, groundMask);
    }

    public virtual bool SideCheck(Vector3 direction, LayerMask mask)
    {
        Vector3 boxStart = transform.position;
        Vector3 scale = transform.localScale;
        Vector3 halfSize;

        if (direction == transform.right || direction == -transform.right)
            halfSize = new(checkDistance, scale.y / 4 - checkLeeway, scale.z / 4 + checkLeeway);
        else if (direction == transform.forward || direction == -transform.forward)
            halfSize = new(scale.x / 4 + checkLeeway, scale.y / 4 - checkLeeway, checkDistance);
        else
            halfSize = new(scale.x / 4 - checkLeeway, checkDistance, scale.y / 4 - checkLeeway);

        return Physics.BoxCast(boxStart, halfSize, direction, out hit, transform.rotation, scale.y / 4, mask);
    }

    public void TriggerImpulse(Vector3 pointOfImpact)
    {
        impulse = true;
        Vector3 impactVector = transform.position - pointOfImpact;
        impactVector.y = 0;
        impactDirection = impactVector.normalized;
    }
}
