using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementTest : MonoBehaviour
{
    public int x, y, z = 0;
    public Vector3 movementDir = Vector3.zero;
    float timeToMove = 0.3f;
    public FlowFieldCell cell;

    // Start is called before the first frame update
    void Start()
    {

    }
    public void Init()
    {
        cell = FlowField.flowField.GetNearestCell(transform.position);
        movementDir = cell.flowValue;
    }

    // Update is called once per frame
    void Update()
    {
        float deltaMove = Time.deltaTime / timeToMove;
        transform.Translate(movementDir * deltaMove);
        if (cell.IsInCell(transform.position)) return;
        cell = FlowField.flowField.GetNearestCell(transform.position);
        movementDir = cell.flowValue;
        //Debug.Log(cell.center);

    }
}
