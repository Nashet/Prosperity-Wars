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
            //AddButton(Country.market.get(pro).ToString().name, null);
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
            AddCell(Game.Player.market.getProductionTotal(product, true).get().ToString(), product);
            ////Adding abstract Demand
            //AddButton(Country.market.get(pro).ToString().name, next);

            ////Adding On market
            AddCell(Game.Player.market.getMarketSupply(product, true).get().ToString(), product);

            ////Adding total consumption
            AddCell(Game.Player.market.getTotalConsumption(product, true).get().ToString(), product);

            ////Adding Bought
            AddCell(Game.Player.market.getBouthOnMarket(product, true).get().ToString(), product);

            ////Adding effective Demand/Supply
            AddCell(Game.Player.market.getDemandSupplyBalance(product, false).ToString(), product);
            //AddButton("-", product);
            ////Adding price
            AddCell(Game.Player.market.getCost(product).Get().ToString(), product);
            ////Adding price Change
            //AddButton(next.loyalty.ToString(), next);
            //counter++;
            //contentPanel.r
        }

        protected override IEnumerable<Product> ContentSelector()
        {
            return Product.AllNonAbstract().Where(x => x.isTradable());
        }
    }
}