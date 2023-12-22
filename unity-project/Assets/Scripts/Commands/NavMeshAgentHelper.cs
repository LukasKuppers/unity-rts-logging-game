using System.Collections;
using System;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshAgentHelper : MonoBehaviour
{
    public event Action destinationReachedEvent;

    [SerializeField]
    private float checkRefreshTime = 0.5f;
    [SerializeField]
    private float arrivalDistanceThreshold = 2.0f;

    private NavMeshAgent navAgent;
    private NavMeshObstacle navObstacle;

    private bool isCheckCoroutineRunning = false;

    private void Awake()
    {
        navAgent = gameObject.GetComponent<NavMeshAgent>();

        if (!navAgent)
        {
            Debug.LogWarning($"NavMeshAgentHelper: Awake: Object {gameObject.name} does not have an attached NavMeshAgent");
        }

        navObstacle = gameObject.GetComponentInChildren<NavMeshObstacle>();

        if (navObstacle)
        {
            // starting under assumption that object is stationary
            navAgent.enabled = false;
            navObstacle.enabled = true;
        }
    }

    /// <summary>
    /// Safe alternative to NavMeshAgent.SetDestination().
    /// If object also is (has) a NavMeshObstacle, this will ensure that only one of agent/obstacle is active at a time.
    /// essentially, allows obstacles (with carving) to act as agents at the same time
    /// </summary>
    /// <param name="target">The desired final position of the agent</param>
    public void SetDestination(Vector3 target)
    {
        if (navObstacle)
        {
            StartCoroutine(TakeActionOnceBound(() =>
            {
                navAgent.SetDestination(target);
                StartCheckForCurrentDest();
            }));
        } else
        {
            // just set the destination as usual
            navAgent.SetDestination(target);
        }
    }

    /// <summary>
    /// Travel to 'radius' meters away from target.
    /// Warning: may cause frame rate drop, as this calls NavMeshAgnt.CalculatePath()
    /// </summary>
    /// <param name="target">The desired target position you wish to approach</param>
    /// <param name="radius">How far away should the agent get from the target?</param>
    public void SetDestinationWithinRadius(Vector3 target, float radius)
    {
        if (radius >= Vector3.Distance(transform.position, target))
        {
            // try to move straight backwards, to radius dist from target
            Vector3 attemptedPos = (transform.position - target).normalized * radius;
            SetDestination(attemptedPos);
            return;
        }
        
        if (radius == 0)
        {
            SetDestination(target);
            return;
        }

        StartCoroutine(TakeActionOnceBound(() =>
        {
            NavMeshPath path = new NavMeshPath();
            navAgent.CalculatePath(target, path);

            if (path.status == NavMeshPathStatus.PathInvalid)
                return;

            Vector3 actualTarget;
            Vector3[] corners = path.corners;
            if (Vector3.Distance(target, corners[corners.Length - 1]) >= radius)
            {
                // path cant get within radius dist of target, 
                // go to closest point on path
                actualTarget = corners[corners.Length - 1];
            }
            else
            {
                // get a position on the path that is radius meters away from the target
                int cornerIndex = corners.Length - 2;
                while (Vector3.Distance(target, corners[cornerIndex]) < radius && cornerIndex > 0)
                {
                    cornerIndex--;
                }

                // point where dist=radius is between corners[cornerIndex] and corners[cornerIndex + 1]
                float t = GetLerpParameterToFindRadiusIntersect(corners[cornerIndex], corners[cornerIndex + 1], target, radius);
                actualTarget = Vector3.Lerp(corners[cornerIndex], corners[cornerIndex + 1], t);
            }

            // Improvement: set path with modified path, instead of calling setDestination (which recalculates path)
            navAgent.SetDestination(actualTarget);
            StartCheckForCurrentDest();
        }));
    }

    public void StartCheckForCurrentDest()
    {
        if (!isCheckCoroutineRunning)
        {
            StartCoroutine(CheckDestReached());
            isCheckCoroutineRunning = true;
        }
    }

    IEnumerator CheckDestReached()
    {
        // wait a frame before immediately checking
        // bad solution - but currently used to mitigate event bubbling issues w.r.t command pattern
        yield return null;


        while (!navAgent.enabled || Vector3.Distance(gameObject.transform.position, navAgent.destination) > arrivalDistanceThreshold)
        {
            yield return new WaitForSeconds(checkRefreshTime);
        }

        if (navObstacle) {
            navAgent.enabled = false;
            navObstacle.enabled = true;
        }

        isCheckCoroutineRunning = false;
        destinationReachedEvent?.Invoke();
    }

    /// <summary>
    /// Used to perform navagent actions. WARNING: disables navObstacle and enables navAgent!
    /// </summary>
    /// <param name="action">function to execute once navagent is bound to mesh</param>
    IEnumerator TakeActionOnceBound(Action action)
    {
        bool isBound = false;
        if (navObstacle)
            navObstacle.enabled = false;
        
        while (!isBound)
        {
            navAgent.enabled = true;
            isBound = navAgent.isOnNavMesh;
            navAgent.enabled = false;

            yield return null;
        }

        navAgent.enabled = true;
        action.Invoke();
    }

    /// <summary>
    /// Used to find a point between two points that is 'radius' distance from target.
    /// </summary>
    /// <param name="outside">The first point, greater than 'radius' dist from target</param>
    /// <param name="inside">The second point, less than 'radius' dist from target</param>
    /// <param name="target">The target point</param>
    /// <param name="radius">How far should the resultant point be from the target?</param>
    /// <returns>The lerp parameter used to find the point between outside and inside</returns>
    private float GetLerpParameterToFindRadiusIntersect(Vector3 outside, Vector3 inside, Vector3 target, float radius)
    {
        // essentially, use quadratic formula to find lerp parameter
        // -b - sqrt(b^2 - 4ac) / 2a

        // for shorthand
        Vector3 o = outside;
        Vector3 i = inside;
        Vector3 t = target;

        float a = Sqr(i.x - o.x) + Sqr(i.y - o.y) + Sqr(i.z - o.z);
        float b = 2 * (((i.x - o.x) * (o.x - t.x)) + ((i.y - o.y) * (o.y - t.y)) + ((i.z - o.z) * (o.z - t.z)));
        float c = Sqr(o.x - t.x) + Sqr(o.y - t.y) + Sqr(o.z - t.z) - Sqr(radius);

        float tOut = (-b - Mathf.Sqrt(Sqr(b) - (4 * a * c))) / (2 * a);
        if (float.IsNaN(tOut))
            return 0;
        return tOut;
    }

    // helper: returns square of x (x^2)
    private float Sqr(float x)
    {
        return x * x;
    }
}
