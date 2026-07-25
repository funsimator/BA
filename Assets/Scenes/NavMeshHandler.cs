using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor.SearchService;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class NavMeshHandler
{
    public static NavMeshHandler navMeshHandler { get; private set; } = new NavMeshHandler();

    private List<FlowFieldCell> startingCells = new List<FlowFieldCell>();

    public NavMeshTriangulation navMesh { get; private set; }

    public GameObject Cube = Resources.Load<GameObject>("VisuCube");
    // Start is called before the first frame update
    private NavMeshHandler()
    {
        Debug.Log(Cube);
        navMesh = NavMesh.CalculateTriangulation();
        //GenerateGraph();
        Debug.Log("isDebugBuild: " + Debug.isDebugBuild);
        VisualizeNavMeshTriangles();


    }

    private void VisualizeNavMeshTriangles()
    {
        int b = 1;
        int c = 2;
        for (int a = 0; a < navMesh.indices.Length; a += 3)
        {
            Debug.DrawLine(navMesh.vertices[navMesh.indices[a]], navMesh.vertices[navMesh.indices[b]], Color.red, 10000);
            Debug.DrawLine(navMesh.vertices[navMesh.indices[a]], navMesh.vertices[navMesh.indices[c]], Color.red, 10000);
            Debug.DrawLine(navMesh.vertices[navMesh.indices[b]], navMesh.vertices[navMesh.indices[c]], Color.red, 10000);

            //GameObject.Instantiate(Cube, navMesh.vertices[navMesh.indices[a]], Quaternion.identity);
            //GameObject.Instantiate(Cube, navMesh.vertices[navMesh.indices[b]], Quaternion.identity);
            //GameObject.Instantiate(Cube, navMesh.vertices[navMesh.indices[c]], Quaternion.identity);
            b += 3;
            c += 3;
        }
    }
    public bool GenerateGraph(float cellSize = 0.5f)
    {
        FlowFieldCell.cellSize = cellSize;

        int b = 1;
        int c = 2;

        for (int a = 0; a < navMesh.indices.Length; a += 3)
        {
            Vector3[] trianglePoints = { navMesh.vertices[navMesh.indices[a]], navMesh.vertices[navMesh.indices[b]], navMesh.vertices[navMesh.indices[c]] };

            Vector3 sideA = trianglePoints[1] - trianglePoints[0];
            Vector3 sideB = trianglePoints[2] - trianglePoints[0];
            Vector3 sideC = trianglePoints[2] - trianglePoints[1];

            double triangleHeight = Vector3.Cross(trianglePoints[1] - trianglePoints[0], trianglePoints[0] - trianglePoints[2]).magnitude / (trianglePoints[1] - trianglePoints[0]).magnitude;

            double sideABasePointDistance = Math.Sqrt(sideB.magnitude * sideB.magnitude - triangleHeight * triangleHeight);

            Vector3 sideABasePoint = trianglePoints[0] + sideA.normalized * (float)sideABasePointDistance;

            Vector3 sideAHeightVector = trianglePoints[2] - sideABasePoint;
            sideAHeightVector.Normalize();


            FlowFieldCell firstCell = new FlowFieldCell(trianglePoints[0] + sideA.normalized * cellSize / 2 + sideAHeightVector * cellSize / 2);

            startingCells.Add(firstCell);

            Debug.DrawLine(trianglePoints[0], firstCell.center, Color.yellow);

            //firstCell.DrawCell();

            Debug.Break();

            b += 3;
            c += 3;
        }

        return true;
    }


    // Explanation of PointInTriangle method:
    // youtu.be/HYAgJN3x4GA?list=PLFt_AvWsXl0cD2LPxcjxVjWTQLxJqKpgZ

    /// Determines whether point P is inside the triangle ABC
    public static bool PointInTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        double s1 = C.y - A.y;
        double s2 = C.x - A.x;
        double s3 = B.y - A.y;
        double s4 = P.y - A.y;

        double w1 = (A.x * s1 + s4 * s2 - P.x * s1) / (s3 * s2 - (B.x - A.x) * s1);
        double w2 = (s4 - w1 * s3) / s1;
        return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
    }

    public void GetMaxCoordinates(out Vector3 min, out Vector3 max)
    {
        min = Vector3.positiveInfinity; max = Vector3.negativeInfinity;


        for (int i = 0; i < navMesh.vertices.Length; i++)
        {
            if (min.x > navMesh.vertices[i].x)
                min.x = navMesh.vertices[i].x;
            if (max.x < navMesh.vertices[i].x)
                max.x = navMesh.vertices[i].x;
            if (min.y > navMesh.vertices[i].y)
                min.y = navMesh.vertices[i].y;
            if (max.y < navMesh.vertices[i].y)
                max.y = navMesh.vertices[i].y;
            if (min.z > navMesh.vertices[i].z)
                min.z = navMesh.vertices[i].z;
            if (max.z < navMesh.vertices[i].z)
                max.z = navMesh.vertices[i].z;
        }

    }
}
