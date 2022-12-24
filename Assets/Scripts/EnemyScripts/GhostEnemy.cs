using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static AIHelpers;

public class GhostEnemy : BaseEnemy
{
    [SerializeField] List<Vector3> playerPositions;
    public LayerMask playerMask;

    float startTimer = 0;
    const float timeBetweenPositions = 5;
    float storePositionTimer = 0;
    const float baseHoverHeight = 5;
    int indexToFollow = 2;
    bool follow = false;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        //StorePlayerPos();
    }

    // Update is called once per frame
    public override void Update()
    {
        if (startTimer < 30)
            startTimer += Time.deltaTime;

        storePositionTimer += Time.deltaTime;
        if (storePositionTimer >= timeBetweenPositions)
        {
            storePositionTimer = 0;
            StorePlayerPos();
            if (playerPositions.Count >= 3)
                follow = true;
        }

        if (follow && !impulse)
        {
            MovementResult movementResult = new();
            Vector3 target;
            if (indexToFollow <= 0 || Physics.CheckSphere(transform.position, 10, playerMask))
            {
                target = playerRef.transform.position;
            }
            else
            {
                target = playerPositions[indexToFollow];
                target = new(target.x, baseHoverHeight, target.z);
            }

            InputParameters inputData = new(transform.position, target, Time.deltaTime, maxSpeed);
            transform.LookAt(target);
            SeekKinematic(inputData, ref movementResult, false);
            transform.position = movementResult.newPosition;

            if (Vector3.Distance(transform.position, target) <= 1 && indexToFollow > 0)
            {
                indexToFollow--;
            }
        }
    }

    public override void FixedUpdate()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Player>().Health = 0;
        }
    }

    void StorePlayerPos()
    {
        playerPositions.Insert(0, playerRef.transform.position);
        while (playerPositions.Count - 1 > indexToFollow)
        {
            playerPositions.RemoveAt(playerPositions.Count - 1);
        }
    }
}
