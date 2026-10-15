using UnityEngine;

public enum CellType { Empty, Tower, Wall, Blocked }

public class GridData
{
    public readonly int width;
    public readonly int height;
    private readonly CellType[,] cells;

    public GridData(int width, int height)
    {
        this.width = width;
        this.height = height;
        cells = new CellType[width, height];
    }

    public bool InBounds(Vector2Int c) =>
        c.x >= 0 && c.x < width && c.y >= 0 && c.y < height;

    public bool IsBlocked(Vector2Int c) => cells[c.x, c.y] != CellType.Empty;

    public CellType GetCell(Vector2Int c) => cells[c.x, c.y];
    public void SetCell(Vector2Int c, CellType type) => cells[c.x, c.y] = type;
}