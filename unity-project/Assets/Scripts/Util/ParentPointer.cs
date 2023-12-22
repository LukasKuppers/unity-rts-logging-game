using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentPointer : MonoBehaviour
{
    [SerializeField]
    private GameObject parent;

    /// <summary>
    /// Irreversibly disable the ParentPointer, so GetParent returns this object
    /// </summary>
    public void Disable()
    {
        parent = gameObject;
    }

    public GameObject GetParent()
    {
        return parent;
    }

    /// <summary>
    /// Change the parent pointed to by this object. Can be dangerous!
    /// </summary>
    /// <param name="newParent">The new parent to point to</param>
    public void SetParent(GameObject newParent)
    {
        if (newParent == null)
        {
            Debug.LogWarning("ParentPointer: SetParent: Provided new parent is null, aborting operation.");
            return;
        }
        parent = newParent;
    }
}
