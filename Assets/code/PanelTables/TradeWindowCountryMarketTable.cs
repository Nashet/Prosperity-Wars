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
        AddButton("Gov. storage");

        AddButton("Gov. needs");

        AddButton("Production tot.");

        AddButton("Used by gov.");

        AddButton("Bought by gov.");

        AddButton("Sold by gov.");

        ////Adding price
        //AddButton("Price");
        ////Adding price Change
        //AddButton(null.loyalty.ToString(), null);
        if (Game.Player != null)
        {
            var needs = Game.Player.getRealAllNeeds();
            foreach (var product in Product.getAll())
            //foreach (var item in Game.market.pr)
            {   
                // Product product = next.getProduct();
                if ( product != Product.Gold && product.isInventedByAnyOne())
                {
                    var storage = Game.Player.countryStorageSet.getFirstStorage(product);
                    // Adding product name 
                    AddButton(product.getName(), storage);

                    ////Adding storage amount
                    AddButton(storage.ToString(), storage);

                    ////Adding needs
                    AddButton(needs.getStorage(product).ToString(), storage);

                    ////Adding Produced
                    if (product.isAbstract())//|| !product.isTradable()
                        AddButton("-", storage);
                    else
                        AddButton(Game.Player.getProducedTotal(product).ToString(), storage);

                    ////Adding taken away
                    AddButton(Game.Player.countryStorageSet.used.getFirstStorage(product).ToString(), storage);

                    ////Adding bought
                    AddButton(Game.Player.getConsumedInMarket().getFirstStorage(product).ToString(), storage);

                    ////Adding Sold
                    if (product.isAbstract())//|| !product.isTradable()
                        AddButton("-", storage);
                    else
                        AddButton(Game.Player.getSentToMarket(product).ToString(), storage);

                    counter++;
                    //contentPanel.r
                }
            }
        }
    }
}