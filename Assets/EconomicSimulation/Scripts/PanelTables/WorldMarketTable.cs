using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    public class WorldMarketTable : UITableNew
    {
        public override void Refresh()
        {
            StartUpdate();
            //lock (gameObject)
            {
                RemoveButtons();
                AddHeader();
                AddButtons();
                //AddHeader();
            }
            EndUpdate();
        }
        protected override void AddHeader()
        {
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

        }
        private void AddButtons()
        {
            int counter = 0;
            //do NOT rely on elements order!
            var elementsToShow = Product.getAll(x => x.isTradable() && !x.isAbstract()).ToList();
            var howMuchRowsShow = CalcSize(elementsToShow.Count);

            //foreach (Product product in Product.getAllNonAbstract())
            for (int i = 0; i < howMuchRowsShow; i++)
            {
                var product = elementsToShow[i + GetRowOffset()];

                // Adding product name 
                AddButton(product.getName(), product);
                ////Adding production
                AddButton(Game.market.getProductionTotal(product, true).get().ToString(), product);
                ////Adding abstract Demand
                //AddButton(Game.market.get(pro).ToString().name, next);

                ////Adding On market
                AddButton(Game.market.getMarketSupply(product, true).get().ToString(), product);

                ////Adding total consumption
                AddButton(Game.market.getTotalConsumption(product, true).get().ToString(), product);

                ////Adding Bought
                AddButton(Game.market.getBouthOnMarket(product, true).get().ToString(), product);

                ////Adding effective Demand/Supply
                AddButton(Game.market.getDemandSupplyBalance(product).ToString(), product);
                //AddButton("-", product);
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