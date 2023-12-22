using UnityEngine;

/// <summary>
/// A collectable object can be stored in an object repository (as defined by IObjectRepository).
/// There is (intended) coupling between them!
/// </summary>
public class CollectableObject : MonoBehaviour
{
    [SerializeField]
    private ObjectType objectType;

    private IObjectRepository currentRepository = null;

    public ObjectType GetObjectType()
    {
        return objectType;
    }

    /// <summary>
    /// Get the object repository this object is currently a member of.
    /// </summary>
    /// <returns>Returns the parent stack, or null if this object is not a member of a stack</returns>
    public IObjectRepository GetCurrentRepository()
    {
        return currentRepository;
    }

    public void RegisterRepositoryMembership(GameObject repoObject)
    {
        if (currentRepository != null)
        {
            Debug.LogWarning("CollectableObject: RegisterRepositoryMembership: cannot register, as object is already part of a repository.");
            return;
        }

        IObjectRepository newRepo = repoObject.GetComponent<IObjectRepository>();
        if (newRepo == null)
        {
            Debug.LogWarning("CollectableObject: RegisterRepositoryMembership: provided object does not have an attached IObjectRepository.");
            return;
        }

        currentRepository = newRepo;
    }

    public void UnregisterFromRepository()
    {
        currentRepository = null;
    }
}
