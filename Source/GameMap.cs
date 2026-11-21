using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Text;
[assembly: InternalsVisibleTo("MonoRobots.Tests")]
namespace MonoRobots;

public abstract record PlayerTurn;
public record Player_DoNothing : PlayerTurn;
public record Player_Move(Point Delta) : PlayerTurn;
public record Player_Teleport() : PlayerTurn;


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
    internal Dictionary<Point, List<Cell>> map;
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
    }

    private Point RandomPos()
    {
        return new Point(rng.Next(size.X), rng.Next(size.Y));
    }

    public bool GetAt(Point pos, ICollection<Cell> cells)
    {
        bool res = false;
        foreach (var cell in map.GetValueOrDefault(pos, []))
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

        var occupants = map.GetValueOrDefault(point, []);
        occupants.Add(cell);
        map[point] = occupants;
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

    public void SetPlayerPosition(int x, int y)
    {
        foreach (var (pos, ocs) in map)
        {
            if (ocs.Count == 1)
            {
                map.Remove(pos);
                break;
            }
        }
        var ply = new Cell
        {
            Position = new(x, y),
            Kind = CellKind.Player
        };
        var plyCellOcs = map.GetValueOrDefault(ply.Position, []);
        plyCellOcs.Add(ply);
        map[ply.Position] = plyCellOcs;
    }
}

