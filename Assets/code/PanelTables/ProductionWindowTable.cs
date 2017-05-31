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
        newButton.transform.SetParent(contentPanel, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text,  this, stor);
    }
   
    override protected void AddButtons()
    {
        int counter = 0;

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

        ////Adding slary
        AddButton("Salary");
        foreach (Factory next in Game.factoriesToShowInProductionPanel)
        {           

            // Adding shownFactory name 
            AddButton(next.type.name +" L"+next.getLevel(), next);

            // Adding province 
            AddButton(next.province.ToString(), next.province);

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
            if (Economy.isMarket.checkIftrue(Game.Player))
                AddButton(next.getSalary().ToString() + " coins", next);
            else
                AddButton(next.getSalary().ToString()+ " food", next);
            counter++;
            //contentPanel.r
        }

    }
}