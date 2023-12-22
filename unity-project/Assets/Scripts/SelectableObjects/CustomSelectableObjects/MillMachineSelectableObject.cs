using UnityEngine;

public class MillMachineSelectableObject : MonoBehaviour, ISelectableObject
{
    private enum MillMachineType
    {
        GENERIC
    }

    [SerializeField]
    private MillMachineType machineType = MillMachineType.GENERIC;

    private HumanoidOperatableControlMode machineControlMode;

    private void Awake()
    {
        switch (machineType)
        {
            case MillMachineType.GENERIC:
                machineControlMode = new MillMachineControlMode(gameObject);
                break;
        }
    }

    public IPlayerControlMode GetPlayerControlMode()
    {
        return machineControlMode;
    }

    public void OnDeselect() { }

    public void OnSelect() { }
}
