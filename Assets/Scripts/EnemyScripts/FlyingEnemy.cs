using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AIHelpers;
using static UnityEngine.Rendering.HableCurve;

public class FlyingEnemy : BaseEnemy
{
    public GameObject[] patrolPoints;
    public GameObject propeller;
    public GameObject projectilePrefab;
    public Transform rayStart;
    public LayerMask playerMask;

    LineRenderer lr;
    Transform target;
    AStarPathFinding pathFinding;

    const float propellerRotationSpeed = 180;
    int increment = 1;
    int patrolIndex = 0;

    const float chaseResetTime = 5;
    float resetChaseTimer = 0;

    const float timeBetweenShots = 1;
    float shootTimer = 0;

    const float minDistance = 2.55f;

    const int maxSegmentCount = 300;

    Vector3[] segments;
    int numSegments = 0;

    [SerializeField] State patrolState = State.Patrol;
    private enum State
    {
        Patrol,
        Chase
    }

    public override void Start()
    {
        base.Start();
        pathFinding = GetComponent<AStarPathFinding>();
        target = patrolPoints[patrolIndex].transform;
        lr = GetComponentInChildren<LineRenderer>();
    }

    public override void Update()
    {
        propeller.transform.Rotate(0, propellerRotationSpeed * Time.deltaTime, 0);

        pathFinding.FindPath(target);

        if (shootTimer < timeBetweenShots)
            shootTimer += Time.deltaTime;

        if (patrolState == State.Patrol)
        {
            if (lr.enabled)
                lr.enabled = false;

            Vector3 startPos;
            Node nextNode;
            if (pathFinding.pathArray.Count > 1)
            {
                nextNode = pathFinding.pathArray[1];
                startPos = nextNode.position;
            }
            else
            {
                startPos = rayStart.position;
            }
            startPos.y /= 2;

            if (Physics.CheckBox(startPos, new Vector3(5, startPos.y, 5), transform.rotation, playerMask))
            {
                float distToPlayer = Vector3.Distance(playerRef.transform.position, rayStart.transform.position);
                if (Physics.Raycast(rayStart.transform.position, playerRef.transform.position - rayStart.transform.position, out hit, distToPlayer))
                {
                    if (hit.collider.gameObject.CompareTag("Player"))
                    {
                        patrolState = State.Chase;
                        target = playerRef.transform;
                    }
                }
            }

            MovementResult movementResult = new();
            InputParameters inputData;

            if (pathFinding.pathArray.Count > 1)
            {
                nextNode = pathFinding.pathArray[1];

                inputData = new(transform.position, nextNode.position, Time.deltaTime, maxSpeed);
                transform.LookAt(new Vector3(nextNode.position.x, transform.position.y, nextNode.position.z));
                SeekKinematic(inputData, ref movementResult, false);

                transform.position = movementResult.newPosition;
            }

            Vector3 hTargetPos = new(target.position.x, transform.position.y, target.position.z);
            if (Vector3.Distance(transform.position, hTargetPos) <= minDistance)
            {
                if (patrolIndex == 0)
                    increment = 1;
                else if (patrolIndex >= patrolPoints.Length - 1)
                    increment = -1;

                patrolIndex += increment;

                target = patrolPoints[patrolIndex].transform;
            }
        }
        else
        {
            float rayDist = Vector3.Distance(rayStart.position, playerRef.transform.position);
            bool rayHit = Physics.Raycast(rayStart.position, playerRef.transform.position - rayStart.position, out hit, rayDist);
            if ((rayHit && !hit.collider.gameObject.CompareTag("Player")) || !rayHit)
            {
                if (lr.enabled && resetChaseTimer > 0.5f)
                    lr.enabled = false;

                resetChaseTimer += Time.deltaTime;
                if (resetChaseTimer >= chaseResetTime)
                {
                    patrolState = State.Patrol;
                    target = patrolPoints[patrolIndex].transform;
                    resetChaseTimer = 0;
                }
            }
            else
            {
                resetChaseTimer = 0;

                if (shootTimer >= timeBetweenShots)
                {
                    ShootProjectile();
                    shootTimer = 0;
                }
            }

            MovementResult movementResult = new();
            InputParameters inputData;
            Node nextNode;
            if (pathFinding.pathArray.Count > 2)
            {
                nextNode = pathFinding.pathArray[1];

                inputData = new(transform.position, nextNode.position, Time.deltaTime, maxSpeed);
                transform.LookAt(new Vector3(nextNode.position.x, transform.position.y, nextNode.position.z));
                SeekKinematic(inputData, ref movementResult, false);

                transform.position = movementResult.newPosition;
            }

        }
    }

    public override void FixedUpdate()
    {

    }

    public void SimulatePath(Vector3 initVel)
    {
        lr.enabled = true;

        float timeStep = Time.fixedDeltaTime;

        Vector3 velocity = initVel * timeStep;
        Vector3 gravity = new(0, 2 * this.gravity, 0);
        Vector3 position = rayStart.transform.position;

        if (segments == null || segments.Length != maxSegmentCount)
        {
            segments = new Vector3[maxSegmentCount];
        }

        segments[0] = position;

        for (int i = 1; i < maxSegmentCount && position.y > 0f; i++)
        {
            velocity += timeStep * timeStep * gravity;

            position += velocity;

            segments[i] = position;
            numSegments = i + 1;
        }

        Draw();
    }

    private void Draw()
    {
        lr.transform.position = segments[0];

        lr.positionCount = numSegments;
        for (int i = 0; i < numSegments; i++)
        {
            lr.SetPosition(i, segments[i]);
        }
    }

    void ShootProjectile()
    {
        Vector3 xDist = playerRef.transform.position - rayStart.position;
        float yDist = xDist.y;
        xDist.y = 0;
        float dropTime = Mathf.Sqrt(yDist / gravity);
        Vector3 initVel = xDist / dropTime;

        GameObject projectile = Instantiate(projectilePrefab, rayStart.transform.position, transform.rotation);
        Projectile projComponent = projectile.GetComponent<Projectile>();
        projComponent.velocity = initVel * Time.fixedDeltaTime;

        SimulatePath(initVel);
    }

}
