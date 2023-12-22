using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidSelectableObject : MonoBehaviour, ISelectableObject
{
    private IPlayerControlMode playerControlMode;

    private void Awake()
    {
        playerControlMode = new HumanoidControlMode(gameObject);
    }

    public IPlayerControlMode GetPlayerControlMode()
    {
        return playerControlMode;
    }

    public void OnSelect()
    {
        return;
    }

    public void OnDeselect()
    {
        return;
    }
}
