using System;
using UnityEngine;

public class OperatableEjectOccupantCommand : ICommand
{
    public event Action commandCompletedEvent;

    private IHumanoidOperatable operatable;
    private GameObject humanoid;

    public OperatableEjectOccupantCommand(IHumanoidOperatable operatable, GameObject humanoidToEject)
    {
        this.operatable = operatable;
        humanoid = humanoidToEject;
    }

    public void Execute()
    {
        operatable.EjectOccupant(humanoid);

        commandCompletedEvent.Invoke();
    }
}
