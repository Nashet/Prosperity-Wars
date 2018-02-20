using UnityEngine;
using UnityEditor;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    public class TemporaryModifier : Name
    {
        static readonly public TemporaryModifier recentlyConquered = new TemporaryModifier("Recently conquered");
        static readonly public TemporaryModifier blockade = new TemporaryModifier("Blockade");

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