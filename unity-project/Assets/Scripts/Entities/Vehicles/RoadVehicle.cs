using System;
using UnityEngine;

public class RoadVehicle : Vehicle
{
    /// <summary>
    /// Event evoked when vehicle moves to a new road segment.
    /// The new segment, and whether or not the segment was entered at its start is passed through the event.
    /// </summary>
    public event Action<RoadSegment, bool> MovedToNewRoad;

    [SerializeField]
    private RoadSegment currentRoadSegment;
    [SerializeField]
    private bool isCurrentlyFacingForwards = true;

    // we set the new segment and orientation in one funtion to ensure the MovedToNewRoad event invokes with consistent results
    public void SetCurrentRoadSegment(RoadSegment segment, bool isFacingForwards)
    {
        isCurrentlyFacingForwards = isFacingForwards;
        RoadSegment prevRoadSegment = currentRoadSegment;
        currentRoadSegment = segment;

        if (prevRoadSegment != currentRoadSegment)
            MovedToNewRoad.Invoke(currentRoadSegment, isCurrentlyFacingForwards);
    }

    public RoadSegment GetCurrentRoadSegment()
    {
        return currentRoadSegment;
    }

    public bool IsCurrentlyFacingForwards()
    {
        return isCurrentlyFacingForwards;
    }
}
