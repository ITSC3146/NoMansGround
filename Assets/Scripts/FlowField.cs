using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

public class FlowField
{
    private readonly GridData grid;
    private readonly int[,] dist;

    private readonly Vector2Int[,] flow;

    private static readonly Vector2Int[] Dirs =
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, new Vector2Int(-1,-1), new Vector2Int(-1,1), new Vector2Int(1,1), new Vector2Int(1,-1) 
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

/**


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
*/
        var open = new SortedSet<(int dist, int x, int y)>();
        dist[goal.x, goal.y] = 0;
        open.Add((0, goal.x, goal.y));
        while (open.Count > 0)
        {
            var top = open.Min;
            open.Remove(top);

            if (top.dist > dist[top.x, top.y])
                continue;
            var cur = new Vector2Int(top.x, top.y);
            foreach(var d in Dirs)
            {
                var next = cur + d;  
                if (!CanMove(cur, d)) continue;
                //Offset Calculation
                

                Vector2Int dir = next - goal;  
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                float snapped = Mathf.Round(angle/90f) * 90f;
                float diff = Mathf.Abs(angle - snapped);
                int anglePenalty = 0;
                if(Mathf.Abs(angle - snapped) > 15f)
                {
                    anglePenalty = 1; 
                }

                
                
                int newDist = dist[cur.x, cur.y] + grid.GetCost(next) + anglePenalty;
                
                if (dist[next.x, next.y] != -1 && dist[next.x, next.y] <= newDist) continue;

                dist[next.x, next.y] = newDist;
                open.Add((newDist, next.x, next.y));
            }
        }

        //This BFS makes a grid essentially and searches through each "Square" in the grid
        for (int x = 0; x < grid.width; x++)
            for (int y = 0; y < grid.height; y++)
            {   
                //The initial Coordinate point is set to contain a Vector2Int of Zero so that it has an intial value for us to change
                flow[x, y] = Vector2Int.zero;
                //If we have have a variable that is outside the grid, skip it.
                if (dist[x, y] <= 0) continue;

                //Makes this coordinate into a cell
                var cell = new Vector2Int(x, y);

                //The Best in this case is set to the initial value we have in the dist[x,y] cell. This will be the distance this individual cell is from the goal
                int best = dist[x, y];
                //Think of it like a clock. We are going to check every neighbor we have and see what distance we get from it.

                foreach (var d in Dirs)
                {
                    var n = cell + d;
                //We will compare this with the distance we get from the next neighbor by combining our cells distance and the neighbors distance.
                //Whichever is lower we will take and move in that direction.
                    if (CanMove(cell, d) && dist[n.x, n.y] != -1 && dist[n.x, n.y] < best)
                    {
                        best = dist[n.x, n.y];
                        //This is then stored into our array flow to give the arrow it should get. If we need the value we use the getDirection function.
                        flow[x, y] = d;
                    }
                }
            }
    }

    public bool IsReachable(Vector2Int cell) => dist[cell.x, cell.y] != -1;

    //Will grab the direction of the grid square.
    public Vector2Int GetDirection(Vector2Int cell) => flow[cell.x, cell.y];

    private bool CanMove(Vector2Int from, Vector2Int d)
    {
        var to = from + d;

        if(!grid.InBounds(to) || grid.IsBlocked(to)) return false;
        
        bool diagonal = d.x != 0 && d.y != 0;;
        if (diagonal && 
        (grid.IsBlocked(from + new Vector2Int(d.x, 0)) ||
         grid.IsBlocked(from + new Vector2Int(0, d.y)))) return false;

         return true;
    }
}
