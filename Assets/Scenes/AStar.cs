using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : IHeuristic
{

    public Vector3 target { get; set; } = Vector3.zero;


    public float CalculateHeuristic(FlowFieldCell cell)
    {
        return (target - cell.center).magnitude;
    }


}
