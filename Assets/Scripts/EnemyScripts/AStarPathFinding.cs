using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AStarPathFinding : MonoBehaviour 
{
    private Transform startPosition;
    private Transform endPosition;

    public Node StartNode { get; set; }
    public Node GoalNode { get; set; }

    public List<Node> pathArray = new();
    
    private GridManager gridManager;
    
	private void Start () 
    {
        gridManager = FindObjectOfType<GridManager>();
	}

    public void FindPath(Transform endPos)
    {
        startPosition = transform;
        endPosition = endPos;
        
        StartNode = new Node(gridManager.GetGridCellCenter(gridManager.GetGridIndex(startPosition.position)));

        int indexCell = gridManager.GetGridIndex(endPosition.position);
        int column = gridManager.GetColumnOfIndex(indexCell);
        int row = gridManager.GetRowOfIndex(indexCell);
        GoalNode = gridManager.Nodes[row, column];

        if (GoalNode.bObstacle)
        {
            List<Node> neighbors = new();
            Node closestNode = new();
            float shortestDistance = 1000000;
            gridManager.GetNeighbors(GoalNode, neighbors);
            for (int i = 0; i < neighbors.Count; i++)
            {
                Node node = neighbors[i];
                if (!node.bObstacle)
                {
                    float dist = Vector3.Distance(endPos.position, node.position);
                    if (dist < shortestDistance)
                    {
                        closestNode = node;
                        shortestDistance = dist;
                    }
                }
            }

            GoalNode = closestNode;
        }

        pathArray = AStar.FindPath(StartNode, GoalNode);
    }

    private void OnDrawGizmos()
    {
        if (pathArray.Count <= 0 || !gridManager.showPathLines) 
        {
            return;
        }

        if (pathArray.Count > 0)
        {
            int index = 1;
            foreach (Node node in pathArray)
            {
                if (index < pathArray.Count)
                {
                    Node nextNode = pathArray[index];
                    Debug.DrawLine(node.position, nextNode.position, Color.green);
                    index++;
                }
            };
        }
    }
}