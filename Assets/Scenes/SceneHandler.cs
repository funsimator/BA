using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneHandler : MonoBehaviour
{
    public int loops;
    private int lastLoops = int.MaxValue;
    private bool isInit = false;
    public MovementTest mov;
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
        if (lastLoops != loops)
        {

            FlowField.flowField.IntegrateCompleteField(GameObject.FindGameObjectWithTag("Target").transform.position);
            FlowField.flowField.GenerateVectorField();
            FlowField.flowField.DrawGraph();
            if (!isInit)
            {
                mov.Init();
                isInit = true;
            }
        }
        lastLoops = loops;
        if (Input.GetKeyDown(KeyCode.RightArrow)) { loops++; }
    }
}
