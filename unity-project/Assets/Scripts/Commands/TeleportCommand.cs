using System;
using UnityEngine;


[System.Serializable]
public class TeleportCommand : ICommand
{
    public event Action commandCompletedEvent;

    private GameObject obj;
    private Vector3 newPos;

    public TeleportCommand(GameObject obj, Vector3 newPos)
    {
        this.obj = obj;
        this.newPos = newPos;
    }

    public void Execute()
    {
        obj.transform.position = newPos;
        commandCompletedEvent.Invoke();
    }
}
