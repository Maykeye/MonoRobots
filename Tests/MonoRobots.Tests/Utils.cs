using Shouldly;
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

    public static int Count(this GameMap gameMap, CellKind kind)
    {
        return gameMap.FindKind(kind).Count;
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


}

