using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class RoadTrailerAnimator : MonoBehaviour
{
    [SerializeField]
    private GameObject parentRoadVehicle;

    [SerializeField]
    [Tooltip("The gameobject that this trailer hitches to - expected to be parent of trailer.")]
    private GameObject hitchPoint;
    
    [SerializeField]
    [Tooltip("The gameobject that acts as a hitch on this trailer - where a chained trailer would hitch to")]
    private GameObject selfHitchPoint;

    private RoadVehicle parentVehicle;
    private float trailerLength;

    private RoadSegment frontRoadSegment;
    private Queue<RoadSegment> intermediateSegments;    // holds every road segment between the parent road vehicle and the selfHitchPoint
    private Dictionary<RoadSegment, bool> enteredViaStart;  // if parent vehicle entered segment n via start, enteredViStart[n] = T
    private float intermediateSegmentLength;

    private void Awake()
    {
        parentVehicle = parentRoadVehicle.GetComponent<RoadVehicle>();
        if (!parentVehicle)
        {
            Debug.LogError("RoadTrailerAnimation: Awake: Parent road vehicle does not have an attached RoadVehicle component. errors will occur.");
        }

        intermediateSegments = new Queue<RoadSegment>();
        enteredViaStart = new Dictionary<RoadSegment, bool>();
        intermediateSegmentLength = 0;
        parentVehicle.MovedToNewRoad += OnParentMovedToNewSegment;

        trailerLength = Vector3.Magnitude(selfHitchPoint.transform.localPosition);
    }

    private void Update()
    {
        RoadSegment frontRoad = parentVehicle.GetCurrentRoadSegment();
        RoadSegment hitchRoad = intermediateSegments.Peek();
        Spline frontSpline = frontRoad.GetSpline();
        Spline hitchSpline = hitchRoad.GetSpline();

        float parentTCoord = frontRoad.GetVehicleTCoordinate(parentRoadVehicle);

        float searchDistance = trailerLength;

        if (intermediateSegments.Count > 1)
        {
            // trailer spans at least two road segments
            float parentRoadStartCoord = enteredViaStart[frontRoad] ? 0f : 1f;
            float parentDelta = SplineUtil.GetLengthBetweenCoordinates(
                frontSpline, frontRoad.transform, parentTCoord, parentRoadStartCoord);

            searchDistance -= (parentDelta + intermediateSegmentLength);
        }

        float searchDir = enteredViaStart[hitchRoad] ? -1f : 1f;

        float searchTCoord = intermediateSegments.Count <= 1 ? parentTCoord :
            enteredViaStart[hitchRoad] ? 1f : 0f;

        Vector3 selfHitchPosition = SplineUtility.GetPointAtLinearDistance(
            hitchSpline, searchTCoord, searchDir * searchDistance, out float hitchTCoord);
        Vector3 selfHitchUp = SplineUtility.EvaluateUpVector(hitchSpline, hitchTCoord);

        selfHitchPosition = hitchRoad.transform.TransformPoint(selfHitchPosition);
        selfHitchUp = hitchRoad.transform.TransformDirection(selfHitchUp);

        SetOrientationBySelfHitchPosition(selfHitchPosition, selfHitchUp);

        // dequeue passed roads
        if (searchDistance <= 0)
        {
            RoadSegment left = intermediateSegments.Dequeue();
            enteredViaStart.Remove(left);

            if (intermediateSegments.Count >= 2)
                intermediateSegmentLength -= intermediateSegments.Peek().GetLength();
        }
    }

    private void OnParentMovedToNewSegment(RoadSegment newSegment, bool enteredAtStart)
    {
        if (intermediateSegments.Count >= 2)
            intermediateSegmentLength += frontRoadSegment.GetLength();

        intermediateSegments.Enqueue(newSegment);
        frontRoadSegment = newSegment;

        // assuming that the same segment will not appear in the queue twice concurrently
        enteredViaStart[newSegment] = enteredAtStart;
    }

    private void SetOrientationBySelfHitchPosition(Vector3 hitchPosition, Vector3 hitchUp)
    {
        Vector3 negativeLook = hitchPosition - transform.position;
        Quaternion rotation = Quaternion.LookRotation(-negativeLook, hitchUp);
        transform.rotation = rotation;
    }
}
