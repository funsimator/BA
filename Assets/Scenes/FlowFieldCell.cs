using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

[Serializable]
public class FlowFieldCellSerialize
{

}

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

    public float integrationValue { get; set; } = float.PositiveInfinity;


    public FlowFieldCell cellWithLowestIntegration;

    public bool queed = false;

    public int activeWaveFront = int.MinValue;

    public int previousWaveFront = int.MinValue;

    private float isVisibleFor = 3f;

    public static float maxIntegration = 0;

    public int portalIndex = 0;

    public int portalPath = 0;
    /// <summary>
    /// This is radius of the cell
    /// </summary>
    public static float cellSize { get; set; } = 1f; // was 1 

    public bool isBLocked = false;

    public bool hasAgent = false;
    /// <summary>
    /// Constructor for a new Flow Field Cell
    /// </summary>
    /// <param name="center">Center in world Space</param>
    /// <param name="x">Index in Flow Field</param>
    /// <param name="y">Index in Flow Field</param>
    /// <param name="z">Index in Flow Field</param>
    public FlowFieldCell(Vector3 center, int x, int y, int z, int portalIndex = 0)
    {
        this.center = center;
        this.x = x; this.y = y; this.z = z;
        this.portalIndex = portalIndex;
    }



    public void AddNeighbour(FlowFieldCell newNeighbour)
    {

        neighbours.SetValue(newNeighbour, neighbourCount);

    }

    public bool IsInCell(Vector3 pos)
    {
        if (pos.x < center.x - cellSize / 2f) return false;
        if (pos.y < center.y - cellSize / 2f) return false;
        if (pos.z < center.z - cellSize / 2f) return false;
        if (pos.x > center.x + cellSize / 2f) return false;
        if (pos.y > center.y + cellSize / 2f) return false;
        if (pos.z > center.z + cellSize / 2f) return false;
        return true;
    }



    public void DrawCell(bool drawVectorOnly)
    {
        DrawFlowValue();
        if (drawVectorOnly) return;
        float halfCellSize = cellSize / 2 - 0.01f;
        Color color = new Color(0, 1, 1 - Mathf.Lerp(0, 1, integrationValue / maxIntegration));
        if (portalIndex > 0) { color = Color.magenta; }
        if (isBLocked) { color = Color.red; }

        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z + halfCellSize), color, isVisibleFor);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z + halfCellSize), color, isVisibleFor);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z - halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z - halfCellSize), color, isVisibleFor);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z - halfCellSize), new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z - halfCellSize), color, isVisibleFor);

        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z - halfCellSize), new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z - halfCellSize), color, isVisibleFor);
        Debug.DrawLine(new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z - halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z - halfCellSize), color, isVisibleFor);
        Debug.DrawLine(new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z + halfCellSize), color, isVisibleFor);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z + halfCellSize), color, isVisibleFor);

        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x + halfCellSize, center.y + halfCellSize, center.z - halfCellSize), color, isVisibleFor);
        Debug.DrawLine(new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y + halfCellSize, center.z - halfCellSize), color, isVisibleFor);
        Debug.DrawLine(new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z + halfCellSize), new Vector3(center.x - halfCellSize, center.y - halfCellSize, center.z - halfCellSize), color, isVisibleFor);
        Debug.DrawLine(new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z + halfCellSize), new Vector3(center.x + halfCellSize, center.y - halfCellSize, center.z - halfCellSize), color, isVisibleFor);


    }

    private void DrawFlowValue()
    {
        Vector3 arrowEnd = flowValue;
        arrowEnd.Normalize();
        arrowEnd *= cellSize;

        Debug.DrawLine(center, center + arrowEnd, Color.magenta, isVisibleFor);
    }

}
