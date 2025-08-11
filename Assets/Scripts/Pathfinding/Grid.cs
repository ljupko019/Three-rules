using System;
using UnityEngine;

public class Grid <TGridObject>
{
    public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
    public class OnGridValueChangedEventArgs : EventArgs 
    {
        public int x;
        public int y;
    }

    private int width;
    private int height;
    private int cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;


    public Grid(int width, int height, int cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject>createGridObject) 
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];


        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                Debug.DrawLine(GetWorldPostion(x, y), GetWorldPostion(x + 1, y), Color.white, 100);
                Debug.DrawLine(GetWorldPostion(x, y), GetWorldPostion(x, y + 1), Color.white, 100);

            }
        }
        Debug.DrawLine(GetWorldPostion(0, height), GetWorldPostion(width, height), Color.white, 100);
        Debug.DrawLine(GetWorldPostion(width, 0), GetWorldPostion(width, height), Color.white, 100);
    }

    private void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            if (OnGridValueChanged != null) 
                OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }
    }

    public void TriggerGridObjectChanged(int x, int y) 
    {
        if (OnGridValueChanged != null)
            OnGridValueChanged(this, new OnGridValueChangedEventArgs { x = x, y = y });
    }
    public void SetGridObject(Vector3 worldPosition, TGridObject value) 
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }

    private Vector3 GetWorldPostion(int x, int y) 
    {
        return new Vector3(x,y) * cellSize + originPosition;
    }


    public TGridObject GetGridObject(int x, int y) 
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPostion) 
    {
        int x, y;
        GetXY(worldPostion, out x, out y);
        return GetGridObject(x, y);
    }
    public void GetXY(Vector3 worldPosition, out int x, out int y) 
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize); 
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize); 
    }

    public int GetWidth() 
    {
        return width;
    }

    public int GetCellSize() 
    {
        return cellSize;
    }

    public int GetHeight()
    {
        return height;
    }

    public Vector3 GetOriginPosition() 
    {
        return originPosition;
    }

}
