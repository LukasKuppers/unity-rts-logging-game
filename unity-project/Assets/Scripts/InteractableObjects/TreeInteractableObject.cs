using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class TreeInteractableObject : MonoBehaviour, IHumanoidInteractableObject
{
    public event Action interactionCompletedEvent;

    [SerializeField]
    private float maxInteractDistance = 2f;
    [SerializeField]
    private float ragdollTime = 15.0f;
    [SerializeField]
    private GameObject treeTopObject;

    private bool isChopped = false;

    private Rigidbody rb;
    private NavMeshObstacle navObstacle;
    private ParentPointer treeTopParentPointer;

    private void Awake()
    {
        rb = treeTopObject.GetComponent<Rigidbody>();
        if (!rb)
        {
            Debug.LogWarning("TreeInteractableObject: Awake: Assigned treeTopObject does not have a rigidbody attached. Adding defeault rigidbody.");
            rb = treeTopObject.AddComponent<Rigidbody>();
        }
        // disable rb by default
        SetRigidbodyEnabled(false);

        navObstacle = treeTopObject.GetComponent<NavMeshObstacle>();
        if (!navObstacle)
        {
            Debug.LogWarning("TreeInteractableObject: Awake: Assigned treeTopObject does not have a navMeshObstacle attached. Adding default NavMeshObstacle.");
            navObstacle = treeTopObject.AddComponent<NavMeshObstacle>();
            navObstacle.carving = true;
        }
        // disable nav mesh obstacle by default
        navObstacle.enabled = false;

        treeTopParentPointer = treeTopObject.GetComponent<ParentPointer>();
    }

    public void Interact(GameObject interactingHumanoid)
    {
        if (!isChopped && Vector3.Distance(interactingHumanoid.transform.position, transform.position) <= maxInteractDistance)
        {
            SetRigidbodyEnabled(true);

            Vector2 direction = UnityEngine.Random.insideUnitCircle;
            rb.AddRelativeTorque(new Vector3(direction.x * 40f, 0, direction.y * 40f));

            StartCoroutine(FixTreeAfterRagdollTime());
        }
        
        interactionCompletedEvent?.Invoke();
    }

    private void SetRigidbodyEnabled(bool isEnabled)
    {
        if (rb)
            rb.isKinematic = !isEnabled;
    }

    private IEnumerator FixTreeAfterRagdollTime()
    {
        yield return new WaitForSeconds(ragdollTime);

        SetRigidbodyEnabled(false);
        
        if (navObstacle)
            navObstacle.enabled = true;

        if (treeTopParentPointer)
            treeTopParentPointer.Disable();

        isChopped = true;
    }
}
