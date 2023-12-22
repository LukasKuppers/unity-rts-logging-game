using System;
using System.Collections.Generic;
using UnityEngine;

public class RoadVehicleMoveCommand : ICommand
{
    public event Action commandCompletedEvent;

    private RoadVehicle vehicle;

    private GameObject vehicleObject;
    private RoadSegment endSegment;
    private bool useTargetTCoordiante;  // go to custom T coord, or use default provided by road segment?
    private float targetTCoordinate;
    private float velocity;

    public RoadVehicleMoveCommand(GameObject vehicleObject, RoadSegment endSegment, float velocity)
    {
        this.vehicleObject = vehicleObject;
        this.endSegment = endSegment;
        this.velocity = velocity;

        vehicle = vehicleObject.GetComponent<RoadVehicle>();
        if (!vehicle)
        {
            Debug.LogWarning($"RoadVehicleMoveCommand: Constructor: Object {vehicleObject.name} doesn't have a RoadVehicle attached. Attaching default RoadVehicle.");
            vehicle = vehicleObject.AddComponent<RoadVehicle>();
        }

        useTargetTCoordiante = false;
    }

    public RoadVehicleMoveCommand(GameObject vehicleObject, RoadSegment endSegment, float velocity, float targetTCoordinate) : 
        this(vehicleObject, endSegment, velocity)
    {
        useTargetTCoordiante = true;
        this.targetTCoordinate = targetTCoordinate;
    }

    public void Execute()
    {
        RoadSegment startSegment = vehicle.GetCurrentRoadSegment();
        bool facingForwards = vehicle.IsCurrentlyFacingForwards();

        float targetT = useTargetTCoordiante ? targetTCoordinate : startSegment.GetParkingSpotTCoordinate();
        float currentT = startSegment.GetVehicleTCoordinate(vehicleObject);
        bool targetInFrontOfVehicle = facingForwards ? targetT >= currentT : targetT <= currentT;

        if (startSegment == endSegment && targetInFrontOfVehicle)
        {
            // move directly to the new target T coordinate
            startSegment.Park(vehicleObject, velocity, true, targetT)
                        .Then(() => commandCompletedEvent?.Invoke());
        }
        else
        {
            List<RoadSegment> path = RoadPathfinding.FindPathNoTurnaround(startSegment, endSegment, facingForwards, true);

            if (path == null || path.Count == 0)
            {
                commandCompletedEvent?.Invoke();
                return;
            }

            Promise<bool> promise = startSegment.Exit(vehicleObject, velocity, facingForwards);
            
            // travel through intermediate roads
            for (int i = 1; i < path.Count - 1; i++)
            {
                RoadSegment current = path[i];

                promise = promise.Then(() =>
                {
                    (bool isVehicleFacingForwards, bool _) = path[i - 1].IsRoadConnectedByStart(current);

                    vehicle.SetCurrentRoadSegment(current, isVehicleFacingForwards);

                    return current.Travel(vehicleObject, velocity, isVehicleFacingForwards);
                });
            }

            // park on final road
            RoadSegment finalSegment = path[^1];
            promise.Then(() =>
            {
                (bool isVehicleFacingForwards, bool _) = path[path.Count - 2].IsRoadConnectedByStart(finalSegment);

                vehicle.SetCurrentRoadSegment(finalSegment, isVehicleFacingForwards);

                if (useTargetTCoordiante)
                    return finalSegment.Park(vehicleObject, velocity, isVehicleFacingForwards, targetTCoordinate);
                else
                    return finalSegment.Park(vehicleObject, velocity, isVehicleFacingForwards);
            }).Then(() => commandCompletedEvent?.Invoke());
        }
        
    }
}
