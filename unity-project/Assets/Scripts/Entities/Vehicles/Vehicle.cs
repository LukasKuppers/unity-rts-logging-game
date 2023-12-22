using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour, IHumanoidOperatable
{
    [SerializeField]
    private int numSeats;
    [SerializeField]
    private GameObject entryPoint;

    private GameObject driver;
    private List<GameObject> passengers;
    private GameObject payload;

    private void Awake()
    {
        // initially, no driver or passengers present in vehicle
        driver = null;
        passengers = new List<GameObject>();
    }

    public bool IsFull()
    {
        return GetOccupantCount() >= numSeats;
    }

    public int GetOccupantCount()
    {
        int count = passengers.Count;
        if (driver != null)
            count++;

        return count;
    }

    public void AddPayload(GameObject payloadObject)
    {
        if (payload == null)
            payload = payloadObject;
        else
            Debug.Log("Vehicle: AddPayload: Can't add object, as vehicle already has a payload.");
    }

    public GameObject RemovePayload()
    {
        GameObject returnObject = null;
        if (payload != null)
        {
            returnObject = payload;
            payload = null;
        }
        else
            Debug.Log("Vehicle: RemovePayload: Can't remove payload, as vehicle does not have an attached payload.");

        return returnObject;
    }

    public GameObject GetPayload()
    {
        return payload;
    }

    public Vector3 GetEntryPoint()
    {
        return entryPoint.transform.position;
    }

    public List<GameObject> GetPassengers() { return passengers; }

    public GameObject GetDriver() { return driver; }

    public void AddOccupant(GameObject humanoid)
    {
        if (GetOccupantCount() < numSeats)
        {
            // there is room, add new occupant
            if (driver == null)
            {
                driver = humanoid;
            } else
            {
                passengers.Add(humanoid);
            }
            humanoid.SetActive(false);
        }
    }

    public void EjectOccupant(GameObject humanoid)
    {
        bool humanoidPresent = false;
        if (passengers.Contains(humanoid))
        {
            passengers.Remove(humanoid);
            humanoidPresent = true;
        } else if (driver != null && ReferenceEquals(driver, humanoid))
        {
            driver = null;
            humanoidPresent = true;
        }

        if (humanoidPresent)
        {
            humanoid.SetActive(true);
            humanoid.transform.position = entryPoint.transform.position;
        }
    }
}
