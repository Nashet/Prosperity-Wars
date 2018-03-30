using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    public class TemporaryModifier : Name
    {
        public static readonly TemporaryModifier recentlyConquered = new TemporaryModifier("Recently conquered");
        public static readonly TemporaryModifier blockade = new TemporaryModifier("Blockade");

        //private readonly DateTime expireDate;
        public TemporaryModifier(string name) : base(name)
        { }

        //public Mod(string name, int years) : base(name)
        //{
        //    expireDate = Game.date;
        //    expireDate.AddYears(years);
        //}
    }
}