using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
[assembly: InternalsVisibleTo("MonoRobots.Tests")]
namespace MonoRobots;


// TODO: remove me

public enum CellKind
{
    Player,
    Robot,
    Garbage
}

public struct Cell
{
    public Point Position;
    public CellKind Kind;
    public bool Alive;
    public Cell()
    {
        Alive = Kind != CellKind.Garbage;
    }
    public readonly override string ToString()
        => $"(Cell Position:{Position}, Kind:{Kind}, Alive:{Alive})";
}

public class GameMap
{
    internal List<Cell> map;
    Point size;
    readonly Random rng;



    public GameMap(int width = 32, int height = 32, Random rng = null)
    {
        size = new Point(width, height);
        this.rng = rng ?? Random.Shared;
        Reset(width);
    }

    public void Reset(int robots)
    {
        robots = int.Max(robots, 0);
        robots = int.Min(robots, size.X * size.Y / 2);
        map = [];

        var playerPos = size / 2;
        Spawn(playerPos, CellKind.Player);

        for (int i = 0; i < robots; i++)
        {
            while (!Spawn(RandomPos(), CellKind.Robot))
            {
            }
        }
    }

    public void Step()
    {
        var newMap = new List<Cell>(map.Count);
        var playerPos = GetPlayer().Position;

        foreach (var cell in map)
        {
            var delta = playerPos - cell.Position;
            delta.X = Math.Sign(delta.X);
            delta.Y = Math.Sign(delta.Y);
            var newCell = cell;
            newCell.Position += delta;
            newMap.Add(newCell);
        }
        map = newMap;
    }

    private Point RandomPos()
    {
        return new Point(rng.Next(size.X), rng.Next(size.Y));
    }

    public bool GetAt(Point pos, ICollection<Cell> cells)
    {
        bool res = false;
        foreach (var cell in map.Where(cell => cell.Position == pos))
        {
            cells.Add(cell);
            res = true;
        }
        return res;
    }

    internal bool Spawn(Point point, CellKind kind)
    {
        if (GetAt(point, []))
        {
            return false;
        }

        var cell = new Cell
        {
            Position = point,
            Kind = kind
        };
        map.Add(cell);
        return true;
    }

    internal string MapDump()
    {
        StringBuilder sb = new("(");
        foreach (var ent in map)
        {
            sb.Append(ent.ToString());
            sb.Append(", ");
        }
        sb.Remove(sb.Length - 2, 2);
        return sb.ToString();
    }

    public bool SetPlayerPosition(int x, int y)
    {
        for (int i = 0; i < map.Count; i++)
        {
            if (map[i].Kind == CellKind.Player)
            {
                var ply = map[i];
                ply.Position = new Point(x, y);
                map[i] = ply;
                return true;
            }
        }
        return false;
    }
    public Cell GetPlayer()
    {
        return map.FirstOrDefault(
            ent => ent.Kind == CellKind.Player,
            new Cell { Kind = CellKind.Player }
        );
    }
}

