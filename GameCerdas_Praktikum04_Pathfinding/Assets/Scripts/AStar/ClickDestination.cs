using UnityEngine;

public class ClickDestination : MonoBehaviour
{
    public AStarPathfinder pathfinder;
    public AgentPathFollower agent;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane gridPlane = new Plane(Vector3.up, pathfinder.gridManager.transform.position);

            if (gridPlane.Raycast(ray, out float distance))
            {
                Vector3 clickPosition = ray.GetPoint(distance);
                GridNode clickedNode = pathfinder.gridManager.NodeFromWorldPosition(clickPosition);

                if (clickedNode != null && clickedNode.walkable)
                {
                    pathfinder.goalMarker.position = clickedNode.worldPosition; //[cite: 2]
                    pathfinder.FindPath(); //[cite: 2]
                    
                    if (agent != null)
                    {
                        agent.RefreshPath(); 
                    }
                }
            }
        }
    }
}