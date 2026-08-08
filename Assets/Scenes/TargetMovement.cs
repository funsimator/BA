using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    private CellIndex cellI;
    private float timer = 0f;
    public float timeToMove;

    GameObject target;
    public GameObject visu;
    // Start is called before the first frame update
    void Start()
    {
        FlowFieldCell cell = FlowField.flowField.GetNearestCell(GameObject.FindGameObjectWithTag("Target").transform.position);
        cellI = new CellIndex(cell.x, cell.y, cell.z);

    }

    private void OnEnable()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > timeToMove)
        {
            timer -= timeToMove;
            FlowFieldCell cell = FlowField.flowField.GetRandomNeighbour(cellI);
            cellI.x = cell.x; cellI.y = cell.y; cellI.z = cell.z;
            FlowField.flowField.IntegrateWithHeuristic(cell.center, new AStar());
            visu.transform.position = cell.center;
        }
    }
}
