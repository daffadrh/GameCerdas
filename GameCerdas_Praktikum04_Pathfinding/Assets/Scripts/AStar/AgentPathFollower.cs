using System.Collections.Generic;
using UnityEngine;

public class AgentPathFollower : MonoBehaviour
{
    public AStarPathfinder pathfinder; //[cite: 1]

    public float moveSpeed = 2f; //[cite: 1]
    public float rotationSpeed = 8f; //[cite: 1]
    public float waypointThreshold = 0.1f; //[cite: 1]

    private List<GridNode> path; //[cite: 1]
    private int currentIndex; //[cite: 1]

    private void Start()
    {
        RefreshPath();
    }

    public void RefreshPath()
    {
        if (pathfinder == null) //[cite: 1]
        {
            return; //[cite: 1]
        }

        path = pathfinder.currentPath; //[cite: 1]
        currentIndex = 0; //[cite: 1]
    }

    private void Update()
    {
        if (path == null || //[cite: 1]
            path.Count == 0 || //[cite: 1]
            currentIndex >= path.Count) //[cite: 1]
        {
            return; //[cite: 1]
        }

        Vector3 target = //[cite: 1]
            path[currentIndex].worldPosition; //[cite: 1]

        target.y = transform.position.y; //[cite: 1]

        Vector3 direction = //[cite: 1]
            target - transform.position; //[cite: 1]

        if (direction.magnitude <= //[cite: 1]
            waypointThreshold) //[cite: 1]
        {
            currentIndex++; //[cite: 1]
            return; //[cite: 1]
        }

        Vector3 moveDirection = //[cite: 1]
            direction.normalized; //[cite: 1]

        transform.position = //[cite: 1]
            Vector3.MoveTowards( //[cite: 1]
                transform.position, //[cite: 1]
                target, //[cite: 1]
                moveSpeed * Time.deltaTime //[cite: 1]
            );

        if (moveDirection.sqrMagnitude > //[cite: 1]
            0.001f) //[cite: 1]
        {
            Quaternion targetRotation = //[cite: 1]
                Quaternion.LookRotation( //[cite: 1]
                    moveDirection, //[cite: 1]
                    Vector3.up //[cite: 1]
                );

            transform.rotation = //[cite: 1]
                Quaternion.Slerp( //[cite: 1]
                    transform.rotation, //[cite: 1]
                    targetRotation, //[cite: 1]
                    rotationSpeed * //[cite: 1]
                    Time.deltaTime //[cite: 1]
                );
        }
    }
}