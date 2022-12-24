using UnityEngine;
using UnityEditor;
using System;

public class AIHelpers
{
    
    static private float maxDistance = 20.0f;
    static private float minDistance = 1;
    static private float currentDistance;

    public class MovementResult
    {
        public Vector3 newPosition = Vector3.zero;
        public Vector3 newOrientation = Vector3.zero;
    }

    public class InputParameters
    {
        public InputParameters(Vector3 current, Vector3 target, float updateDelta, float speed, int dir)
        {
            directionMultiplier = dir;
            currentPosition = current;
            targetPosition = target;
            currentUpdateDuration = updateDelta;
            maxSpeed = speed;
        }

        public InputParameters(Vector3 current, Vector3 target, float updateDelta, float speed)
        {
            currentPosition = current;
            targetPosition = target;
            currentUpdateDuration = updateDelta;
            maxSpeed = speed;
        }

        public InputParameters(InputParameters o)
        {
            currentPosition = o.currentPosition;
            targetPosition = o.targetPosition;
            currentUpdateDuration = o.currentUpdateDuration;
            maxSpeed = o.maxSpeed;
        }

        public InputParameters()
        {
            currentUpdateDuration = 0.0f;
            maxSpeed = 1.0f;
        }

        public int directionMultiplier = 1;
        public Vector3 currentPosition;
        public Vector3 targetPosition;
        public float currentUpdateDuration;
        public float maxSpeed;
    }

    public enum MovementBehaviors
    {
        SeekKinematic,
        FleeKinematic,
        WanderKinematic,
        Null
    }

    internal static void SeekKinematic(InputParameters inputData, ref MovementResult result, bool normalizeY)
    {

        currentDistance = Vector3.Distance(inputData.currentPosition, inputData.targetPosition);

        if (currentDistance > minDistance)
        {
            Vector3 directionToTarget = inputData.targetPosition - inputData.currentPosition;
            if (normalizeY)
                directionToTarget.y = 0;
            directionToTarget.Normalize();

            result.newPosition = inputData.currentPosition + directionToTarget * inputData.currentUpdateDuration * inputData.maxSpeed;
        }
        else
            result.newPosition = inputData.currentPosition;
    }

    internal static void FleeKinematic(InputParameters inputData, ref MovementResult result)
    {
        currentDistance = Vector3.Distance(inputData.currentPosition, inputData.targetPosition);

        if (currentDistance < maxDistance)
        {
            Vector3 directionToTarget = inputData.currentPosition - inputData.targetPosition;
            directionToTarget.y = 0;
            directionToTarget.Normalize();

            result.newPosition = inputData.currentPosition + directionToTarget * inputData.currentUpdateDuration * inputData.maxSpeed;
        }
        else
            result.newPosition = inputData.currentPosition;
    }

}