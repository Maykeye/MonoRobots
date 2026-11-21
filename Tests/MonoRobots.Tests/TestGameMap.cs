using Xunit;
using Shouldly;
using System.Linq;
namespace MonoRobots.Tests;

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
}

public class DummyTest
{
    [Fact]
    public void TestSpawn()
    {
        var map = new GameMap(32, 24);
        map.Reset(1);
        bool res;
        res = map.Spawn(new(2, 3), CellKind.Robot);
        res.ShouldBeTrue("Must spawn at empty cell");
        res = map.Spawn(new(2, 3), CellKind.Robot);
        res.ShouldBeFalse("Must not spawn at occupied cell");
    }


    [Fact]
    public void TestResetPlayerPos()
    {
        var map = new GameMap(32, 24);
        map.Reset(1);
        var ply = map.GetPlayer();
        ply.Position.X.ShouldBe(16, "Map width/2");
        ply.Position.Y.ShouldBe(12, "Map height/2");
    }


    [Fact]
    public void TestStepRetainsPlayer()
    {
        var rng = new MockRng();
        var map = new GameMap(32, 24, rng);
        rng.EnqueueInts([18, 12]);
        map.Reset(1);
        map.Step();
        var ply = map.GetPlayer();
        ply.Position.X.ShouldBe(16, "Map width/2");
        ply.Position.Y.ShouldBe(12, "Map height/2");
    }


    [Fact]
    public void TestRobotSpawn()
    {
        var rng = new MockRng();
        var map = new GameMap(32, 24, rng);
        rng.EnqueueInts([18, 12]);
        map.Reset(1);
        var found = map.GetAt(new(18, 12), []);
        found.ShouldBeTrue("Given RNG, should spawn at given coords");
    }

    [Fact]
    public void TestResetClears()
    {
        var rng = new MockRng();
        var map = new GameMap(32, 24, rng);
        rng.EnqueueInts([18, 12]);
        map.Reset(1);
        map.map.Count.ShouldBe(2, "Player + Robot");

    }

    [Fact]
    public void TestRobotSpawnDouble()
    {
        var rng = new MockRng();
        var map = new GameMap(32, 24, rng);
        rng.EnqueueInts([18, 12, 18, 12, 19, 12]);
        map.Reset(2);
        var found = map.GetAt(new(18, 12), []);
        found.ShouldBeTrue("Given RNG, should spawn at given coords");
        found = map.GetAt(new(19, 12), []);
        found.ShouldBeTrue("Given RNG, should respawn at given coords");
    }


    [Fact]
    public void TestRobotMovesAway()
    {
        var rng = new MockRng();
        var map = new GameMap(32, 24, rng);
        rng.EnqueueInts([18, 12]);
        map.Reset(1);
        map.Step();
        var found = map.GetAt(new(18, 12), []);
        found.ShouldBeFalse("Robot should move");
        map.map.Count.ShouldBe(2, "Player + Robot");
    }

    [Fact]
    public void TestSetPlayerPos()
    {
        var map = new GameMap(32, 24);
        map.Reset(1);
        map.SetPlayerPosition(2, 4);
        var plyPos = map.GetPlayer().Position;
        plyPos.X.ShouldBe(2);
        plyPos.Y.ShouldBe(4);
    }

    [Theory]
    [InlineData(10, 8, 10, 9)]
    [InlineData(10, 12, 10, 11)]
    [InlineData(0, 10, 1, 10)]
    [InlineData(12, 10, 11, 10)]
    [InlineData(9, 1, 10, 2)]
    [InlineData(12, 1, 11, 2)]
    [InlineData(9, 21, 10, 20)]
    [InlineData(12, 21, 11, 20)]
    public void TestMoveRobot(int x0, int y0, int x1, int y1)
    {
        var rng = new MockRng();
        var map = new GameMap(32, 24, rng);
        rng.EnqueueInts([x0, y0]);
        map.Reset(1);
        map.SetPlayerPosition(10, 10);
        map.Step();
        map.map.Count.ShouldBe(2, "Player + Robot");

        var plyPos = map.GetPlayer().Position;
        plyPos.X.ShouldBe(10);
        plyPos.Y.ShouldBe(10);

        var ents = new List<Cell>();
        bool found;

        found = map.GetAt(new(x0, y0), ents);
        found.ShouldBeFalse($"Robot should move away from {x0} {y0}");

        ents = [];
        found = map.GetAt(new(x1, y1), ents);
        found.ShouldBeTrue($"Robot should move to {x1} {y1}; {map.MapDump()}");
        ents.Count.ShouldBe(1);
    }

    [Fact]
    public void TestMoveGarbage()
    {
        var rng = new MockRng();
        var map = new GameMap(32, 24, rng);
        rng.EnqueueInts([10, 12]);
        map.Reset(0);
        map.SetPlayerPosition(10, 10);
        bool ok = map.Spawn(new(4, 5), CellKind.Garbage);
        ok.ShouldBeTrue("Spawn garbo");
        map.Step();
        ok = map.GetAt(new(4, 5), []);
        ok.ShouldBeTrue("Garbo didn't stay");
        map.map.Count.ShouldBe(2, "Player + Garbo");
    }

    [Theory]
    [InlineData(16, 11, /**/0, 1, /**/1, 1)]
    public void TestRobotCollide(int cx, int cy, int dx1, int dy1, int dx2, int dy2)
    {
        var map = new GameMap(32, 24);
        map.Reset(0);
        map.Spawn(new(cx + dx1, cy + dy1), CellKind.Robot);
        map.Spawn(new(cx + dx2, cy + dy2), CellKind.Robot);
        map.Step();

        int robots = map.FindKind(CellKind.Robot).Count;
        int garbo = map.FindKind(CellKind.Garbage).Count;
        robots.ShouldBe(0);
        garbo.ShouldBe(1);
    }
}
