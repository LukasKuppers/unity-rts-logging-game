using UnityEngine;

public class ExcavatorControlMode : VehicleControlMode
{
    private bool lookingForPayloadDropPosition = false;

    public ExcavatorControlMode(GameObject excavatorObject) : base(excavatorObject) { }

    public override void HandleNewSelection(GameObject selectedObject, Vector3 selectionPoint)
    {
        VehiclePickup pickup = selectedObject.GetComponent<VehiclePickup>();
        IObjectRepository objectRepo = selectedObject.GetComponent<IObjectRepository>();

        if (lookingForPayloadDropPosition)
        {
            VehicleMoveCommand moveCommand = new VehicleMoveCommand(operatableObject, selectionPoint, 10f);
            ExcavatorDropCommand dropCommand = new ExcavatorDropCommand(operatableObject, selectedObject, selectionPoint);

            agent.AddCommand(moveCommand);
            agent.AddCommand(dropCommand);

            lookingForPayloadDropPosition = false;
            InvokeSelectionForfeitedEvent();
        }
        else if (pickup || objectRepo != null)
        {
            if (ReferenceEquals(vehicle.GetPayload(), selectedObject))
            {
                // player selected payload, looking for drop position - next click will be drop position
                lookingForPayloadDropPosition = true;
            }
            else if (vehicle.GetPayload() == null)
            {
                // selected pickup, and no payload currently exists, pickup object
                VehicleMoveCommand moveCommand = new VehicleMoveCommand(operatableObject, selectedObject.transform.position, 10f);
                ExcavatorPickupCommand pickupCommand = new ExcavatorPickupCommand(operatableObject, selectedObject);

                agent.AddCommand(moveCommand);
                agent.AddCommand(pickupCommand);

                InvokeSelectionForfeitedEvent();
            }
        } 
        else 
            base.HandleNewSelection(selectedObject, selectionPoint);
    }
}
