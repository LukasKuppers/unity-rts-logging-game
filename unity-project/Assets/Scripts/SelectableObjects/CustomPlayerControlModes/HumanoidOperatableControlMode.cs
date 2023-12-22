using System;
using UnityEngine;

public class HumanoidOperatableControlMode : IPlayerControlMode
{
    public event Action SelectionForfeited;

    private protected GameObject operatableObject;
    private protected IHumanoidOperatable operatable;
    private protected ControllableAgent agent;

    public HumanoidOperatableControlMode(GameObject operatableObject)
    {
        this.operatableObject = operatableObject;
        operatable = operatableObject.GetComponent<IHumanoidOperatable>();
        agent = operatableObject.GetComponent<ControllableAgent>();

        if (operatable == null)
        {
            Debug.LogWarning("HumanoidOperatableControlMode: Constructor: operatable object does not have an attached IHumanoidOperatable component.");
        }

        if (!agent)
        {
            Debug.LogWarning("HumanoidOperatableControlMode: Constructor: operatable object does not have an attached controllable agent. Adding default");
            agent = operatableObject.AddComponent<ControllableAgent>();
        }
    }

    private protected void InvokeSelectionForfeitedEvent()
    {
        SelectionForfeited?.Invoke();
    }

    public virtual void HandleNewSelection(GameObject selectedObject, Vector3 selectionPoint)
    {
        if (ReferenceEquals(selectedObject, operatableObject))
        {
            GameObject driver = operatable.GetDriver();
            if (driver)
            {
                OperatableEjectOccupantCommand ejectCommand = new OperatableEjectOccupantCommand(operatable, driver);
                agent.AddCommand(ejectCommand);
            }
        }

        SelectionForfeited?.Invoke();
    }
}