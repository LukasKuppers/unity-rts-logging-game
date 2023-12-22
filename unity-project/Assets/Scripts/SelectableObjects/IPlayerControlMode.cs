using System;
using UnityEngine;

public interface IPlayerControlMode
{
    /// <summary>
    /// Event invoked when control mode is finished
    /// </summary>
    public event Action SelectionForfeited;

    /// <summary>
    /// Give the control a newly selected object to handle
    /// </summary>
    /// <param name="selectedObject">The selected object</param>
    /// <param name="selectionPoint">The exact point on the object that was selected</param>
    public void HandleNewSelection(GameObject selectedObject, Vector3 selectionPoint);
}
