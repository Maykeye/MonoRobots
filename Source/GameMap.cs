using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Text;
using System.Linq;
[assembly: InternalsVisibleTo("MonoRobots.Tests")]
namespace MonoRobots;

public abstract record PlayerTurn
{
    public sealed record DoNothing : PlayerTurn;
    public sealed record Move(Point Delta) : PlayerTurn;
    public sealed record Teleport() : PlayerTurn;
}

public enum StepResult
{
    Incorrect,
    PlayerAlive,
    PlayerDied
}


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
    internal Point size;
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

    public StepResult Step(PlayerTurn turn)
    {
        var player = GetPlayer();
        var newMap = new Dictionary<Point, List<Cell>>();

        // Copy unmoveable garbo
        foreach (var garbo in map.Values
                .SelectMany(x => x)
                .Where(cell => cell.Kind == CellKind.Garbage))
        {
            var garboCells = newMap.GetValueOrDefault(garbo.Position, []);
            garboCells.Add(garbo);
            newMap[garbo.Position] = garboCells;
        }

        // Move robots
        foreach (var robot in map.Values
                .SelectMany(x => x)
                .Where(cell => cell.Kind == CellKind.Robot))
        {
            var newCell = robot;
            var delta = player.Position - robot.Position;
            delta.X = Math.Clamp(delta.X, -1, 1);
            delta.Y = Math.Clamp(delta.Y, -1, 1);
            newCell.Position += delta;
            // Garbo or other robot exists

            var lst = newMap.GetValueOrDefault(newCell.Position, []);
            if (lst.Count != 0)
            {
                // Occupied => rewrite
                newCell.Alive = false;
                newCell.Kind = CellKind.Garbage;
            }
            newMap[newCell.Position] = [newCell];
        }

        // Move player
        player.Alive = !newMap.ContainsKey(player.Position);
        var newPlyCells = newMap.GetValueOrDefault(player.Position, []);
        newPlyCells.Add(player);
        newMap[player.Position] = newPlyCells;
        map = newMap;
        return StepResult.PlayerAlive;
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
            sb.Append("{");
            sb.Append(ent.Key.ToString());
            sb.Append(": ");
            bool first = true;
            foreach (var cell in ent.Value)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                sb.Append(cell.ToString());
            }
            var x = ent.Value.SelectMany(x => x.ToString());
            sb.Append("}; ");
        }
        sb.Remove(sb.Length - 2, 2);
        return sb.ToString();
    }

    public Cell GetPlayer()
    {
        return map.Values
            .SelectMany(x => x)
            .Where(cell => cell.Kind == CellKind.Player)
            .Cast<Cell?>()
            .FirstOrDefault() ?? throw new Exception("Player not found");
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

