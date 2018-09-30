using Nashet.Conditions;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation.Reforms
{
    public class MinimalWage : AbstractReform
    {
        protected MinWageReformValue typedValue;
        public CashedData<MoneyView> WageSize;

        public static readonly MinWageReformValue None = new MinWageReformValue("No Minimum Wage", "", 0, new DoubleConditionsList(new List<Condition> { Economy.isNotLFOrMoreConservative, new Condition(x => (x as Country).unemploymentSubsidies == Scanty, "Previous reform enacted", true) }));

        public static readonly MinWageReformValue Scanty = new MinWageReformValue("Scant Minimum Wage", " - Half-hungry", 1, new DoubleConditionsList(new List<Condition>
        {
            Invention.Collectivism.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).unemploymentSubsidies == None || (x as Country).unemploymentSubsidies == Minimal, "Previous reform enacted", true)
        }));

        public static readonly MinWageReformValue Minimal = new MinWageReformValue("Subsistence Minimum Wage", " - Just enough to feed yourself", 2, new DoubleConditionsList(new List<Condition>
        {
            Invention.Collectivism.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).unemploymentSubsidies == Scanty || (x as Country).unemploymentSubsidies == Trinket, "Previous reform enacted", true)
        }));

        public static readonly MinWageReformValue Trinket = new MinWageReformValue("Mid-Level Minimum Wage", " - You can buy some small stuff", 3, new DoubleConditionsList(new List<Condition>
        {
            Invention.Collectivism.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).unemploymentSubsidies == Minimal || (x as Country).unemploymentSubsidies == Middle, "Previous reform enacted", true)
        }));

        public static readonly MinWageReformValue Middle = new MinWageReformValue("Social Security", " - Minimum Wage & Retirement benefits", 4, new DoubleConditionsList(new List<Condition>
        {
            Invention.Collectivism.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).unemploymentSubsidies == Trinket || (x as Country).unemploymentSubsidies == Big, "Previous reform enacted", true)
        }));

        public static readonly MinWageReformValue Big = new MinWageReformValue("Generous Minimum Wage", " - Can live almost like a king. Almost..", 5, new DoubleConditionsList(new List<Condition>
        {
            Invention.Collectivism.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).unemploymentSubsidies == Middle, "Previous reform enacted", true)
        }));

        public MinimalWage(Country country, int showOrder) : base("Minimum wage", "", country, showOrder,
            new List<IReformValue> { None, Scanty, Minimal, Trinket, Middle, Big })
        {
            WageSize = new CashedData<MoneyView>(getMinimalWage);
            SetValue(None);            
        }

        public override void SetValue(IReformValue selectedReform)
        {
            base.SetValue(selectedReform);
            typedValue = selectedReform as MinWageReformValue;
            WageSize.Recalculate();
        }
        /// <summary>
        /// Calculates wage basing on consumption cost for 1000 workers
        /// Returns new value
        /// </summary>
        internal MoneyView getMinimalWage()
        {
            var market = owner.market;
            return typedValue.getMinimalWage(market);
        }

        public override string ToString()
        {
            return base.ToString() + " (" + WageSize + " per 1000 men)";
        }

        public class MinWageReformValue : NamedReformValue
        {
            internal MinWageReformValue(string inname, string indescription, int id, DoubleConditionsList condition)
                : base(inname, indescription, id, condition)
            {
                LifeQualityImpact = new Procent(ID, 10f);
            }
            /// <summary>
            /// Calculates wage basing on consumption cost for 1000 workers
            /// Returns new value
            /// </summary>
            internal MoneyView getMinimalWage(Market market)
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
            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                if (pop.Type == PopType.Workers)
                {
                    //positive - reform will be better for worker, [-5..+5]
                    int change = GetRelativeConservatism(pop.Country.minimalWage.typedValue); // ID - pop.Country.minimalWage.value.ID;
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
                    int change = GetRelativeConservatism(pop.Country.minimalWage.typedValue);
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                    if (change < 0)
                        result = new Procent(1f);
                    else
                        //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                        result = new Procent(0f);
                }
                return result;
            }

            public string ToString(Market market)
            {
                return base.ToString() + " (" + getMinimalWage(market) + " per 1000 men)";//getMinimalWage(market)
            }
        }
    }
}