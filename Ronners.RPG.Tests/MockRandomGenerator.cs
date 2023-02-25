namespace Ronners.RPG;

public class MockRandomGenerator : IRandomGenerator
{
    private int _result;
    public MockRandomGenerator(int result)
    {
        _result = result;
    }
    public int Next()
    {
        return _result;
    }

    public int Next(int max)
    {
        return Next();
    }

    public int Next(int min, int max)
    {
        return Next();
    }
}