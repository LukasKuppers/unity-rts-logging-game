using System;
using UnityEngine;

public class HumanoidControlMode : IPlayerControlMode
{
    public event Action SelectionForfeited;

    private GameObject agentObject;
    private ControllableAgent agent;

    public HumanoidControlMode(GameObject agentObject)
    {
        this.agentObject = agentObject;
        agent = agentObject.GetComponent<ControllableAgent>();
    }

    public void HandleNewSelection(GameObject selectedObject, Vector3 selectionPoint)
    {
        IHumanoidOperatable operatable = selectedObject.GetComponent<IHumanoidOperatable>();
        IHumanoidInteractableObject interactableObject = selectedObject.GetComponent<IHumanoidInteractableObject>();
        if (operatable != null)
        {
            // try to enter vehicle
            if (!operatable.IsFull())
            {
                MoveCommand moveCommand = new MoveCommand(agentObject, operatable.GetEntryPoint());
                EnterHumanoidOperatableCommand enterCommand = new EnterHumanoidOperatableCommand(agentObject, operatable);

                agent.AddCommand(moveCommand);
                agent.AddCommand(enterCommand);
            }
        }
        else if (interactableObject != null)
        {
            MoveCommand moveCommand = new MoveCommand(agentObject, selectedObject.transform.position);
            HumanoidInteractCommand interactCommand = new HumanoidInteractCommand(agentObject, selectedObject);

            agent.AddCommand(moveCommand);
            agent.AddCommand(interactCommand);
        }
        else
        {
            MoveCommand moveCommand = new MoveCommand(agentObject, selectionPoint);
            agent.AddCommand(moveCommand);
        }

        SelectionForfeited?.Invoke();
    }
}
