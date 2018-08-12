using System.Collections;
using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class UnemploymentSubsidies : AbstractReform
    {
        public class ReformValue : AbstractReformStepValue
        {
            public ReformValue(string inname, string indescription, int idin, DoubleConditionsList condition)
                : base(inname, indescription, idin, condition, 6)
            {
                //if (!PossibleStatuses.Contains(this))
                PossibleStatuses.Add(this);
                var totalSteps = 6;
                var previousID = ID - 1;
                var nextID = ID + 1;
                if (previousID >= 0 && nextID < totalSteps)
                    condition.add(new Condition(x => (x as Country).unemploymentSubsidies.isThatReformEnacted(previousID)
                    || (x as Country).unemploymentSubsidies.isThatReformEnacted(nextID), "Previous reform enacted", true));
                else
                if (nextID < totalSteps)
                    condition.add(new Condition(x => (x as Country).unemploymentSubsidies.isThatReformEnacted(nextID), "Previous reform enacted", true));
                else
                if (previousID >= 0)
                    condition.add(new Condition(x => (x as Country).unemploymentSubsidies.isThatReformEnacted(previousID), "Previous reform enacted", true));
            }

            public override bool isAvailable(Country country)
            {
                return true;
            }

            /// <summary>
            /// Calculates Unemployment Subsidies basing on consumption cost for 1000 workers
            /// </summary>
            public MoneyView getSubsidiesRate(Market market)
            {
                if (this == None)
                    return MoneyView.Zero;
                else if (this == Scanty)
                {
                    MoneyView result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    //result.multipleInside(0.5f);
                    return result;
                }
                else if (this == Minimal)
                {
                    Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
                    Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
                    everyDayCost.Multiply(0.02m);
                    result.Add(everyDayCost);
                    return result;
                }
                else if (this == Trinket)
                {
                    Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
                    Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
                    everyDayCost.Multiply(0.04m);
                    result.Add(everyDayCost);
                    return result;
                }
                else if (this == Middle)
                {
                    Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
                    Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
                    everyDayCost.Multiply(0.06m);
                    result.Add(everyDayCost);
                    return result;
                }
                else if (this == Big)
                {
                    Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
                    Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
                    everyDayCost.Multiply(0.08m);
                    //Value luxuryCost = Country.market.getCost(PopType.workers.luxuryNeedsPer1000);
                    result.Add(everyDayCost);
                    //result.add(luxuryCost);
                    return result;
                }
                else
                {
                    Debug.Log("Unknown reform");
                    return MoneyView.Zero;
                }
            }

            public override string ToString()
            {
                return base.ToString() + " (" + "get back getSubsidiesRate()" + ")";//getSubsidiesRate()
            }

            protected override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                //positive - higher subsidies
                int change = ID - pop.Country.unemploymentSubsidies.status.ID;
                if (pop.Type.isPoorStrata())
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

        private ReformValue status;
        public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();
        public static readonly ReformValue None = new ReformValue("No Unemployment Benefits", "", 0, new DoubleConditionsList(new List<Condition>()));

        public static readonly ReformValue Scanty = new ReformValue("Bread Lines", "-The people are starving. Let them eat bread.", 1, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly ReformValue Minimal = new ReformValue("Food Stamps", "- Let the people buy what they need.", 2, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly ReformValue Trinket = new ReformValue("Housing & Food Assistance", "- Affordable Housing for the Unemployed.", 3, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly ReformValue Middle = new ReformValue("Welfare Ministry", "- Now there is a minister granting greater access to benefits.", 4, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly ReformValue Big = new ReformValue("Full State Unemployment Benefits", "- Full State benefits for the downtrodden.", 5, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public UnemploymentSubsidies(Country country) : base("Unemployment Subsidies", "", country)
        {
            status = None;
        }

        public bool isThatReformEnacted(int value)
        {
            return status == PossibleStatuses[value];
        }

        public override AbstractReformValue getValue()
        {
            return status;
        }

        //public override AbstractReformValue getValue(int value)
        //{
        //    return PossibleStatuses.Find(x => x.ID == value);
        //    //return PossibleStatuses[value];
        //}
        public override void setValue(AbstractReformValue selectedReform)
        {
            base.setValue(selectedReform);
            status = (ReformValue)selectedReform;
        }
       

        public override IEnumerator GetEnumerator()
        {
            foreach (ReformValue f in PossibleStatuses)
                yield return f;
        }

        public override bool isAvailable(Country country)
        {
            if (country.Science.IsInvented(Invention.Welfare))
                return true;
            else
                return false;
        }

        public override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }
}