using UnityEngine;

public class Node
{
    private Grid<Node> grid;
    public int x;
    public int y;

    public int gCost;
    public int hCost;
    public int fCost;

    public bool isWalkable;
    public Node cameFromNode;

    public Node(Grid<Node> grid, int x, int y) 
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        isWalkable = true;
    }

    public void SetIsWalkable(bool setWalkable) 
    {
        isWalkable = setWalkable;
    }
    public void CalculateFCost() 
    {
        fCost = gCost + hCost;
    }
    public override string ToString()
    {
        return x + "," + y;
    }
}
