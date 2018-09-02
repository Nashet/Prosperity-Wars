
using Nashet.Conditions;
using Nashet.ValueSpace;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation.Reforms
{
    public class TaxationForPoor : ProcentReform
    {
        public TaxationForPoor(Country country, int showOrder) : base("Income tax for the poor", "", country, showOrder,
            new List<IReformValue> {
            PoorTaxValue.TaxRate0, PoorTaxValue.TaxRate10, PoorTaxValue.TaxRate20,
            PoorTaxValue.TaxRate30, PoorTaxValue.TaxRate40, PoorTaxValue.TaxRate50, PoorTaxValue.TaxRate60,
            PoorTaxValue.TaxRate70, PoorTaxValue.TaxRate80, PoorTaxValue.TaxRate90, PoorTaxValue.TaxRate100 })
        {
            SetValue(PoorTaxValue.TaxRate20);
        }
        public class PoorTaxValue : ProcentReformValue
        {
            public static readonly PoorTaxValue
                TaxRate0 = new PoorTaxValue(0, 0f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForPoor == TaxRate10, "Previous reform enacted", true))),
                TaxRate10 = new PoorTaxValue(1, 0.1f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForPoor == TaxRate0 || (x as Country).taxationForPoor == TaxRate20, "Previous reform enacted", true))),
                TaxRate20 = new PoorTaxValue(2, 0.2f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForPoor == TaxRate10 || (x as Country).taxationForPoor == TaxRate30, "Previous reform enacted", true))),
                TaxRate30 = new PoorTaxValue(3, 0.3f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForPoor == TaxRate20 || (x as Country).taxationForPoor == TaxRate40, "Previous reform enacted", true))),
                TaxRate40 = new PoorTaxValue(4, 0.4f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForPoor == TaxRate30 || (x as Country).taxationForPoor == TaxRate50, "Previous reform enacted", true))),
                TaxRate50 = new PoorTaxValue(5, 0.5f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForPoor == TaxRate40 || (x as Country).taxationForPoor == TaxRate60, "Previous reform enacted", true))),
                TaxRate60 = new PoorTaxValue(6, 0.6f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForPoor == TaxRate50 || (x as Country).taxationForPoor == TaxRate70, "Previous reform enacted", true))),
                TaxRate70 = new PoorTaxValue(7, 0.7f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForPoor == TaxRate60 || (x as Country).taxationForPoor == TaxRate80, "Previous reform enacted", true))),
                TaxRate80 = new PoorTaxValue(8, 0.8f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForPoor == TaxRate70 || (x as Country).taxationForPoor == TaxRate90, "Previous reform enacted", true))),
                TaxRate90 = new PoorTaxValue(9, 0.9f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForPoor == TaxRate80 || (x as Country).taxationForPoor == TaxRate100, "Previous reform enacted", true))),
                TaxRate100 = new PoorTaxValue(10, 1f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForPoor == TaxRate90, "Previous reform enacted", true)));
            internal PoorTaxValue(int ID, float number, DoubleConditionsList condition) : base(ID, new Procent(number), condition)
            {
            }
            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                //positive mean higher tax
                int change = GetRelativeConservatism(pop.Country.taxationForPoor.tax);
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
            public override string ToString()
            {
                return Procent.ToString() + " rate for poor income tax";
            }
        }        
    }
}