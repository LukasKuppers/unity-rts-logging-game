using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectSelectionManager : MonoBehaviour
{
    [SerializeField]
    private GameObject cameraObject;

    private Camera cam;
    private GlobalInputActions inputActions;
    private IPlayerControlMode currentControlMode;
    private GameObject currentSelectedObject;

    private void Awake()
    {
        cam = cameraObject.GetComponent<Camera>();
        inputActions = new GlobalInputActions();

        inputActions.Gameplay.Select.performed += OnClick;
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        GameObject clickedObject = GetClickedObject(out Vector3 clickedPoint);
        if (!clickedObject)
            return;     // abort if nothing was clicked

        if (currentControlMode == null)
        {
            // If clicked object is selectable, select it
            ISelectableObject selectableObject = clickedObject.GetComponent<ISelectableObject>();
            if (selectableObject != null)
            {
                currentSelectedObject = clickedObject;
                EnableObjectOutline(currentSelectedObject);

                currentControlMode = selectableObject.GetPlayerControlMode();
                currentControlMode.SelectionForfeited += OnCurrentSelectionForfeited;
                selectableObject.OnSelect();
            }
        }
        else
        {
            // feed clicked object to current player control mode
            currentControlMode.HandleNewSelection(clickedObject, clickedPoint);
        }
    }

    private void OnCurrentSelectionForfeited()
    {
        DisableObjectOutline(currentSelectedObject);

        currentSelectedObject.GetComponent<ISelectableObject>().OnDeselect();
        currentControlMode.SelectionForfeited -= OnCurrentSelectionForfeited;

        currentSelectedObject = null;
        currentControlMode = null;
    }

    private GameObject GetClickedObject(out Vector3 clickPoint)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        clickPoint = Vector3.zero;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            clickPoint = hit.point;

            GameObject colliderObject = hit.collider.gameObject;
            ParentPointer parentPointer = colliderObject.GetComponent<ParentPointer>();
            return parentPointer ? parentPointer.GetParent() : colliderObject;
        }
        return null;
    }

    private void EnableObjectOutline(GameObject obj)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (!outline)
        {
            outline = obj.AddComponent<Outline>();
        }

        outline.enabled = true;
    }

    private void DisableObjectOutline(GameObject obj)
    {
        // assume obj has has an outline
        obj.GetComponent<Outline>().enabled = false;
    }

    private void OnEnable()
    {
        inputActions.Gameplay.Select.Enable();
    }

    private void OnDisable()
    {
        inputActions.Gameplay.Select.Disable();
    }
}
