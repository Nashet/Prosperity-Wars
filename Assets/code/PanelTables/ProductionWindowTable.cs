using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;



public class ProductionWindowTable : MyTableNew
{

    public override void refreshContent()
    {
        alreadyInUpdate = true;
        base.RemoveButtons();
        calcSize(Game.factoriesToShowInProductionPanel.Count);
        //int counter = 0;

        // Adding product name 
        AddButton("Type");

        // Adding province 
        AddButton("Province");

        ////Adding production
        AddButton("Production");

        ////Adding effective resource income
        AddButton("Resources");

        ////Adding workforce
        AddButton("Workforce");

        ////Adding money income
        AddButton("Profit");

        ////Adding profit
        AddButton("% Profit");

        ////Adding salary
        AddButton("Salary");
        for (int i = 0; i < howMuchRowsShow; i++)
        //foreach (Factory next in Game.factoriesToShowInProductionPanel)
        {
            Factory next = Game.factoriesToShowInProductionPanel[i + offset];
            // Adding shownFactory name 
            AddButton(next.getType().name + " L" + next.getLevel(), next);

            // Adding province 
            AddButton(next.getProvince().ToString(), next.getProvince());

            ////Adding production
            AddButton(next.gainGoodsThisTurn.ToString(), next);

            ////Adding effective resource income
            AddButton(next.getInputFactor().ToString(), next);

            ////Adding workforce
            AddButton(next.getWorkForce().ToString(), next);

            ////Adding profit
            AddButton(next.getProfit().ToString(), next);

            ////Adding margin
            if (next.isUpgrading())
                AddButton("Upgrading", next);
            else
            {
                if (next.isBuilding())
                    AddButton("Building", next);
                else
                {
                    if (!next.isWorking())
                        AddButton("Closed", next);
                    else
                        AddButton(next.getMargin().ToString(), next);
                }
            }
            ////Adding salary
            //if (Game.player.isInvented(InventionType.capitalism))
            if (Economy.isMarket.checkIftrue(Game.Player))
                AddButton(next.getSalary().ToString() + " coins", next);
            else
                AddButton(next.getSalary().ToString() + " food", next);
            //counter++;
            //contentPanel.r
        }
        alreadyInUpdate = false;
    }
}