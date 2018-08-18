//using System.Collections;
//using System.Collections.Generic;
//using Nashet.Conditions;
//using Nashet.ValueSpace;

using Nashet.ValueSpace;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public class TaxationForPoor : ProcentReform
    {
        public TaxationForPoor(Country country) : base("Taxation for poor", "", country, new List<IReformValue> { new RefValue(0, 0f), new RefValue(5, 0.5f), new RefValue(10, 1f) })
        {
            tax = new RefValue(1, 0.1f);
        }


        public class RefValue : ProcentReformVal
        {
            public RefValue(int ID, float number) : base(ID, new Procent(number))
            {
            }
            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result; // poor
                                //positive mean higher tax
                int change = RelativeConservatism(pop.Country.taxationForPoor.tax);
                if (pop.Type.isPoorStrata())
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
        //    public class TaxationForPoor : AbstractReform
        //    {
        //        public class ReformValue : AbstractReformStepValue
        //        {
        //            public Procent tax;

        //            public ReformValue(string name, string description, Procent tarrif, int ID, DoubleConditionsList condition) : base(name, description, ID, condition, 11)
        //            {
        //                tax = tarrif;
        //                var totalSteps = 11;
        //                var previousID = ID - 1;
        //                var nextID = ID + 1;
        //                if (previousID >= 0 && nextID < totalSteps)
        //                    condition.add(new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(previousID)
        //                    || (x as Country).taxationForPoor.isThatReformEnacted(nextID), "Previous reform enacted", true));
        //                else if (nextID < totalSteps)
        //                    condition.add(new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(nextID), "Previous reform enacted", true));
        //                else if (previousID >= 0)
        //                    condition.add(new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(previousID), "Previous reform enacted", true));
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
        //                //positive mean higher tax
        //                int change = ID - pop.Country.taxationForPoor.status.ID;
        //                if (pop.Type.isPoorStrata())
        //                {
        //                    if (change > 0)
        //                        result = new Procent(0f);
        //                    else
        //                        result = new Procent(1f);
        //                }
        //                else
        //                {
        //                    result = new Procent(0.5f);
        //                }
        //                return result;
        //            }
        //        }

        //        private ReformValue status;
        //        public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();// { NaturalEconomy, StateCapitalism, PlannedEconomy };

        //        static TaxationForPoor()
        //        {
        //            for (int i = 0; i <= 10; i++)
        //                PossibleStatuses.Add(new ReformValue(" tax for poor", "", new Procent(i * 0.1f), i, new DoubleConditionsList(new List<Condition> { Econ.isNotPlanned, Economy.taxesInsideLFLimit, Economy.taxesInsideSCLimit })));
        //        }

        //        public TaxationForPoor(Country country) : base("Taxation for poor", "", country)
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
        //    }
    }
}