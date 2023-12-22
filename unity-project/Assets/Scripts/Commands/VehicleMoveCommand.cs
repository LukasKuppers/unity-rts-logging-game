using System;
using UnityEngine;
using UnityEngine.AI;


[System.Serializable]
public class VehicleMoveCommand : ICommand
{
    public event Action commandCompletedEvent;

    private NavMeshAgent navAgent;
    private NavMeshAgentHelper navAgentHelper;
    private Vehicle vehicle;
    private Vector3 destination;
    private float distanceFromDestination;

    public VehicleMoveCommand(GameObject agentObject, Vector3 destination)
    {
        navAgent = agentObject.GetComponent<NavMeshAgent>();
        if (!navAgent)
        {
            Debug.LogWarning($"VehicleMoveCommand: Constructor: Object {agentObject.name} was assigned move command without a NavMeshAgent. Attatching default NavMeshAgent.");
            navAgent = agentObject.AddComponent<NavMeshAgent>();
        }

        navAgentHelper = agentObject.GetComponent<NavMeshAgentHelper>();
        if (!navAgentHelper)
        {
            Debug.LogWarning($"VehicleMoveCommand: Constructor: Object {agentObject.name} was assigned move command without a NavMeshAgentHelper. Attatching default NavMeshAgentHelper.");
            navAgentHelper = agentObject.AddComponent<NavMeshAgentHelper>();
        }

        vehicle = agentObject.GetComponent<Vehicle>();
        if (!vehicle)
        {
            Debug.LogWarning($"VehicleMoveCommand: Constructor: Object {agentObject.name} was assigned move command without a Vehicle component. Attatching default Vehicle.");
            vehicle = agentObject.AddComponent<Vehicle>();
        }

        this.destination = destination;
        distanceFromDestination = 0;
    }

    public VehicleMoveCommand(GameObject agentObject, Vector3 destination, float distanceFromDestination) : this(agentObject, destination)
    {
        if (distanceFromDestination > 0)
            this.distanceFromDestination = distanceFromDestination;
    }

    public void Execute()
    {
        if (vehicle.GetDriver() != null)
        {
            if (distanceFromDestination > 0)
                navAgentHelper.SetDestinationWithinRadius(destination, distanceFromDestination);
            else
                navAgentHelper.SetDestination(destination);

            navAgentHelper.destinationReachedEvent += OnPathCompleted;
            navAgentHelper.StartCheckForCurrentDest();
        } else
        {
            commandCompletedEvent?.Invoke();
        }
    }

    private void OnPathCompleted()
    {
        navAgentHelper.destinationReachedEvent -= OnPathCompleted;
        commandCompletedEvent?.Invoke();
    }
}
