using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllableAgent : MonoBehaviour
{
    private Queue<ICommand> activeCommands;
    private bool isCommandExecuting = false;

    private void Start()
    {
        activeCommands = new Queue<ICommand>();
    }

    public void AddCommand(ICommand command)
    {
        activeCommands.Enqueue(command);

        // if no commands currently executing, execute new command
        if (!isCommandExecuting)
            ExecuteNextCommand();
    }

    private void ExecuteNextCommand()
    {
        if (activeCommands.Count > 0 && !isCommandExecuting)
        {
            isCommandExecuting = true;

            ICommand command = activeCommands.Dequeue();
            command.commandCompletedEvent += OnCommandCompleted;
            command.Execute();
        }
    }

    private void OnCommandCompleted()
    {
        isCommandExecuting = false;
        ExecuteNextCommand();
    }
}
