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
            startUpdate();
            //lock (gameObject)
            {
                RemoveButtons();
                var howMuchRowsShow = calcSize(Country.howMuchCountriesAlive());
                addHeader();

                //for (int i = 0; i < howMuchRowsShow; i++)
                int lookingForAlive = 0;
                for (int nextRowNumber = 0; nextRowNumber < howMuchRowsShow; nextRowNumber++)
                {

                    Country country = Country.allCountries[nextRowNumber + getRowOffset() + lookingForAlive];

                    while (!country.isAlive())
                    {
                        lookingForAlive++;
                        country = Country.allCountries[nextRowNumber + getRowOffset() + lookingForAlive];
                    }

                    //foreach (var country in Country.getAllExisting())



                    // Adding number
                    addButton((nextRowNumber + getRowOffset() + 1).ToString(), country);

                    // Adding Country
                    addButton(country.ToString(), country, () => country.ToString());
                    ////Adding population
                    addButton(country.getFamilyPopulation().ToString("N0"), country);

                    addButton(country.getGDP().get().ToString("N3"), country);

                    addButton(country.getGDPPer1000().ToString("F3"), country);

                    addButton(country.getGDPShare().ToString(), country);

                    addButton(country.getUnemployment().ToString(), country);

                    addButton(country.economy.getValue().ToString(), country);

                    addButton(country.getAverageNeedsFulfilling().ToString(), country);

                    addButton(country.taxationForRich.getValue().ToString(), country);

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
            endUpdate();
        }
        protected override void addHeader()
        {
            addButton("Place");
            addButton("Country");
            addButton("Population");
            addButton("GDP");
            addButton("GDP per capita", null, () => "GDP per capita per 1000 men");
            addButton("GDP share");
            addButton("Unemployment");
            addButton("Economy");
            addButton("Av. needs");
            addButton("Rich tax");
        }
    }
}