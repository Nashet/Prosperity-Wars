using Nashet.Conditions;
using Nashet.ValueSpace;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation.Reforms
{
    public class TaxationForRich : ProcentReform//, ICopyable<TaxationForRich>
    {
        public TaxationForRich(Country country, int showOrder) : base("Income tax for the rich", "", country, showOrder, new List<IReformValue> {
            RichTaxValue.TaxRate0, RichTaxValue.TaxRate10, RichTaxValue.TaxRate20,
            RichTaxValue.TaxRate30, RichTaxValue.TaxRate40, RichTaxValue.TaxRate50, RichTaxValue.TaxRate60,
            RichTaxValue.TaxRate70, RichTaxValue.TaxRate80, RichTaxValue.TaxRate90, RichTaxValue.TaxRate100 })
        {
            SetValue(RichTaxValue.TaxRate20);
        }
        public class RichTaxValue : ProcentReformValue
        {
            public static readonly RichTaxValue
                TaxRate0 = new RichTaxValue(0, 0f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForRich == TaxRate10, "Previous reform enacted", true))),
                TaxRate10 = new RichTaxValue(1, 0.1f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForRich == TaxRate0 || (x as Country).taxationForRich == TaxRate20, "Previous reform enacted", true))),
                TaxRate20 = new RichTaxValue(2, 0.2f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForRich == TaxRate10 || (x as Country).taxationForRich == TaxRate30, "Previous reform enacted", true))),
                TaxRate30 = new RichTaxValue(3, 0.3f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForRich == TaxRate20 || (x as Country).taxationForRich == TaxRate40, "Previous reform enacted", true))),
                TaxRate40 = new RichTaxValue(4, 0.4f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForRich == TaxRate30 || (x as Country).taxationForRich == TaxRate50, "Previous reform enacted", true))),
                TaxRate50 = new RichTaxValue(5, 0.5f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForRich == TaxRate40 || (x as Country).taxationForRich == TaxRate60, "Previous reform enacted", true))),
                TaxRate60 = new RichTaxValue(6, 0.6f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForRich == TaxRate50 || (x as Country).taxationForRich == TaxRate70, "Previous reform enacted", true))),
                TaxRate70 = new RichTaxValue(7, 0.7f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForRich == TaxRate60 || (x as Country).taxationForRich == TaxRate80, "Previous reform enacted", true))),
                TaxRate80 = new RichTaxValue(8, 0.8f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForRich == TaxRate70 || (x as Country).taxationForRich == TaxRate90, "Previous reform enacted", true))),
                TaxRate90 = new RichTaxValue(9, 0.9f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForRich == TaxRate80 || (x as Country).taxationForRich == TaxRate100, "Previous reform enacted", true))),
                TaxRate100 = new RichTaxValue(10, 1f, new DoubleConditionsList(new Condition(x => (x as Country).taxationForRich == TaxRate90, "Previous reform enacted", true)));

            internal RichTaxValue(int ID, float number, DoubleConditionsList condition) : base(ID, new Procent(number), condition)
            {
                LifeQualityImpact = new Procent(ID, 10f);
            }            
            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                
                Procent result;
                int change = GetRelativeConservatism(pop.Country.taxationForRich.tax);//positive mean higher tax
                if (pop.Type.isRichStrata())
                {
                    if (change > 0)
                        result = new Procent(0f);
                    else
                        result = new Procent(1f);
                }
                else
                {
                    if (change > 0)
                        if (get() > 0.6f)
                            result = new Procent(0.4f);
                        else
                            result = new Procent(0.5f);
                    else
                        result = new Procent(0.0f);
                }
                return result;
            }
            public override string ToString()
            {
                return Procent.ToString() + " rate for rich income tax";
            }
        }
    }
}
