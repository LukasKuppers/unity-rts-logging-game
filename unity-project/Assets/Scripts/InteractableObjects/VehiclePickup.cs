using UnityEngine;

public class VehiclePickup : MonoBehaviour
{
    private enum VectorDirection { 
        X, Y, Z
    }

    [SerializeField]
    [Tooltip("Along which local vector should the object be grabbed?")]
    private VectorDirection grabDirection = VectorDirection.X;
    [SerializeField]
    private GameObject grabPoint;

    private Vector3 localGrabOffset;

    private void Awake()
    {
        localGrabOffset = grabPoint != null ? grabPoint.transform.localPosition : Vector3.zero;
    }

    /// <summary>
    /// Get the position in world space where the object should be grabbed
    /// </summary>
    /// <returns>The grab position</returns>
    public Vector3 GetGrabPoint()
    {
        return transform.position + localGrabOffset;
    }

    /// <summary>
    /// Get the direction in world space that the object should be grabbed along
    /// </summary>
    /// <returns>The grab direction</returns>
    public Vector3 GetGrabDirection()
    {
        return GetGrabDirectionForTransform(transform);
    }

    /// <summary>
    /// Get the direction in world space that the given transform should be grabbed along (assuming it is a pickup of this type).
    /// </summary>
    /// <param name="trans"></param>
    /// <returns>The grab direction</returns>
    public Vector3 GetGrabDirectionForTransform(Transform trans)
    {
        Vector3 direction = grabDirection == VectorDirection.X ? trans.right :
                            grabDirection == VectorDirection.Y ? trans.up : trans.forward;
        return direction;
    }
}
