//using System.Collections;
//using System.Collections.Generic;
//using Nashet.Conditions;
//using Nashet.ValueSpace;

//namespace Nashet.EconomicSimulation
//{
//    public class TaxationForRich : AbstractReform//, ICopyable<TaxationForRich>
//    {
//        public class ReformValue : AbstractReformStepValue
//        {
//            public Procent tax;

//            public ReformValue(string inname, string indescription, Procent intarrif, int idin, DoubleConditionsList condition) : base(inname, indescription, idin, condition, 11)
//            {
//                tax = intarrif;
//                var totalSteps = 11;
//                var previousID = ID - 1;
//                var nextID = ID + 1;
//                if (previousID >= 0 && nextID < totalSteps)
//                    condition.add(new Condition(x => (x as Country).taxationForRich.isThatReformEnacted(previousID)
//                    || (x as Country).taxationForRich.isThatReformEnacted(nextID), "Previous reform enacted", true));
//                else
//                if (nextID < totalSteps)
//                    condition.add(new Condition(x => (x as Country).taxationForRich.isThatReformEnacted(nextID), "Previous reform enacted", true));
//                else
//                if (previousID >= 0)
//                    condition.add(new Condition(x => (x as Country).taxationForRich.isThatReformEnacted(previousID), "Previous reform enacted", true));
//            }

//            public override string ToString()
//            {
//                return tax + base.ToString();
//            }

//            public override bool isAvailable(Country country)
//            {
//                //if (ID == 2 && !country.isInvented(InventionType.collectivism))
//                //    return false;
//                //else
//                return true;
//            }

//            protected override Procent howIsItGoodForPop(PopUnit pop)
//            {
//                Procent result;
//                int change = ID - pop.Country.taxationForRich.status.ID;//positive mean higher tax
//                if (pop.Type.isRichStrata())
//                {
//                    if (change > 0)
//                        result = new Procent(0f);
//                    else
//                        result = new Procent(1f);
//                }
//                else
//                {
//                    if (change > 0)
//                        if (tax.get() > 0.6f)
//                            result = new Procent(0.4f);
//                        else
//                            result = new Procent(0.5f);
//                    else
//                        result = new Procent(0.0f);
//                }
//                return result;
//            }
//        }

//        private ReformValue status;
//        public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();// { NaturalEconomy, StateCapitalism, PlannedEconomy };

//        static TaxationForRich()
//        {
//            for (int i = 0; i <= 10; i++)
//                PossibleStatuses.Add(new ReformValue(" tax for rich", "", new Procent(i * 0.1f), i, new DoubleConditionsList(new List<Condition> { Econ.isNotPlanned, Econ.taxesInsideLFLimit, Econ.taxesInsideSCLimit })));
//        }

//        public TaxationForRich(Country country) : base("Taxation for rich", "", country)
//        {
//            status = PossibleStatuses[1];
//        }

//        public bool isThatReformEnacted(int value)
//        {
//            return status == PossibleStatuses[value];
//        }

//        public override AbstractReformValue getValue()
//        {
//            return status;
//        }

//        public ReformValue getTypedValue()
//        {
//            return status;
//        }

//        //public override AbstractReformValue getValue(int value)
//        //{
//        //    return PossibleStatuses[value];
//        //}
        

//        public override IEnumerator GetEnumerator()
//        {
//            foreach (ReformValue f in PossibleStatuses)
//                yield return f;
//        }

//        public override void setValue(AbstractReformValue selectedReform)
//        {
//            base.setValue(selectedReform);
//            status = (ReformValue)selectedReform;
//        }

//        public override bool isAvailable(Country country)
//        {
//            return true;
//        }

//        public override bool canHaveValue(AbstractReformValue abstractReformValue)
//        {
//            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
//        }

//        //public TaxationForRich Copy()
//        //{
//        //    return new TaxationForRich(this);
//        //}
//    }
//}