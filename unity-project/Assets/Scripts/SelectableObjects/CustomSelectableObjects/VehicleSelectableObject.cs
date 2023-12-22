using UnityEngine;

public class VehicleSelectableObject : MonoBehaviour, ISelectableObject
{
    private enum VehicleType
    {
        EXCAVATOR, 
        ROAD_VEHICLE, 
        GENERIC
    }

    [SerializeField]
    private VehicleType vehicleType = VehicleType.GENERIC;

    private HumanoidOperatableControlMode vehicleControlMode;

    private void Awake()
    {
        switch (vehicleType)
        {
            case VehicleType.GENERIC:
                vehicleControlMode = new VehicleControlMode(gameObject);
                break;
            case VehicleType.EXCAVATOR:
                vehicleControlMode = new ExcavatorControlMode(gameObject);
                break;
            case VehicleType.ROAD_VEHICLE:
                vehicleControlMode = new RoadVehicleControlMode(gameObject);
                break;
        }
    }

    public IPlayerControlMode GetPlayerControlMode()
    {
        return vehicleControlMode;
    }

    public void OnDeselect() { }

    public void OnSelect() { }
}
