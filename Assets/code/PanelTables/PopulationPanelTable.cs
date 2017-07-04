using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class PopulationPanelTable : MyTableNew
{
    public override void refreshContent()
    {
        alreadyInUpdate = true;
        //lock (gameObject)
        {
            RemoveButtons();
            calcSize(Game.popsToShowInPopulationPanel.Count);

            // Adding number
            // AddButton("Number");

            // Adding PopType
            AddButton("Type");

            ////Adding population
            AddButton("Population");

            ////Adding culture
            AddButton("Culture");

            ////Adding province
            AddButton("Province");

            ////Adding education
            AddButton("Education");

            ////Adding storage
            //if (null.storage != null)
            AddButton("Cash");
            //else AddButton("Administration");

            ////Adding needs fulfilling
            AddButton("Needs fulfilled");

            ////Adding loyalty
            AddButton("Loyalty");

            ////Adding Unemployment
            AddButton("Unemployment");

            //Adding Movement
            AddButton("Movement");

            if (Game.popsToShowInPopulationPanel.Count > 0)
            {
                for (int i = 0; i < howMuchRowsShow; i++)
                //foreach (PopUnit record in Game.popsToShowInPopulationPanel)
                {
                    PopUnit record = Game.popsToShowInPopulationPanel[i + offset];

                    // Adding number
                    //AddButton(Convert.ToString(i + offset), record);

                    // Adding PopType
                    AddButton(record.popType.ToString(), record);
                    ////Adding population
                    AddButton(System.Convert.ToString(record.getPopulation()), record);
                    ////Adding culture
                    AddButton(record.culture.ToString(), record);
                    ////Adding province
                    AddButton(record.province.ToString(), record.province, "Click to select this province");
                    ////Adding education
                    AddButton(record.education.ToString(), record);

                    ////Adding cash
                    AddButton(record.cash.ToString(), record);

                    ////Adding needs fulfilling

                    PopUnit ert = record;
                    AddButton(record.needsFullfilled.ToString(), record,
                        //() => ert.consumedTotal.ToStringWithLines()                        
                        () => ert.consumedTotal.getContainer().getString("\n")
                        );

                    ////Adding loyalty
                    string accu;
                    PopUnit.modifiersLoyaltyChange.getModifier(record, out accu);
                    AddButton(record.loyalty.ToString(), record, accu);

                    //Adding Unemployment
                    AddButton(record.getUnemployedProcent().ToString(), record);

                    //Adding Movement
                    if (record.getMovement() == null)
                        AddButton("", record);
                    else
                        AddButton(record.getMovement().getShortName(), record);
                }

            }
        }
        alreadyInUpdate = false;
    }
}