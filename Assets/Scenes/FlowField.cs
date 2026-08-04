using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SearchService;
using UnityEngine;

public class FlowField
{

    private NavMeshHandler navMesh = NavMeshHandler.navMeshHandler;
    // Start is called before the first frame update
    public GameObject Cube = Resources.Load<GameObject>("VisuCube");

    public static FlowField flowField { get; private set; } = new FlowField();
    int cellsX = 0;
    int cellsY = 0;
    int cellsZ = 0;

    private FlowFieldCell[,,] flowFieldCells = null;

    private Vector3 fieldOrigin = Vector3.zero;

    private GameObject visuCube = Resources.Load<GameObject>("FlowFieldCube");

    private GameObject colliderEmpty = null;

    private FlowField()
    {

    }

    private void PrepFieldForIteration(bool resetIntegration = true)
    {
        for (int x = 0; x < cellsX; x++)
        {
            for (int y = 0; y < cellsY; y++)
            {
                for (int z = 0; z < cellsZ; z++)
                {
                    if (flowFieldCells[x, y, z] != null)
                    {
                        if (resetIntegration) flowFieldCells[x, y, z].integrationValue = float.PositiveInfinity;
                        flowFieldCells[x, y, z].queed = false;

                    }
                }
            }
        }
    }


    public void GenerateGraph()
    {
        navMesh.GetMaxCoordinates(out Vector3 min, out Vector3 max);
        Debug.Log("Min" + min);
        Debug.Log("Max: " + max);
        fieldOrigin = min;

        cellsX = Mathf.Max(Mathf.CeilToInt((max.x - min.x) / FlowFieldCell.cellSize + 1), 1);
        cellsY = Mathf.Max(Mathf.CeilToInt((max.y - min.y) / FlowFieldCell.cellSize + 1), 1);
        cellsZ = Mathf.Max(Mathf.CeilToInt((max.z - min.z) / FlowFieldCell.cellSize + 1), 1);
        flowFieldCells = new FlowFieldCell[cellsX, cellsY, cellsZ];


        for (int x = 0; x < cellsX; x++)
        {
            for (int y = 0; y < cellsY; y++)
            {
                for (int z = 0; z < cellsZ; z++)
                {
                    flowFieldCells[x, y, z] = new FlowFieldCell(new Vector3(min.x + FlowFieldCell.cellSize * x, min.y + FlowFieldCell.cellSize * y, min.z + FlowFieldCell.cellSize * z), x, y, z);
                }
            }
        }
        CleanGraph();

        //Temp
        int[] ind = new int[3];
        GetNearestCell(new Vector3(-6.73799992f, 0, -0.0469999984f), out ind[0], out ind[1], out ind[2]);
        flowFieldCells[ind[0], ind[1], ind[2]].hasAgent = true;
        GetNearestCell(new Vector3(0.790000021f, 0, -5.03999996f), out ind[0], out ind[1], out ind[2]);
        flowFieldCells[ind[0], ind[1], ind[2]].hasAgent = true;
        GetNearestCell(new Vector3(-1.69000006f, 3.91000009f, 20.1399994f), out ind[0], out ind[1], out ind[2]);
        flowFieldCells[ind[0], ind[1], ind[2]].hasAgent = true;
        //Temp

        //IntegrateCompleteField(new Vector3(-6.73799992f, 0, -0.0469999984f));
        //DrawGraph();
    }

    public void DrawGraph()
    {
        Debug.Log("Tst");
        for (int x = 0; x < cellsX; x++)
        {
            for (int y = 0; y < cellsY; y++)
            {
                for (int z = 0; z < cellsZ; z++)
                {
                    //GameObject.Instantiate(visuCube, flowFieldCells[x, y, z].center, Quaternion.identity).transform.localScale = new Vector3(FlowFieldCell.cellSize, FlowFieldCell.cellSize, FlowFieldCell.cellSize);
                    if (flowFieldCells[x, y, z] != null)
                        flowFieldCells[x, y, z].DrawCell();

                }

            }

        }
    }

    private void CleanGraph()
    {
        int activeCellCounter = 0;
        int b = 1;
        int c = 2;
        for (int a = 0; a < navMesh.navMesh.indices.Length; a += 3)
        {
            colliderEmpty = GameObject.Find("ColliderEmpty");

            Vector3[] trianglePoints = { navMesh.navMesh.vertices[navMesh.navMesh.indices[a]], navMesh.navMesh.vertices[navMesh.navMesh.indices[b]], navMesh.navMesh.vertices[navMesh.navMesh.indices[c]] };

            GameObject.Instantiate(Cube, trianglePoints[0], Quaternion.identity);
            GameObject.Instantiate(Cube, trianglePoints[1], Quaternion.identity);
            GameObject.Instantiate(Cube, trianglePoints[2], Quaternion.identity);

            //Find min and Max Coordinates in World Space of the given triangle
            float minX = int.MaxValue, minY = int.MaxValue, minZ = int.MaxValue;
            float maxX = int.MinValue, maxY = int.MinValue, maxZ = int.MinValue;

            if (trianglePoints[0].x < minX)
                minX = trianglePoints[0].x;
            if (trianglePoints[0].y < minY)
                minY = trianglePoints[0].y;
            if (trianglePoints[0].z < minZ)
                minZ = trianglePoints[0].z;
            if (trianglePoints[1].x < minX)
                minX = trianglePoints[1].x;
            if (trianglePoints[1].y < minY)
                minY = trianglePoints[1].y;
            if (trianglePoints[1].z < minZ)
                minZ = trianglePoints[1].z;
            if (trianglePoints[2].x < minX)
                minX = trianglePoints[2].x;
            if (trianglePoints[2].y < minY)
                minY = trianglePoints[2].y;
            if (trianglePoints[2].z < minZ)
                minZ = trianglePoints[2].z;

            if (trianglePoints[0].x > maxX)
                maxX = trianglePoints[0].x;
            if (trianglePoints[0].y > maxY)
                maxY = trianglePoints[0].y;
            if (trianglePoints[0].z > maxZ)
                maxZ = trianglePoints[0].z;
            if (trianglePoints[1].x > maxX)
                maxX = trianglePoints[1].x;
            if (trianglePoints[1].y > maxY)
                maxY = trianglePoints[1].y;
            if (trianglePoints[1].z > maxZ)
                maxZ = trianglePoints[1].z;
            if (trianglePoints[2].x > maxX)
                maxX = trianglePoints[2].x;
            if (trianglePoints[2].y > maxY)
                maxY = trianglePoints[2].y;
            if (trianglePoints[2].z > maxZ)
                maxZ = trianglePoints[2].z;

            int[] sectorOriginCell = { 0, 0, 0 };


            //Find the Indizes in the Flow Field for the Origin of the Triangle         

            while (fieldOrigin.x + sectorOriginCell[0] * FlowFieldCell.cellSize < minX)
                sectorOriginCell[0]++;
            sectorOriginCell[0] = Mathf.Max(sectorOriginCell[0] - 1, 0);

            while (fieldOrigin.y + sectorOriginCell[1] * FlowFieldCell.cellSize < minY)
                sectorOriginCell[1]++;
            sectorOriginCell[1] = Mathf.Max(sectorOriginCell[1] - 1, 0);

            while (fieldOrigin.z + sectorOriginCell[2] * FlowFieldCell.cellSize < minZ)
                sectorOriginCell[2]++;
            sectorOriginCell[2] = Mathf.Max(sectorOriginCell[2] - 1, 0);

            int[] sectorMaxPoint = { sectorOriginCell[0], sectorOriginCell[1], sectorOriginCell[2] };

            //Find the Indizes in the Flow Field for the Max Point of the Triangle

            while (fieldOrigin.x + sectorMaxPoint[0] * FlowFieldCell.cellSize < maxX)
                sectorMaxPoint[0]++;
            if (sectorMaxPoint[0] >= cellsX) sectorMaxPoint[0] = cellsX - 1;
            while (fieldOrigin.y + sectorMaxPoint[1] * FlowFieldCell.cellSize < maxY)
                sectorMaxPoint[1]++;
            if (sectorMaxPoint[1] >= cellsY) sectorMaxPoint[1] = cellsY - 1;
            while (fieldOrigin.z + sectorMaxPoint[2] * FlowFieldCell.cellSize < maxZ)
                sectorMaxPoint[2]++;
            if (sectorMaxPoint[2] >= cellsZ) sectorMaxPoint[2] = cellsZ - 1;

            int xAmount = sectorMaxPoint[0] - sectorOriginCell[0] + 1;
            int yAmount = sectorMaxPoint[1] - sectorOriginCell[1] + 1;
            int zAmount = sectorMaxPoint[2] - sectorOriginCell[2] + 1;
            int cellAmount = xAmount * yAmount * zAmount;


            for (int x = sectorOriginCell[0]; x <= sectorMaxPoint[0]; x++)
            {
                for (int y = sectorOriginCell[1]; y <= sectorMaxPoint[1]; y++)
                {
                    for (int z = sectorOriginCell[2]; z <= sectorMaxPoint[2]; z++)
                    {

                        float cellsize = FlowFieldCell.cellSize;
                        Vector3 unten = flowFieldCells[x, y, z].center - new Vector3(cellsize / 2, cellsize / 2, cellsize / 2);
                        Vector3 oben = flowFieldCells[x, y, z].center + new Vector3(cellsize / 2, cellsize / 2, cellsize / 2);
                        if (IntersectionTest.IsIntersecting(new IAABoxImpl(unten, oben), new TriangleImpl(trianglePoints[0], trianglePoints[1], trianglePoints[2])))
                        {
                            flowFieldCells[x, y, z].containsTriangle = true;
                        }


                    }
                }
            }

            b += 3;
            c += 3;
            //if (a > 3)
            //    break;
        }


        for (int x = 0; x < cellsX; x++)
        {
            for (int y = 0; y < cellsY; y++)
            {
                for (int z = 0; z < cellsZ; z++)
                {
                    if (flowFieldCells[x, y, z].containsTriangle == false)
                        flowFieldCells[x, y, z] = null;
                    else activeCellCounter++;

                }

            }

        }
        Debug.Log("activeCellCounter: " + activeCellCounter);
    }

    public void IntegrateCompleteField(Vector3 goal)
    {
        PrepFieldForIteration();
        int[] goalIndices = new int[3];
        GetNearestCell(goal, out goalIndices[0], out goalIndices[1], out goalIndices[2]);
        flowFieldCells[goalIndices[0], goalIndices[1], goalIndices[2]].integrationValue = 0f;
        flowFieldCells[goalIndices[0], goalIndices[1], goalIndices[2]].queed = true;

        float currentIntegration = 0f;

        List<int[]> activeCells = new List<int[]>();
        AddNeighbours(goalIndices[0], goalIndices[1], goalIndices[2], activeCells);
        int counter = 0;
        int loops = GameObject.Find("Main Camera").GetComponent<SceneHandler>().loops;

        while (activeCells.Count > 0 && counter < loops)
        {
            int cellCounter = 0;
            foreach (var cell in activeCells)
            {
                cellCounter++;
                flowFieldCells[cell[0], cell[1], cell[2]].integrationValue = FindIntegrationValue(cell);
                //Debug.Log(new Vector3(cell[0], cell[1], cell[2]));
                //Debug.Log("Integration Value: " + flowFieldCells[cell[0], cell[1], cell[2]].integrationValue);
            }
            List<int[]> temp = activeCells;
            activeCells = new List<int[]>();
            foreach (var cell in temp)
            {
                AddNeighbours(cell[0], cell[1], cell[2], activeCells);
            }
            FlowFieldCell.maxIntegration = currentIntegration;
            counter++;
        }
        Debug.Log("Amount of Loop while integrating: " + counter);
    }

    private float FindIntegrationValue(int[] ind)
    {
        int x = ind[0];
        int y = ind[1];
        int z = ind[2];
        List<int[]> cells = new List<int[]>();
        AddNeighbours(x, y, z, cells, false, false);
        float lowestIntegration = float.PositiveInfinity;

        foreach (var cell in cells)
        {
            float flow = flowFieldCells[cell[0], cell[1], cell[2]].integrationValue + (flowFieldCells[cell[0], cell[1], cell[2]].center - flowFieldCells[x, y, z].center).magnitude;
            if (flow < lowestIntegration) lowestIntegration = flow;
        }
        return lowestIntegration;
    }

    private void AddNeighbours(int x, int y, int z, List<int[]> list, bool setQueed = true, bool forIntegration = true)
    {

        for (int a = x - 1; a <= x + 1; a++)
        {
            for (int b = z - 1; b <= z + 1; b++)
            {
                if (0 <= a && a < cellsX && 0 <= y + 1 && y + 1 < cellsY && 0 <= b && b < cellsZ && flowFieldCells[a, y + 1, b] != null && (flowFieldCells[a, y + 1, b].integrationValue == float.PositiveInfinity || !forIntegration) && (!flowFieldCells[a, y + 1, b].queed || !forIntegration))
                {
                    list.Add(new int[] { a, y + 1, b });
                    flowFieldCells[a, y + 1, b].queed = setQueed;
                }

                if (0 <= a && a < cellsX && 0 <= y && y < cellsY && 0 <= b && b < cellsZ && flowFieldCells[a, y, b] != null && !(a == x && b == z) && (flowFieldCells[a, y, b].integrationValue == float.PositiveInfinity || !forIntegration) && (!flowFieldCells[a, y, b].queed || !forIntegration))
                {
                    list.Add(new int[] { a, y, b });
                    flowFieldCells[a, y, b].queed = setQueed;
                }

                if (0 <= a && a < cellsX && 0 <= y - 1 && y - 1 < cellsY && 0 <= b && b < cellsZ && flowFieldCells[a, y - 1, b] != null && (flowFieldCells[a, y - 1, b].integrationValue == float.PositiveInfinity || !forIntegration) && (!flowFieldCells[a, y - 1, b].queed || !forIntegration))
                {
                    list.Add(new int[] { a, y - 1, b });
                    flowFieldCells[a, y - 1, b].queed = setQueed;
                }

            }
        }
    }

    public FlowFieldCell GetNearestCell(Vector3 pos)
    {
        int x = 0, y = 0, z = 0;
        GetNearestCell(pos, out x, out y, out z);
        return flowFieldCells[x, y, z];
    }
    private void GetNearestCell(Vector3 pos, out int x, out int y, out int z)
    {
        x = 0; y = 0; z = 0;
        float cellSize = FlowFieldCell.cellSize;

        Vector3 posNormalized = pos - fieldOrigin;
        Vector3 posScaled = posNormalized / cellSize;

        if (pos.x < fieldOrigin.x)
            x = 0;
        else if (pos.x > fieldOrigin.x + cellsX * cellSize)
            x = cellsX - 1;
        else
            x = Mathf.FloorToInt(posScaled.x);

        if (pos.y < fieldOrigin.y)
            y = 0;
        else if (pos.y > fieldOrigin.y + cellsY * cellSize)
            y = cellsX - 1;
        else
            y = Mathf.FloorToInt(posScaled.y);

        if (pos.z < fieldOrigin.z)
            z = 0;
        else if (pos.z > fieldOrigin.z + cellsZ * cellSize)
            z = cellsX - 1;
        else
            z = Mathf.FloorToInt(posScaled.z);

        if (flowFieldCells[x, y, z] != null) return;
        while (y >= 0 && flowFieldCells[x, y, z] == null) y--;
    }

    public void GenerateVectorField()
    {
        PrepFieldForIteration(false);
        List<int[]> neighbours = new List<int[]>();
        for (int x = 0; x < cellsX; x++)
        {
            for (int y = 0; y < cellsY; y++)
            {
                for (int z = 0; z < cellsZ; z++)
                {
                    if (flowFieldCells[x, y, z] == null || flowFieldCells[x, y, z].integrationValue == float.PositiveInfinity) continue;
                    neighbours.Clear();
                    flowFieldCells[x, y, z].queed = false;

                    AddNeighbours(x, y, z, neighbours, false,false);
                    if (neighbours.Count == 0) continue;
                    int highest = FindHighestIntegration(neighbours);
                    int[] highestNeighbour = neighbours[highest];
                    flowFieldCells[x, y, z].flowValue = flowFieldCells[highestNeighbour[0], highestNeighbour[1], highestNeighbour[2]].center - flowFieldCells[x, y, z].center;

                    //Debug.Log("Flow Value: " + flowFieldCells[x, y, z].flowValue);
                    //Debug.Break();
                }
            }
        }


    }
    private int FindHighestIntegration(List<int[]> list)
    {
        int index = 0;
        float lowest = float.MaxValue;
        for (int i = 0; i < list.Count; i++)
        {
            if (flowFieldCells[list[i][0], list[i][1], list[i][2]].integrationValue < lowest)
            {
                lowest = flowFieldCells[list[i][0], list[i][1], list[i][2]].integrationValue;
                index = i;
            }
        }
        return index;
    }

    public Vector3 GetVector(int x, int y, int z)
    {
        if (flowFieldCells[x, y, z] == null) return Vector3.zero;
        return flowFieldCells[x, y, z].flowValue;
    }
}
