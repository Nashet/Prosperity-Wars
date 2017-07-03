using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//public abstract class NameDescriptor
//{ }
//abstract public class AbstractCondition : Name
//{
//    protected AbstractCondition(string name) : base(name)
//    { }
//}
public static class ReformExtensions
{
    public static bool isEnacted(this List<AbstractReform> list, AbstractReformValue reformValue)
    {
        foreach (var item in list)
            if (item.getValue() == reformValue)
                return true;
        return false;
    }
}
abstract public class AbstractReformStepValue : AbstractReformValue
{
    //private readonly int totalSteps;
    public AbstractReformStepValue(string name, string indescription, int ID, ConditionsList condition, int totalSteps)
        : base(name, indescription, ID, condition)
    {
        var previousID = ID - 1;
        var nextID = ID + 1;
        if (previousID >= 0 && nextID < totalSteps)
            condition.add(new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(previousID)
            || (x as Country).taxationForPoor.isThatReformEnacted(nextID), "Previous reform enacted", true));
        else
        if (nextID < totalSteps)
            condition.add(new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(nextID), "Previous reform enacted", true));
        else
        if (previousID >= 0)
            condition.add(new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(previousID), "Previous reform enacted", true));
    }
}
abstract public class AbstractReformValue : Name
{
    readonly string description;
    readonly internal int ID;
    readonly internal ConditionsList allowed;
    readonly public Condition isEnacted;
    static AbstractReformValue()
    {
        //allowed.add();
    }
    protected AbstractReformValue(string name, string indescription, int ID, ConditionsList condition) : base(name)
    {
        this.ID = ID;
        // name = inname;
        description = indescription;
        this.allowed = condition;
        isEnacted = new Condition(x => !(x as Country).reforms.isEnacted(this), "Reform is not enacted yet", true);
        allowed.add(isEnacted);
        wantsReform = new Modifier(x => this.howIsItGoodForPop(x as PopUnit).get(),
                    "How much is it good for population", 1f, true);
        loyalty = new Modifier(x => this.loyaltyBoostFor(x as PopUnit),
                    "Loyalty", 1f, false);
        modVoting = new ModifiersList(new List<Condition>{
        wantsReform, loyalty,education
        });
    }

    private float loyaltyBoostFor(PopUnit popUnit)
    {
        float result;
        if (howIsItGoodForPop(popUnit).get() > 0.5f)
            result = popUnit.loyalty.get() / 4f;
        else
            result = popUnit.loyalty.get50Centre() / 4f;
        return result;
    }

    new internal string getDescription()
    {
        return description;
    }

    abstract internal bool isAvailable(Country country);

    internal abstract Procent howIsItGoodForPop(PopUnit pop);
    Modifier loyalty = new Modifier(Condition.IsNotImplemented, 0f, false);
    Modifier education = new Modifier(Condition.IsNotImplemented, 0f, false);
    Modifier wantsReform;
    public ModifiersList modVoting;
}
public abstract class AbstractReform : Name
{
    //readonly string name;
    readonly string description;


    protected AbstractReform(string name, string indescription, Country incountry) : base(name)
    {
        //name = inname;
        description = indescription;
        incountry.reforms.Add(this);

    }

    internal abstract bool isAvailable(Country country);
    public abstract IEnumerator GetEnumerator();
    internal abstract bool canChange();
    internal abstract void setValue(AbstractReformValue selectedReformValue);

    new internal string getDescription()
    {
        return description;
    }


    abstract internal AbstractReformValue getValue();
    //abstract internal AbstractReformValue getValue(int value);

}
public class Government : AbstractReform
{
    public class ReformValue : AbstractReformValue
    {
        readonly private string prefix;

        public ReformValue(string inname, string indescription, int idin, ConditionsList condition, string prefix)
            : base(inname, indescription, idin, condition)

        {
            // (!PossibleStatuses.Contains(this))
            PossibleStatuses.Add(this);
            this.prefix = prefix;
        }

        internal override bool isAvailable(Country country)
        {
            if (ID == 4 && !country.isInvented(Invention.collectivism))
                return false;
            else
                return true;
        }

        //internal bool isGovernmentEqualsThat(Country forWhom)
        //{
        //    return forWhom.government.status == this;
        //}
        internal override Procent howIsItGoodForPop(PopUnit pop)
        {
            Procent result;
            if (pop.getVotingPower(this) > pop.getVotingPower(pop.getCountry().government.getTypedValue()))
                result = new Procent(1f);
            else
                result = new Procent(0f);
            return result;
        }
        internal string getPrefix()
        {
            return prefix;
        }
    }
    //internal readonly Government that;
    private ReformValue status;
    private readonly Country country;

    readonly internal static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { Tribal, Aristocracy, Despotism, Democracy, ProletarianDictatorship };

    readonly internal static ReformValue Tribal = new ReformValue("Tribal democracy", "- Tribesmen and Aristocrats can vote", 0,
        new ConditionsList(Condition.AlwaysYes), "tribe");

    readonly internal static ReformValue Aristocracy = new ReformValue("Aristocracy", "- Only Aristocrats and Clerics can vote", 1,
        new ConditionsList(Condition.AlwaysYes), "kingdom");

    readonly internal static ReformValue Polis = new ReformValue("Polis", "- Landed individuals allowed to vote, such as Farmers, Aristocrats, Clerics; each vote is equal", 8,
        new ConditionsList(Condition.AlwaysYes), "polis");

    readonly internal static ReformValue Despotism = new ReformValue("Despotism", "- Despot does what he wants", 2,
        new ConditionsList(Condition.AlwaysYes), "empire");

    readonly internal static ReformValue Theocracy = new ReformValue("Theocracy", "- Only Clerics have power", 5,
        new ConditionsList(Condition.AlwaysYes), "");

    readonly internal static ReformValue WealthDemocracy = new ReformValue("Wealth Democracy", "- Landed individuals allowed to vote, such as Farmers, Aristocrats, etc. Rich classes has more votes (5 to 1)", 9,
        new ConditionsList(Condition.IsNotImplemented), "states");

    readonly internal static ReformValue Democracy = new ReformValue("Universal Democracy", "- Everyone can vote; each vote is equal", 3,
        new ConditionsList(new List<Condition> { Invention.IndividualRightsInvented }), "republic");

    readonly internal static ReformValue BourgeoisDictatorship = new ReformValue("Bourgeois dictatorship", "- Only capitalists have power", 6,
        new ConditionsList(new List<Condition> { Invention.IndividualRightsInvented }), "");

    readonly internal static ReformValue Junta = new ReformValue("Military junta", "- Only military guys have power", 7,
        new ConditionsList(new List<Condition> { Invention.ProfessionalArmyInvented }), "junta");

    readonly internal static ReformValue ProletarianDictatorship = new ReformValue("Proletarian dictatorship", "- ProletarianDictatorship is it. Bureaucrats rule you", 4,
        new ConditionsList(Condition.IsNotImplemented), "ssr");

    internal readonly static Condition isPolis = new Condition(x => (x as Country).government.getValue() == Government.Polis, "Government is " + Government.Polis.getName(), true);
    internal readonly static Condition isTribal = new Condition(x => (x as Country).government.getValue() == Government.Tribal, "Government is " + Government.Tribal.getName(), true);
    internal readonly static Condition isAristocracy = new Condition(x => (x as Country).government.getValue() == Government.Aristocracy, "Government is " + Government.Aristocracy.getName(), true);

    internal readonly static Condition isDespotism = new Condition(x => (x as Country).government.getValue() == Government.Despotism, "Government is " + Government.Despotism.getName(), true);
    internal readonly static Condition isTheocracy = new Condition(x => (x as Country).government.getValue() == Government.Theocracy, "Government is " + Government.Theocracy.getName(), true);
    internal readonly static Condition isWealthDemocracy = new Condition(x => (x as Country).government.getValue() == Government.WealthDemocracy, "Government is " + Government.WealthDemocracy.getName(), true);
    internal readonly static Condition isDemocracy = new Condition(x => (x as Country).government.getValue() == Government.Democracy, "Government is " + Government.Democracy.getName(), true);
    internal readonly static Condition isBourgeoisDictatorship = new Condition(x => (x as Country).government.getValue() == Government.BourgeoisDictatorship, "Government is " + Government.BourgeoisDictatorship.getName(), true);
    internal readonly static Condition isMilitaryJunta = new Condition(x => (x as Country).government.getValue() == Government.Junta, "Government is " + Government.Junta.getName(), true);
    internal readonly static Condition isProletarianDictatorship = new Condition(x => (x as Country).government.getValue() == Government.ProletarianDictatorship, "Government is " + Government.ProletarianDictatorship.getName(), true);

    public Government(Country country) : base("Government", "Form of government", country)
    {
        //status = Tribal;
        status = Aristocracy;
        this.country = country;
    }
    internal string getPrefix()
    {
        return status.getPrefix();
    }
    internal override AbstractReformValue getValue()
    {
        return status;
    }
    internal Government.ReformValue getTypedValue()
    {
        return status;
    }
    //internal override AbstractReformValue getValue(int value)
    //{
    //    return PossibleStatuses[value];
    //}
    internal override bool canChange()
    {
        return true;
    }

    public override IEnumerator GetEnumerator()
    {
        foreach (ReformValue f in PossibleStatuses)
            yield return f;
    }

    internal override void setValue(AbstractReformValue selectedReform)
    {
        status = (ReformValue)selectedReform;
        country.setPrefix();
    }

    internal override bool isAvailable(Country country)
    {
        return true;
    }


}
public class Economy : AbstractReform
{
    internal readonly static Condition isNotLF = new Condition(delegate (object forWhom) { return (forWhom as Country).economy.status != Economy.LaissezFaire; }, "Economy policy is not Laissez Faire", true);
    internal readonly static Condition isLF = new Condition(delegate (object forWhom) { return (forWhom as Country).economy.status == Economy.LaissezFaire; }, "Economy policy is Laissez Faire", true);

    internal readonly static Condition isNotNatural = new Condition(x => (x as Country).economy.status != Economy.NaturalEconomy, "Economy policy is not Natural Economy", true);
    internal readonly static Condition isNatural = new Condition(x => (x as Country).economy.status == Economy.NaturalEconomy, "Economy policy is Natural Economy", true);

    internal readonly static Condition isNotState = new Condition(x => (x as Country).economy.status != Economy.StateCapitalism, "Economy policy is not State Capitalism", true);
    internal readonly static Condition isStateCapitlism = new Condition(x => (x as Country).economy.status == Economy.StateCapitalism, "Economy policy is State Capitalism", true);

    internal readonly static Condition isNotInterventionism = new Condition(x => (x as Country).economy.status != Economy.Interventionism, "Economy policy is not Limited Interventionism", true);
    internal readonly static Condition isInterventionism = new Condition(x => (x as Country).economy.status == Economy.Interventionism, "Economy policy is Limited Interventionism", true);

    internal readonly static Condition isNotPlanned = new Condition(x => (x as Country).economy.status != Economy.PlannedEconomy, "Economy policy is not Planned Economy", true);
    internal readonly static Condition isPlanned = new Condition(x => (x as Country).economy.status == Economy.PlannedEconomy, "Economy policy is Planned Economy", true);

    internal static Condition isNotMarket = new Condition(x => (x as Country).economy.status == Economy.NaturalEconomy || (x as Country).economy.status == Economy.PlannedEconomy,
      "Economy is not market economy", true);
    internal static Condition isMarket = new Condition(x => (x as Country).economy.status == Economy.StateCapitalism || (x as Country).economy.status == Economy.Interventionism
        || (x as Country).economy.status == Economy.LaissezFaire
        , "Economy is market economy", true);
    public class ReformValue : AbstractReformValue
    {
        public ReformValue(string inname, string indescription, int idin, ConditionsList condition) : base(inname, indescription, idin, condition)
        {
            PossibleStatuses.Add(this);
        }

        internal override bool isAvailable(Country country)
        {
            ReformValue requested = this;
            if (requested.ID == 0)
                return true;
            else
            if (requested.ID == 1)
                return true;
            else
            if (requested.ID == 2 && country.isInvented(Invention.collectivism))
                return true;
            else
            if (requested.ID == 3)
                return true;
            else
                return false;
        }


        internal override Procent howIsItGoodForPop(PopUnit pop)
        {
            Procent result;
            if (pop.popType == PopType.Capitalists)
            {
                //positive - more liberal
                int change = ID - pop.getCountry().economy.status.ID;
                //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                if (change > 0)
                    result = new Procent(1f);
                else
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                    result = new Procent(0f);
            }
            else
            if (this == Economy.PlannedEconomy)
                result = new Procent(0f);
            else
                result = new Procent(0.5f);
            return result;
        }
    }
    static readonly ConditionsList capitalism = new ConditionsList(new List<Condition>()
        {
            Invention.IndividualRightsInvented,
            Invention.BankingInvented,
            Serfdom.IsAbolishedInAnyWay
        });
    internal ReformValue status;
    internal static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();
    internal static readonly ReformValue PlannedEconomy = new ReformValue("Planned economy", "", 0,
        new ConditionsList(new List<Condition> {
            Invention.collectivismInvented, Government.isProletarianDictatorship, Condition.IsNotImplemented }));
    internal static readonly ReformValue NaturalEconomy = new ReformValue("Natural economy", " ", 1, new ConditionsList(Condition.IsNotImplemented));
    internal static readonly ReformValue StateCapitalism = new ReformValue("State capitalism", "", 2, new ConditionsList(capitalism));
    internal static readonly ReformValue Interventionism = new ReformValue("Limited Interventionism", "", 3, new ConditionsList(capitalism));
    internal static readonly ReformValue LaissezFaire = new ReformValue("Laissez Faire", "", 4, new ConditionsList(capitalism));


    /// ////////////
    public Economy(Country country) : base("Economy", "Your economy policy", country)
    {
        status = NaturalEconomy;
    }
    internal override AbstractReformValue getValue()
    {
        return status;
    }
    //internal override AbstractReformValue getValue(int value)
    //{
    //    return PossibleStatuses[value];
    //}
    internal override bool canChange()
    {
        return true;
    }
    public override IEnumerator GetEnumerator()
    {
        foreach (ReformValue f in PossibleStatuses)
            yield return f;
    }
    internal override void setValue(AbstractReformValue selectedReform)
    {
        status = (ReformValue)selectedReform;
    }
    internal override bool isAvailable(Country country)
    {
        return true;
    }

}

public class Serfdom : AbstractReform
{
    public class ReformValue : AbstractReformValue
    {
        public ReformValue(string inname, string indescription, int idin, ConditionsList condition) : base(inname, indescription, idin, condition)
        {
            //if (!PossibleStatuses.Contains(this))
            PossibleStatuses.Add(this);
            // this.allowed = condition;
        }
        internal override bool isAvailable(Country country)
        {
            ReformValue requested = this;

            if ((requested.ID == 4) && country.isInvented(Invention.collectivism) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 4))
                return true;
            else
            if ((requested.ID == 3) && country.isInvented(Invention.Banking) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 3))
                return true;
            else
            if ((requested.ID == 2) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 2))
                return true;
            else
                if ((requested.ID == 1) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1))
                return true;
            else
            if ((requested.ID == 0))
                return true;
            else
                return false;
        }

        static Procent br = new Procent(0.2f);
        static Procent al = new Procent(0.1f);
        static Procent nu = new Procent(0.0f);
        internal Procent getTax()
        {
            if (this == Brutal)
                return br;
            else
                if (this == Allowed)
                return al;
            else
                return nu;
        }
        internal override Procent howIsItGoodForPop(PopUnit pop)
        {
            Procent result;
            int change = ID - pop.getCountry().serfdom.status.ID; //positive - more liberal
            if (pop.popType == PopType.Aristocrats)
            {
                if (change > 0)
                    result = new Procent(0f);
                else
                    result = new Procent(1f);
            }
            else
            {
                if (change > 0)
                    result = new Procent(1f);
                else
                    result = new Procent(0f);
            }
            return result;
        }
    }
    internal ReformValue status;
    internal static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { Allowed, Brutal, Abolished, AbolishedWithLandPayment, AbolishedAndNationalizated };
    internal static ReformValue Allowed;
    internal static ReformValue Brutal;
    internal static ReformValue Abolished = new ReformValue("Abolished", "- Abolished with no obligations", 2,
        new ConditionsList(new List<Condition>() { Invention.IndividualRightsInvented }));
    internal static ReformValue AbolishedWithLandPayment = new ReformValue("Abolished with land payment", "- Peasants are personally free now but they have to pay debt for land", 3,
        new ConditionsList(new List<Condition>()
        {
            Invention.IndividualRightsInvented,Invention.BankingInvented, Condition.IsNotImplemented
        }));
    internal static ReformValue AbolishedAndNationalizated = new ReformValue("Abolished and nationalization land", "- Aristocrats loose property", 4,
        new ConditionsList(new List<Condition>()
        {
            Government.isProletarianDictatorship, Condition.IsNotImplemented
        }));
    public Serfdom(Country country) : base("Serfdom", "- Aristocrats privileges", country)
    {
        if (Allowed == null)
            Allowed = new ReformValue("Allowed", "- Peasants and other plebes pay 10% of income to Aristocrats", 1,
                new ConditionsList(new List<Condition>()
                {
            Economy.isNotMarket
                }));
        if (Brutal == null)
            Brutal = new ReformValue("Brutal", "- Peasants and other plebes pay 20% of income to Aristocrats", 0,
            new ConditionsList(new List<Condition>()
            {
            Economy.isNotMarket
            }));

        status = Allowed;
    }
    internal override AbstractReformValue getValue()
    {
        return status;
    }
    //internal override AbstractReformValue getValue(int value)
    //{
    //    //return PossibleStatuses.Find(x => x.ID == value);
    //    return PossibleStatuses[value];
    //}
    internal override bool canChange()
    {
        return true;
    }
    public override IEnumerator GetEnumerator()
    {
        foreach (ReformValue f in PossibleStatuses)
            yield return f;
    }

    internal override void setValue(AbstractReformValue selectedReform)
    {
        status = (ReformValue)selectedReform;
    }
    internal override bool isAvailable(Country country)
    {
        return true;
    }
    internal static Condition IsAbolishedInAnyWay = new Condition(x => (x as Country).serfdom.status == Serfdom.Abolished
    || (x as Country).serfdom.status == Serfdom.AbolishedAndNationalizated || (x as Country).serfdom.status == Serfdom.AbolishedWithLandPayment,
        "Serfdom is abolished", true);
    internal static Condition IsNotAbolishedInAnyWay = new Condition(x => (x as Country).serfdom.status == Serfdom.Allowed
    || (x as Country).serfdom.status == Serfdom.Brutal,
        "Serfdom is in power", true);

}
public class MinimalWage : AbstractReform
{
    public class ReformValue : AbstractReformValue
    {
        public ReformValue(string inname, string indescription, int idin, ConditionsList condition)
            : base(inname, indescription, idin, condition)
        {
            // if (!PossibleStatuses.Contains(this))
            PossibleStatuses.Add(this);

        }
        internal override bool isAvailable(Country country)
        {
            return true;
        }

        /// <summary>
        /// Calculates wage basing on consumption cost for 1000 workers
        /// </summary>        
        internal float getWage()
        {
            if (this == None)
                return 0f;
            else if (this == Scanty)
            {
                Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000());
                //result.multipleInside(0.5f);
                return result.get();
            }
            else if (this == Minimal)
            {
                Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000());
                Value everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000());
                everyDayCost.multiple(0.02f);
                result.add(everyDayCost);
                return result.get();
            }
            else if (this == Trinket)
            {
                Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000());
                Value everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000());
                everyDayCost.multiple(0.04f);
                result.add(everyDayCost);
                return result.get();
            }
            else if (this == Middle)
            {
                Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000());
                Value everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000());
                everyDayCost.multiple(0.06f);
                result.add(everyDayCost);
                return result.get();
            }
            else if (this == Big)
            {
                Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000());
                Value everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000());
                everyDayCost.multiple(0.08f);
                //Value luxuryCost = Game.market.getCost(PopType.workers.getLuxuryNeedsPer1000());
                result.add(everyDayCost);
                //result.add(luxuryCost);
                return result.get();
            }
            else
                return 0f;
        }
        override public string ToString()
        {
            return base.ToString() + " (" + getWage() + ")";
        }
        internal override Procent howIsItGoodForPop(PopUnit pop)
        {
            Procent result;
            if (pop.popType == PopType.Workers)
            {
                //positive - reform will be better for worker, [-5..+5]
                int change = ID - pop.getCountry().minimalWage.status.ID;
                //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                if (change > 0)
                    result = new Procent(1f);
                else
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                    result = new Procent(0f);
            }
            else if (pop.popType.isPoorStrata())
                result = new Procent(0.5f);
            else // rich strata
            {
                //positive - reform will be better for rich strata, [-5..+5]
                int change = pop.getCountry().minimalWage.status.ID - ID;
                //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                if (change > 0)
                    result = new Procent(1f);
                else
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                    result = new Procent(0f);
            }
            return result;
        }
    }
    ReformValue status;

    internal readonly static List<ReformValue> PossibleStatuses = new List<ReformValue>();
    internal readonly static ReformValue None = new ReformValue("No minimal wage", "", 0, new ConditionsList(new List<Condition>()));

    internal readonly static ReformValue Scanty = new ReformValue("Scanty minimal wage", "- Half-hungry", 1, new ConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLF,
        }));
    internal readonly static ReformValue Minimal = new ReformValue("Tiny minimal wage", "- Just enough to feed yourself", 2, new ConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLF,
        }));
    internal readonly static ReformValue Trinket = new ReformValue("Trinket minimal wage", "- You can buy some small stuff", 3, new ConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLF,
        }));
    internal readonly static ReformValue Middle = new ReformValue("Middle minimal wage", "- Plenty good wage", 4, new ConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLF,
        }));
    internal readonly static ReformValue Big = new ReformValue("Generous minimal wage", "- Can live almost like a king. Almost..", 5, new ConditionsList(new List<Condition>()
        {
            Invention.WelfareInvented, Economy.isNotLF,
        }));

    public MinimalWage(Country country) : base("Minimal wage", "", country)
    {
        status = None;
    }
    internal override AbstractReformValue getValue()
    {
        return status;
    }
    //internal override AbstractReformValue getValue(int value)
    //{
    //    return PossibleStatuses.Find(x => x.ID == value);
    //    //return PossibleStatuses[value];
    //}
    internal override bool canChange()
    {
        return true;
    }
    public override IEnumerator GetEnumerator()
    {
        foreach (ReformValue f in PossibleStatuses)
            yield return f;
    }
    internal override void setValue(AbstractReformValue selectedReform)
    {
        status = (ReformValue)selectedReform;
    }
    internal override bool isAvailable(Country country)
    {
        if (country.isInvented(Invention.Welfare))
            return true;
        else
            return false;
    }

}
public class UnemploymentSubsidies : AbstractReform
{
    public class ReformValue : AbstractReformValue
    {
        public ReformValue(string inname, string indescription, int idin, ConditionsList condition) : base(inname, indescription, idin, condition)
        {
            //if (!PossibleStatuses.Contains(this))
            PossibleStatuses.Add(this);
        }
        internal override bool isAvailable(Country country)
        {
            return true;
        }

        /// <summary>
        /// Calculates Unemployment Subsidies basing on consumption cost for 1000 workers
        /// </summary>        
        internal float getSubsidiesRate()
        {
            if (this == None)
                return 0f;
            else if (this == Scanty)
            {
                Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000());
                //result.multipleInside(0.5f);
                return result.get();
            }
            else if (this == Minimal)
            {
                Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000());
                Value everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000());
                everyDayCost.multiple(0.02f);
                result.add(everyDayCost);
                return result.get();
            }
            else if (this == Trinket)
            {
                Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000());
                Value everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000());
                everyDayCost.multiple(0.04f);
                result.add(everyDayCost);
                return result.get();
            }
            else if (this == Middle)
            {
                Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000());
                Value everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000());
                everyDayCost.multiple(0.06f);
                result.add(everyDayCost);
                return result.get();
            }
            else if (this == Big)
            {
                Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000());
                Value everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000());
                everyDayCost.multiple(0.08f);
                //Value luxuryCost = Game.market.getCost(PopType.workers.getLuxuryNeedsPer1000());
                result.add(everyDayCost);
                //result.add(luxuryCost);
                return result.get();
            }
            else
                return 0f;
        }
        override public string ToString()
        {
            return base.ToString() + " (" + getSubsidiesRate() + ")";
        }
        internal override Procent howIsItGoodForPop(PopUnit pop)
        {
            Procent result;
            //positive - higher subsidies
            int change = ID - pop.getCountry().unemploymentSubsidies.status.ID;
            if (pop.popType.isPoorStrata())
            {
                if (change > 0)
                    result = new Procent(1f);
                else
                    result = new Procent(0f);
            }
            else
            {
                if (change > 0)
                    result = new Procent(0f);
                else
                    result = new Procent(1f);
            }
            return result;
        }
    }
    ReformValue status;
    internal readonly static List<ReformValue> PossibleStatuses = new List<ReformValue>();
    internal readonly static ReformValue None = new ReformValue("No unemployment subsidies", "", 0, new ConditionsList(new List<Condition>()));
    internal readonly static ReformValue Scanty = new ReformValue("Scanty unemployment subsidies", "- Half-hungry", 1, new ConditionsList(new List<Condition>()
        {
            Invention.WelfareInvented, Economy.isNotLF,
        }));
    internal readonly static ReformValue Minimal = new ReformValue("Minimal unemployment subsidies", "- Just enough to feed yourself", 2, new ConditionsList(new List<Condition>()
        {
            Invention.WelfareInvented, Economy.isNotLF,
        }));
    internal readonly static ReformValue Trinket = new ReformValue("Trinket unemployment subsidies", "- You can buy some small stuff", 3, new ConditionsList(new List<Condition>()
        {
            Invention.WelfareInvented, Economy.isNotLF,
        }));
    internal readonly static ReformValue Middle = new ReformValue("Middle unemployment subsidies", "- Plenty good subsidies", 4, new ConditionsList(new List<Condition>()
        {
            Invention.WelfareInvented, Economy.isNotLF,
        }));
    internal readonly static ReformValue Big = new ReformValue("Generous unemployment subsidies", "- Can live almost like a king. Almost..", 5, new ConditionsList(new List<Condition>()
        {
            Invention.WelfareInvented, Economy.isNotLF,
        }));


    public UnemploymentSubsidies(Country country) : base("Unemployment Subsidies", "", country)
    {
        status = None;
    }
    internal override AbstractReformValue getValue()
    {
        return status;
    }
    //internal override AbstractReformValue getValue(int value)
    //{
    //    return PossibleStatuses.Find(x => x.ID == value);
    //    //return PossibleStatuses[value];
    //}
    internal override bool canChange()
    {
        return true;
    }
    public override IEnumerator GetEnumerator()
    {
        foreach (ReformValue f in PossibleStatuses)
            yield return f;
    }
    internal override void setValue(AbstractReformValue selectedReform)
    {
        status = (ReformValue)selectedReform;
    }
    internal override bool isAvailable(Country country)
    {
        if (country.isInvented(Invention.Welfare))
            return true;
        else
            return false;
    }

}


public class TaxationForPoor : AbstractReform
{
    public class ReformValue : AbstractReformStepValue
    {
        internal Procent tax;
        public ReformValue(string name, string description, Procent tarrif, int ID, ConditionsList condition) : base(name, description, ID, condition, 11)
        {
            tax = tarrif;
        }

        override public string ToString()
        {
            return tax.ToString() + base.ToString();
        }
        internal override bool isAvailable(Country country)
        {
            //if (ID == 2 && !country.isInvented(InventionType.collectivism))
            //    return false;
            //else
            return true;
        }

        internal override Procent howIsItGoodForPop(PopUnit pop)
        {
            Procent result;
            //positive mean higher tax
            int change = ID - pop.getCountry().taxationForPoor.status.ID;
            if (pop.popType.isPoorStrata())
            {
                if (change > 0)
                    result = new Procent(0f);
                else
                    result = new Procent(1f);
            }
            else
            {
                result = new Procent(0.5f);
            }
            return result;
        }
    }
    ReformValue status;
    internal readonly static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { NaturalEconomy, StateCapitalism, PlannedEconomy };
    static TaxationForPoor()
    {
        for (int i = 0; i <= 10; i++)
            PossibleStatuses.Add(new ReformValue(" tax for poor", "", new Procent(i * 0.1f), i, new ConditionsList(Condition.AlwaysYes)));
    }
    public TaxationForPoor(Country country) : base("Taxation for poor", "", country)
    {
        status = PossibleStatuses[1];
    }
    internal override AbstractReformValue getValue()
    {
        return status;
    }
    internal bool isThatReformEnacted(int value)
    {
        return status == PossibleStatuses[value];
    }
    internal override bool canChange()
    {
        return true;
    }
    public override IEnumerator GetEnumerator()
    {
        foreach (ReformValue f in PossibleStatuses)
            yield return f;
    }
    internal override void setValue(AbstractReformValue selectedReform)
    {
        status = (ReformValue)selectedReform;

    }
    internal override bool isAvailable(Country country)
    {
        return true;
    }

}

public class TaxationForRich : AbstractReform
{
    public class ReformValue : AbstractReformStepValue
    {
        internal Procent tax;
        public ReformValue(string inname, string indescription, Procent intarrif, int idin, ConditionsList condition) : base(inname, indescription, idin, condition, 11)
        {
            tax = intarrif;
        }
        override public string ToString()
        {
            return tax.ToString() + base.ToString();
        }
        internal override bool isAvailable(Country country)
        {
            //if (ID == 2 && !country.isInvented(InventionType.collectivism))
            //    return false;
            //else
            return true;
        }

        internal override Procent howIsItGoodForPop(PopUnit pop)
        {
            Procent result;
            int change = ID - pop.getCountry().taxationForRich.status.ID;//positive mean higher tax
            if (pop.popType.isRichStrata())
            {
                if (change > 0)
                    result = new Procent(0f);
                else
                    result = new Procent(1f);
            }
            else
            {
                if (change > 0)
                    result = new Procent(1f);
                else
                    result = new Procent(0f);
            }
            return result;
        }
    }
    ReformValue status;
    internal readonly static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { NaturalEconomy, StateCapitalism, PlannedEconomy };
    static TaxationForRich()
    {
        for (int i = 0; i <= 10; i++)
            PossibleStatuses.Add(new ReformValue(" tax for rich", "", new Procent(i * 0.1f), i, new ConditionsList(Condition.AlwaysYes)));
    }
    public TaxationForRich(Country country) : base("Taxation for rich", "", country)
    {
        status = PossibleStatuses[1];
    }
    internal override AbstractReformValue getValue()
    {
        return status;
    }
    //internal override AbstractReformValue getValue(int value)
    //{
    //    return PossibleStatuses[value];
    //}
    internal override bool canChange()
    {
        return true;
    }
    public override IEnumerator GetEnumerator()
    {
        foreach (ReformValue f in PossibleStatuses)
            yield return f;
    }
    internal override void setValue(AbstractReformValue selectedReform)
    {
        status = (ReformValue)selectedReform;
    }
    internal override bool isAvailable(Country country)
    {
        return true;
    }

}

public class MinorityPolicy : AbstractReform
{
    public class ReformValue : AbstractReformValue
    {
        public ReformValue(string inname, string indescription, int idin, ConditionsList condition) : base(inname, indescription, idin, condition)
        {
            PossibleStatuses.Add(this);
        }
        internal override bool isAvailable(Country country)
        {
            ReformValue requested = this;
            if ((requested.ID == 4) && country.isInvented(Invention.collectivism) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 4))
                return true;
            else
            if ((requested.ID == 3) && country.isInvented(Invention.Banking) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 3))
                return true;
            else
            if ((requested.ID == 2) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 2))
                return true;
            else
                if ((requested.ID == 1) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1))
                return true;
            else
            if ((requested.ID == 0))
                return true;
            else
                return false;
        }
        internal override Procent howIsItGoodForPop(PopUnit pop)
        {
            Procent result;
            if (pop.isStateCulture())
            {
                result = new Procent(0.5f);
            }
            else
            {
                //positive - more rights for minorities
                int change = ID - pop.getCountry().minorityPolicy.status.ID;
                //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                if (change > 0)
                    result = new Procent(1f);
                else
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                    result = new Procent(0f);
            }
            return result;
        }
    }
    internal ReformValue status;
    readonly internal static List<ReformValue> PossibleStatuses = new List<ReformValue>();
    internal static ReformValue Equality; // all can vote
    internal static ReformValue Residency; // state culture only can vote
                                           //todo add no-individual rights condition check?
    internal readonly static ReformValue NoRights = new ReformValue("No rights for minorities", "- Slavery?", 0, new ConditionsList(Condition.IsNotImplemented));

    //internal static Condition IsResidencyPop;
    public MinorityPolicy(Country country) : base("Minority Policy", "- Minority Policy", country)
    {
        if (Equality == null)
            Equality = new ReformValue("Equality for minorities", "- All cultures have same rights", 2,
                new ConditionsList(new List<Condition>() { Invention.IndividualRightsInvented }));
        if (Residency == null)
            Residency = new ReformValue("Restricted rights for minorities", "- Only state culture can vote", 1, new ConditionsList(Condition.AlwaysYes));

        status = Residency;
        //IsResidencyPop = new Condition(x => (x as PopUnit).province.getOwner().minorityPolicy.status == MinorityPolicy.Residency,
        //Residency.getDescription(), true);
    }
    internal override AbstractReformValue getValue()
    {
        return status;
    }
    //internal override AbstractReformValue getValue(int value)
    //{
    //    return PossibleStatuses[value];
    //}
    internal override bool canChange()
    {
        return true;
    }
    public override IEnumerator GetEnumerator()
    {
        foreach (ReformValue f in PossibleStatuses)
            yield return f;
    }

    internal override void setValue(AbstractReformValue selectedReform)
    {
        status = (ReformValue)selectedReform;
    }
    internal override bool isAvailable(Country country)
    {
        return true;
    }

    //internal static Condition IsResidency = new Condition(x => (x as Country).minorityPolicy.status == MinorityPolicy.Residency,
    //    Residency.getDescription(), true);

    //internal static Condition IsEquality = new Condition(x => (x as Country).minorityPolicy.status == MinorityPolicy.Equality,
    //    Equality.getDescription(), true);

}