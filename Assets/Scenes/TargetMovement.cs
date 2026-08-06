using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    private int x, y, z;
    private float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {



    }

    private void OnEnable()
    {
        FlowFieldCell cell = FlowField.flowField.GetNearestCell(GameObject.FindGameObjectWithTag("Target").transform.position);
        x = cell.x; y = cell.y; z = cell.z;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.5f)
        {
            timer -= 0.5f;
            FlowFieldCell cell = FlowField.flowField.GetRandomNeighbour(x, y, z);
            x = cell.x; y = cell.y; z = cell.z;
            FlowField.flowField.IntegrateCompleteField(cell.center);
        }
    }
}
