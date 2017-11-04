using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class TradeWindowCountryMarketTable : MyTable
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
        AddButton("Govern.storage");

        AddButton("Govern.Need"); 

        AddButton("Production");

        AddButton("Consumption");

        AddButton("Bought");

        ////Adding price
        //AddButton("Price");
        ////Adding price Change
        //AddButton(null.loyalty.ToString(), null);
        if (Game.Player != null)
        {
            var needs = Game.Player.getRealAllNeeds();
            foreach (var product in Product.getAll())
            {
                // Product product = next.getProduct();
                if (product != Product.Gold && product.isInventedByAnyOne())
                {
                    // Adding product name 
                    AddButton(product.getName(), product);

                    ////Adding storage amount
                    AddButton(Game.Player.countryStorageSet.getFirstStorage(product).ToString(), product);

                    ////Adding needs
                    AddButton(needs.getStorage(product).ToString(), product);

                    ////Adding Produce
                    AddButton("wip", product);

                    ////Adding Consumption
                    AddButton(Game.Player.countryStorageSet.takenAway.getFirstStorage(product).ToString(), product);

                    ////Adding bought
                    AddButton("wip", product);    

                    counter++;
                    //contentPanel.r
                }
            }
        }
    }
}