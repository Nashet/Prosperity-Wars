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
            AddCell("Product");
            ////Adding production
            AddCell("Production");
            ////Adding On market
            AddCell("On market");
            ////Adding abstract Demand
            //AddButton(Game.market.get(pro).ToString().name, null);
            ////Adding effective Demand
            AddCell("Consumption");
            ////Adding bought
            AddCell("Bought");
            ////Adding effective Demand/Supply
            AddCell("D/S Balance");
            ////Adding price
            AddCell("Price");
            ////Adding price Change

        }
        protected override void AddRow(Product product, int number)
        {
            // Adding product name 
            AddCell(product.getName(), product);
            ////Adding production
            AddCell(Game.market.getProductionTotal(product, true).get().ToString(), product);
            ////Adding abstract Demand
            //AddButton(Game.market.get(pro).ToString().name, next);

            ////Adding On market
            AddCell(Game.market.getMarketSupply(product, true).get().ToString(), product);

            ////Adding total consumption
            AddCell(Game.market.getTotalConsumption(product, true).get().ToString(), product);

            ////Adding Bought
            AddCell(Game.market.getBouthOnMarket(product, true).get().ToString(), product);

            ////Adding effective Demand/Supply
            AddCell(Game.market.getDemandSupplyBalance(product).ToString(), product);
            //AddButton("-", product);
            ////Adding price
            AddCell(Game.market.getPrice(product).get().ToString(), product);
            ////Adding price Change
            //AddButton(next.loyalty.ToString(), next);
            //counter++;
            //contentPanel.r
        }

        protected override IEnumerable<Product> ContentSelector()
        {
            return Product.getAll().Where(x => x.isTradable() && !x.isAbstract());
        }
    }
}