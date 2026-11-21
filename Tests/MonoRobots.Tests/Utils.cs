using Shouldly;
using Xunit;
using Microsoft.Xna.Framework;

namespace MonoRobots.Tests;

public enum Pt
{
    HOME,
    N, W, S, E,
    NW, NE, SE, SW
}


public static class GameMapExtension
{
    public static Cell GetPlayer(this GameMap gameMap)
    {
        var plys = gameMap.FindKind(CellKind.Player);
        plys.Count.ShouldBe(1, "Exactly one player should exist");
        return plys[0];
    }

    public static List<Cell> FindKind(this GameMap gameMap, CellKind kind)
    {
        var foundCells = gameMap.map.Values
            .SelectMany(val => val) // flatten [[a,b,c], [d,e,f]] => [a,b,c,d,e,f]
            .Where(cell => cell.Kind == kind)
            .ToList();

        return foundCells;
    }

    public static void ValidateKindByPos(this GameMap gameMap, CellKind kind, params int[] xys)
    {
        (xys.Length % 2).ShouldBe(0, "Internal error: pass XYs pairs");
        for (int i = 0; i < xys.Length; i += 2)
        {
            var xy = new Point(xys[i], xys[i + 1]);
            var entCount = gameMap.map.GetValueOrDefault(xy, [])
                .Count(cell => cell.Kind == kind);
            entCount.ShouldBe(1, $"At {xy} expected 1 {kind}, not {entCount}");
        }
        gameMap.Count(kind).ShouldBe(xys.Length / 2, $"{gameMap.MapDump()}");
    }

    /// <summary>
    /// Count number of entitied of a given kind
    /// </summary>
    public static int Count(this GameMap gameMap, CellKind kind)
    {
        return gameMap.FindKind(kind).Count;
    }


    /// <summary>
    /// Validate robots and garbage by their xy-positions on the map
    /// </summary>
    public static void ValidateRbtGarbo(this GameMap gameMap, int[] robs, int[] garbo)
    {
        gameMap.ValidateKindByPos(CellKind.Robot, robs ?? []);
        gameMap.ValidateKindByPos(CellKind.Garbage, garbo ?? []);
    }


    /// <summary>
    /// GameMap has map: dict[Pos, Cell], where key-Cell shows which cells are located at this coordinates.
    /// But cells also have position. We ensure sync is fine
    /// </summary>
    public static void ValidateMapInternalPos(this GameMap gameMap, string pfx = "Err")
    {
        foreach (var (posExpected, ents) in gameMap.map)
        {
            foreach (var ent in ents)
            {
                ent.Position.ShouldBe(posExpected, $"{pfx}@{posExpected}: {gameMap.MapDump()}");
            }
        }
    }

    /// <summary>
    /// Validate robots and garbage by their count on the map
    /// Also ensures player = 1
    /// </summary>
    public static void ValidateRbtGarbo(this GameMap gameMap, int rob, int garbo)
    {
        gameMap.ValidateMapInternalPos();
        gameMap.Count(CellKind.Player).ShouldBe(1, "Map invariant broken");
        gameMap.Count(CellKind.Robot).ShouldBe(rob);
        gameMap.Count(CellKind.Garbage).ShouldBe(garbo);
    }

    public static void SafeStep(this GameMap gameMap, int n = 1)
    {
        gameMap.ValidateMapInternalPos("Before step 1");
        for (int i = 0; i < n; i++)
        {
            gameMap.Step();
            gameMap.ValidateMapInternalPos($"After number of steps done: {i + 1}");
        }
    }

}
public static class PtExtension
{
    public static Point Point(this Pt pt)
    {
        return Utils.Pt2Point(pt);
    }
    public static Point Add(this Pt pt, Pt otherAsDelta)
    {
        return Utils.Pt2Point(pt) + otherAsDelta.Point();
    }
}

public static class Utils
{
    public static Point Pt2Point(Pt p) => p switch
    {
        Pt.HOME => new(16, 12),
        Pt.N => new(0, -1),
        Pt.S => new(0, 1),
        Pt.W => new(-1, 0),
        Pt.E => new(1, 0),
        Pt.NW => new(-1, -1),
        Pt.NE => new(1, -1),
        Pt.SW => new(-1, 1),
        Pt.SE => new(1, 1),
        _ => throw new Exception("Internal error")
    };

    public static string Str(IEnumerable<Cell> cells)
    {
        return "[" + string.Join(", ", cells) + "]";
    }

    /// <summary>
    /// 8 directions of compass
    /// </summary>
    public static TheoryData<Pt> COMPASS_8 => [
        Pt.N, Pt.S, Pt.E, Pt.W,
        Pt.NW, Pt.NE, Pt.SW, Pt.SE
    ];

    /// <summary>
    /// Pairs of adjacent compass directions (N-NE, S-SE, etc)
    /// </summary>
    public static TheoryData<Pt, Pt> COMPASS_ADJACENT => [
        (Pt.N, Pt.NE), (Pt.N, Pt.NW),
        (Pt.S, Pt.SW), (Pt.S, Pt.SE),
        (Pt.E, Pt.NE), (Pt.E, Pt.SE),
        (Pt.W, Pt.SW), (Pt.W, Pt.NW),
        (Pt.NW, Pt.NE), (Pt.SW, Pt.SE),
        (Pt.NE, Pt.SE), (Pt.NW, Pt.SW)
    ];

}

