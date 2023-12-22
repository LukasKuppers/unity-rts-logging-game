using System;
using System.Collections.Generic;
using UnityEngine;

public class ExcavatorDropCommand : ICommand
{
    public event Action commandCompletedEvent;

    private ExcavatorController excavatorController;
    private Vehicle vehicle;
    private GameObject excavatorObject;
    private GameObject receivingObject;
    private Vector3 dropPosition;
    private Vector3 dropDirection;
    private float maxArmRange;

    public ExcavatorDropCommand(GameObject excavatorObject, GameObject receivingObject, Vector3 dropPosition)
    {
        excavatorController = excavatorObject.GetComponent<ExcavatorController>();
        if (!excavatorController)
        {
            Debug.LogError("ExcavatorDropCommand: Constructor: Provided Excavator object does not have an attached excavatorController.");
        }

        vehicle = excavatorObject.GetComponent<Vehicle>();
        if (!vehicle)
        {
            Debug.LogError("ExcavatorDropCommand: Constructor: Provided Excavaator object does not have an attached Vehicle component.");
        }

        this.excavatorObject = excavatorObject;
        this.receivingObject = receivingObject;
        this.dropPosition = dropPosition;

        dropDirection = Vector3.zero;
        maxArmRange = 20f;  // default value
    }

    public ExcavatorDropCommand(GameObject excavatorObject, GameObject receivingObject, Vector3 dropPosition, float maxArmRange) :
        this(excavatorObject, receivingObject, dropPosition)
    {
        if (maxArmRange > 0)
            this.maxArmRange = maxArmRange;
    }

    public ExcavatorDropCommand(GameObject excavatorObject, GameObject receivingObject, Vector3 dropPosition, Vector3 dropDirection) : 
        this(excavatorObject, receivingObject, dropPosition)
    {
        this.dropDirection = dropDirection;
    }

    public ExcavatorDropCommand(GameObject excavatorObject, GameObject receivingObject, Vector3 dropPosition, Vector3 dropDirection, float maxArmRange) : 
        this(excavatorObject, receivingObject, dropPosition, dropDirection)
    {
        if (maxArmRange > 0)
            this.maxArmRange = maxArmRange;
    }

    public void Execute()
    {
        if (vehicle.GetDriver() == null)
        {
            commandCompletedEvent.Invoke();
            return;
        }

        GameObject payloadObject = vehicle.GetPayload();

        StackableObject clickedStackable = receivingObject.GetComponent<StackableObject>();
        CollectableObject payloadCollectable = payloadObject.GetComponent<CollectableObject>();
        IObjectRepository receivingRepo = receivingObject.GetComponent<IObjectRepository>();   // the clicked object may itself be a stack

        Vector3 actualDropPosition = dropPosition;
        Vector3 actualDropDirection = dropDirection;

        // resolve clickedStackable and receivingStack
        if (clickedStackable && receivingRepo != null)
            clickedStackable = null;    // This is really an error, but try to continue as if clicked object is an object repo

        if (clickedStackable)
            receivingRepo = clickedStackable.GetCurrentRepository();

        // error handling
        if (receivingRepo != null && !payloadCollectable)
        {
            Debug.LogWarning("ExcavatorDropCommand: Execute: Attempted to drop non-stackable object on a repository. Aborting.");
            commandCompletedEvent?.Invoke();
            return;
        }

        if (receivingRepo != null && payloadCollectable && receivingRepo.GetObjectType() != payloadCollectable.GetObjectType())
        {
            Debug.LogWarning($"ExcavatorDropCommand: Execute: Cannot add type {payloadCollectable.GetObjectType()} to repo of type {clickedStackable.GetObjectType()}. Aborting");
            commandCompletedEvent?.Invoke();
            return;
        }

        if (receivingRepo != null)
        {
            if (receivingRepo.IsFull())
            {
                Debug.Log("ExcavatorDropCommand: Execute: Selected repository is already full. Aborting.");
                commandCompletedEvent?.Invoke();
                return;
            }
            // we are dropping on a stack, update actual drop position and rotation
            Transform dropOrientation = receivingRepo.GetNextAddTransform();

            actualDropPosition = dropOrientation.position;
            actualDropDirection = payloadObject.GetComponent<VehiclePickup>().GetGrabDirectionForTransform(dropOrientation);
        }

        // make sure dropPosition is in range
        if (Vector3.Distance(excavatorObject.transform.position, actualDropPosition) > maxArmRange)
        {
            Debug.LogWarning("ExcavatorDropCommand: Execute: Drop position is out of range. Aborting.");
            commandCompletedEvent?.Invoke();
            return;
        }

        // ready to execute animation
        List<Vector3> armPositions = new List<Vector3>
        {
            actualDropPosition + Vector3.up * 4f,
            actualDropPosition,
        };

        excavatorController.SetClawRotation(actualDropDirection, Space.World)
            .Then(() => excavatorController.TravelArmPositionSequence(armPositions, Space.World))
            .Then(() => excavatorController.SetClawState(true))
            .Then(() =>
            {
                // remove payload from vehicle
                vehicle.RemovePayload();
                payloadObject.transform.parent = null;


                StackableObject payloadStackable = payloadObject.GetComponent<StackableObject>();
                if (payloadStackable && receivingRepo == null)  // create a new stack
                    CreateAndInitStack(payloadStackable.GetStackPrefab(), payloadObject);
                else if (payloadCollectable && receivingRepo != null) // add to repo
                    receivingRepo.Add(payloadObject);

                return excavatorController.MoveArmToPosition(actualDropPosition + Vector3.up * 4f, Space.World);
            })
            .Then(() => commandCompletedEvent?.Invoke())
            .Catch(() =>
            {
                Debug.LogWarning("ExcavatorDropCommand: Execute: Encountered error during animation sequence");
                commandCompletedEvent?.Invoke();
            });
    }

    private void CreateAndInitStack(GameObject stackPrefab, GameObject droppedObject)
    {
        GameObject stackObject = UnityEngine.Object.Instantiate(stackPrefab);
        ObjectStack stack = stackObject.GetComponent<ObjectStack>();

        if (!stack)
        {
            Debug.LogError($"ExcavatorDropCommand: Execute: CreateAndInitStack: Gameobject {droppedObject.name} provided invalid stack prefab.");
            return;
        }

        stack.InitStack(droppedObject);
    }
}