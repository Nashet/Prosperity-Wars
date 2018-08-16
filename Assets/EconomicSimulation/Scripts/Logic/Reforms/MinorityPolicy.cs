using System.Collections;
using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class MinorityPolicy : AbstractReform
    {
        protected MinorityPolicyValue typedValue;

        public static MinorityPolicyValue Equality; // all can vote
        public static MinorityPolicyValue Residency; // state culture only can vote
        public static readonly MinorityPolicyValue NoRights = new MinorityPolicyValue("No Rights for Minorities", "-Slavery?", 0, new DoubleConditionsList(Condition.IsNotImplemented));

        //public readonly static Condition isEquality = new Condition(x => (x as Country).minorityPolicy.getValue() == MinorityPolicy.Equality, "Minority policy is " + MinorityPolicy.Equality.getName(), true);
        //public static Condition IsResidencyPop;
        public MinorityPolicy(Country country) : base("Minority Policy", "- Minority Policy", country, new List<IReformValue> { Equality, Residency, NoRights })
        {
            if (Equality == null)
                Equality = new MinorityPolicyValue("Equality for Minorities", "- All cultures have same rights, assimilation is slower.", 2,
                    new DoubleConditionsList(new List<Condition> { Invention.IndividualRightsInvented }));
            if (Residency == null)
                Residency = new MinorityPolicyValue("Restricted Rights for Minorities", "- Only state culture can vote, assimilation occurs except foreign core provinces", 1, new DoubleConditionsList());

            typedValue = Residency;
            //IsResidencyPop = new Condition(x => (x as PopUnit).province.getOwner().minorityPolicy.status == MinorityPolicy.Residency,
            //Residency.FullName, true);
        }

        public void SetValue(MinorityPolicyValue selectedReform)
        {
            base.SetValue(selectedReform);
            typedValue = selectedReform;
        }

        //public override bool isAvailable(Country country)
        //{
        //    return true;
        //}

        //public static Condition IsResidency = new Condition(x => (x as Country).minorityPolicy.status == MinorityPolicy.Residency,
        //    Residency.FullName, true);

        //public static Condition IsEquality = new Condition(x => (x as Country).minorityPolicy.status == MinorityPolicy.Equality,
        //    Equality.FullName, true);      




        public class MinorityPolicyValue : NamedReformValue
        {
            public MinorityPolicyValue(string inname, string indescription, int idin, DoubleConditionsList condition) : base(inname, indescription, idin, condition)
            {

            }

            //public override bool isAvailable(Country country)
            //{
            //    MinPOlValue requested = this;
            //    if ((requested.ID == 4) && country.Science.IsInvented(Invention.Collectivism) && (country.serfdom == Serfdom.Brutal || country.serfdom == Serfdom.Allowed || country.serfdom == Serfdom.AbolishedAndNationalized))
            //        return true;
            //    else
            //    if ((requested.ID == 3) && country.Science.IsInvented(Invention.Banking) && (country.serfdom == Serfdom.Brutal || country.serfdom == Serfdom.Allowed || country.serfdom == Serfdom.AbolishedWithLandPayment))
            //        return true;
            //    else
            //    if ((requested.ID == 2) && (country.serfdom == Serfdom.Brutal || country.serfdom == Serfdom.Allowed || country.serfdom == Serfdom.Abolished))
            //        return true;
            //    else
            //        if ((requested.ID == 1) && (country.serfdom == Serfdom.Brutal || country.serfdom == Serfdom.Allowed))
            //        return true;
            //    else
            //    if ((requested.ID == 0))
            //        return true;
            //    else
            //        return false;
            //}

            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                if (pop.isStateCulture())
                {
                    result = new Procent(0f);//0.5f);
                }
                else
                {
                    //positive - more rights for minorities
                    int change = RelativeConservatism(pop.Country.minorityPolicy.value);
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                    if (change > 0)
                        result = new Procent(0.3f);// 1f);
                    else
                        //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                        result = new Procent(0f);
                }
                return result;
            }
        }
    }
    //public class OldMinorityPolicy : AbstractReform
    //{
    //    public class ReformValue : AbstractReformValue
    //    {
    //        public ReformValue(string inname, string indescription, int idin, DoubleConditionsList condition) : base(inname, indescription, idin, condition)
    //        {
    //            PossibleStatuses.Add(this);
    //        }

    //        public override bool isAvailable(Country country)
    //        {
    //            ReformValue requested = this;
    //            if ((requested.ID == 4) && country.Science.IsInvented(Invention.Collectivism) && (country.serfdom == Serfdom.Brutal || country.serfdom == Serfdom.Allowed || country.serfdom == Serfdom.AbolishedAndNationalized))
    //                return true;
    //            else
    //            if ((requested.ID == 3) && country.Science.IsInvented(Invention.Banking) && (country.serfdom == Serfdom.Brutal || country.serfdom == Serfdom.Allowed || country.serfdom == Serfdom.AbolishedWithLandPayment))
    //                return true;
    //            else
    //            if ((requested.ID == 2) && (country.serfdom == Serfdom.Brutal || country.serfdom == Serfdom.Allowed || country.serfdom == Serfdom.Abolished))
    //                return true;
    //            else
    //                if ((requested.ID == 1) && (country.serfdom == Serfdom.Brutal || country.serfdom == Serfdom.Allowed))
    //                return true;
    //            else
    //            if ((requested.ID == 0))
    //                return true;
    //            else
    //                return false;
    //        }

    //        protected override Procent howIsItGoodForPop(PopUnit pop)
    //        {
    //            Procent result;
    //            if (pop.isStateCulture())
    //            {
    //                result = new Procent(0f);//0.5f);
    //            }
    //            else
    //            {
    //                //positive - more rights for minorities
    //                int change = ID - pop.Country.minorityPolicy.status.ID;
    //                //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
    //                if (change > 0)
    //                    result = new Procent(0.3f);// 1f);
    //                else
    //                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
    //                    result = new Procent(0f);
    //            }
    //            return result;
    //        }
    //    }

    //    private ReformValue status;
    //    public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();
    //    public static ReformValue Equality; // all can vote
    //    public static ReformValue Residency; // state culture only can vote
    //    public static readonly ReformValue NoRights = new ReformValue("No Rights for Minorities", "-Slavery?", 0, new DoubleConditionsList(Condition.IsNotImplemented));

    //    //public readonly static Condition isEquality = new Condition(x => (x as Country).minorityPolicy.getValue() == MinorityPolicy.Equality, "Minority policy is " + MinorityPolicy.Equality.getName(), true);
    //    //public static Condition IsResidencyPop;
    //    public OldMinorityPolicy(Country country) : base("Minority Policy", "- Minority Policy", country)
    //    {
    //        if (Equality == null)
    //            Equality = new ReformValue("Equality for Minorities", "- All cultures have same rights, assimilation is slower.", 2,
    //                new DoubleConditionsList(new List<Condition> { Invention.IndividualRightsInvented }));
    //        if (Residency == null)
    //            Residency = new ReformValue("Restricted Rights for Minorities", "- Only state culture can vote, assimilation occurs except foreign core provinces", 1, new DoubleConditionsList());

    //        status = Residency;
    //        //IsResidencyPop = new Condition(x => (x as PopUnit).province.getOwner().minorityPolicy.status == MinorityPolicy.Residency,
    //        //Residency.FullName, true);
    //    }

    //    public override AbstractReformValue getValue()
    //    {
    //        return status;
    //    }

    //    //public override AbstractReformValue getValue(int value)
    //    //{
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

    //    //public static Condition IsResidency = new Condition(x => (x as Country).minorityPolicy.status == MinorityPolicy.Residency,
    //    //    Residency.FullName, true);

    //    //public static Condition IsEquality = new Condition(x => (x as Country).minorityPolicy.status == MinorityPolicy.Equality,
    //    //    Equality.FullName, true);
    //    public override bool canHaveValue(AbstractReformValue abstractReformValue)
    //    {
    //        return PossibleStatuses.Contains(abstractReformValue as ReformValue);
    //    }
    //}
}