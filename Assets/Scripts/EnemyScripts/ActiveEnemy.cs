using UnityEngine;
using static AIHelpers;

public class ActiveEnemy : BaseEnemy
{
    const float maxWanderDuration = 2.0f;
    private float wanderCounter = 0.0f;
    readonly System.Random r = new();

    Animator anim;

    FieldOfView fov;
    const float fovIdleRange = 15;
    const float fovAttackRange = 25;

    const float cooldown = 2;
    float cooldownTimer = 0;
    
    const float idleResetTime = 3;
    float idleResetTimer = 0;

    bool canHit = true;
    const float hitCooldown = 2;
    float hitTimer = 0;

    public MovementBehaviors activeMovementBehavior = MovementBehaviors.WanderKinematic;
    [SerializeField] EnemyState state = EnemyState.Idle;
    private enum EnemyState
    {
        Idle,
        Attack,
        AttackCooldown,
        Dead
    }
    private EnemyState State
    {
        get { return state; }
        set 
        { 
            state = value; 
            switch(state) 
            {
                case EnemyState.Idle:
                    activeMovementBehavior = MovementBehaviors.WanderKinematic;
                    idleResetTimer = 0;
                    fov.radius = fovIdleRange;
                    break;
                case EnemyState.Attack:
                    activeMovementBehavior = MovementBehaviors.SeekKinematic;
                    fov.radius = fovAttackRange;
                    break;
                case EnemyState.AttackCooldown:
                    activeMovementBehavior = MovementBehaviors.FleeKinematic;
                    cooldownTimer = 0;
                    break;
            }
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        fov = GetComponent<FieldOfView>();
        fov.radius = fovIdleRange;
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();

        switch (State)
        {
            case EnemyState.Idle:
                if (fov.canSeePlayer)
                    State = EnemyState.Attack;
                break;
            case EnemyState.Attack:
                if (!fov.canSeePlayer)
                {
                    idleResetTimer += Time.deltaTime;
                    if (idleResetTimer > idleResetTime)
                        State = EnemyState.Idle;
                }
                else
                    idleResetTimer = 0;
                break;
            case EnemyState.AttackCooldown:
                cooldownTimer += Time.deltaTime;
                if (cooldownTimer >= cooldown)
                    State = EnemyState.Attack;
                break;
        }

        if ((SideCheck(transform.forward, fov.targetMask) || SideCheck(-transform.forward, fov.targetMask) || SideCheck(transform.right, fov.targetMask) || SideCheck(-transform.right, fov.targetMask)) && canHit)
        {
            State = EnemyState.AttackCooldown;
            canHit = false;
            hit.collider.GetComponent<Player>().TriggerDamage(hit.point, damage);
        }
        else if (SideCheck(Vector3.up, fov.targetMask) && State != EnemyState.Dead)
        {
            transform.localScale = new(transform.localScale.x, transform.localScale.y / 2, transform.localScale.z);
            State = EnemyState.Dead;
            canHit = false;
            Destroy(gameObject, 2);
        }
        else if (impulse && (SideCheck(transform.forward, groundMask) || SideCheck(-transform.forward, groundMask) || SideCheck(transform.right, groundMask) || SideCheck(-transform.right, groundMask)))
        {
            impulse = false;
            impulseTimer = 0;   
        }

        if (!canHit && State != EnemyState.Dead)
        {
            hitTimer += Time.deltaTime;
            if (hitTimer >= hitCooldown)
            {
                canHit = true;
                hitTimer = 0;
            }
        }

        if (isGrounded && State != EnemyState.Dead && !impulse)
        {
            MovementResult movementResult = new();
            InputParameters inputData;

            switch (activeMovementBehavior)
            {
                case MovementBehaviors.FleeKinematic:
                    transform.rotation = Quaternion.LookRotation(new Vector3(transform.position.x - playerRef.transform.position.x, 0, transform.position.z - playerRef.transform.position.z));
                    inputData = new(gameObject.transform.position, playerRef.transform.position, Time.deltaTime, maxSpeed);
                    FleeKinematic(inputData, ref movementResult);
                    break;
                case MovementBehaviors.SeekKinematic:
                    if (fov.canSeePlayer)
                    {
                        inputData = new(gameObject.transform.position, playerRef.transform.position, Time.deltaTime, maxSpeed);
                        transform.LookAt(new Vector3(playerRef.transform.position.x, transform.position.y, playerRef.transform.position.z));
                        SeekKinematic(inputData, ref movementResult, true);
                    }
                    break;
                case MovementBehaviors.WanderKinematic:
                    int dir = Random.Range(0, 2) * 2 - 1;
                    inputData = new(gameObject.transform.position, targetObject.transform.position, Time.deltaTime, maxSpeed, dir);
                    transform.LookAt(new Vector3(targetObject.transform.position.x, transform.position.y, targetObject.transform.position.z));
                    WanderKinematic(inputData, ref movementResult);
                    break;
            }
            
            if (movementResult.newPosition == transform.position || (State == EnemyState.Attack && !fov.canSeePlayer) || SideCheck(transform.forward, groundMask))
            {
                anim.SetFloat("Speed", 0);
            }
            else
            {
                transform.position = movementResult.newPosition;
                anim.SetFloat("Speed", 1);
            }
        }
    }

    void WanderKinematic(InputParameters inputData, ref MovementResult result)
    {
        int range = 5 * inputData.directionMultiplier;
        wanderCounter += inputData.currentUpdateDuration;
        if (wanderCounter > maxWanderDuration)
        {
            Vector3 randomTarget = inputData.targetPosition;
            randomTarget.x += (float)r.NextDouble() * range;
            randomTarget.y = transform.position.y;
            randomTarget.z += (float)r.NextDouble() * range;
            Vector3 direction = randomTarget - transform.position;
            while (Physics.Raycast(transform.position, direction, Vector3.Distance(transform.position, randomTarget), groundMask) 
                || !Physics.Raycast(randomTarget + new Vector3(0, 0.1f, 0), Vector3.down, 1.1f, groundMask))
            {
                randomTarget = transform.position;
                randomTarget.x += (float)r.NextDouble() * range;
                randomTarget.z += (float)r.NextDouble() * range;
                direction = randomTarget - transform.position;
            }
            targetObject.transform.position = randomTarget;
            inputData.targetPosition = randomTarget;
            wanderCounter = 0.0f;
        }

        SeekKinematic(inputData, ref result, true);
    }
}
