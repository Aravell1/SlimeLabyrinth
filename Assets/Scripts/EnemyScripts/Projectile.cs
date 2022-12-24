using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    Rigidbody rb;
    ParticleSystem ps;

    public LayerMask blastMask;
    public bool useGravity = false;
    public bool hasBlastRadius = false;
    public Vector3 velocity;
    public float damage = 1;
    
    const float moveSpeed = 30;
    const float lifeTime = 3;
    const float gravity = -9.8f;
    const float blastRadius = 5;
    bool explodeOnce = true;
    Vector3 nextPosition;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);
        if (!useGravity)
            velocity = moveSpeed * Time.deltaTime * transform.forward;
        if (hasBlastRadius)
            ps = GetComponentInChildren<ParticleSystem>();
    }

    private void FixedUpdate()
    {
        if (useGravity)
        {
            velocity.y += 2 * gravity * Time.deltaTime * Time.deltaTime;
        }
        nextPosition = transform.position + velocity;

        rb.MovePosition(nextPosition);
    }

    private void Update()
    {
        if (transform.position.y <= 0)
        {
            if (hasBlastRadius && explodeOnce)
                Explode();
            else
                Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !hasBlastRadius)
        {
            other.GetComponent<Player>().TriggerDamage(transform.position, damage);
        }

        if (!other.gameObject.CompareTag("FlyingEnemy"))
        {
            if (hasBlastRadius && explodeOnce)
                Explode();
            else
                Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player") && !hasBlastRadius)
        {
            other.gameObject.GetComponent<Player>().TriggerDamage(transform.position, damage);
        }

        if (!other.gameObject.CompareTag("FlyingEnemy"))
        {
            if (hasBlastRadius && explodeOnce)
                Explode();
            else
                Destroy(gameObject);
        }
    }

    void Explode()
    {
        explodeOnce = false;
        ps.Play();
        ps.gameObject.transform.parent = null;

        Collider[] sphereHits = Physics.OverlapSphere(transform.position, blastRadius, blastMask);
        for (int i = 0; i < sphereHits.Length; i++) 
        {
            if (sphereHits[i].gameObject.CompareTag("Player"))
            {
                sphereHits[i].GetComponent<Player>().TriggerDamage(transform.position, damage);
            }
            else if (sphereHits[i].gameObject.CompareTag("Enemy"))
            {
                sphereHits[i].GetComponent<BaseEnemy>().TriggerImpulse(transform.position);
            }
        }

        Destroy(gameObject);
        velocity = Vector3.zero;
        useGravity = false;
    }

}
