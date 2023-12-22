using System;
using UnityEngine;

public class ProcessInputProductCommand : ICommand
{
    private static readonly float SAW_VERTICAL_VELOCITY = 0.2f;
    private static readonly float SAW_HORIZONTAL_FREE_VELOCITY = 1f;
    private static readonly float SAW_HORIZONTAL_CUT_VELOCITY = 0.2f;

    public event Action commandCompletedEvent;

    private MillMachine millMachine;
    private MillProductQueue productQueue;

    public ProcessInputProductCommand(MillMachine millMachine)
    {
        this.millMachine = millMachine;

        productQueue = millMachine.GetProductQueue();
    }

    public void Execute()
    {
        if (!millMachine.GetDriver())
        {
            Debug.LogWarning("ProcessInputProductCommand: Execute: No operator present in machine. Aborting.");
            commandCompletedEvent?.Invoke();
            return;
        }

        if (!productQueue.GetInputObject())
        {
            Debug.LogWarning("ProcessInputProductCommand: Execute: No input product to process. Aborting.");
            commandCompletedEvent?.Invoke();
            return;
        }

        if (productQueue.Peek())
        {
            Debug.LogWarning("ProcessInputProductCommand: Execute: Output space is full. Must clear output product before processing new intput. Aborting.");
            commandCompletedEvent?.Invoke();
            return;
        }

        int numCuts = 1;
        ObjectStack outputStack = productQueue.GetOutputProductPrefab().GetComponent<ObjectStack>();
        if (outputStack)
            numCuts = outputStack.GetMaxCapacity();

        // initially, set saw to start, top
        Promise<bool> promise = millMachine.AnimateSawMovement(0f, SAW_HORIZONTAL_FREE_VELOCITY, true)
            .Then(() => millMachine.AnimateSawMovement(1f, SAW_VERTICAL_VELOCITY, false));

        float verticalCutDelta = 1f / (numCuts + 1f);

        for (int i = 0; i < numCuts; i++)
        {
            // animate single cut
            float targetSawHeight = 1f - ((i + 1) * verticalCutDelta);
            promise = promise
                .Then(() => millMachine.AnimateSawMovement(targetSawHeight, SAW_VERTICAL_VELOCITY, false))  // set cut height
                .Then(() => millMachine.AnimateSawMovement(1f, SAW_HORIZONTAL_CUT_VELOCITY, true))          // cut
                .Then(() => productQueue.ConvertProduct())                                                  // convert product
                .Then(() => millMachine.AnimateSawMovement(1f, SAW_VERTICAL_VELOCITY, false))               // set height to max
                .Then(() => millMachine.AnimateSawMovement(0f, SAW_HORIZONTAL_FREE_VELOCITY, true));        // go back to init pos
        }

        promise
            .Then(() =>commandCompletedEvent?.Invoke())
            .Catch((string msg) => Debug.LogWarning($"ProcessInputProductCommand: Execute: Error encountered during animation: {msg}"));
    }

}
