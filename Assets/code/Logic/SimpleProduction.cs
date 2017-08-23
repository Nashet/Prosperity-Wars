using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class SimpleProduction : Producer
{
    private Agent owner;
    private readonly FactoryType type;
    private readonly PrimitiveStorageSet inputProductsReserve = new PrimitiveStorageSet();

    protected SimpleProduction(FactoryType type, Province province) : base(province)
    {
        this.type = type;
        gainGoodsThisTurn = new Storage(this.getType().basicProduction.getProduct());
        storageNow = new Storage(this.getType().basicProduction.getProduct());
        sentToMarket = new Storage(this.getType().basicProduction.getProduct());
    }
    internal Agent getOwner()
    {
        return owner;
    }
    public void setOwner(Agent agent)
    {
        owner = agent;
    }
    public PrimitiveStorageSet getInputProductsReserve()
    {
        return inputProductsReserve;
    }
    public FactoryType getType()
    {
        return type;
    }
    override public string ToString()
    {
        return "crafting " + getType().basicProduction;
    }
    public override void payTaxes() // currently no taxes for factories
    {
        // there is no corporate taxes yet
    }
    public override void simulate()
    {
        throw new NotImplementedException();
    }
    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        storageNow.set(0f);
    }
    virtual internal float getProfit()
    {
        return moneyIncomethisTurn.get() - getExpences();
    }

    /// <summary>
    /// Fills storageNow and gainGoodsThisTurn
    /// </summary>
    protected void produce(Value multiplier)
    {
        //todo add checks for inputs
        gainGoodsThisTurn = getType().basicProduction.multiplyOutside(multiplier);
        storageNow.add(gainGoodsThisTurn);

        //consume Input Resources
        foreach (Storage next in getRealNeeds())
            getInputProductsReserve().subtract(next, false);        
    }
    abstract internal Procent getInputFactor();
    protected Procent getInputFactor(Procent multiplier)
    {
        float inputFactor = 1;
        List<Storage> realInput = new List<Storage>();
        //Storage available;

        // how much we really want
        foreach (Storage input in getType().resourceInput)
        {
            realInput.Add(input.multiplyOutside(multiplier));
        }

        // checking if there is enough in market
        //old DSB
        //foreach (Storage input in realInput)
        //{
        //    available = Game.market.HowMuchAvailable(input);
        //    if (available.get() < input.get())
        //        input.set(available);
        //}
        foreach (Storage input in realInput)
        {
            if (!getInputProductsReserve().has(input))
            {
                Storage found = getInputProductsReserve().findStorage(input.getProduct());
                if (found == null)
                    input.set(0f);
                else
                    input.set(found);

            }
        }
        //old last turn consumption checking thing
        //foreach (Storage input in realInput)
        //{

        //    //if (Game.market.getDemandSupplyBalance(input.getProduct()) >= 1f)
        //    //available = input

        //    available = consumedLastTurn.findStorage(input.getProduct());
        //    if (available == null)
        //        ;// do nothing - pretend there is 100%, it fires only on shownFactory start
        //    else
        //    if (!justHiredPeople && available.get() < input.get())
        //        input.set(available);
        //}
        // checking if there is enough money to pay for
        // doesn't have sense with inputReserv
        //foreach (Storage input in realInput)
        //{
        //    Storage howMuchCan = wallet.HowMuchCanAfford(input);
        //    input.set(howMuchCan.get());
        //}
        // searching lowest factor
        foreach (Storage rInput in realInput)//todo optimize - convert into for i
        {
            float newFactor = rInput.get() / (getType().resourceInput.findStorage(rInput.getProduct()).get() * multiplier.get());
            if (newFactor < inputFactor)
                inputFactor = newFactor;
        }

        return new Procent(inputFactor);
    }
    abstract public List<Storage> getHowMuchInputProductsReservesWants();
    protected List<Storage> getHowMuchInputProductsReservesWants(Value multiplier)
    {
        //Value multiplier = new Value(getWorkForceFulFilling() * getLevel() * Options.FactoryInputReservInDays);

        List<Storage> result = new List<Storage>();

        foreach (Storage next in getType().resourceInput)
        {
            Storage howMuchWantBuy = new Storage(next);
            howMuchWantBuy.multiply(multiplier);
            Storage reserv = getInputProductsReserve().findStorage(next.getProduct());
            if (reserv == null)
                result.Add(howMuchWantBuy);
            else
            {
                if (howMuchWantBuy.isBiggerOrEqual(reserv))
                {
                    howMuchWantBuy.subtract(reserv);
                    result.Add(howMuchWantBuy);
                }//else  - there is enough reserves, don't buy that
            }
        }
        return result;
    }
    // Should remove market availability assumption since its goes to double- calculation?
    //public List<Storage> getRealNeeds()
    //{
    //    Value multiplier = new Value(getEfficiency(false).get() * getLevel());

    //    List<Storage> result = new List<Storage>();

    //    foreach (Storage next in getType().resourceInput)
    //    {
    //        Storage nStor = new Storage(next.getProduct(), next.get());
    //        nStor.multiple(multiplier);
    //        result.Add(nStor);
    //    }
    //    return result;
    //}

    protected List<Storage> getRealNeeds(Value multiplier)
    {
        //Value multiplier = new Value(getEfficiency(false).get() * getLevel());

        List<Storage> result = new List<Storage>();

        foreach (Storage next in getType().resourceInput)
        {
            Storage nStor = new Storage(next.getProduct(), next.get());
            nStor.multiply(multiplier);
            result.Add(nStor);
        }
        return result;
    }

    /// <summary>  Return in pieces basing on current prices and needs  /// </summary>        
    protected float getLocalEffectiveDemand(Product product, Procent multiplier)
    {
        // need to know how much i Consumed inside my needs
        Storage need = getType().resourceInput.findStorage(product);
        if (need == null)
            return 0f;
        else
        {
            //Storage realNeed = new Storage(need.getProduct(), need.get() * multiplier.get());
            Storage realNeed = need.multiplyOutside(multiplier.get());
            //Storage realNeed = new Storage(need.getProduct(), need.get() * getInputFactor());
            Storage canAfford = howMuchCanAfford(realNeed);
            return canAfford.get();
        }
    }
    virtual internal float getExpences()
    {
        return Game.market.getCost(consumedTotal).get();
    }
    public bool isAllInputProductsCollected()
    {
        var realNeeds = getRealNeeds();
        foreach (var item in realNeeds)
        {
            if (!inputProductsReserve.has(item))
                return false;
        }
        return true;
    }
    
}
