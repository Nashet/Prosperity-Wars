using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    public class WorldMarketTable : UITableNew<Product>
    {
        
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
        protected override void AddRow(Product product)
        {
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
            //counter++;
            //contentPanel.r
        }

        protected override List<Product> ContentSelector()
        {
            return Product.getAll(x => x.isTradable() && !x.isAbstract()).ToList();
        }
    }
}