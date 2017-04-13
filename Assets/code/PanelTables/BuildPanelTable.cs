using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

// represen each opunit record in table

public class BuildPanelTable : MyTable
{
    override protected void Refresh()
    {
        ////if (Game.date != 0)
        {
            base.RemoveButtons();
            AddButtons();
            contentPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentPanel.childCount / this.columnsAmount * rowHeight + 50);
        }
    }
    protected void AddButton(string text, FactoryType type)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(contentPanel, false);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        //if (inventionType == null)
        //    sampleButton.Setup(text, this, null);
        //else
        sampleButton.Setup(text, this, type);
    }
    override protected void AddButtons()
    {
        int counter = 0;

        // Adding shownFactory type
        AddButton("Name", null);

        ////Adding cost
        AddButton("Cost", null);

        ////Adding resource needed
        //AddButton("Input", null);

        ////Adding potential output
        AddButton("Output", null);

        ////Adding potential profit
        AddButton("Potential margin", null);
        if (Game.selectedProvince != null)
        {
            var factoryList = Game.selectedProvince.WhatFactoriesCouldBeBuild();

            foreach (var next in factoryList)
            {
                // Adding shownFactory type
                AddButton(next.ToString(), next);

                ////Adding cost
                //if (Game.player.isInvented(InventionType.capitalism))
                if (Game.player.economy.isMarket())
                    AddButton(Game.market.getCost(next.getBuildNeeds()).ToString(), next);
                else
                    AddButton(next.getBuildNeeds().ToString(), next);


                ////Adding resource needed
                //AddButton(next.resourceInput.ToString(), next);

                ////Adding potential output
                AddButton(next.basicProduction.ToString(), next);

                ////Adding potential profit
                AddButton(next.getPossibleMargin(Game.selectedProvince).ToString(), next);
                
                counter++;
            }
        }
    }
}