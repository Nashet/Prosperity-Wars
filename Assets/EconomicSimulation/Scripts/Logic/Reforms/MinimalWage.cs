using System.Collections;
using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation.Reforms
{
    public class MinimalWage : AbstractReform
    {
        protected MinWageReformValue typedValue;

        public static readonly MinWageReformValue None = new MinWageReformValue("No Minimum Wage", "", 0, new DoubleConditionsList(new List<Condition> { Economy.isNotLFOrMoreConservative }));

        public static readonly MinWageReformValue Scanty = new MinWageReformValue("Scant Minimum Wage", "- Half-hungry", 1, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly MinWageReformValue Minimal = new MinWageReformValue("Subsistence Minimum Wage", "- Just enough to feed yourself", 2, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly MinWageReformValue Trinket = new MinWageReformValue("Mid-Level Minimum Wage", "- You can buy some small stuff", 3, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly MinWageReformValue Middle = new MinWageReformValue("Social Security", "- Minimum Wage & Retirement benefits", 4, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly MinWageReformValue Big = new MinWageReformValue("Generous Minimum Wage", "- Can live almost like a king. Almost..", 5, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public MinimalWage(Country country) : base("Minimum wage", "", country, new List<IReformValue> { None, Scanty, Minimal, Trinket, Middle, Big })
        {

            SetValue(None);
        }

        public void SetValue(MinWageReformValue selectedReform)
        {
            base.SetValue(selectedReform);
            typedValue = selectedReform;
        }
        /// <summary>
        /// Calculates wage basing on consumption cost for 1000 workers
        /// Returns new value
        /// </summary>
        public MoneyView getMinimalWage(Market market)
        {
            if (this == None)
                return Money.Zero;
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
                return new Money(0m);
        }

        //public override bool isAvailable(Country country)
        //{
        //    if (country.Science.IsInvented(Invention.Welfare))
        //        return true;
        //    else
        //        return false;
        //}




        public class MinWageReformValue : NamedReformValue
        {
            internal MinWageReformValue(string inname, string indescription, int id, DoubleConditionsList condition)
                : base(inname, indescription, id, condition)
            {
                LifeQualityImpact = new Procent(ID, 10f);
            }

            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                if (pop.Type == PopType.Workers)
                {
                    //positive - reform will be better for worker, [-5..+5]
                    int change = RelativeConservatism(pop.Country.minimalWage.typedValue); // ID - pop.Country.minimalWage.value.ID;
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                    if (change > 0)
                        result = new Procent(1f);
                    else
                        //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                        result = new Procent(0f);
                }
                else if (pop.Type.isPoorStrata())
                    result = new Procent(0.5f);
                else // rich strata
                {
                    //negative - reform will be better for rich strata, [-5..+5]
                    int change = RelativeConservatism(pop.Country.minimalWage.typedValue);
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                    if (change < 0)
                        result = new Procent(1f);
                    else
                        //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                        result = new Procent(0f);
                }
                return result;
            }





            public override string ToString()//Market market
            {
                return base.ToString();// +" (" + "getwage back" + ")";//getMinimalWage(market)
            }


        }


    }
    //public class OldMinimalWage : AbstractReform
    //{
    //    public class ReformValue : AbstractReformStepValue
    //    {
    //        public ReformValue(string inname, string indescription, int id, DoubleConditionsList condition)
    //            : base(inname, indescription, id, condition, 6)
    //        {
    //            // if (!PossibleStatuses.Contains(this))
    //            PossibleStatuses.Add(this);
    //            var totalSteps = 6;
    //            var previousID = ID - 1;
    //            var nextID = ID + 1;
    //            if (previousID >= 0 && nextID < totalSteps)
    //                condition.add(new Condition(x => (x as Country).minimalWage.isThatReformEnacted(previousID)
    //                || (x as Country).minimalWage.isThatReformEnacted(nextID), "Previous reform enacted", true));
    //            else
    //            {
    //                if (nextID < totalSteps)
    //                    condition.add(new Condition(x => (x as Country).minimalWage.isThatReformEnacted(nextID), "Previous reform enacted", true));
    //                else
    //                {
    //                    if (previousID >= 0)
    //                        condition.add(new Condition(x => (x as Country).minimalWage.isThatReformEnacted(previousID), "Previous reform enacted", true));
    //                }
    //            }
    //        }

    //        public override bool isAvailable(Country country)
    //        {
    //            return true;
    //        }

    //        /// <summary>
    //        /// Calculates wage basing on consumption cost for 1000 workers
    //        /// Returns new value
    //        /// </summary>
    //        public MoneyView getMinimalWage(Market market)
    //        {
    //            if (this == None)
    //                return Money.Zero;
    //            else if (this == Scanty)
    //            {
    //                MoneyView result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
    //                //result.multipleInside(0.5f);
    //                return result;
    //            }
    //            else if (this == Minimal)
    //            {
    //                Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
    //                Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
    //                everyDayCost.Multiply(0.02m);
    //                result.Add(everyDayCost);
    //                return result;
    //            }
    //            else if (this == Trinket)
    //            {
    //                Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
    //                Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
    //                everyDayCost.Multiply(0.04m);
    //                result.Add(everyDayCost);
    //                return result;
    //            }
    //            else if (this == Middle)
    //            {
    //                Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
    //                Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
    //                everyDayCost.Multiply(0.06m);
    //                result.Add(everyDayCost);
    //                return result;
    //            }
    //            else if (this == Big)
    //            {
    //                Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
    //                Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
    //                everyDayCost.Multiply(0.08m);
    //                //Value luxuryCost = Country.market.getCost(PopType.workers.luxuryNeedsPer1000);
    //                result.Add(everyDayCost);
    //                //result.add(luxuryCost);
    //                return result;
    //            }
    //            else
    //                return new Money(0m);
    //        }

    //        public override string ToString()//Market market
    //        {
    //            return base.ToString();// +" (" + "getwage back" + ")";//getMinimalWage(market)
    //        }

    //        protected override Procent howIsItGoodForPop(PopUnit pop)
    //        {
    //            Procent result;
    //            if (pop.Type == PopType.Workers)
    //            {
    //                //positive - reform will be better for worker, [-5..+5]
    //                int change = ID - pop.Country.minimalWage.status.ID;
    //                //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
    //                if (change > 0)
    //                    result = new Procent(1f);
    //                else
    //                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
    //                    result = new Procent(0f);
    //            }
    //            else if (pop.Type.isPoorStrata())
    //                result = new Procent(0.5f);
    //            else // rich strata
    //            {
    //                //positive - reform will be better for rich strata, [-5..+5]
    //                int change = pop.Country.minimalWage.status.ID - ID;
    //                //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
    //                if (change > 0)
    //                    result = new Procent(1f);
    //                else
    //                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
    //                    result = new Procent(0f);
    //            }
    //            return result;
    //        }
    //    }

    //    private ReformValue status;

    //    public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();
    //    public static readonly ReformValue None = new ReformValue("No Minimum Wage", "", 0, new DoubleConditionsList(new List<Condition> { AbstractReformValue.isNotLFOrMoreConservative }));

    //    public static readonly ReformValue Scanty = new ReformValue("Scant Minimum Wage", "- Half-hungry", 1, new DoubleConditionsList(new List<Condition>
    //    {
    //        Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Econ.isNotPlanned
    //    }));

    //    public static readonly ReformValue Minimal = new ReformValue("Subsistence Minimum Wage", "- Just enough to feed yourself", 2, new DoubleConditionsList(new List<Condition>
    //    {
    //        Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Econ.isNotPlanned
    //    }));

    //    public static readonly ReformValue Trinket = new ReformValue("Mid-Level Minimum Wage", "- You can buy some small stuff", 3, new DoubleConditionsList(new List<Condition>
    //    {
    //        Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Econ.isNotPlanned
    //    }));

    //    public static readonly ReformValue Middle = new ReformValue("Social Security", "- Minimum Wage & Retirement benefits", 4, new DoubleConditionsList(new List<Condition>
    //    {
    //        Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Econ.isNotPlanned
    //    }));

    //    public static readonly ReformValue Big = new ReformValue("Generous Minimum Wage", "- Can live almost like a king. Almost..", 5, new DoubleConditionsList(new List<Condition>
    //    {
    //        Invention.WelfareInvented,AbstractReformValue.isNotLFOrMoreConservative, Econ.isNotPlanned
    //    }));

    //    public OldMinimalWage(Country country) : base("Minimum wage", "", country)
    //    {
    //        status = None;
    //    }

    //    public bool isThatReformEnacted(int value)
    //    {
    //        return status == PossibleStatuses[value];
    //    }

    //    public override AbstractReformValue getValue()
    //    {
    //        return status;
    //    }

    //    //public override AbstractReformValue getValue(int value)
    //    //{
    //    //    return PossibleStatuses.Find(x => x.ID == value);
    //    //    //return PossibleStatuses[value];
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
    //        if (country.Science.IsInvented(Invention.Welfare))
    //            return true;
    //        else
    //            return false;
    //    }

    //    public override bool canHaveValue(AbstractReformValue abstractReformValue)
    //    {
    //        return PossibleStatuses.Contains(abstractReformValue as ReformValue);
    //    }
    //}
}