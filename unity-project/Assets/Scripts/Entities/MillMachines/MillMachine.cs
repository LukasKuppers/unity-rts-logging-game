using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MillMachine : MonoBehaviour, IHumanoidOperatable
{
    private enum Axis
    {
        X, Y, Z
    }

    [SerializeField]
    private GameObject entryPoint;
    [SerializeField]
    private GameObject sawObject;
    [SerializeField]
    private Axis sawMoveAxis;
    [SerializeField]
    private Axis sawUpAxis;
    [SerializeField]
    private Vector2 sawMoveRange;
    [SerializeField]
    private Vector2 sawRaiseRange;

    private MillProductQueue productQueue;
    private GameObject operatorHumanoid;

    private Vector3 horizontalSawAxis;
    private Vector3 verticalSawAxis;
    private Vector2 currentSawPosition;
    private bool isSawCoroutineRunning = false;

    // visualize saw movement limits
    private void OnDrawGizmosSelected()
    {
        Vector3 move = GetDirectionFromAxis(sawMoveAxis);
        Vector3 up = GetDirectionFromAxis(sawUpAxis);

        Vector3 a = (move * sawMoveRange.x) + (up * sawRaiseRange.x);
        Vector3 b = (move * sawMoveRange.y) + (up * sawRaiseRange.x);
        Vector3 c = (move * sawMoveRange.x) + (up * sawRaiseRange.y);
        Vector3 d = (move * sawMoveRange.y) + (up * sawRaiseRange.y);

        Vector3[] limits = new Vector3[] {a, b, d, c}.Select((Vector3 localLimit) => transform.TransformPoint(localLimit)).ToArray();

        Gizmos.color = Color.red;
        Gizmos.DrawLineStrip(limits, true);
    }

    private void Awake()
    {
        productQueue = gameObject.GetComponent<MillProductQueue>();
        if (!productQueue)
        {
            Debug.LogWarning($"MillMachine: Awake: {gameObject.name} does not have a MillProductQueue. Attaching default MillProductQueue.");
            productQueue = gameObject.AddComponent<MillProductQueue>();
        }

        horizontalSawAxis = GetDirectionFromAxis(sawMoveAxis);
        verticalSawAxis = GetDirectionFromAxis(sawUpAxis);

        currentSawPosition = Vector2.up;
        SetSawPosition(currentSawPosition);
    }

    public MillProductQueue GetProductQueue()
    {
        return productQueue;
    }

    public void AddOccupant(GameObject humanoid)
    {
        if (!operatorHumanoid)
        {
            operatorHumanoid = humanoid;
            operatorHumanoid.SetActive(false);
        }
        else
            Debug.LogWarning($"MillMachine: AddOccupant: Humanoid {operatorHumanoid.name} is already operating the machine.");
    }

    public void EjectOccupant(GameObject humanoid)
    {
        if (ReferenceEquals(humanoid, operatorHumanoid))
        {
            humanoid.transform.position = entryPoint.transform.position;
            humanoid.SetActive(true);
            operatorHumanoid = null;
        }
    }

    public GameObject GetDriver()
    {
        return operatorHumanoid;
    }

    public Vector3 GetEntryPoint()
    {
        return entryPoint.transform.position;
    }

    public int GetOccupantCount()
    {
        return operatorHumanoid != null ? 1 : 0;
    }

    public List<GameObject> GetPassengers()
    {
        return new List<GameObject>();
    }

    public bool IsFull()
    {
        return operatorHumanoid != null;
    }

    /// <summary>
    /// animate saw movement from the current position to the specified target position.
    /// </summary>
    /// <param name="targetPosition">The target position (between 0 and 1)</param>
    /// <param name="velocity">The speed in m/s the saw should move</param>
    /// <param name="moveHorizontal">will move horizontally if true, else, vertically</param>
    /// <returns>A promise that resolves to true once the animation is complete</returns>
    public Promise<bool> AnimateSawMovement(float targetPosition, float velocity, bool moveHorizontal)
    {
        Promise<bool> promise = new Promise<bool>();

        if (isSawCoroutineRunning)
        {
            Debug.Log("MillMachine: AnimateSawMovement: Saw is already moving, aborting.");
            return promise.Reject("Saw already moving");
        }

        if (targetPosition < 0f || targetPosition > 1f)
        {
            Debug.Log("MillMachine: AnimateSawMovement: Specified target coordinate is out of range. Positions must be between 0 and 1. Aborting.");
            return promise.Reject("Target position out of range");
        }

        if (velocity <= 0)
        {
            Debug.Log("MillMachine: AnimateSawMovement: Specified velocity must be positive. Aborting.");
            return promise.Reject("Invalid velocity");
        }

        isSawCoroutineRunning = true;
        Promise<bool> animPromise = new Promise<bool>();
        animPromise.Then(() =>
        {
            isSawCoroutineRunning = false;
            promise.Resolve(true);
        }).Catch((string msg) => promise.Reject(msg));

        StartCoroutine(LerpSawPosition(targetPosition, moveHorizontal, velocity, animPromise));
        return promise;
    }

    private Vector3 GetDirectionFromAxis(Axis axis)
    {
        Vector3 direction = axis == Axis.X ? Vector3.right :
                            axis == Axis.Y ? Vector3.up : 
                                             Vector3.forward;
        return direction;
    }

    // set the saw position based on coordinates (between 0 and 1).
    // assumes horizontal and vertical saw axis have been set
    private void SetSawPosition(Vector2 sawCoordinates)
    {
        float localHorizontal = Mathf.Lerp(sawMoveRange.x, sawMoveRange.y, sawCoordinates.x);
        float localVertical = Mathf.Lerp(sawRaiseRange.x, sawRaiseRange.y, sawCoordinates.y);

        sawObject.transform.localPosition = (horizontalSawAxis * localHorizontal) + (verticalSawAxis * localVertical);
    }

    // assumes all input parameters are valid
    private IEnumerator LerpSawPosition(float targetCoordinate, bool isHorizontal, float velocity, Promise<bool> promise)
    {
        Vector2 axisRange = isHorizontal ? sawMoveRange : sawRaiseRange;
        float totalAxisLength = Mathf.Abs(axisRange.y - axisRange.x);

        float currentCoord = isHorizontal ? currentSawPosition.x : currentSawPosition.y;
        float moveDirection = targetCoordinate > currentCoord ? 1f : -1f;

        while (currentCoord != targetCoordinate)
        {
            if ((targetCoordinate - currentCoord) * totalAxisLength * moveDirection <= velocity * Time.smoothDeltaTime)
            {
                currentCoord = targetCoordinate;
            }
            else
            {
                // move saw towards target
                float delta = moveDirection * velocity * Time.deltaTime;
                currentCoord += delta;

                if (isHorizontal)
                    currentSawPosition.x = currentCoord;
                else
                    currentSawPosition.y = currentCoord;

                SetSawPosition(currentSawPosition);
                yield return null;
            }
        }

        if (isHorizontal)
            currentSawPosition.x = currentCoord;
        else
            currentSawPosition.y = currentCoord;

        SetSawPosition(currentSawPosition);
        promise.Resolve(true);
    }
}
