using System.Collections.Generic;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class ArtisanProduction : SimpleProduction
    {
        private readonly Artisans artisan;

        public ArtisanProduction(ProductionType type, Province province, Artisans artisan) : base(type, province)
        {
            this.artisan = artisan;
        }

        public override IEnumerable<Storage> getRealAllNeeds()
        {
            return getRealNeeds(artisan.population.Get() / 1000f);
        }

        /// <summary>  Return in pieces basing on current prices and needs  /// </summary>
        //override public float getLocalEffectiveDemand(Product product)
        //{
        //    return getLocalEffectiveDemand(product, new Procent(owner.population.Get() / 1000f));
        //}
        public override List<Storage> getHowMuchInputProductsReservesWants()
        {
            return getHowMuchInputProductsReservesWants(new Value(artisan.population.Get() / 1000f * Options.FactoryInputReservInDays));
        }

        public override Procent getInputFactor()
        {
            return getInputFactor(new Procent(artisan.population.Get() / 1000f));
        }

        /// <summary>
        /// Fills storageNow and gainGoodsThisTurn
        /// </summary>
        public override void produce()
        {
            base.produce(new Value(artisan.population.Get() * PopUnit.modEfficiency.getModifier(artisan) * Options.ArtisansProductionModifier * getInputFactor().get() / 1000f));
            if (getGainGoodsThisTurn().isNotZero())
            {
                artisan.addProduct(getGainGoodsThisTurn());
                if (artisan.storage.isExactlySameProduct(storage))
                    artisan.storage.add(storage);
                else
                    artisan.storage.set(storage);
            }
        }

        /// <summary>
        /// Now includes workforce/efficiency. Also buying for upgrading\building are happening here
        /// </summary>
        public override void consumeNeeds()
        {
            List<Storage> shoppingList = getHowMuchInputProductsReservesWants();
            //Country.market.SellList(this, new StorageSet(shoppingList), null);
            foreach (Storage item in shoppingList)
                if (item.isNotZero())
                    //SendToMarket(item);
                    Buy(item, null);
        }
    }
}