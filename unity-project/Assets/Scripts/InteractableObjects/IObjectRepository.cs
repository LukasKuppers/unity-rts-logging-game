using UnityEngine;

public interface IObjectRepository
{
    /// <summary>
    /// Gets the type of object allowed to be placed in the repository
    /// </summary>
    /// <returns>The permitted object type</returns>
    public ObjectType GetObjectType();

    /// <summary>
    /// Returns whether or not there is more room to add objects in the repository
    /// </summary>
    /// <returns>True iff at least one more object can be added</returns>
    public bool IsFull();

    /// <summary>
    /// Adds a gameobject to the object repository.
    /// </summary>
    /// <param name="newObject">The object to add</param>
    public void Add(GameObject newObject);

    /// <summary>
    /// Removes a gameobject from the object repository. The specific repo manages which object is removed.
    /// </summary>
    /// <returns>The gameobject which was removed</returns>
    public GameObject Remove();

    /// <summary>
    /// Get the transform (position / rotation) where the next object added to the repository will reside.
    /// </summary>
    /// <returns>The transform (position/rotation) of the next repo position</returns>
    public Transform GetNextAddTransform();

    /// <summary>
    /// Returns the next object that will be removed from the repository, without modifying the repository.
    /// </summary>
    /// <returns>The object that will be removed by calling Remove()</returns>
    public GameObject Peek();
}
