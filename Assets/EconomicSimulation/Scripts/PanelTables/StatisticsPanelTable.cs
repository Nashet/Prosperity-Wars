using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.UnityUIUtils;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    public class StatisticsPanelTable : UITableNew<Country>
    {
        protected override List<Country> ContentSelector()
        {
            return Country.getAllExisting().ToList();
        }
        protected override void AddRow(Country country)
        {
            // Adding number
            AddButton((0 + GetRowOffset() + 1).ToString(), country);

            // Adding Country
            AddButton(country.ToString(), country, () => country.ToString());
            ////Adding population
            AddButton(country.getFamilyPopulation().ToString("N0"), country);

            AddButton(country.getGDP().get().ToString("N3"), country);

            AddButton(country.getGDPPer1000().ToString("F3"), country);

            AddButton(country.getGDPShare().ToString(), country);

            AddButton(country.getUnemployment().ToString(), country);

            AddButton(country.economy.getValue().ToString(), country);

            AddButton(country.getAverageNeedsFulfilling().ToString(), country);

            AddButton(country.taxationForRich.getValue().ToString(), country);
        }
        protected override void AddHeader()
        {
            AddButton("Place");
            AddButton("Country");
            AddButton("Population");
            AddButton("GDP");
            AddButton("GDP per capita", null, () => "GDP per capita per 1000 men");
            AddButton("GDP share");
            AddButton("Unemployment");
            AddButton("Economy");
            AddButton("Av. needs");
            AddButton("Rich tax");
        }
    }
}