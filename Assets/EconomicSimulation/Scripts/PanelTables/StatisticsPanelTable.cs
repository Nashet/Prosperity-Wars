using System.Collections.Generic;
using System.Linq;
using Nashet.UnityUIUtils;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    public class StatisticsPanelTable : UITableNew<Country>
    {
        private SortOrder countryOrder, populationOrder, GDPOrder, GDPPerCapitaOrder, unemploymentOrder, averageNeedsOrder,
            richTaxOrder, economyTypeOrder, GDPShareOrder, educationOrder;

        public void Awake()// start doesn't work somehow
        {
            countryOrder = new SortOrder(this, x => x.GetNameWeight());
            populationOrder = new SortOrder(this, x => x.AllPops.Sum(y => y.population.Get()));
            GDPOrder = new SortOrder(this, x => (float)x.getGDP().Get());
            GDPPerCapitaOrder = new SortOrder(this, x => (float)x.getGDPPer1000().Get());
            unemploymentOrder = new SortOrder(this, x => x.AllPops.GetAverageProcent(y => y.getUnemployment()).get());
            averageNeedsOrder = new SortOrder(this, x => x.AllPops.GetAverageProcent(y => y.needsFulfilled).get());
            richTaxOrder = new SortOrder(this, x => (x.taxationForRich.getValue() as TaxationForRich.ReformValue).tax.get());
            economyTypeOrder = new SortOrder(this, x => x.economy.GetType().GetHashCode());
            GDPShareOrder = new SortOrder(this, x => (float)x.getGDP().Get());
            educationOrder = new SortOrder(this, x => x.AllPops.GetAverageProcent(y => y.Education).get());
        }

        protected override IEnumerable<Country> ContentSelector()
        {
            return World.getAllExistingCountries();
        }

        protected override void AddRow(Country country, int number)
        {
            // Adding number
            AddCell((number + GetRowOffset() + 1).ToString(), country);

            // Adding Country
            if (country == Game.Player)
                AddCell(country.ToString().ToUpper(), country, () => country.ToString());
            else
                AddCell(country.ToString(), country, () => country.ToString());
            ////Adding population
            AddCell(country.getFamilyPopulation().ToString("N0"), country);

            AddCell(country.getGDP().Get().ToString("N3"), country);

            AddCell(country.getGDPPer1000().Get().ToString("F3"), country);

            AddCell(country.getGDPShare().ToString(), country);

            AddCell(country.AllPops.GetAverageProcent(x => x.getUnemployment()).ToString(), country);
            AddCell(country.AllPops.GetAverageProcent(y => y.Education).ToString(), country);

            AddCell(country.economy.getValue().ToString(), country);

            AddCell(country.AllPops.GetAverageProcent(x => x.needsFulfilled).ToString(), country);

            AddCell((country.taxationForRich.getValue() as TaxationForRich.ReformValue).tax.ToString(), country);
        }

        protected override void AddHeader()
        {
            AddCell("Place");
            AddCell("Country" + countryOrder.getSymbol(), countryOrder);
            AddCell("Population" + populationOrder.getSymbol(), populationOrder);
            AddCell("GDP" + GDPOrder.getSymbol(), GDPOrder);
            AddCell("GDP per capita" + GDPPerCapitaOrder.getSymbol(), GDPPerCapitaOrder, () => "GDP per capita per 1000 men");
            AddCell("GDP share" + GDPShareOrder.getSymbol(), GDPShareOrder);
            AddCell("Unemployment" + unemploymentOrder.getSymbol(), unemploymentOrder);
            AddCell("Education" + educationOrder.getSymbol(), educationOrder);

            AddCell("Economy" + economyTypeOrder.getSymbol(), economyTypeOrder);
            AddCell("Needs fulfill" + averageNeedsOrder.getSymbol(), averageNeedsOrder);
            AddCell("Rich tax" + richTaxOrder.getSymbol(), richTaxOrder);
        }
    }
}