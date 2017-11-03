using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtisanProduction : SimpleProduction
{
    //private readonly Artisans owner;
    public ArtisanProduction(FactoryType type, Province province, Artisans artisan) : base(type, province)
    {
        base.setOwner(artisan);
    }
    internal Artisans getOwner()
    {
        //todo would it add lags ??
        return base.getOwner() as Artisans;
    }
    [Obsolete("Shouldn't be changed", false)]
    public void setOwner(Agent agent)
    {
        throw new DontUseThatMethod();
    }
    override public List<Storage> getRealAllNeeds()
    {
        return getRealNeeds(new Value(getOwner().getPopulation() / 1000f));
    }
    /// <summary>  Return in pieces basing on current prices and needs  /// </summary>        
    //override public float getLocalEffectiveDemand(Product product)
    //{
    //    return getLocalEffectiveDemand(product, new Procent(owner.getPopulation() / 1000f));
    //}
    public override List<Storage> getHowMuchInputProductsReservesWants()
    {
        return getHowMuchInputProductsReservesWants(new Value(getOwner().getPopulation() / 1000f * Options.FactoryInputReservInDays));
    }
    internal override Procent getInputFactor()
    {
        return getInputFactor(new Procent(getOwner().getPopulation() / 1000f));
    }
    /// <summary>
    /// Fills storageNow and gainGoodsThisTurn
    /// </summary>
    public override void produce()
    {
        base.produce(new Value(getOwner().getPopulation() * PopUnit.modEfficiency.getModifier(getOwner()) * Options.ArtisansProductionModifier * getInputFactor().get() / 1000f));
        if (this.gainGoodsThisTurn.isNotZero())
        {
            getOwner().gainGoodsThisTurn.set(this.gainGoodsThisTurn);
            if (getOwner().storage.isExactlySameProduct(this.storage))
                getOwner().storage.add(this.storage);
            else
                getOwner().storage.set(this.storage);
        }
    }
    /// <summary>
    /// Now includes workforce/efficiency. Also buying for upgrading\building are happening here 
    /// </summary>
    override public void consumeNeeds()
    {
        List<Storage> shoppingList = getHowMuchInputProductsReservesWants();

        //todo !CAPITALISM part
        //if (isSubsidized())
        //    Game.market.buy(this, new PrimitiveStorageSet(shoppingList), getCountry());
        //else
        //shoppingList - getInputProductsReserve(); that is included in getHowMuchInputProductsReservesWants()
        Game.market.buy(this, new StorageSet(shoppingList), null);
    }
}