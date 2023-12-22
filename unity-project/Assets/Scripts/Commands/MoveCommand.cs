using System;
using UnityEngine;
using UnityEngine.AI;


[System.Serializable]
public class MoveCommand : ICommand
{
    public event Action commandCompletedEvent;

    private NavMeshAgent navAgent;
    private NavMeshAgentHelper navAgentHelper;
    private Vector3 destination;

    public MoveCommand(GameObject agentObject, Vector3 destination)
    {
        navAgent = agentObject.GetComponent<NavMeshAgent>();
        if (!navAgent)
        {
            Debug.LogWarning($"MoveCommand: new MoveCommand: Object {agentObject.name} was assigned move command without a NavMeshAgent. Attatching default NavMeshAgent.");
            agentObject.AddComponent<NavMeshAgent>();
            navAgent = agentObject.GetComponent<NavMeshAgent>();
        }

        navAgentHelper = agentObject.GetComponent<NavMeshAgentHelper>();
        if (!navAgentHelper)
        {
            Debug.LogWarning($"MoveCommand: new MoveCommand: Object {agentObject.name} was assigned move command without a NavMeshAgentHelper. Attatching default NavMeshAgentHelper.");
            agentObject.AddComponent<NavMeshAgentHelper>();
            navAgentHelper = agentObject.GetComponent<NavMeshAgentHelper>();
        }

        this.destination = destination;
    }

    public void Execute()
    {
        navAgent.SetDestination(destination);
        navAgentHelper.destinationReachedEvent += OnPathCompleted;
        navAgentHelper.StartCheckForCurrentDest();
    }

    private void OnPathCompleted()
    {
        navAgentHelper.destinationReachedEvent -= OnPathCompleted;
        commandCompletedEvent?.Invoke();
    }
}
