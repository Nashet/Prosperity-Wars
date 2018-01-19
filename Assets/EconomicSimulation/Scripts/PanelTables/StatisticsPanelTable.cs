using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.UnityUIUtils;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    public class StatisticsPanelTable : UITableNew<Country>
    {
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

            AddCell(country.taxationForRich.getValue().ToString(), country);
        }
        protected override void AddHeader()
        {
            AddCell("Place");
            AddCell("Country");
            AddCell("Population");
            AddCell("GDP");
            AddCell("GDP per capita", null, () => "GDP per capita per 1000 men");
            AddCell("GDP share");
            AddCell("Unemployment");
            AddCell("Economy");
            AddCell("Av. needs");
            AddCell("Rich tax");
        }
    }
}