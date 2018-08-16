using System.Collections;
using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Serfdom : AbstractReform
    {
        protected SerfValue typedValue;

        public static SerfValue SerfdomAllowed;
        public static SerfValue Brutal;

        public Procent AristocratTax { get {return typedValue.AristocratTax;  } }

        public static SerfValue Abolished = new SerfValue("Abolished", "- Abolished with no obligations", 2,
            new DoubleConditionsList(new List<Condition> { Invention.IndividualRightsInvented, Condition.IsNotImplemented }));

        public static SerfValue AbolishedWithLandPayment = new SerfValue("Abolished with land payment", "- Peasants are personally free now but they have to pay debt for land", 3,
            new DoubleConditionsList(new List<Condition>
            {
            Invention.IndividualRightsInvented,Invention.BankingInvented, Condition.IsNotImplemented
            }));

        public static SerfValue AbolishedAndNationalized = new SerfValue("Abolished and Nationalized land", "- Aristocrats loose property", 4,
            new DoubleConditionsList(new List<Condition>
            {
            Government.isProletarianDictatorship, Condition.IsNotImplemented
            }));

        public Serfdom(Country country) : base("Serfdom", "- Aristocratic Privileges", country, new List<IReformValue> { SerfdomAllowed, Brutal, Abolished, AbolishedWithLandPayment, AbolishedAndNationalized })
        {
            if (SerfdomAllowed == null)
                SerfdomAllowed = new SerfValue("Allowed", "- Peasants and other plebes pay 10% of income to Aristocrats", 1,
                    new DoubleConditionsList(new List<Condition>
                    {
            Economy.isNotMarket,  Condition.IsNotImplemented
                    }));
            if (Brutal == null)
                Brutal = new SerfValue("Brutal", "- Peasants and other plebes pay 20% of income to Aristocrats", 0,
                new DoubleConditionsList(new List<Condition>
                {
            Economy.isNotMarket, Condition.IsNotImplemented
                }));

            typedValue = SerfdomAllowed;
        }


        public void SetValue(SerfValue selectedReform)
        {
            base.SetValue(selectedReform);
            typedValue = selectedReform;
        }


        public static Condition IsAbolishedInAnyWay = new Condition(x => (x as Country).serfdom.typedValue == Abolished
        || (x as Country).serfdom.typedValue == AbolishedAndNationalized || (x as Country).serfdom.typedValue == AbolishedWithLandPayment,
            "Serfdom is abolished", true);

        public static Condition IsNotAbolishedInAnyWay = new Condition(x => (x as Country).serfdom.typedValue == SerfdomAllowed
        || (x as Country).serfdom.typedValue == Brutal,
            "Serfdom is in power", true);

       
        public class SerfValue : NamedReformValue
        {
            //private static Procent brutalTax = new Procent(0.2f);
            //private static Procent allowedTax = new Procent(0.1f);
            //private static Procent nullTax = new Procent(0.0f);

            public Procent AristocratTax { get; protected set; }

            public SerfValue(string name, string description, int id, DoubleConditionsList condition) : base(name, description, id, condition)
            {                
                // this.allowed = condition;
            }

            //public override bool isAvailable(Country country)
            //{
            //    SerfValue requested = this;

            //    if ((requested.ID == 4) && country.Science.IsInvented(Invention.Collectivism) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 4))
            //        return true;
            //    else
            //    if ((requested.ID == 3) && country.Science.IsInvented(Invention.Banking) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 3))
            //        return true;
            //    else
            //    if ((requested.ID == 2) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 2))
            //        return true;
            //    else
            //        if ((requested.ID == 1) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1))
            //        return true;
            //    else
            //    if ((requested.ID == 0))
            //        return true;
            //    else
            //        return false;
            //}



            //public Procent getTax()
            //{
            //    if (this == Brutal)
            //        return brutalTax;
            //    else
            //        if (this == SerfdomAllowed)
            //        return allowedTax;
            //    else
            //        return nullTax;
            //}
            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                int change = RelativeConservatism(pop.Country.serfdom.value); //positive - more liberal
                if (pop.Type == PopType.Aristocrats)
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


    }
    //public class wOldSerfdom : AbstractReform
    //{
    //    public class ReformValue : AbstractReformValue
    //    {
    //        public ReformValue(string inname, string indescription, int idin, DoubleConditionsList condition) : base(inname, indescription, idin, condition)
    //        {
    //            //if (!PossibleStatuses.Contains(this))
    //            PossibleStatuses.Add(this);
    //            // this.allowed = condition;
    //        }

    //        public override bool isAvailable(Country country)
    //        {
    //            ReformValue requested = this;

    //            if ((requested.ID == 4) && country.Science.IsInvented(Invention.Collectivism) && (country.serfdom.typedValue.ID == 0 || country.serfdom.typedValue.ID == 1 || country.serfdom.typedValue.ID == 4))
    //                return true;
    //            else
    //            if ((requested.ID == 3) && country.Science.IsInvented(Invention.Banking) && (country.serfdom.typedValue.ID == 0 || country.serfdom.typedValue.ID == 1 || country.serfdom.typedValue.ID == 3))
    //                return true;
    //            else
    //            if ((requested.ID == 2) && (country.serfdom.typedValue.ID == 0 || country.serfdom.typedValue.ID == 1 || country.serfdom.typedValue.ID == 2))
    //                return true;
    //            else
    //                if ((requested.ID == 1) && (country.serfdom.typedValue.ID == 0 || country.serfdom.typedValue.ID == 1))
    //                return true;
    //            else
    //            if ((requested.ID == 0))
    //                return true;
    //            else
    //                return false;
    //        }

    //        private static Procent br = new Procent(0.2f);
    //        private static Procent al = new Procent(0.1f);
    //        private static Procent nu = new Procent(0.0f);

    //        public Procent getTax()
    //        {
    //            if (this == Brutal)
    //                return br;
    //            else
    //                if (this == Allowed)
    //                return al;
    //            else
    //                return nu;
    //        }

    //        protected override Procent howIsItGoodForPop(PopUnit pop)
    //        {
    //            Procent result;
    //            int change = ID - pop.Country.serfdom.typedValue.ID; //positive - more liberal
    //            if (pop.Type == PopType.Aristocrats)
    //            {
    //                if (change > 0)
    //                    result = new Procent(0f);
    //                else
    //                    result = new Procent(1f);
    //            }
    //            else
    //            {
    //                if (change > 0)
    //                    result = new Procent(1f);
    //                else
    //                    result = new Procent(0f);
    //            }
    //            return result;
    //        }
    //    }

    //    public ReformValue status;
    //    public static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { Allowed, Brutal, Abolished, AbolishedWithLandPayment, AbolishedAndNationalizated };
    //    public static ReformValue Allowed;
    //    public static ReformValue Brutal;

    //    public static ReformValue Abolished = new ReformValue("Abolished", "- Abolished with no obligations", 2,
    //        new DoubleConditionsList(new List<Condition> { Invention.IndividualRightsInvented, Condition.IsNotImplemented }));

    //    public static ReformValue AbolishedWithLandPayment = new ReformValue("Abolished with land payment", "- Peasants are personally free now but they have to pay debt for land", 3,
    //        new DoubleConditionsList(new List<Condition>
    //        {
    //        Invention.IndividualRightsInvented,Invention.BankingInvented, Condition.IsNotImplemented
    //        }));

    //    public static ReformValue AbolishedAndNationalized = new ReformValue("Abolished and Nationalized land", "- Aristocrats loose property", 4,
    //        new DoubleConditionsList(new List<Condition>
    //        {
    //        Gov.isProletarianDictatorship, Condition.IsNotImplemented
    //        }));

    //    public wOldSerfdom(Country country) : base("Serfdom", "- Aristocratic Privileges", country)
    //    {
    //        if (Allowed == null)
    //            Allowed = new ReformValue("Allowed", "- Peasants and other plebes pay 10% of income to Aristocrats", 1,
    //                new DoubleConditionsList(new List<Condition>
    //                {
    //        Econ.isNotMarket,  Condition.IsNotImplemented
    //                }));
    //        if (Brutal == null)
    //            Brutal = new ReformValue("Brutal", "- Peasants and other plebes pay 20% of income to Aristocrats", 0,
    //            new DoubleConditionsList(new List<Condition>
    //            {
    //        Econ.isNotMarket, Condition.IsNotImplemented
    //            }));

    //        status = Allowed;
    //    }

    //    public override AbstractReformValue getValue()
    //    {
    //        return status;
    //    }

    //    //public override AbstractReformValue getValue(int value)
    //    //{
    //    //    //return PossibleStatuses.Find(x => x.ID == value);
    //    //    return PossibleStatuses[value];
    //    //}


    //    public override IEnumerator GetEnumerator()
    //    {
    //        foreach (ReformValue f in PossibleStatuses)
    //            yield return f;
    //    }

    //    public override void setValue(AbstractReformValue selectedReform)
    //    {
    //        base.setValue(selectedReform);
    //        status = (ReformValue)selectedReform;
    //    }

    //    public override bool isAvailable(Country country)
    //    {
    //        return true;
    //    }

    //    public static Condition IsAbolishedInAnyWay = new Condition(x => (x as Country).serfdom == Abolished
    //    || (x as Country).serfdom.typedValue == AbolishedAndNationalized || (x as Country).serfdom == AbolishedWithLandPayment,
    //        "Serfdom is abolished", true);

    //    public static Condition IsNotAbolishedInAnyWay = new Condition(x => (x as Country).serfdom.typedValue == Allowed
    //    || (x as Country).serfdom.typedValue == Brutal,
    //        "Serfdom is in power", true);

    //    public override bool canHaveValue(AbstractReformValue abstractReformValue)
    //    {
    //        return PossibleStatuses.Contains(abstractReformValue as ReformValue);
    //    }
    //}
}