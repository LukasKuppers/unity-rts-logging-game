using System.Collections.Generic;
using UnityEngine;

public interface IHumanoidOperatable
{
    /// <summary>
    /// Returns whether or not the entity is full, or if there is space for more operators
    /// </summary>
    /// <returns>True iff there are no more empty spaces for more humanoids</returns>
    public bool IsFull();

    /// <summary>
    /// Get the number of occupants currently present in the operatable entity
    /// </summary>
    /// <returns>The number of occupants present</returns>
    public int GetOccupantCount();

    /// <summary>
    /// Returns the position at which humanoids must be to enter the operatable entity in world space
    /// </summary>
    /// <returns>The entry point in world space</returns>
    public Vector3 GetEntryPoint();

    /// <summary>
    /// Gets all the passengers currently present in the operatable entity (not including a spcific driver)
    /// </summary>
    /// <returns>A list of passengers</returns>
    public List<GameObject> GetPassengers();

    /// <summary>
    /// Gets the current driver of the operatable entity. If there are any occupants, there will always be a driver
    /// </summary>
    /// <returns>The current driver</returns>
    public GameObject GetDriver();

    /// <summary>
    /// Add a new occupant to the operatable entity
    /// </summary>
    /// <param name="humanoid">The humanoid to add</param>
    public void AddOccupant(GameObject humanoid);

    /// <summary>
    /// Remove an occupant from the operatable entity
    /// </summary>
    /// <param name="humanoid">The occupant to remove</param>
    public void EjectOccupant(GameObject humanoid);
}
