using System;
using System.Collections.Generic;
using UnityEngine;

public class ExcavatorPickupCommand : ICommand
{
    public event Action commandCompletedEvent;

    private ExcavatorController excavatorController;
    private Vehicle vehicle;
    private GameObject excavatorObject;
    private GameObject selectedObject;
    private float maxArmRange;

    public ExcavatorPickupCommand(GameObject excavatorObject, GameObject selectedObject)
    {
        excavatorController = excavatorObject.GetComponent<ExcavatorController>();
        if (!excavatorController)
        {
            Debug.LogError("ExcavatorPickupCommand: Constructor: Provided Excavator object does not have an attached excavatorController.");
        }

        vehicle = excavatorObject.GetComponent<Vehicle>();
        if (!vehicle)
        {
            Debug.LogError("ExcavatorPickupCommand: Constructor: Provided Excavaator object does not have an attached Vehicle component.");
        }

        maxArmRange = 20f;  // default value
        this.excavatorObject = excavatorObject;
        this.selectedObject = selectedObject;
    }

    public ExcavatorPickupCommand(GameObject excavatorObject, GameObject pickupObject, float maxArmRange) :
        this(excavatorObject, pickupObject)
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

        VehiclePickup pickup = selectedObject.GetComponent<VehiclePickup>();
        IObjectRepository repo = selectedObject.GetComponent<IObjectRepository>();
        CollectableObject collectable = selectedObject.GetComponent<CollectableObject>();

        if (collectable != null && collectable.GetCurrentRepository() != null)
            repo = collectable.GetCurrentRepository();

        if ((repo != null && pickup) || (repo != null && collectable != null))  // this is really an error, but try to proceed as if selected object is a stack
        {
            pickup = null;
            collectable = null;
        }

        if (!pickup && repo == null)
        {
            Debug.LogError("ExcavatorPickupCommand: Execute: Selected object cannot be picked up. Aborting.");
            commandCompletedEvent?.Invoke();
            return;
        }

        GameObject pickupObject;
        VehiclePickup actualPickup;

        if (repo != null)  // pickup object off stack
        {
            pickupObject = repo.Peek();
            actualPickup = pickupObject.GetComponent<VehiclePickup>();

            if (!actualPickup)
            {
                Debug.LogWarning("ExcavatorPickupCommand: Execute: attempting to pickup stacked object that doesn't have a VehiclePickup attatched. Aborting.");
                commandCompletedEvent.Invoke();
                return;
            }
        }
        else        // pickup object off ground
        {
            pickupObject = selectedObject;
            actualPickup = pickup;
        }

        // ensure object in range
        if (Vector3.Distance(actualPickup.GetGrabPoint(), excavatorObject.transform.position) > maxArmRange)
        {
            Debug.LogWarning("ExcavatorPickupCommand: Execute: Selected pickup is out of range. Aborting.");
            commandCompletedEvent?.Invoke();
            return;
        }
        

        excavatorController.SetClawRotation(actualPickup.GetGrabDirection(), Space.World)
            .Then(() => excavatorController.SetClawState(true))
            .Then(() =>
            {
                List<Vector3> armPositions = new List<Vector3>
                {
                    actualPickup.GetGrabPoint() + Vector3.up * 4f,
                    actualPickup.GetGrabPoint()
                };

                return excavatorController.TravelArmPositionSequence(armPositions, Space.World);
            })
            .Then(() => excavatorController.SetClawState(false))
            .Then(() =>
            {
                if (repo != null)
                    pickupObject = repo.Remove();

                excavatorController.AttachObjectToClaw(pickupObject);
                return excavatorController.MoveArmToPosition(actualPickup.GetGrabPoint() + Vector3.up * 4f, Space.World);
            })
            .Then(() =>
            {
                vehicle.AddPayload(pickupObject);
                commandCompletedEvent?.Invoke();
            })
            .Catch(() =>
            {
                Debug.LogWarning("ExcavatorPickupCommand: Execute: Encountered error during animation sequence");
                commandCompletedEvent?.Invoke();
            });
    }
}
