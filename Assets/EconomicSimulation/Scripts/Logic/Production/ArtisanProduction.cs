using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.ValueSpace;
using Nashet.Utils;
namespace Nashet.EconomicSimulation
{
    public class ArtisanProduction : SimpleProduction
    {
        private readonly Artisans artisan;
        public ArtisanProduction(FactoryType type, Province province, Artisans artisan) : base(type, province)
        {
            this.artisan = artisan;
        }       
        override public List<Storage> getRealAllNeeds()
        {
            return getRealNeeds(new Value(artisan.getPopulation() / 1000f));
        }
        /// <summary>  Return in pieces basing on current prices and needs  /// </summary>        
        //override public float getLocalEffectiveDemand(Product product)
        //{
        //    return getLocalEffectiveDemand(product, new Procent(owner.getPopulation() / 1000f));
        //}
        public override List<Storage> getHowMuchInputProductsReservesWants()
        {
            return getHowMuchInputProductsReservesWants(new Value(artisan.getPopulation() / 1000f * Options.FactoryInputReservInDays));
        }
        internal override Procent getInputFactor()
        {
            return getInputFactor(new Procent(artisan.getPopulation() / 1000f));
        }
        /// <summary>
        /// Fills storageNow and gainGoodsThisTurn
        /// </summary>
        public override void produce()
        {
            base.produce(new Value(artisan.getPopulation() * PopUnit.modEfficiency.getModifier(artisan) * Options.ArtisansProductionModifier * getInputFactor().get() / 1000f));
            if (this.getGainGoodsThisTurn().isNotZero())
            {
                artisan.addProduct(this.getGainGoodsThisTurn());
                if (artisan.storage.isExactlySameProduct(this.storage))
                    artisan.storage.add(this.storage);
                else
                    artisan.storage.set(this.storage);
            }
        }
        /// <summary>
        /// Now includes workforce/efficiency. Also buying for upgrading\building are happening here 
        /// </summary>
        override public void consumeNeeds()
        {
            List<Storage> shoppingList = getHowMuchInputProductsReservesWants();

            //if (isSubsidized())
            //    Game.market.buy(this, new PrimitiveStorageSet(shoppingList), Country);
            //else
            //shoppingList - getInputProductsReserve(); that is included in getHowMuchInputProductsReservesWants()
            Game.market.buy(this, new StorageSet(shoppingList), null);
        }
    }
}