using Xunit;
using Shouldly;
using System.Text.Json;
using System.Drawing;
namespace MonoRobots.Tests;

public class MockRng : Random
{
    public Queue<int> Ints = [];

    /// <summary>
    /// (Supposed to) return in minValue..maxValue
    /// </summary>
    public override int Next(int minValue, int maxValue)
    {
        if (Ints.TryDequeue(out int res))
        {
            return res;
        }

        return base.Next(minValue, maxValue);
    }
    /// <summary>
    /// (Supposed to) return in 0..maxValue
    /// </summary>
    public override int Next(int maxValue)
    {
        if (Ints.TryDequeue(out int res))
        {
            return res;
        }

        return base.Next(maxValue);
    }

    public void EnqueueInts(IEnumerable<int> xs)
    {
        foreach (var x in xs)
        {
            Ints.Enqueue(x);
        }
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
    public void TestMove(int x0, int y0, int x1, int y1)
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
}
