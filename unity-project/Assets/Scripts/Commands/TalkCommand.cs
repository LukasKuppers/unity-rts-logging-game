using System;
using UnityEngine;


[System.Serializable]
public class TalkCommand : ICommand
{
    public event Action commandCompletedEvent;

    private string message;

    public TalkCommand(string message)
    {
        this.message = message;
    }

    public void Execute()
    {
        Debug.Log(message);
        commandCompletedEvent?.Invoke();
    }
}
