namespace Ronners.Loot
{
    public class Prefix : Affix
    {
        public Prefix(string data): base(data)
        {
        }
        public Prefix(Affix data) : base(data)
        {
        }

        public static Prefix GeneratePrefix(Prefix prefix)
        {
            return new Prefix(prefix);
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}