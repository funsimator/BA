using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NavMeshHandler test = NavMeshHandler.navMeshHandler;
        Debug.Log(test);
        FlowField.flowField.GenerateGraph();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
