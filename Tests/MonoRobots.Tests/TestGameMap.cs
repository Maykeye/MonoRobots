using Xunit;
using Shouldly;
using static MonoRobots.Tests.Utils;
namespace MonoRobots.Tests;

public class TestGameMap
{

    private readonly MockRng rng;
    private readonly List<Cell> cells;
    private readonly GameMap map;

    public TestGameMap()
    {
        rng = new MockRng();
        cells = [];
        map = new GameMap(32, 24, rng);
    }

    // Do map test and validate pos (key==value.pos)
    void ValidatePos(string pfx = "Err")
    {
        foreach (var (posExpected, ents) in map.map)
        {
            foreach (var ent in ents)
            {
                ent.Position.ShouldBe(posExpected, $"{pfx}@{posExpected}: {map.MapDump()}");
            }
        }
    }

    void Step()
    {
        ValidatePos("Before Step");
        map.Step();
        ValidatePos("After Step");
    }



    [Fact]
    public void TestSpawn()
    {
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
        map.Reset(1);
        var ply = map.GetPlayer();
        ply.Position.X.ShouldBe(16, "Map width/2");
        ply.Position.Y.ShouldBe(12, "Map height/2");
    }


    [Fact]
    public void TestStepRetainsPlayer()
    {
        rng.EnqueueInts([18, 12]);
        map.Reset(1);
        Step();
        var ply = map.GetPlayer();
        ply.Position.X.ShouldBe(16, "Map width/2");
        ply.Position.Y.ShouldBe(12, "Map height/2");
    }


    [Fact]
    public void TestRobotSpawn()
    {
        rng.EnqueueInts([18, 12]);
        map.Reset(1);
        var found = map.GetAt(new(18, 12), []);
        found.ShouldBeTrue("Given RNG, should spawn at given coords");
    }

    [Fact]
    public void TestResetClears()
    {
        rng.EnqueueInts([18, 12]);
        map.Reset(1);
        map.map.Count.ShouldBe(2, "Player + Robot");

    }

    [Fact]
    public void TestRobotSpawnDouble()
    {
        rng.EnqueueInts([18, 12, 18, 12, 19, 12]);
        map.Reset(2);
        var found = map.GetAt(new(18, 12), []);
        found.ShouldBeTrue("Given RNG, should spawn at given coords");
        found = map.GetAt(new(19, 12), []);
        found.ShouldBeTrue("Given RNG, should respawn at given coords");
    }


    private string Str(IEnumerable<Cell> cells)
    {
        return "[" + string.Join(", ", cells) + "]";
    }

    [Fact]
    public void TestRobotMovesAway()
    {
        rng.EnqueueInts([18, 12]);
        map.Reset(1);
        Step();
        List<Cell> tst = [];
        var found = map.GetAt(new(18, 12), tst);
        found.ShouldBeFalse($"Robot should move {Str(tst)}");
        map.map.Count.ShouldBe(2, "Player + Robot");
    }

    [Fact]
    public void TestSetPlayerPos()
    {
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
        rng.EnqueueInts([x0, y0]);
        map.Reset(1);
        map.SetPlayerPosition(10, 10);
        Step();
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
        rng.EnqueueInts([10, 12]);
        map.Reset(0);
        map.SetPlayerPosition(10, 10);
        bool ok = map.Spawn(new(4, 5), CellKind.Garbage);
        ok.ShouldBeTrue("Spawn garbo");
        Step();
        ok = map.GetAt(new(4, 5), cells);
        ok.ShouldBeTrue($"Garbo didn't stay: {map.MapDump()}");
        map.map.Count.ShouldBe(2, "Player + Garbo");
    }

    [Theory]
    [InlineData(Pt.HOME, Pt.N, Pt.NE)]
    [InlineData(Pt.HOME, Pt.N, Pt.NW)]
    [InlineData(Pt.HOME, Pt.S, Pt.SW)]
    [InlineData(Pt.HOME, Pt.S, Pt.SE)]
    [InlineData(Pt.HOME, Pt.E, Pt.NE)]
    [InlineData(Pt.HOME, Pt.E, Pt.SE)]
    [InlineData(Pt.HOME, Pt.W, Pt.SW)]
    [InlineData(Pt.HOME, Pt.W, Pt.NW)]
    [InlineData(Pt.HOME, Pt.NW, Pt.NE)]
    [InlineData(Pt.HOME, Pt.SW, Pt.SE)]
    [InlineData(Pt.HOME, Pt.NE, Pt.SE)]
    [InlineData(Pt.HOME, Pt.NW, Pt.SW)]
    public void TestRobotCollide(Pt cur, Pt r1, Pt r2)
    {
        map.Reset(0);
        var ply = map.GetPlayer();
        bool isPlyAlive = ply.Position != cur.Point();
        map.Spawn(r1.Add(cur), CellKind.Robot);
        map.Spawn(r2.Add(cur), CellKind.Robot);
        Step();
        map.Count(CellKind.Robot).ShouldBe(0, $"Rbt: {map.MapDump()}");
        map.Count(CellKind.Garbage).ShouldBe(1);
        map.GetPlayer().Alive.ShouldBe(isPlyAlive, $"Ply: {map.MapDump()}");
    }
}
