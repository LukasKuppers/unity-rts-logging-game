using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class MonoTestRunner : MonoBehaviour
{
    [SerializeField]
    private GameObject roadObject;
    [SerializeField]
    private GameObject roundAbout1Object;
    [SerializeField]
    private GameObject roundAbout2Object;

    [SerializeField]
    private GameObject vehicle;

    private RoadVehicle vehicleData;

    private RoadSegment road;
    private RoadSegment parking1;
    private RoadSegment parking2;


    // Start is called before the first frame update
    void Start()
    {
        road = roadObject.GetComponent<RoadSegment>();
        parking1 = roundAbout1Object.GetComponent<RoadSegment>();
        parking2 = roundAbout2Object.GetComponent<RoadSegment>();

        parking1.AddConnectedRoad(road, true, true);
        parking1.AddConnectedRoad(road, false, true);

        road.AddConnectedRoad(parking1, true, true);
        road.AddConnectedRoad(parking2, false, true);

        parking2.AddConnectedRoad(road, true, false);
        parking2.AddConnectedRoad(road, false, false);

        parking1.Park(vehicle, 10f, true);
        vehicleData = vehicle.GetComponent<RoadVehicle>();
        vehicleData.SetCurrentRoadSegment(parking1, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
