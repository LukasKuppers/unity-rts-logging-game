using UnityEngine;

public class MillProductQueue : MonoBehaviour, IObjectRepository
{
    [SerializeField]
    private ObjectType inputObjectType;
    [SerializeField]
    private GameObject inputPosition;
    [SerializeField]
    private GameObject outputPosition;
    [SerializeField]
    private GameObject outputObjectPrefab;

    private GameObject inputProduct;
    private GameObject outputProduct;

    public void ConvertProduct()
    {
        if (!inputProduct)
            return;

        ObjectStack prefabStack = outputObjectPrefab.GetComponent<ObjectStack>();
        if (prefabStack)
        {
            if (!outputProduct)
            {
                outputProduct = Instantiate(outputObjectPrefab, outputPosition.transform);
                SetParentAndCenter(outputProduct.transform, outputPosition.transform);
            }

            ObjectStack actualStack = outputProduct.GetComponent<ObjectStack>();
            GameObject newStackableObject = Instantiate(actualStack.GetChildObjectPrefab());
            actualStack.Add(newStackableObject);

            if (actualStack.IsFull())
            {
                Destroy(inputProduct);
                inputProduct = null;
            }
        }
        else if (!outputProduct)
        {
            Destroy(inputProduct);
            inputProduct = null;

            outputProduct = Instantiate(outputObjectPrefab, outputPosition.transform);
            SetParentAndCenter(outputProduct.transform, outputPosition.transform);
        }
    }

    public ObjectType GetObjectType()
    {
        return inputObjectType;
    }

    public GameObject GetOutputProductPrefab()
    {
        return outputObjectPrefab;
    }

    public void Add(GameObject newObject)
    {
        if (!inputProduct)
        {
            SetParentAndCenter(newObject.transform, inputPosition.transform);
            inputProduct = newObject;

            CollectableObject collectable = inputProduct.GetComponent<CollectableObject>();
            if (collectable)
                collectable.RegisterRepositoryMembership(gameObject);
        }
    }

    public Transform GetNextAddTransform()
    {
        return inputPosition.transform;
    }

    public bool IsFull()
    {
        return inputProduct != null;
    }

    public GameObject GetInputObject()
    {
        return inputProduct;
    }

    public GameObject Peek()
    {
        return outputProduct;
    }

    public GameObject Remove()
    {
        GameObject removedObject = outputProduct;
        outputProduct = null;

        CollectableObject collectable = null;
        if (removedObject)
            collectable = removedObject.GetComponent<CollectableObject>();

        if (collectable != null)
            collectable.UnregisterFromRepository();

        return removedObject;
    }

    private void SetParentAndCenter(Transform child, Transform parent)
    {
        child.parent = parent;
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
    }
}
