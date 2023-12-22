using UnityEngine;

public class BlankSelectableObject : MonoBehaviour, ISelectableObject
{
    private BlankPlayerControlMode controlMode;

    private void Awake()
    {
        controlMode = new BlankPlayerControlMode();
    }

    public IPlayerControlMode GetPlayerControlMode()
    {
        return controlMode;
    }

    public void OnSelect()
    {
        Debug.Log($"Seleted blank object with name: {gameObject.name}");
    }

    public void OnDeselect()
    {
        Debug.Log($"Deseleted blank object with name: {gameObject.name}");
    }
}
