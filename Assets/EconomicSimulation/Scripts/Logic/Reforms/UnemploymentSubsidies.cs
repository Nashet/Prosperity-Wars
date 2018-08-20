using System.Collections;
using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation.Reforms
{
    public class UnemploymentSubsidies : AbstractReform
    {

        protected UnemploymentReformValue typedvalue;

        public static readonly UnemploymentReformValue None = new UnemploymentReformValue("No Unemployment Benefits", "", 0, new DoubleConditionsList(new List<Condition>()));

        public static readonly UnemploymentReformValue Scanty = new UnemploymentReformValue("Bread Lines", "-The people are starving. Let them eat bread.", 1, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly UnemploymentReformValue Minimal = new UnemploymentReformValue("Food Stamps", "- Let the people buy what they need.", 2, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly UnemploymentReformValue Trinket = new UnemploymentReformValue("Housing & Food Assistance", "- Affordable Housing for the Unemployed.", 3, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly UnemploymentReformValue Middle = new UnemploymentReformValue("Welfare Ministry", "- Now there is a minister granting greater access to benefits.", 4, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly UnemploymentReformValue Big = new UnemploymentReformValue("Full State Unemployment Benefits", "- Full State benefits for the downtrodden.", 5, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, Economy.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public UnemploymentSubsidies(Country country) : base("Unemployment Subsidies", "", country, new List<IReformValue> { None, Scanty, Minimal, Trinket, Middle, Big })
        {
            SetValue(None);
        }

        //public bool isThatReformEnacted(int value)
        //{
        //    return typedvalue == PossibleStatuses[value];
        //}


        public void SetValue(UnemploymentReformValue selectedReform)
        {
            base.SetValue(selectedReform);
            typedvalue = selectedReform;
        }


        //public override bool isAvailable(Country country)
        //{
        //    if (country.Science.IsInvented(Invention.Welfare))
        //        return true;
        //    else
        //        return false;
        //}

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
        public class UnemploymentReformValue : NamedReformValue
        {
            internal UnemploymentReformValue(string name, string description, int id, DoubleConditionsList condition)//, Procent procent
                : base(name, description, id, condition)
            {
                LifeQualityImpact = new Procent(ID * 2f, 10f); // doubles impact
            }

            //public override bool isAvailable(Country country)
            //{
            //    return true;
            //}



            public override string ToString()
            {
                return base.ToString() + " (" + "get back getSubsidiesRate()" + ")";//getSubsidiesRate()
            }
            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                //positive - higher subsidies
                int change = RelativeConservatism(pop.Country.unemploymentSubsidies.typedvalue);
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