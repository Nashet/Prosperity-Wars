using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace Nashet.EconomicSimulation
{
    public class PopulationPanelTable : MyTableNew
    {
        public override void refreshContent()
        {
            startUpdate();
            //lock (gameObject)
            {
                RemoveButtons();
                var howMuchRowsShow = calcSize(Game.popsToShowInPopulationPanel.Count);
                addHeader();
                if (Game.popsToShowInPopulationPanel.Count > 0)
                {
                    for (int i = 0; i < howMuchRowsShow; i++)
                    //foreach (PopUnit record in Game.popsToShowInPopulationPanel)
                    {
                        PopUnit pop = Game.popsToShowInPopulationPanel[i + getRowOffset()];

                        // Adding number
                        //AddButton(Convert.ToString(i + offset), record);

                        // Adding PopType
                        addButton(pop.popType.ToString(), pop);
                        ////Adding province
                        addButton(pop.getProvince().ToString(), pop.getProvince(), () => "Click to select this province");
                        ////Adding population
                        addButton(System.Convert.ToString(pop.getPopulation()), pop);
                        ////Adding culture
                        addButton(pop.culture.ToString(), pop);

                        ////Adding education
                        addButton(pop.education.ToString(), pop);

                        ////Adding cash
                        addButton(pop.cash.ToString(), pop);

                        ////Adding needs fulfilling

                        //PopUnit ert = record;
                        addButton(pop.needsFullfilled.ToString(), pop,
                            //() => ert.consumedTotal.ToStringWithLines()                        
                            () => "Consumed:\n" + pop.getConsumed().getContainer().getString("\n")
                            );

                        ////Adding loyalty
                        string accu;
                        PopUnit.modifiersLoyaltyChange.getModifier(pop, out accu);
                        addButton(pop.loyalty.ToString(), pop, () => accu);

                        //Adding Unemployment
                        addButton(pop.getUnemployedProcent().ToString(), pop);

                        //Adding Movement
                        if (pop.getMovement() == null)
                            addButton("", pop);
                        else
                            addButton(pop.getMovement().getShortName(), pop, () => pop.getMovement().getName());
                    }
                }
            }
            endUpdate();
        }
        protected override void addHeader()
        {
            // Adding number
            // AddButton("Number");

            // Adding PopType
            addButton("Type");

            ////Adding province
            addButton("Province");

            ////Adding population
            addButton("Population");

            ////Adding culture
            addButton("Culture");

            ////Adding education
            addButton("Education");

            ////Adding storage
            //if (null.storage != null)
            addButton("Cash");
            //else AddButton("Administration");

            ////Adding needs fulfilling
            addButton("Needs fulfilled");

            ////Adding loyalty
            addButton("Loyalty");

            ////Adding Unemployment
            addButton("Unemployment");

            //Adding Movement
            addButton("Movement");
        }
    }
}