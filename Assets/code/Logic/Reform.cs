using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//public abstract class NameDescriptor
//{ }

abstract public class AbstractReformValue
{
    string name;
    string description;
    internal int ID;
    internal Condition condition;
    internal AbstractReformValue(string inname, string indescription, int IDin)
    {
        ID = IDin;
        name = inname;
        description = indescription;
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
        public ReformValue(string inname, string indescription, int idin) : base(inname, indescription, idin)
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
    }
    internal ReformValue status;
    internal static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { Tribal, Aristocracy, Despotism, Democracy, ProletarianDictatorship };
    internal static ReformValue Tribal = new ReformValue("Tribal democracy", "Tribesmen and Aristocrats can vote", 0);
    internal static ReformValue Aristocracy = new ReformValue("Aristocracy", "Only Aristocrats and Clerics can vote", 1);
    internal static ReformValue AnticRespublic = new ReformValue("Antique respublic", "Landed individuals allowed to vote, such as Farmers, Aristocrats, Clerics; each vote is equal", 8);
    internal static ReformValue Despotism = new ReformValue("Despotism", "Despot does what he wants", 2);
    internal static ReformValue Theocracy = new ReformValue("Theocracy", "Only Clerics have power", 5);

    internal static ReformValue WealthDemocracy = new ReformValue("Wealth Democracy", "Landed individuals allowed to vote, such as Farmers, Aristocrats, etc. Rich classes has more votes", 9);
    internal static ReformValue Democracy = new ReformValue("Universal Democracy", "Everyone can vote; each vote is equal", 3);
    internal static ReformValue BourgeoisDictatorship = new ReformValue("Bourgeois dictatorship", "Only capitalists have power", 6);
    internal static ReformValue MilitaryJunta = new ReformValue("Military junta", "Only military guys have power", 7);    
    
    internal static ReformValue ProletarianDictatorship = new ReformValue("Proletarian dictatorship", "ProletarianDictatorship is it. Bureaucrats rule you", 4);
    
    // more weited voting?
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
    public class LocalReformValue : AbstractReformValue
    {
       
        public LocalReformValue(string inname, string indescription, int idin, Condition condition) : base(inname, indescription, idin)
        {
            PossibleStatuses.Add(this);
            this.condition = condition;
        }
        internal override bool isAvailable(Country country)
        {
            LocalReformValue requested = this;
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
        internal override void onReformEnacted()
        {

        }
    }
    static Condition capitalism = new Condition(new List<ConditionString>()
        {
            new ConditionString(delegate (Country forWhom) { return forWhom.isInvented(InventionType.individualRights); }, "Invented Individual Rights", true),
            new ConditionString(delegate (Country forWhom) { return forWhom.isInvented(InventionType.banking); }, "Invented banking", true)
        });
    internal LocalReformValue status;
    internal static List<LocalReformValue> PossibleStatuses = new List<LocalReformValue>();// { NaturalEconomy, StateCapitalism, PlannedEconomy };
    internal static LocalReformValue NaturalEconomy = new LocalReformValue("Natural economy", " SSS", 0,Game.alwaysYesCondition);
    internal static LocalReformValue StateCapitalism = new LocalReformValue("State capitalism", "dddd", 1, capitalism);
    internal static LocalReformValue Interventionism = new LocalReformValue("Limited Interventionism", "zz", 1, capitalism );
    internal static LocalReformValue PlannedEconomy = new LocalReformValue("Planned economy", "dirty pants", 2, new Condition(new List<ConditionString>()
        {
            new ConditionString(delegate (Country forWhom) { return forWhom.isInvented(InventionType.collectivism); }, "Invented Colectivism", true),
            new ConditionString(delegate (Country forWhom) { return forWhom.government.status == Government.ProletarianDictatorship; }, "Government is Prletarian Dictatorship", true)
        }));
    internal static LocalReformValue LaissezFaire = new LocalReformValue("Laissez Faire", "not dirty pants", 3, capitalism);

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
        foreach (LocalReformValue f in PossibleStatuses)
            yield return f;
    }
    internal override void setValue(AbstractReformValue selectedReform)
    {
        status = (LocalReformValue)selectedReform;
    }

    internal bool isMarket()
    {
        if (status == Economy.NaturalEconomy || status == Economy.PlannedEconomy)
            return false;
        else
            return true;
    }
    internal override bool isAvailable(Country country)
    {
        return true;
    }

    internal bool allowsFactoryUpgradeByGovernment()
    {        
        if (status == Economy.LaissezFaire)
            return false;
        else return true;
    }
    internal bool allowsFactoryCloseByGovernment()
    {
        if (status != Economy.LaissezFaire)
            return true;
        else return false;
    }
    internal bool allowsFactoryReopenByGovernment()
    {
        if (status != Economy.LaissezFaire)
            return true;
        else return false;
    }
    internal bool allowsFactoryDestroyByGovernment()
    {
        if (status == Economy.StateCapitalism || status == Economy.PlannedEconomy || status == Economy.NaturalEconomy)
        //if (status != Economy.LaissezFaire ||)
            return true;
        else return false;
    }
    internal bool allowsFactorySellByGovernment()
    {
        if (status == Economy.LaissezFaire || status == Economy.Interventionism || status == Economy.NaturalEconomy)
            return true;
        else return false;
    }
    internal bool allowsFactoryBuyByGovernment()
    {
        if (status == Economy.StateCapitalism || status == Economy.Interventionism || status == Economy.NaturalEconomy)
            return true;
        else return false;
    }
    internal bool allowsFactoryNatonalizeByGovernment()
    {
        if (status == Economy.PlannedEconomy || status == Economy.NaturalEconomy || status == Economy.StateCapitalism)
            return true;
        else return false;
    }

    internal bool allowsFactoryBuildingByGovernment()
    {
        if (status == Economy.PlannedEconomy || status == Economy.NaturalEconomy || status == Economy.StateCapitalism)
            return true;
        else return false;
    }

    
}

public class Serfdom : AbstractReform
{
    public class LocalReformValue : AbstractReformValue
    {
        public LocalReformValue(string inname, string indescription, int idin) : base(inname, indescription, idin)
        {
            PossibleStatuses.Add(this);
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
    internal static LocalReformValue Allowed = new LocalReformValue("Allowed", "Peasants and other plebs pay 10% of income to Aristocrats", 0);
    internal static LocalReformValue Brutal = new LocalReformValue("Brutal", "Peasants and other plebs pay 20% of income to Aristocrats", 1);
    internal static LocalReformValue Abolished = new LocalReformValue("Abolished", "Abolished with no obligations", 2);
    internal static LocalReformValue AbolishedWithLandPayment = new LocalReformValue("Abolished with land payment", "Peasants are personally free now but they have to pay debt for land", 3);
    internal static LocalReformValue AbolishedAndNationalizated = new LocalReformValue("Abolished and nationalizated land", "Aristocrats loose property", 4);


    public Serfdom(Country country) : base("Serfdom", "Aristocrats privilegies", country)
    {
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
}
public class MinimalWage : AbstractReform
{
    public class LocalReformValue : AbstractReformValue
    {
        public LocalReformValue(string inname, string indescription, int idin) : base(inname, indescription, idin)
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
    internal static LocalReformValue None = new LocalReformValue("None", "", 0);
    internal static LocalReformValue Minimal = new LocalReformValue("Minimal", "Just enough to feed yourself", 1);
    internal static LocalReformValue Middle = new LocalReformValue("Middle", "Plenty good wage", 2);
    internal static LocalReformValue Big = new LocalReformValue("Generous", "Can live almost likea king. Almost..", 3);
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
        public ReformValue(string inname, string indescription, Procent intarrif, int idin) : base(inname, indescription, idin)
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
            PossibleStatuses.Add(new ReformValue(" tax", "", new Procent(i * 0.1f), i));
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



public class InventionsList
{
    internal Dictionary<InventionType, bool> list = new Dictionary<InventionType, bool>();
    public InventionsList()
    {
        foreach (var each in InventionType.allInventions)
            list.Add(each, false);
    }
    public void MarkInvented(InventionType type)
    {
        //bool result = false;
        //if (list.TryGetValue(type, out result))
        //    result = true;
        //else
        //    result = false;
        list[type] = true;
    }
    public bool isInvented(InventionType type)
    {
        bool result = false;
        list.TryGetValue(type, out result);
        return result;
    }

}
public class InventionType
{
    internal static List<InventionType> allInventions = new List<InventionType>();
    string name;
    string description;
    internal Value cost;
    public static InventionType farming = new InventionType("Farming", "Allows farming and farmers", new Value(100f)),
        //capitalism = new InventionType("Capitalism", "", new Value(50f)),
        banking = new InventionType("Banking", "Allows national bank, credits and deposits. Also allows serfdom abolishment with compensation for aristocrats", new Value(100.34f)),
        manufactories = new InventionType("Manufactories", "Allows building manufactories to process raw product\n Testestestestestest Testestestestestest Testestestestestest testestesttestestest testestest testestest", new Value(67.83f)),
        mining = new InventionType("Mining", "Allows resource gathering from holes in ground, increasing efficience", new Value(100f)),
        religion = new InventionType("Religion", "Allows clerics, gives loyalty boost", new Value(100f)),
        metal = new InventionType("Metal", "Allows metal ore and smelting. Metal is good for tools and weapons", new Value(100f)),
        individualRights = new InventionType("Individual rights", "Allows Capitalism, Serfdom & Slavery abolishments", new Value(100f)),
        collectivism = new InventionType("Collectivism", "Allows Proletarian dictatorship & Planned Economy", new Value(100f)),
        Welfare = new InventionType("Welfare", "Allows min wage and.. other", new Value(100f))
        ;
    internal InventionType(string inname, string indescription, Value incost)
    {
        name = inname;
        description = indescription;
        cost = incost;
        allInventions.Add(this);
    }
    internal InventionType()
    { }
    internal string getDescription()
    {
        return description;
    }
    override public string ToString()
    {
        return name;
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