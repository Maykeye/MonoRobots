using Xunit;
using Shouldly;
using Microsoft.Xna.Framework;
namespace MonoRobots.Tests;

public class TestGameMap
{

    private readonly MockRng rng;
    private readonly List<Cell> cells;
    private readonly GameMap map;
    Point home_point;

    public TestGameMap()
    {
        rng = new MockRng();
        cells = [];
        map = new GameMap(32, 24, rng);
        home_point = map.Size / 2;
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(4, false)]
    [InlineData(1000, false)]
    public void TestRetainPlayerPos(int turns, bool alive)
    {
        var robotPos = home_point + new Point(2, 0);
        rng.EnqueueInts(robotPos); // x=ply.x+2, y=ply.y
        map.Reset(1);
        map.ValidateIdleStep(turns);
        var ply = map.GetPlayer();
        ply.Position.ShouldBe(home_point);
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
        map.ValidateIdleStep();
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

    [Fact]
    public void TestGarbageNoMove()
    {
        rng.EnqueueInts([10, 12]);
        map.Reset(0);
        map.SetPlayerPosition(10, 10);
        map.Spawn(new(4, 5), CellKind.Garbage).ShouldBeTrue("Spawn garbo");
        map.ValidateIdleStep();
        map.ValidateRbtGarbo([], [4, 5]);
        map.map.Count.ShouldBe(2, "Player + Garbo");
    }

    [Theory]
    [MemberData(nameof(Utils.COMPASS_ADJACENT), MemberType = typeof(Utils))]
    public void TestRobotCollide(Pt r1, Pt r2)
    {
        map.Reset(0);
        map.Spawn(r1.Add(Pt.HOME), CellKind.Robot);
        map.Spawn(r2.Add(Pt.HOME), CellKind.Robot);
        map.ValidateIdleStep();
        map.ValidateRbtGarbo([], [home_point.X, home_point.Y]);
        map.GetPlayer().Alive.ShouldBeFalse($"Ply: {map.MapDump()}");
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
        map.ValidateIdleStep();
        map.ValidateRbtGarbo([], [10, 12]);
        map.GetPlayer().Alive.ShouldBeTrue();
    }


    [Theory]
    [MemberData(nameof(Utils.COMPASS_8), MemberType = typeof(Utils))]
    public void TestOneRobotMovesSafely(Pt deltaKind)
    {
        var delta = deltaKind.Point();
        var robotSpawn = home_point + delta * 2; // E.g. @.R.
        rng.EnqueueInts(robotSpawn.X, robotSpawn.Y);
        map.Reset(1);
        map.ValidateIdleStep();
        var dst = home_point + delta; // E.g. @R..
        map.ValidateRbtGarbo([dst.X, dst.Y], []);
    }


    [Theory]
    [MemberData(nameof(Utils.COMPASS_8), MemberType = typeof(Utils))]
    public void TestTwoRobotsMoveSafely(Pt deltaKind)
    {
        void doTest(bool swapSpawn)
        {
            var spawn1 = home_point + deltaKind.Point() * 2; // E.g. @.Rr where r not our focus
            var spawn2 = home_point + deltaKind.Point() * 3; // E.g. @.rR
            if (swapSpawn)
                (spawn1, spawn2) = (spawn2, spawn1);

            var delta = deltaKind.Point();
            rng.EnqueueInts(spawn1, spawn2);
            map.Reset(2);
            map.ValidateIdleStep();
            var dst1 = home_point + delta * 1; // E.g. @RR.
            var dst2 = home_point + delta * 2; // 
            map.ValidateRbtGarbo([dst1.X, dst1.Y, dst2.X, dst2.Y], []);
        }
        doTest(false);
        doTest(true);
    }
}

