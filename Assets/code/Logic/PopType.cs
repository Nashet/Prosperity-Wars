using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class PopType
{
    private readonly static List<PopType> allPopTypes = new List<PopType>();
    public static readonly PopType TribeMen, Aristocrats, Farmers, Artisans, Soldiers, Workers, Capitalists;


    ///<summary> per 1000 men </summary>    
    private readonly PrimitiveStorageSet lifeNeeds = new PrimitiveStorageSet();
    private readonly PrimitiveStorageSet everyDayNeeds = new PrimitiveStorageSet();
    private readonly PrimitiveStorageSet luxuryNeeds = new PrimitiveStorageSet();
    private readonly PrimitiveStorageSet militaryNeeds = new PrimitiveStorageSet();

    ///<summary> per 1000 men </summary>
    private readonly Storage basicProduction;
    private readonly string name;
    /// <summary>
    /// SHOULD not be zero!
    /// </summary>
    private readonly float strenght;
    static PopType()
    {
        var tribemenLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 1) });
        var tribemenEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 2) });
        var tribemenLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 3) });
        TribeMen = new PopType("Tribesmen", new Storage(Product.Food, 1.0f), 2f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
            tribemenLifeNeeds, tribemenEveryDayNeeds, tribemenLuxuryNeeds);
        //***************************************next type***************************
        var aristocratsLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 1) });
        var aristocratsEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Fruit, 1),
            new Storage(Product.ColdArms, 1),
            new Storage(Product.Clothes, 1),
            new Storage(Product.Furniture, 1) });
        var aristocratsLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Wine, 2),
            new Storage(Product.Cars, 1f),
            new Storage(Product.Fuel, 1f),
            new Storage(Product.Airplanes, 1f) });
        Aristocrats = new PopType("Aristocrats", null, 4f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
            aristocratsLifeNeeds, aristocratsEveryDayNeeds, aristocratsLuxuryNeeds);
        //***************************************next type***************************
        var capitalistsLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 1) });
        var capitalistsEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Clothes, 1f),
            new Storage(Product.Furniture, 1f),
            new Storage(Product.Fruit, 1f) });
        var capitalistsLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Wine, 2f),
            new Storage(Product.Firearms, 1f),
            new Storage(Product.Ammunition, 0.5f),
            new Storage(Product.Cars, 1f),
            new Storage(Product.Fuel, 1f),
            new Storage(Product.Airplanes, 1f)});
        Capitalists = new PopType("Capitalists", null, 1f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
            capitalistsLifeNeeds, capitalistsEveryDayNeeds, capitalistsLuxuryNeeds);
        //***************************************next type***************************
        {
            var artisansLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 1) });
            var artisansEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Clothes, 1f),
            new Storage(Product.Furniture, 1f),
            new Storage(Product.Fruit, 1f) });
            var artisansLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Wine, 2f),
            new Storage(Product.Firearms, 1f),
            new Storage(Product.Ammunition, 0.5f),
            new Storage(Product.Cars, 1f),
            new Storage(Product.Fuel, 1f),
            new Storage(Product.Airplanes, 1f)});
            Artisans = new PopType("Artisans", null, 1f,
                new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
                artisansLifeNeeds, artisansEveryDayNeeds, artisansLuxuryNeeds);
        }
        //***************************************next type***************************
        var farmersLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 1) });
        var farmersEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> {
           //everyDayNeeds.Set(new Storage(Product.Fruit, 1),
            new Storage(Product.Stone, 1),
            new Storage(Product.Wood, 1),
            //everyDayNeeds.set(new Storage(Product.Wool, 1),
            new Storage(Product.Lumber, 1),
            new Storage(Product.Cars, 1f),
            new Storage(Product.Fuel, 1f)});
        var farmersLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Clothes, 1),
            new Storage(Product.Furniture, 1),
            new Storage(Product.Wine, 2)
            //new Storage(Product.Metal, 1),
            //new Storage(Product.Cement, 0.5f)
                                            });
        Farmers = new PopType("Farmers", new Storage(Product.Food, 1.5f), 1f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
            farmersLifeNeeds, farmersEveryDayNeeds, farmersLuxuryNeeds);
        //***************************************next type***************************
        var workersLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 1) });
        var workersEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Clothes, 1f),
            new Storage(Product.Wine, 2f),
            new Storage(Product.Furniture, 1f)
             });
        var workersLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Fruit, 1),
            new Storage(Product.Cars, 1f),
            new Storage(Product.Fuel, 1f)
            });
        Workers = new PopType("Workers", null, 1f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
            workersLifeNeeds, workersEveryDayNeeds, workersLuxuryNeeds);
        //***************************************next type***************************
        var soldiersLifeNeeds = new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 2) });
        var soldiersEveryDayNeeds = new PrimitiveStorageSet(new List<Storage> {
            new Storage(Product.Fruit, 2),
            new Storage(Product.Wine, 5),
            new Storage(Product.Clothes, 4),
            new Storage(Product.Furniture, 2),
            //new Storage(Product.Wood, 1)
        });
        var soldiersLuxuryNeeds = new PrimitiveStorageSet(new List<Storage> {
             new Storage(Product.Firearms, 1f),
            new Storage(Product.Ammunition, 0.5f),
            new Storage(Product.Cars, 1f), // temporally
            new Storage(Product.Fuel, 1f),// temporally
            new Storage(Product.Airplanes, 1f) // temporally
            });
        Soldiers = new PopType("Soldiers", null, 2f,
            new PrimitiveStorageSet(new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.Fuel, 0.6f) }),
            soldiersLifeNeeds, soldiersEveryDayNeeds, soldiersLuxuryNeeds);
        //new PopType(PopType.PopTypes.Artisans, null, "Artisans");
        //new PopType(PopType.PopTypes.Soldiers, null, "Soldiers");
    }
    private PopType(string name, Storage produces, float strenght, PrimitiveStorageSet militaryNeeds,
        PrimitiveStorageSet lifeNeeds, PrimitiveStorageSet everyDayNeeds, PrimitiveStorageSet luxuryNeeds)
    {
        this.militaryNeeds = militaryNeeds;
        this.strenght = strenght;

        this.name = name;
        basicProduction = produces;
        this.lifeNeeds = lifeNeeds;
        this.everyDayNeeds = everyDayNeeds;
        this.luxuryNeeds = luxuryNeeds;
        allPopTypes.Add(this);
    }
    public static IEnumerable<PopType> getAllPopTypes()
    {
        foreach (var item in allPopTypes)
            yield return item;
    }

    public Storage getBasicProduction()
    {
        return basicProduction;
    }
    public bool canMobilize(Staff byWhom)
    {
        if (byWhom is Country)
        {
            if (this == PopType.Capitalists)
                return false;
            else
                return true;
        }
        else // movement
            return true;
    }
    public PrimitiveStorageSet getMilitaryNeedsPer1000()
    {
        return militaryNeeds;
    }

    /////<summary> per 1000 men </summary>
    //public Storage getLifeNeedsPer1000(PopType popType)
    //{
    //    foreach (Needs next in lifeNeeds)
    //        if (next.popType == popType)
    //            return next.needs;
    //    return null;
    //}
    ///<summary> per 1000 men </summary>
    public List<Storage> getLifeNeedsPer1000()
    {
        List<Storage> result = new List<Storage>();
        foreach (Storage next in lifeNeeds)
            //if (next.popType == this)
            result.Add(next);
        return result;
    }
    ///<summary> per 1000 men </summary>
    public List<Storage> getEveryDayNeedsPer1000()
    {
        List<Storage> result = new List<Storage>();
        foreach (Storage next in everyDayNeeds)
            // (next.popType == this)
            result.Add(next);
        return result;
    }
    ///<summary> per 1000 men </summary>
    public List<Storage> getLuxuryNeedsPer1000()
    {
        List<Storage> result = new List<Storage>();
        foreach (Storage next in luxuryNeeds)
            // if (next.popType == this)
            result.Add(next);
        return result;
        // Needs
    }
    ///<summary> per 1000 men </summary>
    public List<Storage> getAllNeedsPer1000()
    {
        List<Storage> result = getLifeNeedsPer1000();
        result.AddRange(getEveryDayNeedsPer1000());
        result.AddRange(getLuxuryNeedsPer1000());
        return result;

    }
    override public string ToString()
    {
        return name;
    }

    internal bool isPoorStrata()
    {
        return this == PopType.Farmers || this == PopType.Workers || this == PopType.TribeMen || this == PopType.Soldiers;
    }

    internal bool isRichStrata()
    {
        return this == PopType.Aristocrats || this == PopType.Capitalists || this == PopType.Artisans;
    }

    internal float getStrenght()
    {
        return strenght;
    }
    public bool canBeUnemployed()
    {
        return this == PopType.Farmers || this == PopType.Workers || this == PopType.TribeMen;
    }
    /// <summary>
    /// Returns true if can produce something by himself
    /// </summary>    
    internal bool isProducer()
    {
        return this == PopType.Farmers || this == PopType.TribeMen || this == PopType.Artisans;
    }

}

abstract public class SimpleProduction : Producer
{
    private readonly FactoryType type;
    private readonly PrimitiveStorageSet inputProducts = new PrimitiveStorageSet();

    public SimpleProduction(FactoryType type, Province province) : base(province)
    {
        this.type = type;
        gainGoodsThisTurn = new Storage(this.getType().basicProduction.getProduct());
        storageNow = new Storage(this.getType().basicProduction.getProduct());
        sentToMarket = new Storage(this.getType().basicProduction.getProduct());

    }
    public PrimitiveStorageSet getInputProducts()
    {
        return inputProducts;
    }
    public FactoryType getType()
    {
        return type;
    }
    override public string ToString()
    {
        return  "crafting " + getType().basicProduction;
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
        return moneyIncomethisTurn.get() - Game.market.getCost(consumedTotal).get();
    }


    /// <summary>
    /// Fills storageNow and gainGoodsThisTurn
    /// </summary>
    protected void produce(Value multiplier)
    {
        //if (isWorking())
        //{
        //if (workers > 0)
        {

            //Storage producedAmount = new Storage(type.basicProduction.getProduct(), type.basicProduction.get() * getEfficiency(true).get() * getLevel()); // * getLevel());
            //gainGoodsThisTurn = getType().basicProduction.multipleOutside(artisans.getPopulation() * PopUnit.modEfficiency.getModifier(this) * Options.ArtisansProductionModifier / 1000f);
            gainGoodsThisTurn = getType().basicProduction.multipleOutside(multiplier);

            storageNow.add(gainGoodsThisTurn);
            //gainGoodsThisTurn.set(producedAmount);

            //consumeInputResources
            foreach (Storage next in getRealNeeds())
                getInputProducts().subtract(next, false);

            if (getType() == FactoryType.GoldMine)
            {
                this.ConvertFromGoldAndAdd(storageNow);
                //send 50% to government
                Value sentToGovernment = new Value(moneyIncomethisTurn.get() * Options.GovernmentTakesShareOfGoldOutput);
                pay(getCountry(), sentToGovernment);
                getCountry().goldMinesIncomeAdd(sentToGovernment);
            }
            else
            {
                sentToMarket.set(gainGoodsThisTurn);
                storageNow.setZero();
                Game.market.sentToMarket.add(gainGoodsThisTurn);
            }
            if (Economy.isMarket.checkIftrue(province.getCountry()))
            {
                // Buyers should come and buy something...
                // its in other files.
            }
            else // send all production to owner
                ; // todo write !capitalism
                  //storageNow.sendAll(owner.storageSet);
        }
        // }
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
            realInput.Add(input.multipleOutside(multiplier));
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
            if (!getInputProducts().has(input))
            {
                Storage found = getInputProducts().findStorage(input.getProduct());
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
            Storage howMuchWantBuy = new Storage(next.getProduct(), next.get());
            howMuchWantBuy.multiple(multiplier);
            Storage reserv = getInputProducts().findStorage(next.getProduct());
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
            nStor.multiple(multiplier);
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
            Storage realNeed = need.multipleOutside(multiplier.get());
            //Storage realNeed = new Storage(need.getProduct(), need.get() * getInputFactor());
            Storage canAfford = HowMuchCanAfford(realNeed);
            return canAfford.get();
        }
    }
   
    
    virtual internal float getExpences()
    {
        return Game.market.getCost(consumedTotal).get();
    }
}
public class ArtisanProduction : SimpleProduction
{
    private Artisans owner;
    public ArtisanProduction(FactoryType type, Province province, Artisans artisan) : base(type, province)
    {
        this.owner = artisan;
    }
    override public List<Storage> getRealNeeds()
    {
        return getRealNeeds(new Value(owner.getPopulation() / 1000f));
    }
    /// <summary>  Return in pieces basing on current prices and needs  /// </summary>        
    override public float getLocalEffectiveDemand(Product product)
    {
        return getLocalEffectiveDemand(product, new Procent(owner.getPopulation() / 1000f));
    }
    public override List<Storage> getHowMuchInputProductsReservesWants()
    {
        return getHowMuchInputProductsReservesWants(new Value(owner.getPopulation() / 1000f));
    }
    internal override Procent getInputFactor()
    {
        return getInputFactor(new Procent(owner.getPopulation() / 1000f));
    }
    /// <summary>
    /// Fills storageNow and gainGoodsThisTurn
    /// </summary>
    public override void produce()
    {
        produce(new Value(owner.getPopulation() * PopUnit.modEfficiency.getModifier(owner) * Options.ArtisansProductionModifier / 1000f));
    }
    /// <summary>
    /// Now includes workforce/efficiency. Also buying for upgrading\building are happening here 
    /// </summary>
    override public void buyNeeds()
    {
        List<Storage> shoppingList = getHowMuchInputProductsReservesWants();

        //todo !CAPITALISM part
        //if (isSubsidized())
        //    Game.market.buy(this, new PrimitiveStorageSet(shoppingList), province.getCountry());
        //else
            Game.market.buy(this, new PrimitiveStorageSet(shoppingList), null);
    }
}