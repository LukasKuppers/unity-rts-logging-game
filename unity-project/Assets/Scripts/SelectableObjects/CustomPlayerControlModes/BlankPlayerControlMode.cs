using System;
using UnityEngine;

public class BlankPlayerControlMode : IPlayerControlMode
{
    public event Action SelectionForfeited;
    
    public void HandleNewSelection(GameObject selectedObject, Vector3 selectionPoint)
    {
        SelectionForfeited.Invoke();
    }
}
