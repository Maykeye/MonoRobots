using Xunit;
using Shouldly;
using Microsoft.Xna.Framework;
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

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(4, false)]
    [InlineData(1000, false)]
    public void TestRetainPlayerPos(int turns, bool alive)
    {
        rng.EnqueueInts([18, 12]); // x=ply.x+2, y=ply.y
        map.Reset(1);
        map.SafeStep(turns);
        var ply = map.GetPlayer();
        ply.Position.ShouldBe(new(16, 12));
        ply.Alive.ShouldBe(alive);
        map.ValidateRbtGarbo(1, 0);
    }

    [Fact]
    public void TestRobotSpawnAtRandomPoint()
    {
        rng.EnqueueInts([18, 12]);
        map.Reset(1);
        map.ValidateRbtGarbo([18, 12], []);
    }

    [Fact]
    public void TestRobotSpawnDouble()
    {
        rng.EnqueueInts(18, 12, 18, 12, 19, 12);
        map.Reset(2);
        map.ValidateRbtGarbo([18, 12, 19, 12], []);
    }

    [Fact]
    public void TestRobotToPlayer()
    {
        rng.EnqueueInts(18, 12);
        map.Reset(1);
        map.SafeStep();
        map.ValidateRbtGarbo([17, 12], []);
    }

    [Fact]
    public void TestSetPlayerPos()
    {
        map.Reset(2);
        map.SetPlayerPosition(2, 4);
        map.GetPlayer().Position.ShouldBe(new(2, 4));
        map.ValidateRbtGarbo(2, 0);
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
        map.SafeStep();
        map.ValidateRbtGarbo([x1, y1], []);
    }

    [Fact]
    public void TestMoveGarbage()
    {
        rng.EnqueueInts([10, 12]);
        map.Reset(0);
        map.SetPlayerPosition(10, 10);
        map.Spawn(new(4, 5), CellKind.Garbage).ShouldBeTrue("Spawn garbo");
        map.SafeStep();
        map.GetAt(new(4, 5), cells).ShouldBeTrue($"Garbo didn't stay: {map.MapDump()}");
        map.map.Count.ShouldBe(2, "Player + Garbo");
    }

    [Theory]
    [MemberData(nameof(Utils.COMPASS_ADJACENT), MemberType = typeof(Utils))]
    public void TestRobotCollide(Pt r1, Pt r2)
    {
        map.Reset(0);
        var ply = map.GetPlayer();
        bool isPlyAlive = ply.Position != Pt.HOME.Point();
        map.Spawn(r1.Add(Pt.HOME), CellKind.Robot);
        map.Spawn(r2.Add(Pt.HOME), CellKind.Robot);
        map.SafeStep();
        map.ValidateRbtGarbo(0, 1);
        map.GetPlayer().Alive.ShouldBe(isPlyAlive, $"Ply: {map.MapDump()}");
    }

    [Theory]
    [MemberData(nameof(Utils.COMPASS_8), MemberType = typeof(Utils))]
    public void TestRobotCollideGarbage(Pt deltaKind)
    {
        var delta = deltaKind.Point();
        map.Reset(0);
        map.Spawn(new(10, 12), CellKind.Garbage);
        map.SetPlayerPosition(10 + delta.X, 12 + delta.Y); // ply behind garbo
        var robotPos = new Point(10 - delta.X, 12 - delta.Y); //  rbt on opposite side
        map.Spawn(robotPos, CellKind.Robot);
        map.ValidateRbtGarbo([robotPos.X, robotPos.Y], [10, 12]);
        map.SafeStep();
        map.ValidateRbtGarbo([], [10, 12]);
        map.GetPlayer().Alive.ShouldBeTrue();
    }
}
