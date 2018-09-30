using Nashet.Conditions;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.EconomicSimulation.Reforms
{
    public class UnemploymentSubsidies : AbstractReform
    {
        protected UnemploymentReformValue typedValue;

        public CashedData<MoneyView> SubsizionSize { get; protected set; }

        public static readonly UnemploymentReformValue None = new UnemploymentReformValue("No Unemployment Benefits", "", 0, new DoubleConditionsList(new List<Condition> { Economy.isNotLFOrMoreConservative, new Condition(x => (x as Country).unemploymentSubsidies == Scanty, "Previous reform enacted", true) }));

        public static readonly UnemploymentReformValue Scanty = new UnemploymentReformValue("Bread Lines", " - The people are starving. Let them eat bread.", 1, new DoubleConditionsList(new List<Condition>
        {
            Invention.Welfare.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).unemploymentSubsidies == None || (x as Country).unemploymentSubsidies == Minimal, "Previous reform enacted", true)
        }));

        public static readonly UnemploymentReformValue Minimal = new UnemploymentReformValue("Food Stamps", " - Let the people buy what they need.", 2, new DoubleConditionsList(new List<Condition>
        {
            Invention.Welfare.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).unemploymentSubsidies == Scanty || (x as Country).unemploymentSubsidies == Trinket, "Previous reform enacted", true)
        }));

        public static readonly UnemploymentReformValue Trinket = new UnemploymentReformValue("Housing & Food Assistance", " - Affordable Housing for the Unemployed.", 3, new DoubleConditionsList(new List<Condition>
        {
            Invention.Welfare.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).unemploymentSubsidies == Minimal || (x as Country).unemploymentSubsidies == Middle, "Previous reform enacted", true)
        }));

        public static readonly UnemploymentReformValue Middle = new UnemploymentReformValue("Welfare Ministry", " - Now there is a minister granting greater access to benefits.", 4, new DoubleConditionsList(new List<Condition>
        {
            Invention.Welfare.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).unemploymentSubsidies == Trinket || (x as Country).unemploymentSubsidies == Big, "Previous reform enacted", true)
        }));

        public static readonly UnemploymentReformValue Big = new UnemploymentReformValue("Full State Unemployment Benefits", " - Full State benefits for the downtrodden.", 5, new DoubleConditionsList(new List<Condition>
        {
            Invention.Welfare.Invented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned, new Condition(x => (x as Country).unemploymentSubsidies == Middle, "Previous reform enacted", true)
        }));

        public UnemploymentSubsidies(Country country, int showOrder) : base("Unemployment Subsidies", "", country, showOrder, 
            new List<IReformValue> { None, Scanty, Minimal, Trinket, Middle, Big })
        {
            SubsizionSize = new CashedData<MoneyView>(GetSubsidiesRate);
            SetValue(None);
        }

        //public bool isThatReformEnacted(int value)
        //{
        //    return typedvalue == PossibleStatuses[value];
        //}


        public override void SetValue(IReformValue selectedReform)
        {
            base.SetValue(selectedReform);
            typedValue = selectedReform as UnemploymentReformValue;
            SubsizionSize.Recalculate();
        }



        //public override bool isAvailable(Country country)
        //{
        //    if (country.Science.IsInvented(Invention.Welfare))
        //        return true;
        //    else
        //        return false;
        //}

        public override string ToString()
        {
            return base.ToString() + " (" + SubsizionSize + " per 1000 men)";
        }

        /// <summary>
        /// Calculates Unemployment Subsidies basing on consumption cost for 1000 workers
        /// </summary>
        protected virtual MoneyView GetSubsidiesRate()
        {
            var market = owner.market;
            return typedValue.GetSubsidiesRate(market);
        }

        public class UnemploymentReformValue : NamedReformValue
        {
            internal UnemploymentReformValue(string name, string description, int id, DoubleConditionsList condition)//, Procent procent
                : base(name, description, id, condition)
            {
                LifeQualityImpact = new Procent(ID * 2f, 10f); // doubles impact
            }
            /// <summary>
            /// Calculates Unemployment Subsidies basing on consumption cost for 1000 workers
            /// </summary>
            public virtual MoneyView GetSubsidiesRate(Market market)
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
                return ToString() + " (" + GetSubsidiesRate(market) + " per 1000 men)";
            }


            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                //positive - higher subsidies
                int change = GetRelativeConservatism(pop.Country.unemploymentSubsidies.typedValue);
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