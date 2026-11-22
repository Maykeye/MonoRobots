using Xunit;
using Shouldly;
using Microsoft.Xna.Framework;
namespace MonoRobots.Tests;

public class TestPlayerMovement
{
    private readonly MockRng rng;
    private readonly GameMap map;

    public TestPlayerMovement()
    {
        rng = new MockRng();
        map = new GameMap(32, 24, rng);
    }

    [Theory]
    [MemberData(nameof(Utils.COMPASS_8), MemberType = typeof(Utils))]
    public void TestMoveOutOfBounds(Pt mapDirection)
    {
        var playerPos = map.PointOnMapEdge(mapDirection);
        rng.EnqueueInts(10, 20);
        map.Reset(1);
        map.SetPlayerPosition(playerPos.X, playerPos.Y);
        var res = map.Step(new PlayerTurn.Move(mapDirection.Point()));
        res.ShouldBe(StepResult.Incorrect);
        map.ValidateRbtGarbo([10, 20], []); //no movement of robot
    }

    [Fact]
    public void TestTeleportWorks()
    {
        map.Reset(0); // player@16,12
        rng.EnqueueInts(11, 8);
        map.Step(new PlayerTurn.Teleport());
        map.GetPlayer().Position.ShouldBe(new Point(11, 8));
    }

    [Theory]
    [MemberData(nameof(Utils.COMPASS_8), MemberType = typeof(Utils))]
    public void TestTeleportAffectsRobot(Pt direction)
    {
        Point teleportPos = new(5, 12);

        var delta = direction.Point();
        var robotSpawn = teleportPos + delta * 2;  // @.R - spawn 2 steps away from teleport destination
        var robotExpect = teleportPos + delta * 1; // @R. - moved to 1 step away from teleport destination

        // Spawn robot almost at telport
        rng.EnqueueInts(robotSpawn);
        map.Reset(1);

        // Teleport to teleportPos
        rng.EnqueueInts(teleportPos);
        var stepRes = map.Step(new PlayerTurn.Teleport());
        stepRes.ShouldBe(StepResult.PlayerAlive); // always alive in this test

        map.ValidateRbtGarbo([robotExpect.X, robotExpect.Y], []);
    }

    [Fact]
    public void TestTeleportNotOnRoboGarbo()
    {
        map.Reset(0);
        map.Spawn(new(10, 20), CellKind.Garbage);
        map.Spawn(new(5, 17), CellKind.Robot);
        rng.EnqueueInts(10, 20, 5, 17, 10, 20, 5, 17, 1, 2);
        var stepRes = map.Step(new PlayerTurn.Teleport());
        stepRes.ShouldBe(StepResult.PlayerAlive); // always alive in this test
        map.GetPlayer().Position.ShouldBe(new Point(1, 2));
    }

    [Theory]
    [MemberData(nameof(Utils.COMPASS_8), MemberType = typeof(Utils))]
    public void TestLandNextToTheRobot(Pt direction)
    {
        var robotPos = new Point(5, 3);
        var plyNewPos = robotPos + direction.Point();
        map.Reset(0);
        map.Spawn(robotPos, CellKind.Robot);
        rng.EnqueueInts(plyNewPos);
        var stepRes = map.Step(new PlayerTurn.Teleport());
        map.ValidateRbtGarbo([plyNewPos.X, plyNewPos.Y], []);
        stepRes.ShouldBe(StepResult.PlayerDied); // always by dead in this test
    }

}

