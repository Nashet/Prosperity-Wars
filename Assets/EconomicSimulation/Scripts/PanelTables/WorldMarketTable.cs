using System.Collections.Generic;
using System.Linq;
using Nashet.UnityUIUtils;

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
            //AddButton(World.market.get(pro).ToString().name, null);
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
            AddCell(product.ToString(), product);
            ////Adding production
            AddCell(World.market.getProductionTotal(product, true).get().ToString(), product);
            ////Adding abstract Demand
            //AddButton(World.market.get(pro).ToString().name, next);

            ////Adding On market
            AddCell(World.market.getMarketSupply(product, true).get().ToString(), product);

            ////Adding total consumption
            AddCell(World.market.getTotalConsumption(product, true).get().ToString(), product);

            ////Adding Bought
            AddCell(World.market.getBouthOnMarket(product, true).get().ToString(), product);

            ////Adding effective Demand/Supply
            AddCell(World.market.getDemandSupplyBalance(product, false).ToString(), product);
            //AddButton("-", product);
            ////Adding price
            AddCell(World.market.getCost(product).Get().ToString(), product);
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