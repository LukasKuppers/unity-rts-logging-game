using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;


public class NavTest : MonoBehaviour
{
    [SerializeField]
    private NavMeshSurface navMeshSurface;

    private void Start()
    {
        navMeshSurface.BuildNavMesh();   
    }
}
