using System.Collections.Generic;
using UnityEngine;

public class ObjectStack : MonoBehaviour, IObjectRepository
{
    [SerializeField]
    private ObjectType stackType;
    [SerializeField]
    [Tooltip("non-permanent stacks will be destroyed when completeely emptied")]
    private bool isPermanent = false;
    [SerializeField]
    [Tooltip("The positions where objects will be stacked, where lower indicies should be lower in the stack")]
    private GameObject[] stackTransforms;
    [SerializeField]
    private GameObject childObjectPrefab;

    private Stack<GameObject> stackedObjects;

    private void Awake()
    {
        stackedObjects = new Stack<GameObject>();
    }

    public ObjectType GetObjectType()
    {
        return stackType;
    }

    public GameObject GetChildObjectPrefab()
    {
        return childObjectPrefab;
    }

    public int GetMaxCapacity()
    {
        return stackTransforms.Length;
    }

    public bool IsFull()
    {
        return stackedObjects.Count >= stackTransforms.Length;
    }

    /// <summary>
    /// Add firstStackObject as the first object in the stack, and orient the stack to match the postion and rotation of firstStackObject
    /// </summary>
    /// <param name="firstStackObject">The first object in the stack, that determines the stack orientation</param>
    public void InitStack(GameObject firstStackObject)
    {
        StackableObject stackable = firstStackObject.GetComponent<StackableObject>();
        if (stackable.GetObjectType() != stackType)
        {
            Debug.LogWarning($"ObjectStack: InitStack: Cannot add object of type {stackable.GetType()} to stack of type {stackType}.");
        }

        if (isPermanent)
        {
            Debug.LogWarning("ObjectStack: InitStack: Cannot initialize permanent stack. Use the AddObject method to add objects.");
            return;
        }

        if (stackedObjects.Count > 0)
        {
            Debug.LogWarning("ObjectStack: InitStack: Cannot initilize stack that already contains stacked objects.");
            return;
        }

        // orient the stack to match firstStackObject, and add firstStackObject to the stack
        GameObject child = stackTransforms[0];
        Vector3 localChildPos = child.transform.localPosition;
        transform.eulerAngles = firstStackObject.transform.eulerAngles - child.transform.localEulerAngles;

        Vector3 worldChildOffset = (transform.right * localChildPos.x) + (transform.up * localChildPos.y) + (transform.forward * localChildPos.z);
        transform.position = firstStackObject.transform.position - worldChildOffset;

        Add(firstStackObject);
    }

    /// <summary>
    /// Add an object to the stack, if there is space
    /// </summary>
    /// <param name="stackObject">The object to add to the stack - should have a StackableObject attached with matching StackableObjectType</param>
    public void Add(GameObject stackObject)
    {
        if (stackedObjects.Count >= stackTransforms.Length)
        {
            Debug.Log($"ObjectStack: AddToStack: Can't add object {stackObject.name} because the stack is full.");
            return;
        }

        StackableObject stackable = stackObject.GetComponent<StackableObject>();
        if (stackable.GetObjectType() != stackType)
        {
            Debug.LogWarning($"ObjectStack: AddToStack: Cannot add object of type {stackable.GetType()} to stack of type {stackType}.");
            return;
        }

        GameObject nextStackTransform = stackTransforms[stackedObjects.Count];

        // align object with stack transform
        stackObject.transform.parent = nextStackTransform.transform;
        stackObject.transform.localPosition = Vector3.zero;
        stackObject.transform.localEulerAngles = Vector3.zero;

        stackable.RegisterRepositoryMembership(gameObject);

        stackedObjects.Push(stackObject);
    }

    /// <summary>
    /// Remove the top object from the stack and return it (if the stack is not empty
    /// </summary>
    /// <returns>The top object taken off the stack, or null if the stack is empty</returns>
    public GameObject Remove()
    {
        if (stackedObjects.Count == 0)
        {
            return null;
        }

        GameObject topObject = stackedObjects.Pop();

        // unatatch transform from stack
        topObject.transform.parent = null;

        StackableObject stackable = topObject.GetComponent<StackableObject>();
        if (stackable)
            stackable.UnregisterFromRepository();

        if (!isPermanent && stackedObjects.Count == 0)
            Destroy(gameObject);    // stack isn't destroyed until the current update loop is complete (so this works)

        return topObject;
    }

    /// <summary>
    /// Get the world space position and rotation of the next stack position
    /// </summary>
    /// <returns>the Transform of the next stack position, or null if the stack is full</returns>
    public Transform GetNextAddTransform()
    {
        int nextObjectIndex = stackedObjects.Count;
        if (nextObjectIndex >= stackTransforms.Length)
        {
            return null;
        }

        GameObject nextStackTransform = stackTransforms[nextObjectIndex];
        return nextStackTransform.transform;
    }

    /// <summary>
    /// Get the top object, without removing it from the stack
    /// </summary>
    /// <returns>the Transform of the top stack object, or null if the stack is empty</returns>
    public GameObject Peek()
    {
        if (stackedObjects.Count <= 0)
        {
            return null;
        }

        GameObject topStackObject = stackedObjects.Peek();
        return topStackObject;
    }
}
