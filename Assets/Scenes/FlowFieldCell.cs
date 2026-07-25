using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

public class FlowFieldCell
{
    public Vector3 center;
    /// <summary>
    /// 0-8 y+ 
    /// 9-16 y=
    /// 17-27 y-
    /// </summary>
    public FlowFieldCell[] neighbours = new FlowFieldCell[28];
    public int neighbourCount { get; private set; } = 0;

    private static GameObject cellVisualizer = Resources.Load<GameObject>("GreenSphere");

    public bool containsTriangle { get; set; } = false;

    /// <summary>
    /// This is radius of the cell
    /// </summary>
    public static float cellSize { get; set; } = 0.5f;
    public Vector3 flowFieldValue { get; private set; } = Vector3.zero;

    public FlowFieldCell(Vector3 center)
    {
        this.center = center;

    }

    public void AddNeighbour(FlowFieldCell newNeighbour)
    {

        neighbours.SetValue(newNeighbour, neighbourCount);

    }

    public void DrawCell()
    {
        float halfCellSize = cellSize / 2;

        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z + halfCellSize), Color.green, 10000);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z + halfCellSize), Color.green, 10000);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z - halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z - halfCellSize), Color.green, 10000);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z - halfCellSize), new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z - halfCellSize), Color.green, 10000);

        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z - halfCellSize), new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z - halfCellSize), Color.green, 10000);
        Debug.DrawLine(new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z - halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z - halfCellSize), Color.green, 10000);
        Debug.DrawLine(new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z + halfCellSize), Color.green, 10000);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z + halfCellSize), Color.green, 10000);

        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z - halfCellSize), Color.green, 10000);
        Debug.DrawLine(new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z - halfCellSize), Color.green, 10000);
        Debug.DrawLine(new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z - halfCellSize), Color.green, 10000);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z + halfCellSize), new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z - halfCellSize), Color.green, 10000);



    }


}
