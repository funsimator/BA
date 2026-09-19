using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Transactions;
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

    private FlowFieldCell lastblock = null;

    private int blockRadius = 15;

    private FlowFieldCell[,,] flowFieldCells = null;

    private FlowFieldCell[,,] altFlowFieldCells = null;

    private Vector3 fieldOrigin = Vector3.zero;

    private Vector3 fieldMax = Vector3.zero;

    public MovementTest dummyEmpty;

    private GameObject visuCube = Resources.Load<GameObject>("FlowFieldCube");

    private GameObject colliderEmpty = null;

    private SeriField seri = new SeriField();

    private int maxCellsPerTick = 3000;

    private bool loadFromFile = false;

    private SceneHandler sceneHandler = null;

    private List<MovementTest> agents = null;

    private MovementTest nearestAgent = null;

    private IHeuristic heuristic = null;

    SortedList<float, FlowFieldCell> activeWavefront = new SortedList<float, FlowFieldCell>();

    private bool resumeDivergence = false;

    private FlowFieldCell[,,] currentField;

    int currentWaveFront = 0;
    private FlowField()
    {


    }

    public bool IntegrateWithHeuristic(Vector3 goal, IHeuristic heuristic, int fieldIndex, bool resume = false, MovementTest to = null)
    {
        Debug.Log("IntegrateWithHeuristic");
        this.heuristic = heuristic;
        FlowFieldCell start = GetNearestCell(goal, fieldIndex);
        Queue<FlowFieldCell> integratedCells = new Queue<FlowFieldCell>();
        SetCurrentField(fieldIndex);
        if (!resume)
        {
            activeWavefront.Clear();
            if (to == null) currentWaveFront++;
            if (sceneHandler == null) sceneHandler = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<SceneHandler>();
            agents = new List<MovementTest>(sceneHandler.agents);
            nearestAgent = UpdateAgents(start);
            activeWavefront = new SortedList<float, FlowFieldCell>(new DuplicateKeyComparer<float>());
            start.integrationValue = 0;
            start.cellWithLowestIntegration = start;
            activeWavefront.Add(0, start);

        }

        heuristic.target = nearestAgent.cell.center;

        bool portalMode = false;
        if (to != null)
        {
            agents.Clear();
            agents.Add(to);
            if (to.cell != null)
                heuristic.target = to.cell.center;
        }


        while (agents.Count > 0 && activeWavefront.Count > 0 && (integratedCells.Count < maxCellsPerTick || to != null))
        {
            nearestAgent = UpdateAgents(start);
            heuristic.target = nearestAgent.cell.center;
            FlowFieldCell currentAdvance = activeWavefront.First().Value;
            activeWavefront.RemoveAt(0);

            currentAdvance.integrationValue = currentAdvance.cellWithLowestIntegration.integrationValue + (currentAdvance.cellWithLowestIntegration.center - currentAdvance.center).magnitude;


            if (currentAdvance.portalIndex > 0 /*&& currentAdvance.previousWaveFront == currentWaveFront - 1*/)
            {
                agents.RemoveAll(item => item.portal == currentAdvance.portalIndex);
                //activeWavefront.Clear();
                //portalMode = true;
            }

            AdvanceWavefront(currentAdvance, heuristic, portalMode);

            agents.RemoveAll(item => (item.cell.x == currentAdvance.x && item.cell.y == currentAdvance.y && item.cell.z == currentAdvance.z));
            if (to != null && nearestAgent.cell.x == currentAdvance.x && nearestAgent.cell.y == currentAdvance.y && nearestAgent.cell.z == currentAdvance.z) break;

            integratedCells.Enqueue(currentAdvance);
        }

        if (to != null)
            FilldDeviationEndpoint(to.cell, 1, integratedCells);

        GenerateVectorField(integratedCells, sceneHandler.DrawGraph && fieldIndex == 0);
        //if (integratedCells.Count != maxCellsPerTick && fieldIndex == 0) DivergePath(goal);
        return (integratedCells.Count == maxCellsPerTick);
    }

    private void FilldDeviationEndpoint(FlowFieldCell cell, int fieldIndex, Queue<FlowFieldCell> integratedCells)
    {
        SetCurrentField(fieldIndex);
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int z = 0; z < 3; z++)
                {
                    if (isInField(cell.x + x, cell.y + y, cell.z + z))
                    {
                        FillGraph(currentField[cell.x + x, cell.y + y, cell.z + z], fieldIndex);
                        integratedCells.Enqueue(currentField[cell.x + x, cell.y + y, cell.z + z]);
                    }
                    if (isInField(cell.x - x, cell.y - y, cell.z - z))
                    {
                        FillGraph(currentField[cell.x - x, cell.y - y, cell.z - z], fieldIndex);
                        integratedCells.Enqueue(currentField[cell.x - x, cell.y - y, cell.z - z]);
                    }

                }
            }
        }
    }

    public Vector3 FillGraph(FlowFieldCell cell, int fieldIndex)
    {
        Debug.Log("Fill Graph!");
        if (fieldIndex == 0) currentField = flowFieldCells;
        else currentField = altFlowFieldCells;
        float lowestIntegration = float.PositiveInfinity;
        FlowFieldCell lowestCell = cell;

        for (int a = cell.x - 1; a <= cell.x + 1; a++)
        {
            for (int b = cell.z - 1; b <= cell.z + 1; b++)
            {
                if (isInField(a, cell.y - 1, b) && !currentField[a, cell.y - 1, b].isBLocked)
                {
                    if (currentField[a, cell.y - 1, b].integrationValue < lowestIntegration)
                    {
                        lowestIntegration = currentField[a, cell.y - 1, b].integrationValue;
                        lowestCell = currentField[a, cell.y - 1, b];
                    }
                }

                if (isInField(a, cell.y, b) && !currentField[a, cell.y, b].isBLocked)
                {
                    if (currentField[a, cell.y, b].integrationValue < lowestIntegration)
                    {
                        lowestIntegration = currentField[a, cell.y, b].integrationValue;
                        lowestCell = currentField[a, cell.y, b];
                    }

                }

                if (isInField(a, cell.y + 1, b) && !currentField[a, cell.y + 1, b].isBLocked)
                {
                    if (currentField[a, cell.y + 1, b].integrationValue < lowestIntegration)
                    {
                        lowestIntegration = currentField[a, cell.y + 1, b].integrationValue;
                        lowestCell = currentField[a, cell.y + 1, b];
                    }

                }
            }

        }

        cell.flowValue = lowestCell.center - cell.center;
        cell.integrationValue = lowestIntegration + cell.flowValue.magnitude;
        cell.cellWithLowestIntegration = lowestCell;
        return cell.flowValue;
    }


    //private void PrepFieldForIteration(bool resetIntegration = true)
    //{
    //    integratedCells.Clear();
    //    for (int x = 0; x < cellsX; x++)
    //    {
    //        for (int y = 0; y < cellsY; y++)
    //        {
    //            for (int z = 0; z < cellsZ; z++)
    //            {
    //                if (flowFieldCells[x, y, z] != null)
    //                {
    //                    if (resetIntegration) flowFieldCells[x, y, z].integrationValue = float.PositiveInfinity;
    //                    flowFieldCells[x, y, z].queed = false;

    //                }
    //            }
    //        }
    //    }
    //}

    private void LoadFromFile()
    {
        FileStream fs = new FileStream("Assets\\JSON.txt", FileMode.Open);
        BinaryFormatter formatter = new BinaryFormatter();
        seri = (SeriField)formatter.Deserialize(fs);
        flowFieldCells = new FlowFieldCell[seri.x[0], seri.y[0], seri.z[0]];
        altFlowFieldCells = new FlowFieldCell[seri.x[0], seri.y[0], seri.z[0]];
        fieldOrigin = new Vector3(seri.originX, seri.originY, seri.originZ);
        GameObject.FindGameObjectWithTag("Visu").transform.position = fieldOrigin;
        fieldMax = fieldOrigin + new Vector3(seri.x[0] * FlowFieldCell.cellSize, seri.y[0] * FlowFieldCell.cellSize, seri.z[0] * FlowFieldCell.cellSize);

        cellsX = seri.x[0];
        cellsY = seri.y[0];
        cellsZ = seri.z[0];
        for (int i = 1; i < seri.x.Count; i++)
        {

            flowFieldCells[seri.x[i], seri.y[i], seri.z[i]] = new FlowFieldCell(fieldOrigin + new Vector3(seri.x[i] * FlowFieldCell.cellSize + FlowFieldCell.cellSize / 2, seri.y[i] * FlowFieldCell.cellSize + FlowFieldCell.cellSize / 2, seri.z[i] * FlowFieldCell.cellSize + FlowFieldCell.cellSize / 2), seri.x[i], seri.y[i], seri.z[i], seri.portalIndex[i - 1]);
            altFlowFieldCells[seri.x[i], seri.y[i], seri.z[i]] = new FlowFieldCell(fieldOrigin + new Vector3(seri.x[i] * FlowFieldCell.cellSize + FlowFieldCell.cellSize / 2, seri.y[i] * FlowFieldCell.cellSize + FlowFieldCell.cellSize / 2, seri.z[i] * FlowFieldCell.cellSize + FlowFieldCell.cellSize / 2), seri.x[i], seri.y[i], seri.z[i], seri.portalIndex[i - 1]);
        }

    }

    public void GenerateGraph(bool loadFromFile, TextAsset file)
    {
        dummyEmpty = GameObject.FindGameObjectWithTag("Dummy").GetComponent<MovementTest>();
        this.loadFromFile = loadFromFile;
        if (loadFromFile)
        {
            LoadFromFile();
            return;
        }
        navMesh.GetMaxCoordinates(out Vector3 min, out Vector3 max);
        Debug.Log("Min" + min);
        Debug.Log("Max: " + max);
        fieldOrigin = min;
        fieldMax = max;

        cellsX = Mathf.Max(Mathf.CeilToInt((max.x - min.x) / FlowFieldCell.cellSize + 1), 1);
        cellsY = Mathf.Max(Mathf.CeilToInt((max.y - min.y) / FlowFieldCell.cellSize + 1), 1);
        cellsZ = Mathf.Max(Mathf.CeilToInt((max.z - min.z) / FlowFieldCell.cellSize + 1), 1);
        flowFieldCells = new FlowFieldCell[cellsX, cellsY, cellsZ];

        if (!loadFromFile)
        {
            seri.x.Add(cellsX);
            seri.y.Add(cellsY);
            seri.z.Add(cellsZ);
            seri.originX = fieldOrigin.x;
            seri.originY = fieldOrigin.y;
            seri.originZ = fieldOrigin.z;
        }



        CleanGraph();

        if (!loadFromFile)
        {
            FileStream fs = new FileStream("Assets\\JSON.txt", FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(fs, seri);
            fs.Close();
        }
    }

    //public void DrawGraph(bool drawVectorOnly)
    //{
    //    for (int x = 0; x < cellsX; x++)
    //    {
    //        for (int y = 0; y < cellsY; y++)
    //        {
    //            for (int z = 0; z < cellsZ; z++)
    //            {
    //                if (flowFieldCells[x, y, z] != null)
    //                    flowFieldCells[x, y, z].DrawCell(drawVectorOnly);
    //            }

    //        }

    //    }
    //}

    private void CleanGraph()
    {
        int b = 1;
        int c = 2;

        Vector3 min = fieldOrigin;
        Vector3 max = fieldMax;


        for (int a = 0; a < navMesh.navMesh.indices.Length; a += 3)
        {
            colliderEmpty = GameObject.Find("ColliderEmpty");


            Vector3[] trianglePoints = { navMesh.navMesh.vertices[navMesh.navMesh.indices[a]], navMesh.navMesh.vertices[navMesh.navMesh.indices[b]], navMesh.navMesh.vertices[navMesh.navMesh.indices[c]] };




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
                        Vector3 newCenter = new Vector3(min.x + cellsize * x + cellsize / 2, min.y + cellsize * y + cellsize / 2, min.z + cellsize * z + cellsize / 2);
                        Vector3 unten = newCenter - new Vector3(cellsize / 2, cellsize / 2, cellsize / 2);
                        Vector3 oben = newCenter + new Vector3(cellsize / 2, cellsize / 2, cellsize / 2);
                        if (IntersectionTest.IsIntersecting(new IAABoxImpl(unten, oben), new TriangleImpl(trianglePoints[0], trianglePoints[1], trianglePoints[2])))
                        {
                            flowFieldCells[x, y, z] = new FlowFieldCell(newCenter, x, y, z);
                            if (!loadFromFile)
                            {
                                seri.x.Add(x);
                                seri.y.Add(y);
                                seri.z.Add(z);
                            }
                        }

                    }
                }
            }

            b += 3;
            c += 3;
        }

        CreatePortals();

    }

    //public void IntegrateCompleteField(Vector3 goal)
    //{
    //    Debug.Log("Integrating Complete Field!");
    //    PrepFieldForIteration();
    //    int[] goalIndices = new int[3];
    //    GetNearestCell(goal, out goalIndices[0], out goalIndices[1], out goalIndices[2]);
    //    flowFieldCells[goalIndices[0], goalIndices[1], goalIndices[2]].integrationValue = 0f;
    //    flowFieldCells[goalIndices[0], goalIndices[1], goalIndices[2]].queed = true;

    //    float currentIntegration = 0f;

    //    List<CellIndex> activeCells = new List<CellIndex>();
    //    AddNeighbours(goalIndices[0], goalIndices[1], goalIndices[2], activeCells);
    //    int counter = 0;
    //    int loops = GameObject.Find("Main Camera").GetComponent<SceneHandler>().loops;
    //    if (loops < 0) loops = int.MaxValue;

    //    while (activeCells.Count > 0 && counter < loops)
    //    {
    //        int cellCounter = 0;
    //        foreach (var cell in activeCells)
    //        {
    //            cellCounter++;
    //            CellIndex prevCell;
    //            flowFieldCells[cell.x, cell.y, cell.z].integrationValue = FindIntegrationValue(cell, out prevCell);
    //            flowFieldCells[cell.x, cell.y, cell.z].cellWithLowestIntegration = flowFieldCells[prevCell.x, prevCell.y, prevCell.z];

    //            integratedCells.Enqueue(flowFieldCells[cell.x, cell.y, cell.z]);
    //        }
    //        List<CellIndex> temp = activeCells;
    //        activeCells = new List<CellIndex>();
    //        foreach (var cell in temp)
    //        {
    //            AddNeighbours(cell.x, cell.y, cell.z, activeCells);
    //        }
    //        FlowFieldCell.maxIntegration = currentIntegration;
    //        counter++;
    //    }
    //    Debug.Log("Amount of Loop while integrating: " + counter);
    //    GenerateVectorField();
    //}

    //private float FindIntegrationValue(CellIndex ind, out CellIndex prevCell)
    //{
    //    int x = ind.x;
    //    int y = ind.y;
    //    int z = ind.z;
    //    List<CellIndex> cells = new List<CellIndex>();
    //    AddNeighbours(x, y, z, cells, false, false);
    //    float lowestIntegration = float.PositiveInfinity;
    //    prevCell = new CellIndex(0, 0, 0);

    //    foreach (var cell in cells)
    //    {
    //        float flow = flowFieldCells[cell.x, cell.y, cell.z].integrationValue + (flowFieldCells[cell.x, cell.y, cell.z].center - flowFieldCells[x, y, z].center).magnitude;
    //        if (flow < lowestIntegration)
    //        {
    //            lowestIntegration = flow;
    //            prevCell = cell;
    //        }
    //    }
    //    return lowestIntegration;
    //}

    private MovementTest UpdateAgents(FlowFieldCell goal)
    {
        float lowestDistance = float.PositiveInfinity;
        MovementTest returnAgent = null;
        foreach (var agent in agents)
        {
            if ((goal.center - agent.position.position).magnitude < lowestDistance)
            {
                returnAgent = agent;
                lowestDistance = (goal.center - agent.position.position).magnitude;
            }
        }
        return returnAgent;
    }

    private void GetPath(MovementTest agent, out Queue<FlowFieldCell> path, int fieldIndex)
    {
        SetCurrentField(fieldIndex);
        path = new Queue<FlowFieldCell>();
        FlowFieldCell current = currentField[agent.cell.x, agent.cell.y, agent.cell.z];
        if (current == null) return;
        while (current.integrationValue != 0)
        {
            path.Enqueue(current);
            current = current.cellWithLowestIntegration;
            if (current == null) break;
        }
        Debug.Log("Path Length is: " + path.Count);

    }

    public void DivergePath(Vector3 goal)
    {
        MovementTest agent = sceneHandler.agents.First();
        Queue<FlowFieldCell> path;
        int counter = 0;

        foreach (var mov in agent.group)
        {
            mov.fieldIndex = 1;
            counter++;
            if (counter > agent.group.Count / 2) mov.fieldIndex = 0;
        }

        GetPath(agent, out path, 0);


        SetCurrentField(1);
        counter = 0;
        FlowFieldCell blockCell = null;
        FlowFieldCell maxDistance = null;
        foreach (var cell in path.Reverse())
        {
            maxDistance = cell;
            if (counter++ == 30)
                blockCell = cell;
            if (counter == 50)
            {
                break;
            }
        }

        if (counter > 30 + blockRadius)
        {
            ClearLastBlock();
            lastblock = blockCell;
            altFlowFieldCells[blockCell.x, blockCell.y, blockCell.z].isBLocked = true;
            for (int x = blockCell.x - blockRadius; x < blockCell.x + blockRadius; x++)
            {
                for (int y = blockCell.y - blockRadius; y < blockCell.y + blockRadius; y++)
                {
                    for (int z = blockCell.z - blockRadius; z < blockCell.z + blockRadius; z++)
                    {
                        if (isInField(x, y, z))
                            altFlowFieldCells[x, y, z].isBLocked = true;

                    }
                }
            }
        }

        dummyEmpty.position.position = maxDistance.center;
        dummyEmpty.cell = maxDistance;
        IntegrateWithHeuristic(goal, new AStar(), 1, false, dummyEmpty);
    }

    private void ClearLastBlock()
    {
        if (lastblock == null) return;
        for (int x = lastblock.x - blockRadius; x < lastblock.x + blockRadius; x++)
        {
            for (int y = lastblock.y - blockRadius; y < lastblock.y + blockRadius; y++)
            {
                for (int z = lastblock.z - blockRadius; z < lastblock.z + blockRadius; z++)
                {
                    if (isInField(x, y, z))
                        altFlowFieldCells[x, y, z].isBLocked = false;

                }
            }
        }
    }

    private void AdvanceWavefront(FlowFieldCell cell, IHeuristic heuristic, bool portalMode)
    {

        for (int a = cell.x - 1; a <= cell.x + 1; a++)
        {
            for (int b = cell.z - 1; b <= cell.z + 1; b++)
            {
                if (isInField(a, cell.y - 1, b) && currentField[a, cell.y - 1, b].activeWaveFront != currentWaveFront && (cell.portalIndex == currentField[a, cell.y - 1, b].portalIndex || !portalMode) && !currentField[a, cell.y - 1, b].isBLocked)
                {
                    activeWavefront.Add(heuristic.CalculateHeuristic(currentField[a, cell.y - 1, b]) + cell.integrationValue, currentField[a, cell.y - 1, b]);
                    currentField[a, cell.y - 1, b].previousWaveFront = currentField[a, cell.y - 1, b].activeWaveFront;
                    currentField[a, cell.y - 1, b].activeWaveFront = currentWaveFront;
                    currentField[a, cell.y - 1, b].cellWithLowestIntegration = cell;
                    if (cell.portalIndex > 0)
                        cell.portalPath = cell.portalIndex;

                    currentField[a, cell.y - 1, b].portalPath = cell.portalPath;
                }

                if (isInField(a, cell.y, b) && currentField[a, cell.y, b].activeWaveFront != currentWaveFront && (cell.portalIndex == currentField[a, cell.y, b].portalIndex || !portalMode) && !currentField[a, cell.y, b].isBLocked)
                {
                    activeWavefront.Add(heuristic.CalculateHeuristic(currentField[a, cell.y, b]) + cell.integrationValue, currentField[a, cell.y, b]);
                    currentField[a, cell.y, b].previousWaveFront = currentField[a, cell.y, b].activeWaveFront;
                    currentField[a, cell.y, b].activeWaveFront = currentWaveFront;
                    currentField[a, cell.y, b].cellWithLowestIntegration = cell;

                    if (cell.portalIndex > 0)
                        cell.portalPath = cell.portalIndex;

                    currentField[a, cell.y, b].portalPath = cell.portalPath;
                }

                if (isInField(a, cell.y + 1, b) && currentField[a, cell.y + 1, b].activeWaveFront != currentWaveFront && (cell.portalIndex == currentField[a, cell.y + 1, b].portalIndex || !portalMode) && !currentField[a, cell.y + 1, b].isBLocked)
                {
                    activeWavefront.Add(heuristic.CalculateHeuristic(currentField[a, cell.y + 1, b]) + cell.integrationValue, currentField[a, cell.y + 1, b]);
                    currentField[a, cell.y + 1, b].previousWaveFront = currentField[a, cell.y + 1, b].activeWaveFront;
                    currentField[a, cell.y + 1, b].activeWaveFront = currentWaveFront;
                    currentField[a, cell.y + 1, b].cellWithLowestIntegration = cell;

                    if (cell.portalIndex > 0)
                        cell.portalPath = cell.portalIndex;

                    currentField[a, cell.y + 1, b].portalPath = cell.portalPath;
                }



            }

        }


    }


    /// <summary>
    /// Used to Move the Target
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="list"></param>
    /// <param name="setQueed">Use if this gets called muliple times to prevent duplicates.</param>
    /// <param name="forIntegration"></param>
    private void AddNeighbours(int x, int y, int z, List<CellIndex> list, bool setQueed = true, bool forIntegration = true)
    {

        for (int a = x - 1; a <= x + 1; a++)
        {
            for (int b = z - 1; b <= z + 1; b++)
            {
                if (0 <= a && a < cellsX && 0 <= y + 1 && y + 1 < cellsY && 0 <= b && b < cellsZ && flowFieldCells[a, y + 1, b] != null && (flowFieldCells[a, y + 1, b].integrationValue == float.PositiveInfinity || !forIntegration) && (!flowFieldCells[a, y + 1, b].queed || !forIntegration))
                {
                    list.Add(new CellIndex(a, y + 1, b));
                    flowFieldCells[a, y + 1, b].queed = setQueed;
                }

                if (0 <= a && a < cellsX && 0 <= y && y < cellsY && 0 <= b && b < cellsZ && flowFieldCells[a, y, b] != null && !(a == x && b == z) && (flowFieldCells[a, y, b].integrationValue == float.PositiveInfinity || !forIntegration) && (!flowFieldCells[a, y, b].queed || !forIntegration))
                {
                    list.Add(new CellIndex(a, y, b));
                    flowFieldCells[a, y, b].queed = setQueed;
                }

                if (0 <= a && a < cellsX && 0 <= y - 1 && y - 1 < cellsY && 0 <= b && b < cellsZ && flowFieldCells[a, y - 1, b] != null && (flowFieldCells[a, y - 1, b].integrationValue == float.PositiveInfinity || !forIntegration) && (!flowFieldCells[a, y - 1, b].queed || !forIntegration))
                {
                    list.Add(new CellIndex(a, y - 1, b));
                    flowFieldCells[a, y - 1, b].queed = setQueed;
                }

            }
        }
    }

    public FlowFieldCell GetNearestCell(Vector3 pos, int fieldIndex)
    {
        int x = 0, y = 0, z = 0;
        SetCurrentField(fieldIndex);
        GetNearestCell(pos, out x, out y, out z, fieldIndex);
        return currentField[x, y, z];
    }
    private void SetCurrentField(int index)
    {
        if (index == 0) currentField = flowFieldCells;
        else currentField = altFlowFieldCells;
    }

    private void GetNearestCell(Vector3 pos, out int x, out int y, out int z, int fieldIndex)
    {
        SetCurrentField(fieldIndex);
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
            y = cellsY - 1;
        else
            y = Mathf.FloorToInt(posScaled.y);

        if (pos.z < fieldOrigin.z)
            z = 0;
        else if (pos.z > fieldOrigin.z + cellsZ * cellSize)
            z = cellsZ - 1;
        else
            z = Mathf.FloorToInt(posScaled.z);

        if (currentField[x, y, z] != null) return;
        while (y > 0 && currentField[x, y, z] == null) y--;
        if (currentField[x, y, z] != null) return;
        while (y < cellsY - 1 && currentField[x, y, z] == null) y++;

    }
    private bool isInField(Vector3 pos)
    {
        var cell = GetNearestCell(pos, 0);
        if (cell == null) return false;
        return isInField(cell.x, cell.y, cell.z);
    }
    private bool isInField(int x, int y, int z, bool fieldIsInitalized = true)
    {
        return x >= 0 && x < cellsX && y >= 0 && y < cellsY && z >= 0 && z < cellsZ && (currentField[x, y, z] != null || !fieldIsInitalized);
    }

    private void CreatePortals()
    {
        GameObject[] portals = GameObject.FindGameObjectsWithTag("Portal");
        int portalIndex = 0;
        foreach (GameObject p in portals)
        {
            portalIndex++;
            Vector3 pos = p.transform.position;
            Vector3 min = p.GetComponent<BoxCollider>().bounds.min;
            Vector3 max = p.GetComponent<BoxCollider>().bounds.max;

            if (!isInField(min) || !isInField(max)) continue;
            FlowFieldCell minCell = GetNearestCell(min, 0);
            FlowFieldCell maxCell = GetNearestCell(max, 0);

            for (int x = minCell.x; x <= maxCell.x; x++)
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    for (int z = minCell.z; z <= maxCell.z; z++)
                    {
                        if (!isInField(x, y, z)) continue;
                        flowFieldCells[x, y, z].portalIndex = portalIndex;
                    }
                }
            }

            for (int i = 1; i < seri.x.Count; i++)
            {
                if (isInField(seri.x[i], seri.y[i], seri.z[i]))
                    seri.portalIndex.Add(flowFieldCells[seri.x[i], seri.y[i], seri.z[i]].portalIndex);
            }
        }


    }
    /// <summary>
    /// USed to Move the Target
    /// </summary>
    /// <param name="ind"></param>
    /// <returns></returns>
    public FlowFieldCell GetRandomNeighbour(CellIndex ind)
    {
        bool[,,] neighbours = new bool[3, 3, 3];
        int x = ind.x;
        int y = ind.y;
        int z = ind.z;
        List<CellIndex> cells = new List<CellIndex>();
        AddNeighbours(ind.x, ind.y, ind.z, cells, false, false);


        int rand = UnityEngine.Random.Range(0, cells.Count);
        return flowFieldCells[cells[rand].x, cells[rand].y, cells[rand].z];

    }

    public void GenerateVectorField(Queue<FlowFieldCell> integratedCells, bool draw = false)
    {
        Debug.Log("Generating Vector Field for " + integratedCells.Count + " cells!");
        foreach (var cell in integratedCells)
        {
            cell.flowValue = currentField[cell.cellWithLowestIntegration.x, cell.cellWithLowestIntegration.y, cell.cellWithLowestIntegration.z].center - cell.center;
            if (draw) cell.DrawCell(sceneHandler.DrawVectorOnly);
        }
        return;

    }
    //private int FindHighestIntegration(List<CellIndex> list)
    //{
    //    int index = 0;
    //    float lowest = float.MaxValue;
    //    for (int i = 0; i < list.Count; i++)
    //    {
    //        if (flowFieldCells[list[i].x, list[i].y, list[i].z].integrationValue < lowest)
    //        {
    //            lowest = flowFieldCells[list[i].x, list[i].y, list[i].z].integrationValue;
    //            index = i;
    //        }
    //    }
    //    return index;
    //}


}

[Serializable]
public class SeriField
{
    public float originX;
    public float originY;
    public float originZ;
    public List<int> x = new List<int>();
    public List<int> y = new List<int>();
    public List<int> z = new List<int>();
    public List<int> portalIndex = new List<int>();
}

