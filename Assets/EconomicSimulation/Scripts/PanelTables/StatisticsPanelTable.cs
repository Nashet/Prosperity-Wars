using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.UnityUIUtils;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    public class StatisticsPanelTable : UITableNew<Country>
    {
        private SortOrder countryOrder, populationOrder, GDPOrder, GDPPerCapitaOrder, unemploymentOrder, averageNeedsOrder            ,
            richTaxOrder, economyTypeOrder, GDPShareOrder;
        public void Start()
        {
            countryOrder = new SortOrder(this, x => x.GetHashCode());
            populationOrder = new SortOrder(this, x => x.getMenPopulation());
            GDPOrder = new SortOrder(this, x => x.getGDP().get());
            GDPPerCapitaOrder = new SortOrder(this, x => x.getGDPPer1000());
            unemploymentOrder = new SortOrder(this, x => x.getUnemployment().get());
            averageNeedsOrder = new SortOrder(this, x => x.getAverageNeedsFulfilling().get());
            richTaxOrder = new SortOrder(this, x => (x.taxationForRich.getValue() as TaxationForRich.ReformValue).tax.get());
            economyTypeOrder = new SortOrder(this, x => x.economy.GetHashCode());
            GDPShareOrder = new SortOrder(this, x => x.getGDP().get());
        }
        protected override IEnumerable<Country> ContentSelector()
        {
            return Country.getAllExisting();
        }
        protected override void AddRow(Country country, int number)
        {
            // Adding number
            AddCell((number + GetRowOffset() + 1).ToString(), country);

            // Adding Country
            AddCell(country.ToString(), country, () => country.ToString());
            ////Adding population
            AddCell(country.getFamilyPopulation().ToString("N0"), country);

            AddCell(country.getGDP().get().ToString("N3"), country);

            AddCell(country.getGDPPer1000().ToString("F3"), country);

            AddCell(country.getGDPShare().ToString(), country);

            AddCell(country.getUnemployment().ToString(), country);

            AddCell(country.economy.getValue().ToString(), country);

            AddCell(country.getAverageNeedsFulfilling().ToString(), country);

            AddCell((country.taxationForRich.getValue() as TaxationForRich.ReformValue).tax.ToString(), country);
        }
        protected override void AddHeader()
        {
            if (countryOrder == null)
                Start(); //fixes Start call
            AddCell("Place");
            AddCell("Country" + countryOrder.getSymbol(), countryOrder);
            AddCell("Population" + populationOrder.getSymbol(), populationOrder);
            AddCell("GDP" + GDPOrder.getSymbol(), GDPOrder);
            AddCell("GDP per capita" + GDPPerCapitaOrder.getSymbol(), GDPPerCapitaOrder, () => "GDP per capita per 1000 men");
            AddCell("GDP share" + GDPShareOrder.getSymbol(), GDPShareOrder);
            AddCell("Unemployment" + unemploymentOrder.getSymbol(), unemploymentOrder);
            AddCell("Economy" + economyTypeOrder.getSymbol(), economyTypeOrder);
            AddCell("Av. needs" + averageNeedsOrder.getSymbol(), averageNeedsOrder);
            AddCell("Rich tax" + richTaxOrder.getSymbol(), richTaxOrder);
        }
    }
}