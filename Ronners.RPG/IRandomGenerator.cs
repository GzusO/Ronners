namespace Ronners.RPG;

public interface IRandomGenerator
{
    public abstract int Next();
    public abstract int Next(int max);
    public abstract int Next(int min, int max);
}
