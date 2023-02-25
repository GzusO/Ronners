namespace Ronners.RPG;

public class RandomGenerator : IRandomGenerator
{
    private Random _rand;

    public RandomGenerator(Random rand)
    {
        _rand = rand;
    }
    public int Next()
    {
        return _rand.Next();
    }

    public int Next(int max)
    {
        return _rand.Next(max);
    }

    public int Next(int min, int max)
    {
        return _rand.Next(min,max);
    }
}