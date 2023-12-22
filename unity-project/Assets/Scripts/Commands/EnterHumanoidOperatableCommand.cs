using System;
using UnityEngine;

public class EnterHumanoidOperatableCommand : ICommand
{
    public event Action commandCompletedEvent;

    private GameObject agentObject;
    private IHumanoidOperatable operatable;
    private float enterRadius = 5f;

    public EnterHumanoidOperatableCommand(GameObject agentObject, IHumanoidOperatable operatable)
    {
        this.agentObject = agentObject;
        this.operatable = operatable;
    }

    public void Execute()
    {
        if (Vector3.Distance(agentObject.transform.position, operatable.GetEntryPoint()) <= enterRadius)
        {
            // agent is close enough, enter the vehicle, if enough room
            if (!operatable.IsFull())
                operatable.AddOccupant(agentObject);
        } else
            Debug.LogWarning("EnterHumanoidOperatableCommand: Execute: Agent could not enter because it is not close enough to the entry point.");

        commandCompletedEvent.Invoke();
    }
}
