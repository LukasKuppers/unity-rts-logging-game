using UnityEngine;

public class AimConstraintWithOffset : MonoBehaviour
{
    private enum Axis
    {
        X, X_REV, 
        Y, Y_REV, 
        Z, Z_REV
    }

    private enum RelativeDirection
    {
        LEFT, RIGHT
    }

    [SerializeField]
    private Transform constrainedObject;
    [SerializeField]
    [Tooltip("Which axis of the constrained object should point at the target")]
    private Axis aimAxis;
    [SerializeField]
    private Transform sourceObject;
    [SerializeField]
    private RelativeDirection offsetDirection;
    [SerializeField]
    private float offsetAmount = 0f;

    private void Update()
    {
        Vector3 localSourcePos = transform.InverseTransformPoint(sourceObject.transform.position);

        (Vector2 leftLook, Vector2 rightLook) =new Vector2(localSourcePos.x, localSourcePos.z).OriginToCircleTangent(offsetAmount);
        Vector2 lookDir = offsetDirection == RelativeDirection.LEFT ? leftLook : rightLook;

        float rotationDeg = -Vector2.SignedAngle(Vector2.right, lookDir) - 90f;

        constrainedObject.localEulerAngles = new Vector3(0, rotationDeg, 0);
    }
}
