using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField]
    private GameObject cameraObject;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float cameraRotateSpeed = 0.5f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float cameraPanSpeed = 0.5f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float cameraZoomSpeed = 0.5f;

    private GlobalInputActions inputActions;
    private Camera cam;

    private float zoomDistance = 50f;
    private float horizontalRotation = 45f;
    private float verticalRotation = 45f;

    private readonly float MIN_ZOOM = 15f;
    private readonly float MAX_ZOOM = 50f;
    private readonly float MIN_ANGLE = 10f;
    private readonly float MAX_ANGLE = 90f;

    private void Awake()
    {
        inputActions = new GlobalInputActions();
        cam = cameraObject.GetComponent<Camera>();

        ApplyRotation(horizontalRotation, verticalRotation);
    }

    private void Update()
    {
        // handle zoom
        Vector2 zoomDelta = inputActions.Gameplay.ZoomCamera.ReadValue<Vector2>();
        zoomDistance = Mathf.Clamp(zoomDistance += zoomDelta.y * -cameraZoomSpeed, MIN_ZOOM, MAX_ZOOM);
        cam.orthographicSize = zoomDistance;

        if (inputActions.Gameplay.EnableRotateCamera.IsPressed())   // handle rotation
        {
            Vector2 rotateInput = inputActions.Gameplay.RotateCamera.ReadValue<Vector2>();
            Vector2 finalRotation = ProcessInput(rotateInput, cameraRotateSpeed);

            horizontalRotation += finalRotation.x;
            verticalRotation = Mathf.Clamp(verticalRotation -= finalRotation.y, MIN_ANGLE, MAX_ANGLE);

            ApplyRotation(horizontalRotation, verticalRotation);
        } 
        else if (inputActions.Gameplay.EnablePanCamera.IsPressed()) // handle pan
        {
            Vector2 panInput = inputActions.Gameplay.PanCamera.ReadValue<Vector2>();
            Vector2 finalPan = ProcessInput(panInput, cameraPanSpeed);

            ApplyTranslation(finalPan.x, finalPan.y);
        }
    }

    private void ApplyRotation(float horizontalRotation, float verticalRotation)
    {
        transform.rotation = Quaternion.identity;
        transform.Rotate(Vector3.up, horizontalRotation, Space.World);
        transform.Rotate(Vector3.right, verticalRotation, Space.Self);
    }

    private void ApplyTranslation(float lateralPan, float forwardsPan)
    {
        // scale translation according to zoom amount:
        float zoomSlowingCoefficient = Mathf.InverseLerp(MIN_ZOOM, MAX_ZOOM, zoomDistance) + 0.2f;
        lateralPan *= zoomSlowingCoefficient;
        forwardsPan *= zoomSlowingCoefficient;

        Vector3 localForwardFlat = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 localRightFlat = transform.right;

        transform.position += localForwardFlat * -forwardsPan;
        transform.position += localRightFlat * -lateralPan;
    }

    private Vector2 ProcessInput(Vector2 input, float sensitivityScaler)
    {
        return input * sensitivityScaler;
    }

    private void OnEnable()
    {
        inputActions.Gameplay.ZoomCamera.Enable();

        inputActions.Gameplay.EnableRotateCamera.Enable();
        inputActions.Gameplay.RotateCamera.Enable();

        inputActions.Gameplay.EnablePanCamera.Enable();
        inputActions.Gameplay.PanCamera.Enable();
    }

    private void OnDisable()
    {
        inputActions.Gameplay.ZoomCamera.Disable();

        inputActions.Gameplay.EnableRotateCamera.Disable();
        inputActions.Gameplay.RotateCamera.Disable();

        inputActions.Gameplay.EnablePanCamera.Disable();
        inputActions.Gameplay.PanCamera.Disable();
    }
}
