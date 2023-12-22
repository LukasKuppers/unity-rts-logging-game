using System;
using UnityEngine;

public class FelledTreeInteractableObject : MonoBehaviour, IHumanoidInteractableObject
{
    public event Action interactionCompletedEvent;

    [SerializeField]
    private GameObject logPrefab;
    [SerializeField]
    private float maxInteractionDistance = 2f;
    [SerializeField]
    private float spawnVerticalOffset = 0.5f;

    // process felled tree (delete current gameobject, replace with log)
    public void Interact(GameObject interactingHumanoid)
    {
        if (Vector3.Distance(interactingHumanoid.transform.position, transform.position) <= maxInteractionDistance)
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();

            Vector3 spawnPosition = rb != null ? rb.worldCenterOfMass : transform.position;
            spawnPosition += Vector3.down * spawnVerticalOffset;

            Instantiate(logPrefab, spawnPosition, transform.rotation);
            interactionCompletedEvent?.Invoke();
            Destroy(gameObject);
        } 
        else
        {
            interactionCompletedEvent.Invoke();
        }
    }
}
