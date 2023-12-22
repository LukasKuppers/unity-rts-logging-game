using System;
using UnityEngine;

public class HumanoidInteractCommand : ICommand
{
    public event Action commandCompletedEvent;

    private GameObject interactingAgentObject;
    private IHumanoidInteractableObject interactable;

    public HumanoidInteractCommand(GameObject interactingAgentObject, GameObject interactionObject)
    {
        this.interactingAgentObject = interactingAgentObject;
        interactable = interactionObject.GetComponent<IHumanoidInteractableObject>();

        if (interactable == null)
        {
            Debug.LogWarning("HumanoidInteractCommand: Constructor: Interact command has been issued on object that does not contain a IHumanoidInteractableObject.");
        }
    }

    public void Execute()
    {
        if (interactable == null)
        {
            Debug.LogWarning("HumanoidInteractCommand: Execute: Attempting to execute interaction on object that does not contain a IHumanoidInteractableObject: Aborting.");
            commandCompletedEvent?.Invoke();
        }
        else
        {
            interactable.interactionCompletedEvent += () => commandCompletedEvent?.Invoke();
            interactable.Interact(interactingAgentObject);
        }
    }
}
