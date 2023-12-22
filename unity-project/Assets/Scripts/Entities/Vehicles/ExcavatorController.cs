using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExcavatorController : MonoBehaviour
{
    [SerializeField]
    private GameObject targetObject;
    [SerializeField]
    private GameObject frontClawBone;
    [SerializeField]
    private GameObject backClawBone;
    [SerializeField]
    private float armMoveSpeed = 1f;
    [SerializeField]
    private float clawGrabSpeed = 20f;
    [SerializeField]
    private float minClawDistFromCenter = 7f;
    private float sqrMinClawDist;
    [SerializeField]
    [Tooltip("x value is closed rotation, y value is open rotation")]
    private Vector2 clawRotationLimits;
    [SerializeField]
    [Tooltip("How far below the claw root is the grab point?")]
    private float verticalClawOffset = 0f;

    private float currentClawRotation;
    private float currentTargetRotation = 0f;

    private bool isArmCoroutineRunning = false;
    private bool isClawGrabCoroutineRunning = false;
    private bool isClawRotateCoroutineRunning = false;

    private Queue<Vector3> finalTargetQueue;


    private void Awake()
    {
        sqrMinClawDist = minClawDistFromCenter * minClawDistFromCenter;

        // set claw to closed
        currentClawRotation = clawRotationLimits.x;
        SetClawsToRotation(currentClawRotation);
        SetTargetToRotation(currentTargetRotation);
    }
    
    /// <summary>
    /// Make a gameobject a child of the claw, so that it is carried with the claw.
    /// Preserves any pre-existing offset between claw and object.
    /// </summary>
    /// <param name="obj">The object to attach</param>
    public void AttachObjectToClaw(GameObject obj)
    {
        obj.transform.parent = targetObject.transform;
    }

    /// <summary>
    /// Try to set the claw to open or closed.
    /// </summary>
    /// <param name="isOpen">The desired claw state (open if true, closed if false)</param>
    /// <returns>Promise that will evaluate to true once the operation is complete</returns>
    public Promise<bool> SetClawState(bool isOpen)
    {
        Promise<bool> promise = new Promise<bool>();
        if (isClawGrabCoroutineRunning)
            return promise.Reject("Claw state already changing");

        isClawGrabCoroutineRunning = true;
        StartCoroutine(LerpClawToState(isOpen, promise));
        return promise;
    }

    /// <summary>
    /// Try to set the claw rotation to rotationDeg (in the local up axis).
    /// The claws will be rotated to the target relative to the excavator rotation at the time of invoking the function.
    /// </summary>
    /// <param name="grabDirection">The vector along which you wish the claws to grab (direction)</param>
    /// <param name="relativeTo">Is the desired grab vector in world space, or relative to the excavator?</param>
    /// <returns>Promise that will evaluate to true once the operation is complete</returns>
    public Promise<bool> SetClawRotation(Vector3 grabDirection, Space relativeTo)
    {
        Promise<bool> promise = new Promise<bool>();

        if (isClawRotateCoroutineRunning)
        {
            Debug.Log("ExcavatorController: SetClawRotation: Cannot rotate claw, because claw is already actively rotating. Aborting...");
            return promise.Reject("Claw rotation already changing");
        }

        if (relativeTo == Space.World)
            grabDirection = transform.InverseTransformDirection(grabDirection);

        float targetRotation = -Vector2.SignedAngle(Vector2.right, new Vector2(grabDirection.x, grabDirection.z));

        isClawRotateCoroutineRunning = true;
        StartCoroutine(LerpClawToRotation(targetRotation, promise));
        return promise;
    }

    /// <summary>
    /// Try to move the arm to position the claws around specified target.
    /// The claws will be moved to the target relative to the excator position at the time of invoking the function.
    /// </summary>
    /// <param name="desiredTarget">The desired position for the excavator claws</param>
    /// <param name="relativeTo">Is the desiredTarget in world space, or relative to the excavator?</param>
    /// <returns>Promise that will evaluate to true once the operation is complete</returns>
    public Promise<bool> MoveArmToPosition(Vector3 desiredTarget, Space relativeTo)
    {
        Promise<bool> promise = new Promise<bool>();

        if (isArmCoroutineRunning)
        {
            Debug.Log("ExcavatorController: MoveArmToPostiion: Cannot move arm, because arm is already actively moving. Aborting...");
            return promise.Reject("Arm position already changing");
        }

        Vector3 finalTargetLocal = relativeTo == Space.Self ? desiredTarget : transform.InverseTransformPoint(desiredTarget);

        if (!IsPositionValid(finalTargetLocal))
        {
            Debug.LogWarning("ExcavatorController: MoveArmToPosition: specified target is too close to excavator center. Aborting...");
            return promise.Reject("Specified target too close to excavator center");
        }

        finalTargetLocal += Vector3.up * verticalClawOffset;

        finalTargetQueue = new Queue<Vector3>();
        finalTargetQueue.Enqueue(finalTargetLocal);

        isArmCoroutineRunning = true;
        StartCoroutine(LerpArmToFinalTarget(promise));
        return promise;
    }

    /// <summary>
    /// Try to move the arm to a sequence of positions - moves to next position once previous has been achieved.
    /// All positions will be considered relative to the position of the excavator at the time of invoking the function.
    /// </summary>
    /// <param name="targetSequence">Sequence of positions to move through</param>
    /// <param name="relativeTo">Is each target in the sequence in world space, or relative to the excavator?</param>
    /// <returns>Promise that will evaluate to true once the operation is complete</returns>
    public Promise<bool> TravelArmPositionSequence(List<Vector3> targetSequence, Space relativeTo)
    {
        Promise<bool> promise = new Promise<bool>();

        if (targetSequence.Count == 0)
            return promise.Resolve(true);

        if (isArmCoroutineRunning)
        {
            Debug.Log("ExcavatorController: TravelArmPositionSequence: Cannot execute sequence, because arm is already actively moving. Aborting...");
            return promise.Reject("Arm position already changing");
        }

        if (relativeTo == Space.World)
        {
            // translate positions to local space
            targetSequence = targetSequence.Select(target =>
            {
                Vector3 outputTarget = transform.InverseTransformPoint(target);
                return outputTarget + (Vector3.up * verticalClawOffset);
            }).ToList();
        }

        // check that every position is valid
        foreach (Vector3 target in targetSequence)
        {
            if (!IsPositionValid(target))
            {
                Debug.LogWarning($"ExcavatorController: TravelArmPositionSequence: Target position {target} is invalid. Aborting...");
                return promise.Reject("Specified target too close to excavator center");
            }
        }

        // everything is valid, ready to execute sequence
        finalTargetQueue = new Queue<Vector3>(targetSequence);

        isArmCoroutineRunning = true;
        StartCoroutine(LerpArmToFinalTarget(promise));
        return promise;
    }

    // is the target (in local space) a valid claw position?
    private bool IsPositionValid(Vector3 targetPostion)
    {
        return new Vector2(targetPostion.x, targetPostion.z).sqrMagnitude >= sqrMinClawDist;
    }

    // set claws both to the specified rotation
    private void SetClawsToRotation(float rotationDeg)
    {
        backClawBone.transform.localEulerAngles = new Vector3(rotationDeg, 0, 0);
        frontClawBone.transform.localEulerAngles = new Vector3(-rotationDeg, 0, 0);
    }

    // set the local target Y rotation
    private void SetTargetToRotation(float rotationDeg)
    {
        targetObject.transform.localEulerAngles = new Vector3(0, rotationDeg, 0);
    }

    private IEnumerator LerpArmToFinalTarget(Promise<bool> promise)
    {
        while (finalTargetQueue.Count > 0)
        {
            Vector3 finalTargetLocal = finalTargetQueue.Peek();

            while (targetObject.transform.localPosition != finalTargetLocal)
            {
                if (Vector3.Distance(targetObject.transform.localPosition, finalTargetLocal) <= armMoveSpeed * Time.smoothDeltaTime)
                {
                    targetObject.transform.localPosition = finalTargetLocal;
                }
                else
                {
                    Vector3 originalPosition = targetObject.transform.localPosition;
                    Vector3 moveDirection = (finalTargetLocal - originalPosition).normalized;
                    targetObject.transform.localPosition += moveDirection * (armMoveSpeed * Time.deltaTime);

                    Vector3 lookDir = new Vector3(targetObject.transform.localPosition.x, 0, targetObject.transform.localPosition.z);
                    float targetDist = lookDir.magnitude;
                    if (targetDist < minClawDistFromCenter)
                    {
                        // target too close - need to rotate towards final target
                        // first, reset position
                        // targetObject.transform.localPosition = originalPosition;
                        targetObject.transform.localPosition = (Vector3.up * originalPosition.y) + (lookDir.normalized * minClawDistFromCenter);

                        // calculate possible translation vectors (in local space)
                        Vector3 vec1 = Vector3.Cross(lookDir, Vector3.up).normalized * (armMoveSpeed * Time.deltaTime);
                        Vector3 vec2 = -vec1;

                        // calculate distance target will be from final target for each translation
                        float sqrDist1 = (finalTargetLocal - (targetObject.transform.localPosition + vec1)).sqrMagnitude;
                        float sqrDist2 = (finalTargetLocal - (targetObject.transform.localPosition + vec2)).sqrMagnitude;

                        // translate by vector that will get us closer to target
                        Vector3 localTranslation = sqrDist1 < sqrDist2 ? vec1 : vec2;
                        targetObject.transform.Translate(localTranslation, Space.Self);
                    }

                    yield return null;
                }
            }
            finalTargetQueue.Dequeue();
        }

        promise.Resolve(true);
        isArmCoroutineRunning = false;
    }

    private IEnumerator LerpClawToState(bool open, Promise<bool> promise)
    {
        float targetRotation = open ? clawRotationLimits.y : clawRotationLimits.x;

        while (currentClawRotation != targetRotation)
        {
            if (Mathf.Abs(targetRotation - currentClawRotation) <= clawGrabSpeed * Time.smoothDeltaTime)
            {
                currentClawRotation = targetRotation;
                SetClawsToRotation(currentClawRotation);
            } 
            else
            {
                float direction = targetRotation > currentClawRotation ? 1f : -1f;
                currentClawRotation += direction * (clawGrabSpeed * Time.deltaTime);
                SetClawsToRotation(currentClawRotation);

                yield return null;
            }
        }

        promise.Resolve(true);
        isClawGrabCoroutineRunning = false;
    }

    private IEnumerator LerpClawToRotation(float targetRotation, Promise<bool> promise)
    {
        while (currentTargetRotation != targetRotation)
        {
            if (Mathf.Abs(targetRotation - currentTargetRotation) <= clawGrabSpeed * Time.smoothDeltaTime)
            {
                currentTargetRotation = targetRotation;
                SetTargetToRotation(currentTargetRotation);
            }
            else
            {
                float direction = targetRotation > currentTargetRotation ? 1f : -1f;
                currentTargetRotation += direction * (clawGrabSpeed * Time.deltaTime);
                SetTargetToRotation(currentTargetRotation);

                yield return null;
            }
        }

        promise.Resolve(true);
        isClawRotateCoroutineRunning = false;
    }
}
