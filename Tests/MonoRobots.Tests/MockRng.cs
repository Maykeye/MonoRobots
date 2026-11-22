using Microsoft.Xna.Framework;
using Xunit.Internal;

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
        return Next(0, maxValue);
    }

    /// <summary>
    /// Enqueue integers in direct order, i.e. `EnqueueInts(1,2,3)` means
    /// `Next(10, 20)` is 1
    /// </summary>
    public void EnqueueInts(params IEnumerable<int> xs)
    {
        xs.ForEach(Ints.Enqueue);
    }

    /// <summary>
    /// Enqueue Point as integers in order X, Y, i.e. `EnqueueInts(new Point(1,2))` means
    /// `Next(10, 20)` is 1
    /// </summary>
    public void EnqueueInts(params IEnumerable<Point> ps)
    {
        foreach (var p in ps)
        {
            Ints.Enqueue(p.X);
            Ints.Enqueue(p.Y);
        }
    }

}
