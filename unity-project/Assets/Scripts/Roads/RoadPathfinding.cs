using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public static class RoadPathfinding
{
    /// <summary>
    /// Works just like base FindPathNoTurnaround, with optional parameter to control whether or not we can turn around on the current road
    /// This is only relevant if the start and target segments are the same
    /// </summary>
    /// <param name="start">The first road segment the agent is currently on</param>
    /// <param name="target">The target road segment</param>
    /// <param name="facingForwardsAtStart">will the agent exit the rear of the first segment, or vice versa?</param>
    /// <param name="turnAround">If true, and start = target, will look for an alternate path to the target</param>
    /// <returns>A list of road segments from start to target to travel, or null if no path exists</returns>
    public static List<RoadSegment> FindPathNoTurnaround(RoadSegment start, RoadSegment target, bool facingForwardsAtStart, bool turnAround)
    {
        if (!turnAround || start != target)
            return FindPathNoTurnaround(start, target, facingForwardsAtStart);
        else
        {
            // start and target are the same, but we want to turn around - so find path to return to start but facing opposite direction
            // use dummy road segment as surrogate for start segment, then replace it with the start segment in the fnial path
            GameObject dummyObject = new GameObject("dummy");
            dummyObject.AddComponent<SplineContainer>();
            RoadSegment dummyRoad = dummyObject.AddComponent<RoadSegment>();

            // hook up outgoing connections to dummy road
            (RoadSegment[] connectedRoads, bool[] connectionEnds) = start.GetConnectedRoads(!facingForwardsAtStart);
            for (int i = 0; i < connectedRoads.Length; i++)
            {
                dummyRoad.AddConnectedRoad(connectedRoads[i], !facingForwardsAtStart, connectionEnds[i]);
            }

            // find path
            List<RoadSegment> path = FindPathNoTurnaround(dummyRoad, target, facingForwardsAtStart);

            Object.Destroy(dummyObject);

            if (path == null)
                return null;

            // replace first road with actual
            path[0] = start;
            return path;
        }
    }

    /// <summary>
    /// Finds the shortest path from start to target, if one exists.
    /// Does not allow for turning around on a road - only forwards travel.
    /// </summary>
    /// <param name="start">The first road segment the agent is currently on</param>
    /// <param name="target">The target road segment</param>
    /// <param name="facingForwardsAtStart">will the agent exit the rear of the first segment, or vice versa?</param>
    /// <returns>A list of road segments from start to target to travel, or null if no path exists</returns>
    public static List<RoadSegment> FindPathNoTurnaround(RoadSegment start, RoadSegment target, bool facingForwardsAtStart)
    {
        // Dijkstra's algorithm (A* wihtout heuristic)

        List<RoadSegment> openSet = new List<RoadSegment>() { start };

        // for segment n, facingForwards[n] is the direction the agent is facing on segment n
        Dictionary<RoadSegment, bool> facingForwards = new Dictionary<RoadSegment, bool>();
        facingForwards[start] = facingForwardsAtStart;

        // for segment n, cameFrom[n] is the segment immediately preceding it on the shortest path currently known
        Dictionary<RoadSegment, RoadSegment> cameFrom = new Dictionary<RoadSegment, RoadSegment>();

        // for segment n, cost[n] is the cost of the cheapest path from start to n currently known
        Dictionary<RoadSegment, float> cost = new Dictionary<RoadSegment, float>();
        cost[start] = 0;

        while (openSet.Count > 0)
        {
            RoadSegment current = DequeueMinCostNode(openSet, cost);

            if (current == target)
            {
                // found target - construct path and return
                List<RoadSegment> path = new List<RoadSegment>() { current };
                while (cameFrom.ContainsKey(current))
                {
                    current = cameFrom[current];
                    path.Insert(0, current);
                }
                return path;
            }

            // current is not the target node
            (RoadSegment[] roads, bool[] connectionEnds) = current.GetConnectedRoads(!facingForwards[current]);
            for (int i = 0; i < roads.Length; i++)
            {
                RoadSegment neighbor = roads[i];
                float tenativeCost = cost[current] + neighbor.GetLength();
                if (!cost.ContainsKey(neighbor) || tenativeCost < cost[neighbor])
                {
                    cameFrom[neighbor] = current;
                    cost[neighbor] = tenativeCost;
                    facingForwards[neighbor] = connectionEnds[i];
                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return null;
    }

    // get the segment in openSet with min cost, and remove it from openSet
    // IMPORTANT: assumes openSet is non-empty
    private static RoadSegment DequeueMinCostNode(List<RoadSegment> openSet, Dictionary<RoadSegment, float> cost)
    {
        RoadSegment minSegment = openSet[0];
        int minIndex = 0;
        float minCost = cost[minSegment];

        for (int i = 1; i < openSet.Count; i++)
        {
            RoadSegment currentSegment = openSet[i];
            float currentCost = cost[currentSegment];

            if (currentCost < minCost)
            {
                minSegment = currentSegment;
                minCost = currentCost;
                minIndex = i;
            }
        }

        openSet.RemoveAt(minIndex);
        return minSegment;
    }
}
