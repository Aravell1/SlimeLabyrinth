using UnityEngine;
using static AIHelpers;

public class PassiveEnemy : BaseEnemy
{
    public LayerMask playerMask;
    public LayerMask enemyMask;
    public GameObject[] patrolPoints;

    AStarPathFinding pathFinding;
    Transform target;

    int increment = 1;
    int patrolIndex = 0;
    bool canFlip = true;
    float flipTimer = 0;
    const float flipTime = 1;

    bool canHit = true;
    const float hitCooldown = 2;
    float hitTimer = 0;

    const float minDistance = 2.75f;

    [SerializeField] private PatrolType patrol = PatrolType.Reverse;
    private enum PatrolType
    {
        Loop,
        Reverse
    }

    public override void Start()
    {
        base.Start();
        target = patrolPoints[patrolIndex].transform;
        pathFinding = GetComponent<AStarPathFinding>();
    }

    public override void Update()
    {
        base.Update();

        pathFinding.FindPath(target);

        if (Physics.Raycast(transform.position, (playerRef.transform.position - transform.position).normalized, out hit, attackDistance, playerMask) && canHit)
        {
            hit.collider.GetComponent<Player>().TriggerDamage(hit.point, damage);
            canHit = false;
            FlipDirection();
        }
        else if ((SideCheck(transform.forward, enemyMask) || SideCheck(-transform.forward, enemyMask) || SideCheck(transform.right, enemyMask) || SideCheck(-transform.right, enemyMask)) && canFlip)
        {
            if (hit.collider.GetComponent<PassiveEnemy>())
                hit.collider.GetComponent<PassiveEnemy>().FlipDirection();
            FlipDirection();
        }
        else if (impulse && (SideCheck(transform.forward, groundMask) || SideCheck(-transform.forward, groundMask) || SideCheck(transform.right, groundMask) || SideCheck(-transform.right, groundMask)))
        {
            impulse = false;
            impulseTimer = 0;
        }

        if (!canFlip)
        {
            flipTimer += Time.deltaTime;
            if (flipTimer >= flipTime)
            {
                canFlip = true;
                flipTimer = 0;
            }
        }

        if (!canHit)
        {
            hitTimer += Time.deltaTime;
            if (hitTimer >= hitCooldown)
            {
                canHit = true;
                hitTimer = 0;
            }
        }

        Vector3 hTargetPos;
        if (!impulse && pathFinding.pathArray.Count > 1)
        {
            MovementResult movementResult = new();
            InputParameters inputData;
            Node nextNode = (Node)pathFinding.pathArray[1];

            inputData = new(transform.position, nextNode.position, Time.deltaTime, maxSpeed);
            transform.LookAt(new Vector3(nextNode.position.x, transform.position.y, nextNode.position.z));
            SeekKinematic(inputData, ref movementResult, true);

            transform.position = movementResult.newPosition;
        }

        hTargetPos = new(target.position.x, transform.position.y, target.position.z);
        if (Vector3.Distance(transform.position, hTargetPos) <= minDistance)
        {
            if (patrol == PatrolType.Reverse)
            {
                if (patrolIndex == 0)
                    increment = 1;
                else if (patrolIndex >= patrolPoints.Length - 1)
                    increment = -1;

                patrolIndex += increment;
            }
            else
            {
                if (patrolIndex >= patrolPoints.Length - 1 && increment == 1)
                    patrolIndex = 0;
                else if (patrolIndex == 0 && increment == -1)
                    patrolIndex = patrolPoints.Length - 1;
                else
                    patrolIndex += increment;
            }
            target = patrolPoints[patrolIndex].transform;
        }

    }

    public void FlipDirection()
    {
        canFlip = false;

        increment *= -1;
        patrolIndex += increment;
        if (patrolIndex < 0)
            patrolIndex = patrolPoints.Length - 1;
        else if (patrolIndex > patrolPoints.Length - 1)
            patrolIndex = 0;
        target = patrolPoints[patrolIndex].transform;

    }
}
