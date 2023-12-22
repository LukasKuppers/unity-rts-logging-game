using UnityEngine;

public class VehicleControlMode : HumanoidOperatableControlMode
{
    private protected Vehicle vehicle;

    public VehicleControlMode(GameObject vehicleObject) : base(vehicleObject)
    {
        vehicle = vehicleObject.GetComponent<Vehicle>();

        if (!vehicle)
        {
            Debug.LogWarning("VehicleControlMode: Constructor: Provided vehicle object does not have Vehicle. Attaching default Vehicle component.");
            vehicle = vehicleObject.AddComponent<Vehicle>();
        }
    }

    private new protected void InvokeSelectionForfeitedEvent()
    {
        base.InvokeSelectionForfeitedEvent();
    }

    public override void HandleNewSelection(GameObject selectedObject, Vector3 selectionPoint)
    {
        if (!ReferenceEquals(selectedObject, operatableObject))
        {
            VehicleMoveCommand moveCommand = new VehicleMoveCommand(operatableObject, selectionPoint);
            agent.AddCommand(moveCommand);

            InvokeSelectionForfeitedEvent();
        }
        else
            base.HandleNewSelection(selectedObject, selectionPoint);
    }
}
