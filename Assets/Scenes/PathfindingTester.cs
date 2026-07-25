using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathfindingTester : MonoBehaviour
{
    private NavMeshAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) == false) return;
        Debug.Log("MouseButtonDown");
        RaycastHit hit;
        Ray rayOrigin = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(rayOrigin, out hit);
        agent.destination = hit.point;
    }

    private void OnMouseDown()
    {

    }
}
