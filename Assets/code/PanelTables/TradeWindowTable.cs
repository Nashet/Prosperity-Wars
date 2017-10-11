using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;



public class TradeWindowTable : MyTable
{
    override protected void refresh()
    {
        ////if (Game.date != 0)
        {
            base.RemoveButtons();
            AddButtons();
            gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gameObject.transform.childCount / this.columnsAmount * rowHeight + 50);
        }
    }
   
    override protected void AddButtons()
    {
        int counter = 0;

        // Adding product name 
        AddButton("Product");
        ////Adding production
        AddButton("Production");
        ////Adding On market
        AddButton("On market");
        ////Adding abstract Demand
        //AddButton(Game.market.get(pro).ToString().name, null);
        ////Adding effective Demand
        AddButton("Consumption");
        ////Adding bought
        AddButton("Bought");
        ////Adding effective Demand/Supply
        AddButton("D/S Balance");
        ////Adding price
        AddButton("Price");
        ////Adding price Change
        //AddButton(null.loyalty.ToString(), null);
        foreach (Product product in Product.getAllNonAbstract())
        {               
            if (product != Product.Gold && product.isInventedByAnyOne())// && !product.isAbstract())
            {
                // Adding product name 
                AddButton(product.getName(), product);
                ////Adding production
                AddButton(Game.market.getProductionTotal(product, !Game.devMode).ToString(), product);
                ////Adding abstract Demand
                //AddButton(Game.market.get(pro).ToString().name, next);

                ////Adding On market
                AddButton(Game.market.getSupply(product, !Game.devMode).ToString(), product);

                ////Adding total consumption
                AddButton(Game.market.getTotalConsumption(product, !Game.devMode).ToString(), product);

                ////Adding Bought
                AddButton(Game.market.getBouth(product, !Game.devMode).ToString(), product);

                ////Adding effective Demand/Supply
                AddButton(Game.market.getDemandSupplyBalance(product).ToString(), product);
                ////Adding price
                AddButton(Game.market.getPrice(product).get().ToString(), product);
                ////Adding price Change
                //AddButton(next.loyalty.ToString(), next);
                counter++;
                //contentPanel.r
            }
        }

    }
}