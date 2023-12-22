using System;


public interface ICommand
{
    /// <summary>
    /// Event invoked when the command is complete
    /// </summary>
    public event Action commandCompletedEvent;

    /// <summary>
    /// execute the command in-game
    /// </summary>
    public void Execute();
}
