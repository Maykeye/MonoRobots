using Microsoft.Xna.Framework;

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

    public void EnqueueInts(params IEnumerable<Point> ps)
    {
        foreach (var p in ps)
        {
            Ints.Enqueue(p.X);
            Ints.Enqueue(p.Y);
        }
    }
    public void EnqueueInts(params IEnumerable<int> xs)
    {
        foreach (var x in xs)
        {
            Ints.Enqueue(x);
        }
    }

}
