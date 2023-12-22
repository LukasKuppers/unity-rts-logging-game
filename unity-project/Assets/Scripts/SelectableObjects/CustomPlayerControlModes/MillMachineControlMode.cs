using UnityEngine;

public class MillMachineControlMode : HumanoidOperatableControlMode
{
    private MillMachine machine;

    public MillMachineControlMode(GameObject machineObject) : base(machineObject)
    {
        machine = machineObject.GetComponent<MillMachine>();
        if (!machine)
        {
            Debug.LogWarning("MillMachineControlMode: Constructor: provided machine object does not have MillMachine. Attaching default MillMachine.");
            machine = machineObject.AddComponent<MillMachine>();
        }
    }

    public override void HandleNewSelection(GameObject selectedObject, Vector3 selectionPoint)
    {
        if (selectedObject && ReferenceEquals(machine.GetProductQueue().GetInputObject(), selectedObject))
        {
            ProcessInputProductCommand processCommand = new ProcessInputProductCommand(machine);

            agent.AddCommand(processCommand);
            base.InvokeSelectionForfeitedEvent();
        }
        else
            base.HandleNewSelection(selectedObject, selectionPoint);
    }
}
