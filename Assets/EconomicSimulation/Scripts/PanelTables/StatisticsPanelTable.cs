using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.UnityUIUtils;
using System.Linq;
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
            populationOrder = new SortOrder(this, x => x.GetAllPopulation().Sum(y=>y.getPopulation()));
            GDPOrder = new SortOrder(this, x => x.getGDP().get());
            GDPPerCapitaOrder = new SortOrder(this, x => x.getGDPPer1000());
            unemploymentOrder = new SortOrder(this, x => x.GetAllPopulation().GetAverageProcent(y => y.getUnemployment()).get());
            averageNeedsOrder = new SortOrder(this, x => x.GetAllPopulation().GetAverageProcent(y => y.needsFulfilled).get());
            richTaxOrder = new SortOrder(this, x => (x.taxationForRich.getValue() as TaxationForRich.ReformValue).tax.get());
            economyTypeOrder = new SortOrder(this, x => x.economy.GetType().GetHashCode());
            GDPShareOrder = new SortOrder(this, x => x.getGDP().get());
            educationOrder = new SortOrder(this, x => x.GetAllPopulation().GetAverageProcent(y => y.Education).get());
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

            AddCell(country.getGDP().get().ToString("N3"), country);

            AddCell(country.getGDPPer1000().ToString("F3"), country);

            AddCell(country.getGDPShare().ToString(), country);

            AddCell(country.GetAllPopulation().GetAverageProcent(x=>x.getUnemployment()).ToString(), country);
            AddCell(country.GetAllPopulation().GetAverageProcent(y => y.Education).ToString(), country);

            AddCell(country.economy.getValue().ToString(), country);

            AddCell(country.GetAllPopulation().GetAverageProcent(x => x.needsFulfilled).ToString(), country);

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