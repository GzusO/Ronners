namespace Ronners.Loot
{
    public class Suffix : Affix
    {
        public Suffix(string data) : base(data)
        {
        }
        public Suffix(Affix data) : base(data)
        {
        }

        public static Suffix GenerateSuffix(Suffix suffix)
        {
            return new Suffix(suffix);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}