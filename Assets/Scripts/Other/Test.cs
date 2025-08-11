using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Test : MonoBehaviour
{
    Grid grid;
    Pathfinding pathfinding;
    void Start()
    {
        //pathfinding = new Pathfinding(10, 10);
        //grid = new Grid(5, 2, 1, new Vector3(4, 0));
    }

    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Vector3 mouseWorldPosition = GetMousePostion();
    //        pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
    //        Debug.Log("x" + x + "y " + y);
    //        List<Node> path = pathfinding.FindPath(0, 0, x, y);
    //        if (path != null) 
    //        {
    //            for (int i = 0; i < path.Count; i++)
    //            {
    //                Debug.Log(path[i]);
    //               // Debug.DrawLine(new Vector3(path[i].x, path[i].y) + Vector3.one * 5, new Vector3(path[i + 1].x, path[i + 1].y) + Vector3.one * 5, Color.white, 100);
    //            }
    //        }
    //    }
    //    if (Input.GetMouseButtonDown(1))
    //    {
    //        Vector3 mouseWorldPosition = GetMousePostion();
    //        pathfinding.GetGrid().GetXY(mouseWorldPosition, out int x, out int y);
    //        pathfinding.GetNode(x, y).SetIsWalkable(false);
    //    }
    //}

    private Vector3 GetMousePostion()
    {
        Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        vector.z = 0f;
        return vector;
    }

}
