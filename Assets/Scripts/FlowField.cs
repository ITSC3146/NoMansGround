using System.Collections.Generic;
using UnityEngine;

public class FlowField
{
    private readonly GridData grid;
    private readonly int[,] dist;
    private readonly Vector2Int[,] flow;

    private static readonly Vector2Int[] Dirs =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    public int GetDistance(Vector2Int cell) => dist[cell.x, cell.y];
    public FlowField(GridData grid)
    {
        this.grid = grid;
        dist = new int[grid.width, grid.height];
        flow = new Vector2Int[grid.width, grid.height];
    }

    public void Rebuild(Vector2Int goal)
    {
        for (int x = 0; x < grid.width; x++)
            for (int y = 0; y < grid.height; y++)
                dist[x, y] = -1;

        var queue = new Queue<Vector2Int>();
        dist[goal.x, goal.y] = 0;
        queue.Enqueue(goal);

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            foreach (var d in Dirs)
            {
                var next = cur + d;
                if (!grid.InBounds(next) || grid.IsBlocked(next) || dist[next.x, next.y] != -1)
                    continue;

                dist[next.x, next.y] = dist[cur.x, cur.y] + 1;
                queue.Enqueue(next);
            }
        }

        for (int x = 0; x < grid.width; x++)
            for (int y = 0; y < grid.height; y++)
            {
                flow[x, y] = Vector2Int.zero;
                if (dist[x, y] <= 0) continue;

                var cell = new Vector2Int(x, y);
                int best = dist[x, y];
                foreach (var d in Dirs)
                {
                    var n = cell + d;
                    if (grid.InBounds(n) && dist[n.x, n.y] != -1 && dist[n.x, n.y] < best)
                    {
                        best = dist[n.x, n.y];
                        flow[x, y] = d;
                    }
                }
            }
    }

    public bool IsReachable(Vector2Int cell) => dist[cell.x, cell.y] != -1;
    public Vector2Int GetDirection(Vector2Int cell) => flow[cell.x, cell.y];
}