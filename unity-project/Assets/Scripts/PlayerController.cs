using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private GameObject agentObject;

    private ControllableAgent agent;
    private int counter = 0;

    private void Start()
    {
        agent = agentObject.GetComponent<ControllableAgent>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            agent.AddCommand(new TalkCommand($"Message: count: {counter}"));
            counter++;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                MoveCommand moveCommand = new MoveCommand(agentObject, hit.point + Vector3.up);
                agent.AddCommand(moveCommand);
            }
        }
    }
}
