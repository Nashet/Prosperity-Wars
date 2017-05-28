using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;



public class TradeWindowCountryMarketTable   : MyTable
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
        newButton.transform.SetParent(contentPanel, true);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        if (stor == null)
            sampleButton.Setup(text, this, null);
        else
            sampleButton.Setup(text, this, stor.getProduct());
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
        
        AddButton("Consumption?");
       
        AddButton("Bought");

        ////Adding price
        //AddButton("Price");
        ////Adding price Change
        //AddButton(null.loyalty.ToString(), null);
        if (Game.player != null)
        {
            var needs = Game.player.getNeeds();
            foreach (Storage next in Game.market.marketPrice)

            {
                Product product = next.getProduct();
                if (product != Product.Gold && product.isInventedByAnyOne())
                {
                    // Adding product name 
                    AddButton(product.getName(), next);

                    ////Adding storage amount
                    AddButton(Game.player.storageSet.getStorage(next.getProduct()).ToString(), next);


                    ////Adding needs
                    AddButton(needs.getStorage(next.getProduct()).ToString(), next);

                    ////Adding Produce
                    AddButton("wip", next);

                    ////Adding Consumption
                    AddButton("wip", next);

                    ////Adding bought
                    AddButton("wip", next);


                    counter++;
                    //contentPanel.r
                }
            }
        }
    }
}