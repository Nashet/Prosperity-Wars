using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

// represen each opunit record in table


public class TradeWindowTable : MyTable
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
    protected void AddButton(string text, Storage stor)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(contentPanel, false);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        if (stor==null)
            sampleButton.Setup(text, this, null);
        else
        sampleButton.Setup(text, this, stor.getProduct());
    }
    override protected void AddButtons()
    {
        int counter = 0;

        // Adding product name 
        AddButton("Product", null);
        ////Adding production
        AddButton("Production", null);
        ////Adding On market
        AddButton("On market", null);
        ////Adding abstract Demand
        //AddButton(Game.market.get(pro).ToString().name, null);
        ////Adding effective Demand
        AddButton("Consumption", null);
        ////Adding bought
        AddButton("Bought", null);
        ////Adding effective Demand/Supply
        AddButton("D/S Balance", null);
        ////Adding price
        AddButton("Price", null);
        ////Adding price Change
        //AddButton(null.loyalty.ToString(), null);
        foreach (Storage next in Game.market.marketPrice)

        {
            Product product = next.getProduct();
            if (product != Product.Gold && product.isInventedByAnyOne())
            {
                // Adding product name 
                AddButton(product.getName(), next);
                ////Adding production
                AddButton(Game.market.getProductionTotal(product, !Game.devMode).ToString(), next);
                ////Adding abstract Demand
                //AddButton(Game.market.get(pro).ToString().name, next);

                ////Adding On market
                AddButton(Game.market.getSupply(product, !Game.devMode).ToString(), next);
                
                ////Adding total consumption
                AddButton(Game.market.getTotalConsumption(product, !Game.devMode).ToString(), next);

                ////Adding Bought
                AddButton(Game.market.getBouth(product, !Game.devMode).ToString(), next);

                ////Adding effective Demand/Supply
                AddButton(Game.market.getDemandSupplyBalance(product).ToString(), next);
                ////Adding price
                AddButton(next.get().ToString(), next);
                ////Adding price Change
                //AddButton(next.loyalty.ToString(), next);
                counter++;
                //contentPanel.r
            }
        }

    }
}