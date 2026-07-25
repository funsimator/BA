using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
                    flowFieldCells[x, y, z] = new FlowFieldCell(new Vector3(min.x + FlowFieldCell.cellSize * x, min.y + FlowFieldCell.cellSize * y, min.z + FlowFieldCell.cellSize * z));
                }
            }
        }
        CleanGraph();

        DrawGraph();
    }

    private void DrawGraph()
    {
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
                        else Debug.Log("Not Intersecting");


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

    private void SculptGraph()
    {

        int b = 1;
        int c = 2;
        for (int a = 0; a < navMesh.navMesh.indices.Length; a += 3)
        {
            Vector3[] trianglePoints = { navMesh.navMesh.vertices[navMesh.navMesh.indices[a]], navMesh.navMesh.vertices[navMesh.navMesh.indices[b]], navMesh.navMesh.vertices[navMesh.navMesh.indices[c]] };


            b += 3;
            c += 3;
        }


    }

}
