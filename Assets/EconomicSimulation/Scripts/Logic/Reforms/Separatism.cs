using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Separatism : AbstractReform
    {        
        protected static readonly Procent willing = new Procent(3f);
        protected readonly Condition separatismAllowed;

        public Country separatismTarget;

        private Separatism(Country country) : base(country.ShortName + " independence", "", country,
            null)//new ConditionsList(Condition.AlwaysYes))
        {
            //separatismAllowed = new Condition(x => isAvailable(x as Country), "Separatism target is valid", true);
            //allowed.add(separatismAllowed);
            separatismTarget = country;
            
        }

        //public static Separatism find(Country country)
        //{
        //    var found = allSeparatists.Find(x => x.separatismTarget == country);
        //    if (found == null)
        //        return new Separatism(country);
        //    else
        //        return found;
        //}

        public Procent howIsItGoodForPop(PopUnit pop)
        {
            //return Procent.HundredProcent;
            return willing;
        }

        //public override bool isAvailable(Country country)
        //{
        //    return !separatismTarget.IsAlive;
        //}

        public override void OnReformEnactedInProvince(Province province)
        {
            throw new System.NotImplementedException();
        }
    }
    //public class OldSeparatism : AbstractReformValue, IHasCountry
    //{
    //    private static readonly List<Separatism> allSeparatists = new List<Separatism>();
    //    private static readonly Procent willing = new Procent(3f);
    //    private readonly Condition separatismAllowed;

    //    private readonly Country separatismTarget;

    //    private OldSeparatism(Country country) : base(country.ShortName + " independence", "", 0,
    //        new DoubleConditionsList())//new ConditionsList(Condition.AlwaysYes))
    //    {
    //        separatismAllowed = new Condition(x => isAvailable(x as Country), "Separatism target is valid", true);
    //        allowed.add(separatismAllowed);
    //        separatismTarget = country;
    //        allSeparatists.Add(this);
    //    }

    //    public static Separatism find(Country country)
    //    {
    //        var found = allSeparatists.Find(x => x.separatismTarget == country);
    //        if (found == null)
    //            return new Separatism(country);
    //        else
    //            return found;
    //    }

    //    protected override Procent howIsItGoodForPop(PopUnit pop)
    //    {
    //        //return Procent.HundredProcent;
    //        return willing;
    //    }

    //    public override bool isAvailable(Country country)
    //    {
    //        return !separatismTarget.IsAlive;
    //    }

    //    public Country Country
    //    {
    //        get { return separatismTarget; }
    //    }
    //}
}