using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public bool LogTickTimes;
    public TextAsset FlowFieldJSON;

    private int frameCounter = 0;
    private float[] deltaTimes = new float[10];
    public List<MovementTest> agents;


    // Start is called before the first frame update
    void Start()
    {
        //FlowField.flowField.IntegrateCompleteField(GameObject.FindGameObjectWithTag("Target").transform.position);
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

        Vector3 targetPos = GameObject.FindGameObjectWithTag("Target").transform.position;
        FlowField.flowField.IntegrateWithHeuristic(targetPos, new AStar(), 0);
        //foreach (MovementTest go in agents)
        //{

        //    go.Init();
        //}

        MovementTest.UpdateGroups();
        //if (DrawGraph) FlowField.flowField.DrawGraph(DrawVectorOnly);

    }

    private void Awake()
    {
        NavMeshHandler test = NavMeshHandler.navMeshHandler;

        FlowField.flowField.GenerateGraph(LoadFieldFromFile, FlowFieldJSON);
        if (DrawNavMesh) test.VisualizeNavMeshTriangles();

    }

    private void OnEnable()
    {


    }

    // Update is called once per frame
    void Update()
    {
        if (LogTickTimes)
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
        }
        if (lastLoops != loops)
        {
            lastLoops = loops;


        }
        if (Input.GetKeyDown(KeyCode.RightArrow)) { loops++; }
    }
}
