using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//public abstract class NameDescriptor
//{ }
public interface AbstractCondition { }

abstract public class AbstractReformValue : AbstractCondition
{
    string name;
    string description;
    internal int ID;
    internal ConditionsList allowed;
    internal AbstractReformValue(string inname, string indescription, int IDin, ConditionsList condition)
    {
        ID = IDin;
        name = inname;
        description = indescription;
        this.allowed = condition;
    }
    internal string getDescription()
    {
        return description;
    }
    override public string ToString()
    {
        return name;
    }
    abstract internal bool isAvailable(Country country);
    abstract internal void onReformEnacted();

}
public abstract class AbstractReform
{
    string name;
    string description;
    internal Country country;

    internal AbstractReform(string inname, string indescription, Country incountry)
    {
        name = inname;
        description = indescription;
        incountry.reforms.Add(this);
        country = incountry;
    }
    abstract internal bool isAvailable(Country country);
    abstract public System.Collections.IEnumerator GetEnumerator();
    internal abstract bool canChange();
    internal abstract void setValue(AbstractReformValue selectedReformValue);

    internal string getDescription()
    {
        return description;
    }

    override public string ToString()
    {
        return name;
    }
    abstract internal AbstractReformValue getValue();
    abstract internal AbstractReformValue getValue(int value);

}
public class Government : AbstractReform
{
    public class ReformValue : AbstractReformValue
    {
        public ReformValue(string inname, string indescription, int idin, ConditionsList condition) : base(inname, indescription, idin, condition)
        {
            PossibleStatuses.Add(this);
        }

        internal override bool isAvailable(Country country)
        {
            if (ID == 4 && !country.isInvented(InventionType.collectivism))
                return false;
            else
                return true;
        }
        internal override void onReformEnacted()
        {

        }

        internal bool isGovernmentEqualsThat(Country forWhom)
        {
            return forWhom.government.status == this;
        }
    }
    //public override string ToString()
    //{
    //    return this.status.ToString();
    //}
    internal ReformValue status;
    internal static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { Tribal, Aristocracy, Despotism, Democracy, ProletarianDictatorship };
    internal static ReformValue Tribal = new ReformValue("Tribal democracy", "Tribesmen and Aristocrats can vote", 0, ConditionsList.AlwaysYes);
    internal static ReformValue Aristocracy = new ReformValue("Aristocracy", "Only Aristocrats and Clerics can vote", 1, ConditionsList.AlwaysYes);
    internal static ReformValue AnticRespublic = new ReformValue("Antique respublic", "Landed individuals allowed to vote, such as Farmers, Aristocrats, Clerics; each vote is equal", 8, ConditionsList.AlwaysYes);
    internal static ReformValue Despotism = new ReformValue("Despotism", "Despot does what he wants", 2, ConditionsList.AlwaysYes);
    internal static ReformValue Theocracy = new ReformValue("Theocracy", "Only Clerics have power", 5, ConditionsList.AlwaysYes);

    internal static ReformValue WealthDemocracy = new ReformValue("Wealth Democracy", "Landed individuals allowed to vote, such as Farmers, Aristocrats, etc. Rich classes has more votes", 9, ConditionsList.AlwaysYes);
    internal static ReformValue Democracy = new ReformValue("Universal Democracy", "Everyone can vote; each vote is equal", 3, ConditionsList.AlwaysYes);
    internal static ReformValue BourgeoisDictatorship = new ReformValue("Bourgeois dictatorship", "Only capitalists have power", 6, ConditionsList.AlwaysYes);
    internal static ReformValue MilitaryJunta = new ReformValue("Military junta", "Only military guys have power", 7, ConditionsList.AlwaysYes);

    internal static ReformValue ProletarianDictatorship = new ReformValue("Proletarian dictatorship", "ProletarianDictatorship is it. Bureaucrats rule you", 4, ConditionsList.AlwaysYes);


    public Government(Country country) : base("Government", "Form of government", country)
    {
        status = Tribal;
    }

    internal override AbstractReformValue getValue()
    {
        return status;
    }
    internal override AbstractReformValue getValue(int value)
    {
        return PossibleStatuses[value];
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
public class Economy : AbstractReform
{
    internal static Condition isNotLF = new Condition(delegate (Country forWhom) { return forWhom.economy.status != Economy.LaissezFaire; }, "Economy policy is not Laissez Faire", true);
    internal static Condition isNotNatural = new Condition(delegate (Country forWhom) { return forWhom.economy.status != Economy.NaturalEconomy; }, "Economy policy is not Natural Economy", true);

    internal static Condition isNotState = new Condition(delegate (Country forWhom) { return forWhom.economy.status != Economy.StateCapitalism; }, "Economy policy is not State Capitalism", true);
    internal static Condition isNotInterventionism = new Condition(delegate (Country forWhom) { return forWhom.economy.status != Economy.Interventionism; }, "Economy policy is not Limited Interventionism", true);
    internal static Condition isNotPlanned = new Condition(delegate (Country forWhom) { return forWhom.economy.status != Economy.PlannedEconomy; }, "Economy policy is not Planned Economy", true);



    internal static Condition isNotMarket = new Condition(delegate (Country forWhom) { return forWhom.economy.status == Economy.NaturalEconomy || forWhom.economy.status == Economy.PlannedEconomy; },
      "Economy is not market economy", true);
    internal static Condition isMarket = new Condition(delegate (Country forWhom)
        {
            return forWhom.economy.status == Economy.StateCapitalism || forWhom.economy.status == Economy.Interventionism
        || forWhom.economy.status == Economy.LaissezFaire;
        }, "Economy is market economy", true);
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
            if (requested.ID == 2 && country.isInvented(InventionType.collectivism))
                return true;
            else
            if (requested.ID == 3)
                return true;
            else
                return false;
        }

        internal bool isEconomyEqualsThat(Country forWhom)
        {
            return forWhom.economy.status == this;
        }

        internal override void onReformEnacted()
        {

        }
    }
    static ConditionsList capitalism = new ConditionsList(new List<Condition>()
        {
            //new ConditionString(delegate (Country forWhom) { return forWhom.isInvented(InventionType.individualRights); }, InventionType.individualRights.getInventedPhrase(), true),
            //new ConditionString(delegate (Country forWhom) { return forWhom.isInvented(InventionType.banking); }, InventionType.banking.getInventedPhrase(), true)
            new Condition(InventionType.individualRights, true),
            new Condition(InventionType.banking, true),
            Serfdom.IsAbolishedInAnyWay
        });
    internal ReformValue status;
    internal static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { NaturalEconomy, StateCapitalism, PlannedEconomy };
    internal static ReformValue NaturalEconomy = new ReformValue("Natural economy", " SSS", 0, ConditionsList.IsNotImplemented);
    internal static ReformValue StateCapitalism = new ReformValue("State capitalism", "dddd", 1, capitalism);
    internal static ReformValue Interventionism = new ReformValue("Limited Interventionism", "zz", 1, capitalism);
    internal static ReformValue PlannedEconomy = new ReformValue("Planned economy", "dirty pants", 2, new ConditionsList(new List<AbstractCondition>()
        {
            InventionType.collectivism, Government.ProletarianDictatorship, Condition.IsNotImplemented
        }));
    internal static ReformValue LaissezFaire = new ReformValue("Laissez Faire", "not dirty pants", 3, capitalism);

    /// ////////////


    /// ////////////
    public Economy(Country country) : base("Economy", "Your economy policy", country)
    {
        status = NaturalEconomy;

    }
    internal override AbstractReformValue getValue()
    {
        return status;
    }
    internal override AbstractReformValue getValue(int value)
    {
        return PossibleStatuses[value];
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

    //internal bool isMarket()
    //{
    //    if (status == Economy.NaturalEconomy || status == Economy.PlannedEconomy)
    //        return false;
    //    else
    //        return true;
    //}
    internal override bool isAvailable(Country country)
    {
        return true;
    }
}

public class Serfdom : AbstractReform
{
    public class LocalReformValue : AbstractReformValue
    {
        public LocalReformValue(string inname, string indescription, int idin, ConditionsList condition) : base(inname, indescription, idin, condition)
        {
            PossibleStatuses.Add(this);
            this.allowed = condition;
        }
        internal override bool isAvailable(Country country)
        {
            LocalReformValue requested = this;
            //alowed
            if ((requested.ID == 4) && country.isInvented(InventionType.collectivism) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 4))
                return true;
            else
            if ((requested.ID == 3) && country.isInvented(InventionType.banking) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 3))
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
        internal override void onReformEnacted()
        {

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


    }
    internal LocalReformValue status;
    internal static List<LocalReformValue> PossibleStatuses = new List<LocalReformValue>();// { Allowed, Brutal, Abolished, AbolishedWithLandPayment, AbolishedAndNationalizated };
    internal static LocalReformValue Allowed;
    internal static LocalReformValue Brutal;
    internal static LocalReformValue Abolished = new LocalReformValue("Abolished", "Abolished with no obligations", 2,
        new ConditionsList(new List<Condition>()
        {
            new Condition(InventionType.individualRights, true)
        }));
    internal static LocalReformValue AbolishedWithLandPayment = new LocalReformValue("Abolished with land payment", "Peasants are personally free now but they have to pay debt for land", 3,
        new ConditionsList(new List<Condition>()
        {
            new Condition(InventionType.individualRights, true),
            new Condition(InventionType.banking, true), Condition.IsNotImplemented
        }));
    internal static LocalReformValue AbolishedAndNationalizated = new LocalReformValue("Abolished and nationalizated land", "Aristocrats loose property", 4,
        new ConditionsList(new List<Condition>()
        {
            new Condition( Government.ProletarianDictatorship, true), Condition.IsNotImplemented
        }));


    public Serfdom(Country country) : base("Serfdom", "Aristocrats privilegies", country)
    {

        Allowed = new LocalReformValue("Allowed", "Peasants and other plebs pay 10% of income to Aristocrats", 0,
        new ConditionsList(new List<Condition>()
        {
            Economy.isNotMarket
        }));
        Brutal = new LocalReformValue("Brutal", "Peasants and other plebs pay 20% of income to Aristocrats", 1,
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
    internal override AbstractReformValue getValue(int value)
    {
        return PossibleStatuses.Find(x => x.ID == value);
        //return PossibleStatuses[value];
    }
    internal override bool canChange()
    {
        return true;
    }
    public override IEnumerator GetEnumerator()
    {
        foreach (LocalReformValue f in PossibleStatuses)
            yield return f;
    }
    internal override void setValue(AbstractReformValue selectedReform)
    {
        status = (LocalReformValue)selectedReform;
    }
    internal override bool isAvailable(Country country)
    {
        return true;
    }
    internal static Condition IsAbolishedInAnyWay = new Condition(delegate (Country forWhom)
    {
        return forWhom.serfdom.status == Serfdom.Abolished
|| forWhom.serfdom.status == Serfdom.AbolishedAndNationalizated || forWhom.serfdom.status == Serfdom.AbolishedWithLandPayment;
    },
        "Serfdom is abolished", true);
    internal static Condition IsNotAbolishedInAnyWay = new Condition(delegate (Country forWhom)
    {
        return forWhom.serfdom.status == Serfdom.Allowed
|| forWhom.serfdom.status == Serfdom.Brutal;
    },
        "Serfdom is in power", true);
}
public class MinimalWage : AbstractReform
{
    public class LocalReformValue : AbstractReformValue
    {
        public LocalReformValue(string inname, string indescription, int idin, ConditionsList condition) : base(inname, indescription, idin, condition)
        {
            PossibleStatuses.Add(this);
        }
        internal override bool isAvailable(Country country)
        {
            LocalReformValue requested = this;
            //alowed
            //if ((requested.ID == 4) && country.isInvented(InventionType.collectivism) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 4))
            //    return true;
            //else
            //if ((requested.ID == 3) && country.isInvented(InventionType.banking) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 3))
            //    return true;
            //else
            //if ((requested.ID == 2) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 2))
            //    return true;
            //else
            //    if ((requested.ID == 1) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1))
            //    return true;
            //else
            //if ((requested.ID == 0))
            //    return true;
            //else
            //    return false;
            return true;

        }
        internal override void onReformEnacted()
        {

        }
        static Procent br = new Procent(0.2f);
        static Procent al = new Procent(0.1f);
        static Procent nu = new Procent(0.0f);
        internal Procent getTax()
        {
            if (this == Minimal)
                return br;
            else
                if (this == Middle)
                return al;
            else
                return nu;
        }
    }
    internal LocalReformValue status;
    internal static List<LocalReformValue> PossibleStatuses = new List<LocalReformValue>();// { Allowed, Brutal, Abolished, AbolishedWithLandPayment, AbolishedAndNationalizated };
    internal static LocalReformValue None = new LocalReformValue("None", "", 0, new ConditionsList(new List<Condition>()
        {
            new Condition(InventionType.individualRights, true),
            new Condition(InventionType.banking, true),
            Condition.IsNotImplemented
        }));
    internal static LocalReformValue Scanty = new LocalReformValue("Scanty", "Half-hungry", 5, new ConditionsList(new List<Condition>()
        {
            new Condition(InventionType.Welfare, true),
            new Condition(Economy.LaissezFaire, true),
            Condition.IsNotImplemented
        }));
    internal static LocalReformValue Minimal = new LocalReformValue("Minimal", "Just enough to feed yourself", 1, ConditionsList.IsNotImplemented);
    //internal static LocalReformValue Minimal = new LocalReformValue("Minimal", "Just enough to feed yourself", 1);
    internal static LocalReformValue Middle = new LocalReformValue("Middle", "Plenty good wage", 2, ConditionsList.IsNotImplemented);
    internal static LocalReformValue Big = new LocalReformValue("Generous", "Can live almost likea king. Almost..", 3, ConditionsList.IsNotImplemented);
    //internal static ReformValue AbolishedAndNationalizated = new ReformValue("Abolished and nationalizated land", "Aristocrats loose property", 4);


    public MinimalWage(Country country) : base("Minimal wage", "", country)
    {
        status = None;
    }
    internal override AbstractReformValue getValue()
    {
        return status;
    }
    internal override AbstractReformValue getValue(int value)
    {
        return PossibleStatuses.Find(x => x.ID == value);
        //return PossibleStatuses[value];
    }
    internal override bool canChange()
    {
        return true;
    }
    public override IEnumerator GetEnumerator()
    {
        foreach (LocalReformValue f in PossibleStatuses)
            yield return f;
    }
    internal override void setValue(AbstractReformValue selectedReform)
    {
        status = (LocalReformValue)selectedReform;
    }
    internal override bool isAvailable(Country country)
    {
        if (country.isInvented(InventionType.Welfare))
            return true;
        else
            return false;
    }
}

public class TaxationForPoor : AbstractReform
{
    public class ReformValue : AbstractReformValue
    {
        internal Procent tax;
        public ReformValue(string inname, string indescription, Procent intarrif, int idin, ConditionsList condition) : base(inname, indescription, idin, condition)
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
        internal override void onReformEnacted()
        {

        }
    }
    ReformValue status;
    internal static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { NaturalEconomy, StateCapitalism, PlannedEconomy };
    public TaxationForPoor(Country country) : base("Taxation for poor", "", country)
    {
        for (int i = 0; i <= 10; i++)
            PossibleStatuses.Add(new ReformValue(" tax", "", new Procent(i * 0.1f), i, ConditionsList.AlwaysYes));
        status = PossibleStatuses[1];
    }
    internal override AbstractReformValue getValue()
    {
        return status;
    }
    internal override AbstractReformValue getValue(int value)
    {
        return PossibleStatuses[value];
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
    public class ReformValue : AbstractReformValue
    {
        internal Procent tax;
        public ReformValue(string inname, string indescription, Procent intarrif, int idin, ConditionsList condition) : base(inname, indescription, idin, condition)
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
        internal override void onReformEnacted()
        {

        }
    }
    ReformValue status;
    internal static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { NaturalEconomy, StateCapitalism, PlannedEconomy };
    public TaxationForRich(Country country) : base("Taxation for rich", "", country)
    {
        for (int i = 0; i <= 10; i++)
            PossibleStatuses.Add(new ReformValue(" tax", "", new Procent(i * 0.1f), i, ConditionsList.AlwaysYes));
        status = PossibleStatuses[1];
    }
    internal override AbstractReformValue getValue()
    {
        return status;
    }
    internal override AbstractReformValue getValue(int value)
    {
        return PossibleStatuses[value];
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



public class Needs
{
    //Product product;
    public PopType popType;
    //*For 1000 population*//
    //public Value needs;
    public Storage needs;
    public Needs(PopType ipopType, Product iproduct, uint iamount)
    {
        //product = iproduct;
        popType = ipopType;
        needs = new Storage(iproduct, iamount);
    }

}