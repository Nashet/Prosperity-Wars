using Nashet.Conditions;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.EconomicSimulation.Reforms
{
    public class UBI : AbstractReform
    {
        protected UBIReformValue typedValue;
        public CashedData<MoneyView> UBISize;

        public static readonly UBIReformValue None = new UBIReformValue("No UBI", "", 0, new DoubleConditionsList(new List<Condition> { Economy.isNotLFOrMoreConservative, new Condition(x => (x as Country).UBI == Scanty, "Previous reform enacted", true) }));

        public static readonly UBIReformValue Scanty = new UBIReformValue("Scant UBI", " - Half-hungry", 1, new DoubleConditionsList(new List<Condition>
        {
            Invention.Welfare.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).UBI == None || (x as Country).UBI == Minimal, "Previous reform enacted", true)
        }));

        public static readonly UBIReformValue Minimal = new UBIReformValue("Subsistence UBI", " - Just enough to feed yourself", 2, new DoubleConditionsList(new List<Condition>
        {
            Invention.Welfare.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).UBI == Scanty || (x as Country).UBI == Trinket, "Previous reform enacted", true)
        }));

        public static readonly UBIReformValue Trinket = new UBIReformValue("Mid-Level UBI", " - You can buy some small stuff", 3, new DoubleConditionsList(new List<Condition>
        {
            Invention.Welfare.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).UBI == Minimal || (x as Country).UBI == Middle, "Previous reform enacted", true)
        }));

        public static readonly UBIReformValue Middle = new UBIReformValue("Mediocre UBI", " - Pops will start to leave job with that benefits", 4, new DoubleConditionsList(new List<Condition>
        {
            Invention.Welfare.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).UBI == Trinket || (x as Country).UBI == Big, "Previous reform enacted", true)
        }));

        public static readonly UBIReformValue Big = new UBIReformValue("Generous UBI", " - Can live almost like a king. Almost..", 5, new DoubleConditionsList(new List<Condition>
        {
            Invention.Welfare.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).UBI == Middle, "Previous reform enacted", true)
        }));

        
        public UBI(Country country, int showOrder) : base("Unconditional basic income", " - give money unconditionally to every citizen",
            country, showOrder, new List<IReformValue> { None, Scanty, Minimal, Trinket, Middle, Big })
        {
            UBISize = new CashedData<MoneyView>(GetUBISize);
            SetValue(None);
        }

        /// <summary>
        /// Gives UBI size for 1000 persons
        /// </summary>        
        protected MoneyView GetUBISize()
        {
            var market = owner.market;
            return typedValue.GetUBISize(market);
        }

        public override void SetValue(IReformValue reformValue)
        {
            base.SetValue(reformValue);
            typedValue = reformValue as UBIReformValue;
            UBISize.Recalculate();
        }

        public override string ToString()
        {
            return base.ToString() + " (" + UBISize + " per 1000 men)";
        }

        public class UBIReformValue : NamedReformValue
        {
            public UBIReformValue(string name, string description, int id, DoubleConditionsList condition) : base(name, description, id, condition)
            {
            }

            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                //positive - higher subsidies
                int change = GetRelativeConservatism(pop.Country.UBI.typedValue);
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

            internal MoneyView GetUBISize(Market market)
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
            public string ToString(Market market)
            {
                return ToString() + " (" + GetUBISize(market) + " per 1000 men)";
            }
        }
    }
}