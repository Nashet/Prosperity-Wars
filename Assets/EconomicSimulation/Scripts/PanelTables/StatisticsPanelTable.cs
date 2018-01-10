using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.UnityUIUtils;
namespace Nashet.EconomicSimulation
{
    public class StatisticsPanelTable : MyTableNew
    {
        public override void Refresh()
        {
            StartUpdate();
            //lock (gameObject)
            {
                RemoveButtons();
                var howMuchRowsShow = CalcSize(Country.howMuchCountriesAlive());
                AddHeader();

                //for (int i = 0; i < howMuchRowsShow; i++)
                int lookingForAlive = 0;
                for (int nextRowNumber = 0; nextRowNumber < howMuchRowsShow; nextRowNumber++)
                {

                    Country country = Country.allCountries[nextRowNumber + GetRowOffset() + lookingForAlive];

                    while (!country.isAlive())
                    {
                        lookingForAlive++;
                        country = Country.allCountries[nextRowNumber + GetRowOffset() + lookingForAlive];
                    }

                    //foreach (var country in Country.getAllExisting())



                    // Adding number
                    AddButton((nextRowNumber + GetRowOffset() + 1).ToString(), country);

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

                    //AddButton(country.needsFullfilled.ToString(), country,
                    //    //() => ert.consumedTotal.ToStringWithLines()                        
                    //    () => "Consumed:\n" + country.getConsumed().getContainer().getString("\n")
                    //    );

                    ////Adding loyalty
                    //string accu;
                    //PopUnit.modifiersLoyaltyChange.getModifier(country, out accu);
                    //AddButton(country.loyalty.ToString(), country, accu);                

                }
            }
            EndUpdate();
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