using System;
using UnityEngine;

public interface IHumanoidInteractableObject
{
    /// <summary>
    /// Event invoked when the interaction is complete
    /// </summary>
    public event Action interactionCompletedEvent;

    /// <summary>
    /// Interact with the object - typically requires the agent to be near the object
    /// </summary>
    /// <param name="interactingHumanoid">The humanoid agent interacting with the object</param>
    public void Interact(GameObject interactingHumanoid);
}
