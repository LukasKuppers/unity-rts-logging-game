using UnityEngine;

public class StackableObject : CollectableObject
{
    [SerializeField]
    private GameObject stackPrefab;

    /// <summary>
    /// Get the prefab for the stack that should typically contain this object
    /// </summary>
    /// <returns>An ObjectStack prefab</returns>
    public GameObject GetStackPrefab()
    {
        return stackPrefab;
    }
}
