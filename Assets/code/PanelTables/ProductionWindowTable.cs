using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;



public class ProductionWindowTable : MyTable
{

    override protected void Refresh()
    {
        if (Game.factoriesToShowInProductionPanel != null)
        {
            base.RemoveButtons();
            AddButtons();
            contentPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentPanel.childCount / this.columnsAmount * rowHeight + 50);
        }
    }
    protected void AddButton(string text, Factory stor)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(contentPanel, false);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text,  this, stor);
    }
   
    override protected void AddButtons()
    {
        int counter = 0;

        // Adding product name 
        AddButton("Type", null);

        // Adding province 
        AddButton("Province", null);

        ////Adding production
        AddButton("Production", null);

        ////Adding effective resource income
        AddButton("Resources", null);

        ////Adding workforce
        AddButton("Workforce", null);

        ////Adding money income
        AddButton("Profit", null);

        ////Adding profit
        AddButton("% Profit", null);

        ////Adding slary
        AddButton("Salary", null);
        foreach (Factory next in Game.factoriesToShowInProductionPanel)
        {           

            // Adding shownFactory name 
            AddButton(next.type.name +" L"+next.getLevel(), next);

            // Adding province 
            AddButton(next.province.ToString(), next);

            ////Adding production
            AddButton(next.gainGoodsThisTurn.ToString(), next);

            ////Adding effective resource income
            AddButton(next.getResouceFullfillig().ToString(), next);

            ////Adding workforce
            AddButton(next.getWorkForce().ToString(), next);

            ////Adding profit
            AddButton(next.getProfit().ToString(), next);

            ////Adding margin
            if (next.isUpgrading())
                AddButton("Upgrading", next);
            else
            if (next.isBuilding())
                AddButton("Building", next);
            else
                if (!next.isWorking())
                AddButton("Closed", next);
            else
                AddButton(next.getMargin().ToString(), next);

            ////Adding salary
            //if (Game.player.isInvented(InventionType.capitalism))
            if (Game.player.economy.isMarket())
                AddButton(next.getSalary().ToString() + " coins", next);
            else
                AddButton(next.getSalary().ToString()+ " food", next);
            counter++;
            //contentPanel.r
        }

    }
}