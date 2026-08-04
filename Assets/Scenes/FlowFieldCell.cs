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

    private static GameObject cellVisualizer = Resources.Load<GameObject>("FlowFieldCube");

    public int x, y, z = 0;
    public bool containsTriangle { get; set; } = false;

    public Vector3 flowValue { get; set; } = Vector3.zero;

    public float integrationValue { get; set; } = float.MaxValue;

    public bool queed = false;


    public static float maxIntegration = 0;

    private bool hasBeenDrawn = false;

    /// <summary>
    /// This is radius of the cell
    /// </summary>
    public static float cellSize { get; set; } = 0.5f;

    public bool hasAgent = false;

    public FlowFieldCell(Vector3 center, int x, int y, int z)
    {
        this.center = center;
        x = this.x; y = this.y; z = this.z;
    }

    public void AddNeighbour(FlowFieldCell newNeighbour)
    {

        neighbours.SetValue(newNeighbour, neighbourCount);

    }

    public bool IsInCell(Vector3 pos)
    {
        if (pos.x < center.x - cellSize / 2) return false;
        if (pos.y < center.y - cellSize / 2) return false;
        if (pos.z < center.z - cellSize / 2) return false;
        if (pos.x > center.x + cellSize / 2) return false;
        if (pos.y > center.y + cellSize / 2) return false;
        if (pos.z > center.z + cellSize / 2) return false;
        return true;
    }

    public void DrawCell()
    {
        if (hasBeenDrawn || integrationValue == float.PositiveInfinity) return;
        hasBeenDrawn = true;
        float halfCellSize = cellSize / 2 - 0.01f;
        Color color = new Color(0, 1, 1 - Mathf.Lerp(0, 1, integrationValue / maxIntegration));
        if (hasAgent) color = Color.red;


        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z + halfCellSize), color, 10000);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z + halfCellSize), color, 10000);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z - halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z - halfCellSize), color, 10000);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z - halfCellSize), new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z - halfCellSize), color, 10000);

        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z - halfCellSize), new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z - halfCellSize), color, 10000);
        Debug.DrawLine(new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z - halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z - halfCellSize), color, 10000);
        Debug.DrawLine(new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z + halfCellSize), color, 10000);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z + halfCellSize), color, 10000);

        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z - halfCellSize), color, 10000);
        Debug.DrawLine(new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z - halfCellSize), color, 10000);
        Debug.DrawLine(new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z - halfCellSize), color, 10000);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z + halfCellSize), new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z - halfCellSize), color, 10000);

        DrawFlowValue();

    }

    private void DrawFlowValue()
    {
        Vector3 arrowEnd = flowValue;
        arrowEnd.Normalize();
        arrowEnd *= cellSize;

        Debug.DrawLine(center, center + arrowEnd, Color.magenta, 10000);
    }

}
