using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsTargetShoot : MonoBehaviour
{
    public GameObject target;
    public GameObject verticalMarker;

    public Vector3 startingVelocity;

    public bool shootPlayer = false;

    public GameObject projectilePrefab;

    private List<Vector3> gizmosPos = new List<Vector3>();

    void Update()
    {
        if (shootPlayer)
        {
            UpdateInitialVelocity();

            float x0 = transform.position.x;
            float y0 = transform.position.y;
            float z0 = transform.position.z;


            gizmosPos.Clear();

            float gravity = Mathf.Abs(Physics.gravity.y);

            float maxGizmoTime = (2.0f * startingVelocity.y) / gravity;

            float maxDistanceX = (2.0f * startingVelocity.x * startingVelocity.y) / gravity;
            float maxDistanceZ = (2.0f * startingVelocity.z * startingVelocity.y) / gravity;

            float timeStep = maxGizmoTime / Mathf.Max(maxDistanceX, maxDistanceZ);
            timeStep /= 10.0f;

            for (float projTime = 0.0f; projTime < maxGizmoTime; projTime += timeStep)
            {
                Vector3 newPos;
                newPos.x = x0 + startingVelocity.x * projTime;
                newPos.y = (y0 + startingVelocity.y * projTime) - (0.5f * gravity * projTime * projTime);
                newPos.z = z0 + startingVelocity.z * projTime;

                gizmosPos.Add(newPos);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        foreach (Vector3 positions in gizmosPos)
        {
            Gizmos.DrawSphere(positions, 0.03f);
        }

    }

    public void ShootProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
        projectile.GetComponent<Rigidbody>().velocity = startingVelocity;
    }

    Vector3 GetSquaredVector(Vector3 vecToSquare)
    {
        return new Vector3(vecToSquare.x * vecToSquare.x, vecToSquare.y * vecToSquare.y, vecToSquare.z * vecToSquare.z);
    }

    Vector3 GetSqrtVector(Vector3 vecToSqrt)
    {
        float length = Mathf.Sqrt(vecToSqrt.magnitude);
        return vecToSqrt.normalized * length;
    }

    Vector3 GetFinalVelocity(Vector3 displacement, Vector3 initialVelocity, Vector3 acceleration)
    {
        Vector3 finalVelocitySq = GetSquaredVector(initialVelocity) + 2.0f * acceleration * displacement.magnitude;
        return GetSqrtVector(finalVelocitySq);
    }

    float GetFlightTime(Vector3 displacement, Vector3 initialVelocity, Vector3 acceleration)
    {
        Vector3 finalVelocity = GetFinalVelocity(displacement, Vector3.zero, Physics.gravity);

        return 2 * displacement.magnitude / (finalVelocity.magnitude - initialVelocity.magnitude);
    }

    void UpdateInitialVelocity()
    {
        Vector3 verticalDisplacement = transform.position - target.transform.position;
        verticalDisplacement.x = 0;
        verticalDisplacement.z = 0;

        Vector3 finalVelocity = GetFinalVelocity(verticalDisplacement, Vector3.zero, Physics.gravity);
        Vector3 initialVelocity = -finalVelocity;

        float flightTime = 2.0f * GetFlightTime(verticalDisplacement, Vector3.zero, Physics.gravity);

        Vector3 horizontalDisplacement = target.transform.position - transform.position;
        Vector3 horizontalVelocity = horizontalDisplacement / flightTime;
        initialVelocity += horizontalVelocity;

        Vector3 projectileDirection = initialVelocity;
        projectileDirection.Normalize();

        float finalSpeed = initialVelocity.magnitude;

        startingVelocity = projectileDirection * finalSpeed;
    }
}
