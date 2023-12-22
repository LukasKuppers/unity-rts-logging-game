using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class RoadVehicleControlMode : HumanoidOperatableControlMode
{
    private static readonly int SPLINE_SEARCH_RESOLUTION = 4;
    private static readonly int SPLINE_SEARCH_ITERATIONS = 2;

    private RoadVehicle vehicle;

    public RoadVehicleControlMode(GameObject vehicleObject) : base(vehicleObject) 
    {
        vehicle = vehicleObject.GetComponent<RoadVehicle>();
        if (!vehicle)
        {
            Debug.LogWarning($"RoadVehicleControlMode: Object {vehicleObject.name} does not have a Vehicle attached. Attaching default Vehicle component.");
            vehicle = vehicleObject.AddComponent<RoadVehicle>();
        }
    }

    public override void HandleNewSelection(GameObject selectedObject, Vector3 selectionPoint)
    {
        RoadSegment road = selectedObject.GetComponent<RoadSegment>();

        if (road && operatable.GetDriver() != null)
        {
            SplineContainer roadSplineContainer = selectedObject.GetComponent<SplineContainer>();
            if (roadSplineContainer)
            {
                // find selected point on spline and move there
                Spline roadSpline = roadSplineContainer.Spline;
                SplineUtility.GetNearestPoint(roadSpline, selectionPoint, out float3 nearestPoint, out float tCoord,
                    SPLINE_SEARCH_RESOLUTION, SPLINE_SEARCH_ITERATIONS);

                RoadVehicleMoveCommand moveCommand = new RoadVehicleMoveCommand(operatableObject, road, 10f, tCoord);
                agent.AddCommand(moveCommand);
                base.InvokeSelectionForfeitedEvent();
            }
        }

        base.HandleNewSelection(selectedObject, selectionPoint);
    }
}
