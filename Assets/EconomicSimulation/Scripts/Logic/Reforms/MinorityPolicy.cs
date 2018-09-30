using System.Collections;
using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation.Reforms
{
    public class MinorityPolicy : AbstractReform
    {
        protected MinorityPolicyValue typedValue;

        public static MinorityPolicyValue Equality; // all can vote
        public static MinorityPolicyValue Residency; // state culture only can vote
        public static readonly MinorityPolicyValue NoRights = new MinorityPolicyValue("No Rights for Minorities", " ", 0, new DoubleConditionsList(Condition.IsNotImplemented));

        //public readonly static Condition isEquality = new Condition(x => (x as Country).minorityPolicy.getValue() == MinorityPolicy.Equality, "Minority policy is " + MinorityPolicy.Equality.getName(), true);
        //public static Condition IsResidencyPop;
        public MinorityPolicy(Country country, int showOrder) : base("Minority Policy", "", country, showOrder,
            new List<IReformValue> { Equality, Residency, NoRights })
        {
            if (Equality == null)
                Equality = new MinorityPolicyValue("Equality for Minorities", " - All cultures have same rights, assimilation is slower", 2,
                    new DoubleConditionsList(new List<Condition> { Invention.IndividualRights.Invented }));
            if (Residency == null)
                Residency = new MinorityPolicyValue("Restricted Rights for Minorities", " - Only state culture can vote, assimilation occurs except foreign core provinces", 1, new DoubleConditionsList());

            SetValue(Residency);
            //IsResidencyPop = new Condition(x => (x as PopUnit).province.getOwner().minorityPolicy.status == MinorityPolicy.Residency,
            //Residency.FullName, true);
        }

        public override void SetValue(IReformValue selectedReform)
        {
            base.SetValue(selectedReform);
            typedValue = selectedReform as MinorityPolicyValue;
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
            internal MinorityPolicyValue(string inname, string indescription, int idin, DoubleConditionsList condition) : base(inname, indescription, idin, condition)
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
                    int change = GetRelativeConservatism(pop.Country.minorityPolicy.typedValue);
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
}