using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementTest : MonoBehaviour
{
    public Vector3 movementDir = Vector3.zero;
    public float speed = 1f;
    public FlowFieldCell cell;
    public NavMeshAgent agent;
    public GameObject visuCube;
    private GameObject visuInstance;
    public Transform position;
    public bool hasBeenReached = false;
    public int portal;
    public int fieldIndex = 0;
    public static SceneHandler scene = null;

    public static int groupAmount = 0;
    public static float groupRadius = 30;
    public int groupIndex = 0;
    public Queue<MovementTest> group = null;


    // Start is called before the first frame update
    void Start()
    {

    }
    public void Init()
    {
        if (scene == null) scene = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<SceneHandler>();
        cell = FlowField.flowField.GetNearestCell(position.position, fieldIndex);
        visuInstance = GameObject.Instantiate(visuCube);
        visuInstance.transform.localScale = new Vector3(FlowFieldCell.cellSize, FlowFieldCell.cellSize, FlowFieldCell.cellSize);
        if (cell != null)
        {
            movementDir = cell.flowValue;
            agent.SetDestination(cell.center + movementDir);
            visuInstance.transform.position = cell.center;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //float deltaMove = Time.deltaTime / (1 / speed);
        //transform.Translate(movementDir * Time.deltaTime);
        if (cell != null && cell.IsInCell(position.position) && agent.velocity != Vector3.zero)
        {
            return;
        }

        cell = FlowField.flowField.GetNearestCell(position.position, fieldIndex);

        movementDir = cell.flowValue;

        if (movementDir == Vector3.zero)
            movementDir = FlowField.flowField.GetNearestCell(position.position, 0).flowValue;

        if (movementDir == Vector3.zero)
            movementDir = FlowField.flowField.FillGraph(cell, fieldIndex);


        agent.SetDestination(cell.center + movementDir);

        visuInstance.transform.position = cell.center;
        portal = cell.portalPath;

    }

    public static void UpdateGroups()
    {
        int currentGroup = 0;
        foreach (var agent in scene.agents)
        {
            agent.groupIndex = currentGroup++;
            agent.group = null;
        }

        foreach (var agentOne in scene.agents)
        {
            if (agentOne.group != null) continue;
            agentOne.group = new Queue<MovementTest>();
            agentOne.group.Enqueue(agentOne);
            foreach (var agentTwo in scene.agents)
            {
                if (agentTwo.group != null) continue;
                if (agentOne != agentTwo && (agentOne.position.position - agentTwo.position.position).magnitude < groupRadius)
                {
                    agentTwo.groupIndex = agentOne.groupIndex;
                    agentTwo.group = agentOne.group;
                    agentTwo.group.Enqueue(agentTwo);
                }
            }
        }
    }
}
