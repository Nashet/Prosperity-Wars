using System.Collections.Generic;
using System.Linq;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class CountryStorageTable : UITableNew<Product>
    {
        protected override IEnumerable<Product> ContentSelector()
        {
            return Product.All().Where(x => x.IsInventedByAnyOne());
        }

        protected override void AddHeader()
        {
            // Adding product name
            AddCell("Product");

            ////Adding production
            AddCell("Gov. storage");

            AddCell("Mil. needs");

            AddCell("Production tot.");

            AddCell("Used by gov.");

            AddCell("Bought by gov.");

            AddCell("Sold by gov.");
        }

        protected override void AddRow(Product product, int number)
        {
            var needs = Game.Player.getRealAllNeeds().ToList();
            // Adding product name
            if (product.isAbstract())
            {
                AddCell(product + " total", null, () => product.getSubstitutes().ToList().ToString(" or "));

                ////Adding total amount
                AddCell(Game.Player.countryStorageSet.getTotal(product).get().ToString());

                ////Adding mil. needs
                AddCell(needs.getStorage(product).ToStringWithoutSubstitutes(), null, () => "That doesn't include non-abstract needs");
                //AddButton(needs.getStorageIncludingSubstitutes(product).get().ToString());
                //AddButton("-");

                ////Adding Produced total
                AddCell(Game.Player.getProducedTotalIncludingSubstitutes(product).get().ToString());
                //, null , () => Game.Player.getWorldProductionShare(product) + " of world production");
                // can't add statistic about share of abstract product due to Market.GetProduction can't in abstract products

                ////Adding used by gov.
                AddCell(Game.Player.countryStorageSet.used.getTotal(product).get().ToString());

                ////Adding bought
                AddCell(Game.Player.AllConsumedInMarket(Game.Player.market).getTotal(product).get().ToString());

                ////Adding Sold
                //AddButton(Game.Player.getSentToMarketIncludingSubstituts(product).get().ToString());
                AddCell("-");
            }
            else
            {
                var storage = Game.Player.countryStorageSet.GetFirstSubstituteStorage(product);

                // Adding product name
                AddCell(product.ToString(), storage);

                ////Adding storage amount
                AddCell(storage.get().ToString(), storage);

                ////Adding mil. needs
                AddCell(needs.getStorage(product).get().ToString(), storage);

                ////Adding Produced
                AddCell(Game.Player.getProducedTotal(product).get().ToString(), storage, () => Game.Player.getWorldProductionShare(product) + " of world production");

                ////Adding used by gov.
                AddCell(Game.Player.countryStorageSet.used.GetFirstSubstituteStorage(product).get().ToString(), storage);

                ////Adding bought
                AddCell(Game.Player.AllConsumedInMarket(Game.Player.market).GetFirstSubstituteStorage(product).get().ToString(), storage);

                ////Adding Sold
                //// finding actually sold from sentToMarket
                //var str = Game.Player.getSentToMarket(product);
                //var DSB = Country.market.getDemandSupplyBalance(product);
                //if (DSB.GetHashCode() == Options.MarketInfiniteDSB.GetHashCode())
                //    str.setZero();
                //else
                //    str.multiply(DSB);

                //AddCell(Game.Player.getSoldByGovernment(product).get().ToString(), storage, () => "Actually sold according to demand\nCould be less than sent to market");
                AddCell(Market.GiveTotalSoldProduct(Game.Player, product).get().ToString(), storage, () => "Actually sold according to demand\nCould be less than sent to market");
            }
        }

        //private void AddButtons()
        //{
        //    int counter = 0;

        //    //do NOT rely on elements order!
        //    var elementsToShow = Product.getAll(x => x.isTradable()).ToList();
        //    var howMuchRowsShow = ReCalcSize(elementsToShow.Count);
        //    var needs = Game.Player.getRealAllNeeds();
        //    //foreach (var product in Product.getAll())
        //    for (int i = 0; i < howMuchRowsShow; i++)
        //    {
        //        var product = elementsToShow[i + GetRowOffset()];

        //    }

        //    counter++;
        //    //contentPanel.r
        //}
    }
}