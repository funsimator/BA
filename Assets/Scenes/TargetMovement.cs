using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    private CellIndex cellI;
    private float timer = 0f;
    public float timeToMove;
    public bool callIntegrationNextTurn = false;

    GameObject target;
    public GameObject visu;
    // Start is called before the first frame update
    void Start()
    {
        FlowFieldCell cell = FlowField.flowField.GetNearestCell(GameObject.FindGameObjectWithTag("Target").transform.position, 0);
        cellI = new CellIndex(cell.x, cell.y, cell.z);

    }

    private void OnEnable()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (callIntegrationNextTurn)
            callIntegrationNextTurn = FlowField.flowField.IntegrateWithHeuristic(visu.transform.position, new AStar(), 0, true);

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            FlowFieldCell cell = FlowField.flowField.GetNearestCell(hit.point, 0);
            cellI.x = cell.x; cellI.y = cell.y; cellI.z = cell.z;
            callIntegrationNextTurn = FlowField.flowField.IntegrateWithHeuristic(cell.center, new AStar(), 0);
            visu.transform.position = cell.center;
        }

        timer += Time.deltaTime;
        if (timer > timeToMove)
        {
            timer -= timeToMove;
            FlowFieldCell cell = FlowField.flowField.GetRandomNeighbour(cellI);
            cellI.x = cell.x; cellI.y = cell.y; cellI.z = cell.z;
            callIntegrationNextTurn = FlowField.flowField.IntegrateWithHeuristic(cell.center, new AStar(), 0);
            visu.transform.position = cell.center;
        }
    }
}
