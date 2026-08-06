using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor.PackageManager;
using UnityEngine;

public class SceneHandler : MonoBehaviour
{
    public int loops = 0;
    private int lastLoops = int.MaxValue;
    private bool isInit = false;
    public bool DrawGraph;
    public bool DrawVectorOnly;
    public bool DrawNavMesh;
    public bool LoadFieldFromFile;
    public TextAsset FlowFieldJSON;

    private int frameCounter = 0;
    private float[] deltaTimes = new float[10];
    private List<MovementTest> agents;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        NavMeshHandler test = NavMeshHandler.navMeshHandler;

        FlowField.flowField.GenerateGraph(LoadFieldFromFile, FlowFieldJSON);
        if (DrawNavMesh) test.VisualizeNavMeshTriangles();

        FlowField.flowField.IntegrateCompleteField(GameObject.FindGameObjectWithTag("Target").transform.position);
        FlowField.flowField.GenerateVectorField();

        if (DrawGraph) FlowField.flowField.DrawGraph(DrawVectorOnly);

        GameObject[] temp = GameObject.FindGameObjectsWithTag("Agent");
        agents = new List<MovementTest>(temp.Length);
        foreach (GameObject go in temp)
        {
            agents.Add(go.GetComponent<MovementTest>());
        }
        foreach (MovementTest go in agents)
        {

            go.Init();
        }
    }

    // Update is called once per frame
    void Update()
    {
        deltaTimes[frameCounter] = Time.deltaTime;
        frameCounter++;
        if (frameCounter > 9)
        {
            float total = 0f;
            foreach (float time in deltaTimes)
                total += time;
            Debug.Log("Average Tick time: " + total / 10);

            frameCounter = 0;
        }
        if (lastLoops != loops)
        {
            lastLoops = loops;


        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { loops++; }
    }
}
