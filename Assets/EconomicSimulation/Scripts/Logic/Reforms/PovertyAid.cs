using Nashet.Conditions;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.EconomicSimulation.Reforms
{
    public class PovertyAid : AbstractReform
    {
        protected PovertyAidReformValue typedValue;

        public CashedData<MoneyView> PovertyAidSize { get; protected set; }

        public static readonly PovertyAidReformValue None = new PovertyAidReformValue("No Poverty Aid", "", 0, new DoubleConditionsList(new List<Condition> { Economy.isNotLFOrMoreConservative, new Condition(x => (x as Country).PovertyAid == Scanty, "Previous reform enacted", true) }));

        public static readonly PovertyAidReformValue Scanty = new PovertyAidReformValue("Scanty Poverty Aid", "", 1, new DoubleConditionsList(new List<Condition>
        {
            Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).PovertyAid == None || (x as Country).PovertyAid == Minimal, "Previous reform enacted", true)
        }));

        public static readonly PovertyAidReformValue Minimal = new PovertyAidReformValue("Minimal Poverty Aid", "", 2, new DoubleConditionsList(new List<Condition>
        {
            Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).PovertyAid == Scanty || (x as Country).PovertyAid == Trinket, "Previous reform enacted", true)
        }));

        public static readonly PovertyAidReformValue Trinket = new PovertyAidReformValue("Trinket Poverty Aid", "", 3, new DoubleConditionsList(new List<Condition>
        {
            Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).PovertyAid == Minimal || (x as Country).PovertyAid == Middle, "Previous reform enacted", true)
        }));

        public static readonly PovertyAidReformValue Middle = new PovertyAidReformValue("Middle Poverty Aid", "", 4, new DoubleConditionsList(new List<Condition>
        {
            Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).PovertyAid == Trinket || (x as Country).PovertyAid == Big, "Previous reform enacted", true)
        }));

        public static readonly PovertyAidReformValue Big = new PovertyAidReformValue("Big Poverty Aid", "", 5, new DoubleConditionsList(new List<Condition>
        {
            Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).PovertyAid == Middle, "Previous reform enacted", true)
        }));

        public PovertyAid(Country country, int showOrder) : base("Poverty Aid", " - goes to everyone who is poorer than current reform level", country, showOrder,
            new List<IReformValue> { None, Scanty, Minimal, Trinket, Middle, Big })
        {
            PovertyAidSize = new CashedData<MoneyView>(GetPovertyAidSize);
            SetValue(None);
        }

        public override void SetValue(IReformValue selectedReform)
        {
            base.SetValue(selectedReform);
            typedValue = selectedReform as PovertyAidReformValue;
            PovertyAidSize.Recalculate();
        }        

        public override string ToString()
        {
            return base.ToString() + " (" + PovertyAidSize + " per 1000 men)";
        }

        /// <summary>
        /// Calculates Unemployment Subsidies basing on consumption cost for 1000 workers
        /// </summary>
        internal virtual MoneyView GetPovertyAidSize()
        {
            var market = owner.market;
            return typedValue.GetPovertyAidSize(market);
        }
        public class PovertyAidReformValue : NamedReformValue
        {
            internal PovertyAidReformValue(string name, string description, int id, DoubleConditionsList condition)//, Procent procent
                : base(name, description, id, condition)
            {
                LifeQualityImpact = new Procent(ID * 2f, 10f); // doubles impact
            }

            /// <summary>
            /// Calculates Unemployment Subsidies basing on consumption cost for 1000 workers
            /// </summary>
            public virtual MoneyView GetPovertyAidSize(Market market)
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
            //public override bool isAvailable(Country country)
            //{
            //    return true;
            //}

            public string ToString(Market market)
            {
                return ToString() + " (" + GetPovertyAidSize(market) + " per 1000 men)";
            }


            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                //positive - higher subsidies
                int change = GetRelativeConservatism(pop.Country.PovertyAid.typedValue);
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
    }
}