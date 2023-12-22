using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class RoadSegment : MonoBehaviour
{
    /// <summary>
    /// How many frames a vehicle need to be blocked until it quits its movement and enters a parked state.
    /// </summary>
    private static readonly int MIN_BLOCKED_FRAMES_TO_STOP = 30;

    [SerializeField]
    private float minVehicleDistance = 10f;
    [SerializeField]
    private bool hasParkingSpot = false;
    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("Where to park on the spline - only considered if hasParkingSpot is true")]
    private float parkingSpotTCoord = 0.5f;

    private Spline spline;
    private float splineLength;
    private float minTDistance;

    // map objects on road to their spline t-coordinate
    private Dictionary<GameObject, (float position, bool isDriving)> activeVehicles;

    // map connected road to connection end (start = true, end = false)
    private Dictionary<RoadSegment, bool> inRoads;  // roads connecting to start
    private Dictionary<RoadSegment, bool> outRoads; // roads connecting to end

    // draw parking position in editor
    private void OnDrawGizmosSelected()
    {
        if (hasParkingSpot)
        {
            Spline spline = gameObject.GetComponent<SplineContainer>().Spline;
            Vector3 parkingPosLocal = SplineUtility.EvaluatePosition(spline, parkingSpotTCoord);
            Vector3 parkingPos = transform.TransformPoint(parkingPosLocal);

            Gizmos.DrawWireSphere(parkingPos, 1f);
        }
    }

    private void Awake()
    {
        SplineContainer splineContainer = gameObject.GetComponent<SplineContainer>();
        if (!splineContainer)
        {
            Debug.LogError($"RoadSegment: Awake: Gameobject {gameObject.name} does not have a spline attached.");
            return;
        }

        spline = splineContainer.Spline;
        splineLength = SplineUtil.GetSplineLength(spline, transform);
        minTDistance = minVehicleDistance / splineLength;

        activeVehicles = new Dictionary<GameObject, (float, bool)>();
        inRoads = new Dictionary<RoadSegment, bool>();
        outRoads = new Dictionary<RoadSegment, bool>();
    }

    public float GetLength()
    {
        return splineLength;
    }

    public bool HasParkingSpot()
    {
        return hasParkingSpot;
    }

    public float GetParkingSpotTCoordinate()
    {
        return parkingSpotTCoord;
    }

    public Spline GetSpline()
    {
        return spline;
    }

    /// <summary>
    /// Get the t-coordinate of the specified vehicle, if it is on the road
    /// </summary>
    /// <param name="vehicle">The desired vehicle</param>
    /// <returns>The t-coordinate of the vehicle, or -1 of the vehicle is not on the road</returns>
    public float GetVehicleTCoordinate(GameObject vehicle)
    {
        if (activeVehicles.ContainsKey(vehicle))
            return activeVehicles[vehicle].position;

        return -1f;
    }

    /// <summary>
    /// Add a road connection
    /// </summary>
    /// <param name="road">The road to connect to</param>
    /// <param name="isConnectedToStart">Is the road connected to this roads start?</param>
    /// <param name="isConnectedByStart">Is the new road connected by its start?</param>
    public void AddConnectedRoad(RoadSegment road, bool isConnectedToStart, bool isConnectedByStart)
    {
        if (!road)
        {
            Debug.LogWarning("RoadSegment: AddConnectedRoad: provided road is null. Aborting.");
            return;
        }

        if (isConnectedToStart)
            inRoads.Add(road, isConnectedByStart);
        else
            outRoads.Add(road, isConnectedByStart);
    }

    /// <summary>
    /// Gets all the roads connected to this road at the specified end
    /// </summary>
    /// <param name="roadsConnectedToStart">if true, gets the roads connected to this roads start</param>
    /// <returns>Array of roads and array of isConnectedByStart - roads[i] corresponds to isConnectedByStart[i]</returns>
    public (RoadSegment[] roads, bool[] connectionEnds) GetConnectedRoads(bool roadsConnectedToStart)
    {
        Dictionary<RoadSegment, bool> connections = roadsConnectedToStart ? inRoads : outRoads;

        RoadSegment[] roads = new RoadSegment[connections.Count];
        bool[] connectionEnds = new bool[connections.Count];

        int index = 0;
        foreach (KeyValuePair<RoadSegment, bool> entry in connections)
        {
            roads[index] = entry.Key;
            connectionEnds[index] = entry.Value;
            index++;
        }
        return (roads, connectionEnds);
    }

    /// <summary>
    /// Returns whether or not the specified road is connected to this road by its start, if they are connected
    /// </summary>
    /// <param name="road">The connected road of interest</param>
    /// <returns>If the road is connected by its start, and whether or not the road is actually connected (if not, the first value is always false)</returns>
    public (bool isConnectedByStart, bool isConnected) IsRoadConnectedByStart(RoadSegment road)
    {
        if (inRoads.ContainsKey(road))
            return (inRoads[road], true);
        else if (outRoads.ContainsKey(road))
            return (outRoads[road], true);

        return (false, false);
    }

    /// <summary>
    /// Drive a vehicle across the entire length of the road segment.
    /// </summary>
    /// <param name="vehicleObject">The gameObject to move</param>
    /// <param name="velocity">The speed (in m/s) that the vehicle should travel</param>
    /// <param name="enterFromStart">Specifies which end to enter the raod</param>
    /// <returns>A promise that evaluates to true once the operation is complete</returns>
    public Promise<bool> Travel(GameObject vehicleObject, float velocity, bool enterFromStart)
    {
        Promise<bool> promise = new Promise<bool>();

        if (activeVehicles.ContainsKey(vehicleObject))
        {
            Debug.LogWarning($"RoadSegment: Travel: provided vehicle {vehicleObject.name} is already on the road. Aborting.");
            return promise.Reject("Vehicle already on road");
        }

        float startT = enterFromStart ? 0 : 1;
        float endT = 1f - startT;

        Promise<bool> animPromise = new Promise<bool>();
        animPromise.Then(() =>
            {
                activeVehicles.Remove(vehicleObject);
                promise.Resolve(true);
            })
            .Catch((string msg) => promise.Reject(msg));

        activeVehicles.Add(vehicleObject, (startT, true));
        StartCoroutine(MoveObjectAlongSpline(vehicleObject, velocity, startT, endT, animPromise));
        return promise;
    }

    /// <summary>
    /// Park a vehicle on the road segment at the specified T Coordinate
    /// </summary>
    /// <param name="vehicleObject">The gameObject to park</param>
    /// <param name="velocity">The speed (in m/s) that the vehicle should travel</param>
    /// <param name="enterFromStart">Specifies which end to enter the road. Ignored if vehicle is alreay on the road</param>
    /// <param name="parkingTCoord">The parking T coordinate</param>
    /// <returns>A promise that evaluates to true once the operation is complete</returns>
    public Promise<bool> Park(GameObject vehicleObject, float velocity, bool enterFromStart, float parkingTCoord)
    {
        Promise<bool> promise = new Promise<bool>();

        if (parkingTCoord < 0f || parkingTCoord > 1f)
        {
            Debug.LogWarning("RoadSegment: Park: Parking T Coordinate is out of range. Aborting.");
            return promise.Reject("parking coordinate out of range");
        }

        if (activeVehicles.ContainsKey(vehicleObject) && activeVehicles[vehicleObject].isDriving)
        {
            Debug.LogWarning("RoadSegment: Park: Specified vehicle is already driving on this road. Aborting.");
            return promise.Reject("vehicle already driving");
        }

        float startT;
        if (activeVehicles.ContainsKey(vehicleObject))
            startT = activeVehicles[vehicleObject].position;
        else
            startT = enterFromStart ? 0f : 1f;

        Promise<bool> animPromise = new Promise<bool>();
        animPromise.Then(() =>
            {
                activeVehicles[vehicleObject] = (parkingTCoord, false); // set isDriving to false
                promise.Resolve(true);
            })
            .Catch((string msg) => promise.Reject(msg));

        if (!activeVehicles.ContainsKey(vehicleObject))
            activeVehicles.Add(vehicleObject, (startT, true));
        StartCoroutine(MoveObjectAlongSpline(vehicleObject, velocity, startT, parkingTCoord, animPromise));
        return promise;
    }

    /// <summary>
    /// Park a vehicle on the road segment at the default parking position. Only allowed if hasParkingSpot is true
    /// </summary>
    /// <param name="vehicleObject">The gameObject to park</param>
    /// <param name="velocity">The speed (in m/s) that the vehicle should travel</param>
    /// <param name="enterFromStart">Specifies which end to enter the road. Ignored if vehicle is alreay on the road</param>
    /// <returns>A promise that evaluates to true once the operation is complete</returns>
    public Promise<bool> Park(GameObject vehicleObject, float velocity, bool enterFromStart)
    {
        Promise<bool> promise = new Promise<bool>();

        float targetTCoord = hasParkingSpot ? parkingSpotTCoord : 0.5f;

        return Park(vehicleObject, velocity, enterFromStart, targetTCoord);
    }

    public Promise<bool> Exit(GameObject vehicleObject, float velocity, bool exitThroughBack)
    {
        Promise<bool> promise = new Promise<bool>();

        if (!activeVehicles.ContainsKey(vehicleObject))
        {
            Debug.LogWarning($"RoadSegment: Exit: Vehicle {vehicleObject.name} is not currently on the road.");
            return promise.Reject("Vehicle not on road");
        }

        if (activeVehicles[vehicleObject].isDriving)
        {
            Debug.LogWarning("RoadSegment: Exit: Specified vehicle is already driving on the road. Aborting.");
            return promise.Reject("vehicle already driving");
        }

        Promise<bool> animPromise = new Promise<bool>();
        animPromise.Then(() =>
            {
                activeVehicles.Remove(vehicleObject);
                promise.Resolve(true);
            })
            .Catch((string msg) => promise.Reject(msg));

        float startT = activeVehicles[vehicleObject].position;
        float endT = exitThroughBack ? 1f : 0f;
        StartCoroutine(MoveObjectAlongSpline(vehicleObject, velocity, startT, endT, animPromise));
        return promise;
    }

    private protected IEnumerator MoveObjectAlongSpline(GameObject obj, float velocity, float tStart, float tEnd, Promise<bool> promise)
    {
        float tCurrent = tStart;
        float tReachedThreshold = (velocity * Time.smoothDeltaTime) / splineLength;
        float direction = tStart < tEnd ? 1f : -1f;

        int consecutiveBlockedFrames = 0;

        while (tCurrent != tEnd)
        {
            if ((tEnd - tCurrent) * direction <= tReachedThreshold)
            {
                // we've effectively reached the target t value
                tCurrent = tEnd;
                continue;
            }
            else
            {
                float tDelta = (velocity * direction * Time.deltaTime) / splineLength;

                foreach (KeyValuePair<GameObject, (float, bool)> entry in activeVehicles)
                {
                    float otherPos = entry.Value.Item1;
                    if (!ReferenceEquals(entry.Key, obj) && Mathf.Abs((tCurrent + tDelta) - otherPos) <= minTDistance)
                    {
                        // found other object on road that is too close to this object. Need to adjust tDelta
                        tDelta = (otherPos - (direction * minTDistance)) - tCurrent;
                    }
                }

                // check if vehicle blocked
                if (Mathf.Abs(tDelta * splineLength) < 0.0001f)
                {
                    consecutiveBlockedFrames++;
                    if (consecutiveBlockedFrames >= MIN_BLOCKED_FRAMES_TO_STOP)
                    {
                        Debug.Log("VEHICLE BLOCKED!!");

                        activeVehicles[obj] = (tCurrent, false);
                        promise.Reject("Vehicle encountered blockage. Parking.");
                        yield break;
                    }
                }
                else
                    consecutiveBlockedFrames = 0;

                tCurrent += tDelta;
                SplineUtility.Evaluate(spline, tCurrent, out float3 position, out float3 tangent, out float3 upVector);
                
                obj.transform.position = transform.TransformPoint(position);
                Quaternion localRotation = Quaternion.LookRotation(tangent * direction, upVector);
                obj.transform.rotation = transform.rotation * localRotation;
                activeVehicles[obj] = (tCurrent, true);

                yield return null;
            }
        }

        Vector3 finalPos = transform.TransformPoint(SplineUtility.EvaluatePosition(spline, tEnd));
        obj.transform.position = finalPos;

        promise.Resolve(true);
    }

    // Below methods allow RoadSegment use as dict key
    // IMPORTANT: It is assumed that only one road segment will be attached to any gameobject
    // if two road segements are attached to the same gameobject, they will be considered equal!
    public override int GetHashCode()
    {
        return gameObject.GetHashCode();
    }

    public override bool Equals(object other)
    {
        if (!ReferenceEquals(other.GetType(), GetType()))
            return false;

        RoadSegment otherRoad = (RoadSegment)other;
        return ReferenceEquals(gameObject, otherRoad.gameObject);
    }
}
