using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementTest : MonoBehaviour
{
    public int x, y, z = 0;
    public Vector3 movementDir = Vector3.zero;
    public float speed = 1f;
    public FlowFieldCell cell;
    public NavMeshAgent agent;
    public GameObject visuCube;
    private GameObject visuInstance;
    public Transform position;
    public bool hasBeenReached = false;

    // Start is called before the first frame update
    void Start()
    {

    }
    public void Init()
    {
        cell = FlowField.flowField.GetNearestCell(position.position);
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
        //transform.Translate(movementDir * deltaMove);
        if (cell != null && cell.IsInCell(position.position) && agent.velocity != Vector3.zero)
        {
            return;
        }
        cell = FlowField.flowField.GetNearestCell(position.position);
        if (cell == null) return;
        visuInstance.transform.position = cell.center;
        movementDir = cell.flowValue;
        agent.SetDestination(cell.center + movementDir);



    }
}
