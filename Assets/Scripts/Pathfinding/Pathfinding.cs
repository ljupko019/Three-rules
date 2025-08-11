using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour { 

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public static Pathfinding Instance { get; private set; }
    Grid<Node> grid;
    private List<Node> openList;
    private List<Node> closeList;

    [SerializeField] private int width = 25;
    [SerializeField] private int height = 14;
    [SerializeField] private int cellSize = 1;
    [SerializeField] private Vector3 originPosition = new Vector3(-4, -2);

    [SerializeField] private LayerMask nodeColliderLayer;

    private void Awake()
    {
        Instance = this;
        grid = new Grid<Node>(width, height, cellSize, originPosition,
            (Grid<Node> grid, int x, int y) => new Node(grid, x, y));

        SetUnwalkableNodes();
    }

    private void SetUnwalkableNodes() 
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                Node node = grid.GetGridObject(x, y);

                Vector3 worldPos = new Vector3(x, y) * grid.GetCellSize() + originPosition + Vector3.one * grid.GetCellSize() * 0.5f;

                Collider2D collider = Physics2D.OverlapPoint(worldPos, nodeColliderLayer);
                if (collider != null)
                {
                    node.SetIsWalkable(false);
                }
                else
                {
                    node.SetIsWalkable(true);
                }
            }
        }
    }
    public List<Vector3> FindPath(Vector3 startWorldPosition, Vector3 endWorldPosition) 
    {
        grid.GetXY(startWorldPosition, out int startX, out int startY);
        grid.GetXY(endWorldPosition, out int endX, out int endY);
        
        List<Node> path = FindPath(startX, startY, endX, endY);

        if (path == null)
        {
            return null;
        }
        else
        {
            List<Vector3> vectorPath = new List<Vector3>();
            foreach (Node node in path) 
            {
                vectorPath.Add(new Vector3(node.x, node.y) * grid.GetCellSize() + Vector3.one * grid.GetCellSize() * 0.5f);
            }
            return vectorPath;
        }
    }

    public List<Node> FindPath(int startX, int startY, int endX, int endY)
    {
        Node startNode = grid.GetGridObject(startX, startY);
        Node endNode = grid.GetGridObject(endX, endY);
        openList = new List<Node> { startNode};
        closeList = new List<Node>();

        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                Node node = grid.GetGridObject(x, y);
                node.gCost =  int.MaxValue;
                node.CalculateFCost();
                node.cameFromNode = null;
            }
        }
        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0) 
        {
            Node currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
            {
                return CalculatePath(endNode);
            }

            openList.Remove(currentNode);
            closeList.Add(currentNode);

            foreach (Node neighbourNode in GetNeighbourList(currentNode)) 
            {
                if (closeList.Contains(neighbourNode))
                    continue;
                if (!neighbourNode.isWalkable) 
                {
                    closeList.Add(neighbourNode);
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost) 
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode)) 
                    {
                        openList.Add(neighbourNode);
                    }
                }
            }
        }
        //nema vise nodova u openList
        return null;
    }

    private List<Node> GetNeighbourList(Node currentNode)
    {
        List<Node> neighbourList = new List<Node>();

        //levo
        if (currentNode.x - 1 >= 0)
        {
            neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y));

            //levo dole
            if (currentNode.y - 1 >= 0)
            {
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y - 1));
            }
            //levo gore
            if (currentNode.y + 1 < grid.GetHeight())
            {
                neighbourList.Add(GetNode(currentNode.x - 1, currentNode.y + 1));
            }
        }

        //desno
        if (currentNode.x + 1 < grid.GetWidth())
        {
            neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y));

            //desno dole
            if (currentNode.y - 1 >= 0)
            {
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y - 1));
            }
            //desno gore
            if (currentNode.y + 1 < grid.GetHeight())
            {
                neighbourList.Add(GetNode(currentNode.x + 1, currentNode.y + 1));
            }
        }

        //dole
        if (currentNode.y - 1 >= 0)
        {
            neighbourList.Add(GetNode(currentNode.x, currentNode.y - 1));
        }

        //gore
        if (currentNode.y + 1 < grid.GetHeight())
        {
            neighbourList.Add(GetNode(currentNode.x, currentNode.y + 1));
        }

        return neighbourList;
    }

    public Node GetNode(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    private List<Node> CalculatePath(Node endNode) 
    {
        List<Node> path = new List<Node>();
        path.Add(endNode);
        Node currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse();
        return path;
    }

    private int CalculateDistanceCost(Node a, Node b) 
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    private Node GetLowestFCostNode(List<Node> nodeList) 
    {
        Node lowestFCostNode = nodeList[0];

        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].fCost < lowestFCostNode.fCost) 
            {
                lowestFCostNode = nodeList[i];
            }
        }

        return lowestFCostNode;
    }

    public Grid<Node> GetGrid() 
    {
        return grid;
    }

    //private void OnDrawGizmos()
    //{
    //    if (grid == null) return;

    //    for (int x = 0; x < grid.GetWidth(); x++)
    //    {
    //        for (int y = 0; y < grid.GetHeight(); y++)
    //        {
    //            Node node = grid.GetGridObject(x, y);
    //            Vector3 nodeWorldPos = new Vector3(x, y) * grid.GetCellSize() + originPosition + Vector3.one * grid.GetCellSize() * 0.5f;

    //            Gizmos.color = node.isWalkable ? Color.green : Color.red;
    //            Gizmos.DrawCube(nodeWorldPos, Vector3.one * (grid.GetCellSize() * 0.9f));
    //        }
    //    }
    //}
}


