using System.Collections;
using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Serfdom : AbstractReform
    {
        public class ReformValue : AbstractReformValue
        {
            public ReformValue(string inname, string indescription, int idin, DoubleConditionsList condition) : base(inname, indescription, idin, condition)
            {
                //if (!PossibleStatuses.Contains(this))
                PossibleStatuses.Add(this);
                // this.allowed = condition;
            }

            public override bool isAvailable(Country country)
            {
                ReformValue requested = this;

                if ((requested.ID == 4) && country.Science.IsInvented(Invention.Collectivism) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 4))
                    return true;
                else
                if ((requested.ID == 3) && country.Science.IsInvented(Invention.Banking) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 3))
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

            private static Procent br = new Procent(0.2f);
            private static Procent al = new Procent(0.1f);
            private static Procent nu = new Procent(0.0f);

            public Procent getTax()
            {
                if (this == Brutal)
                    return br;
                else
                    if (this == Allowed)
                    return al;
                else
                    return nu;
            }

            protected override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                int change = ID - pop.Country.serfdom.status.ID; //positive - more liberal
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

        public ReformValue status;
        public static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { Allowed, Brutal, Abolished, AbolishedWithLandPayment, AbolishedAndNationalizated };
        public static ReformValue Allowed;
        public static ReformValue Brutal;

        public static ReformValue Abolished = new ReformValue("Abolished", "- Abolished with no obligations", 2,
            new DoubleConditionsList(new List<Condition> { Invention.IndividualRightsInvented, Condition.IsNotImplemented }));

        public static ReformValue AbolishedWithLandPayment = new ReformValue("Abolished with land payment", "- Peasants are personally free now but they have to pay debt for land", 3,
            new DoubleConditionsList(new List<Condition>
            {
            Invention.IndividualRightsInvented,Invention.BankingInvented, Condition.IsNotImplemented
            }));

        public static ReformValue AbolishedAndNationalized = new ReformValue("Abolished and Nationalized land", "- Aristocrats loose property", 4,
            new DoubleConditionsList(new List<Condition>
            {
            Government.isProletarianDictatorship, Condition.IsNotImplemented
            }));

        public Serfdom(Country country) : base("Serfdom", "- Aristocratic Privileges", country)
        {
            if (Allowed == null)
                Allowed = new ReformValue("Allowed", "- Peasants and other plebes pay 10% of income to Aristocrats", 1,
                    new DoubleConditionsList(new List<Condition>
                    {
            Economy.isNotMarket,  Condition.IsNotImplemented
                    }));
            if (Brutal == null)
                Brutal = new ReformValue("Brutal", "- Peasants and other plebes pay 20% of income to Aristocrats", 0,
                new DoubleConditionsList(new List<Condition>
                {
            Economy.isNotMarket, Condition.IsNotImplemented
                }));

            status = Allowed;
        }

        public override AbstractReformValue getValue()
        {
            return status;
        }

        //public override AbstractReformValue getValue(int value)
        //{
        //    //return PossibleStatuses.Find(x => x.ID == value);
        //    return PossibleStatuses[value];
        //}
        public override bool canChange()
        {
            return true;
        }

        public override IEnumerator GetEnumerator()
        {
            foreach (ReformValue f in PossibleStatuses)
                yield return f;
        }

        public override void setValue(AbstractReformValue selectedReform)
        {
            base.setValue(selectedReform);
            status = (ReformValue)selectedReform;
        }

        public override bool isAvailable(Country country)
        {
            return true;
        }

        public static Condition IsAbolishedInAnyWay = new Condition(x => (x as Country).serfdom.status == Abolished
        || (x as Country).serfdom.status == AbolishedAndNationalized || (x as Country).serfdom.status == AbolishedWithLandPayment,
            "Serfdom is abolished", true);

        public static Condition IsNotAbolishedInAnyWay = new Condition(x => (x as Country).serfdom.status == Allowed
        || (x as Country).serfdom.status == Brutal,
            "Serfdom is in power", true);

        public override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }
}