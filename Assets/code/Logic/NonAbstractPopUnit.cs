using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class GrainGetter : PopUnit
{
    protected GrainGetter(PopUnit source, int sizeOfNewPop, PopType newPopType, Province where, Culture culture) : base(source, sizeOfNewPop, newPopType, where, culture)
    {

        changeProductionType(Product.Grain);
        //sentToMarket = new Storage(Product.Grain);
    }
    protected GrainGetter(int amount, PopType popType, Culture culture, Province where) : base(amount, popType, culture, where)
    {
        //storage = new Storage(Product.Grain);
        //gainGoodsThisTurn = new Storage(Product.Grain);
        changeProductionType(Product.Grain);
        //sentToMarket = new Storage(Product.Grain);
    }
}
abstract public class CattleGetter : PopUnit
{
    protected CattleGetter(PopUnit source, int sizeOfNewPop, PopType newPopType, Province where, Culture culture) : base(source, sizeOfNewPop, newPopType, where, culture)
    {
        //storage = new Storage(Product.Cattle);
        //gainGoodsThisTurn = new Storage(Product.Cattle);
        //sentToMarket = new Storage(Product.Cattle);
        changeProductionType(Product.Cattle);
    }
    protected CattleGetter(int amount, PopType popType, Culture culture, Province where) : base(amount, popType, culture, where)
    {
        //storage = new Storage(Product.Cattle);
        //gainGoodsThisTurn = new Storage(Product.Cattle);
        //sentToMarket = new Storage(Product.Cattle);
        changeProductionType(Product.Cattle);
    }
}
public class Tribesmen : CattleGetter
{
    public Tribesmen(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Tribesmen, where, culture)
    {
    }
    public Tribesmen(int iamount, Culture iculture, Province where) : base(iamount, PopType.Tribesmen, iculture, where)
    {
    }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == PopType.Workers
            //|| targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming) // commented this to get more workers &  more ec. growth           
            || targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy))
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == PopType.Aristocrats
            //|| targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
            //|| targetType == PopType.Soldiers && !getCountry().isInvented(Invention.ProfessionalArmy))
            )
            return true;
        else
            return false;
    }
    public override void produce()
    {
        Storage producedAmount;
        float overpopulation = getProvince().getOverpopulation();
        if (overpopulation <= 1f) // all is OK
            producedAmount = new Storage(popType.getBasicProduction().getProduct(), getPopulation() * popType.getBasicProduction().get() / 1000f);
        else
            producedAmount = new Storage(popType.getBasicProduction().getProduct(), getPopulation() * popType.getBasicProduction().get() / 1000f / overpopulation);


        if (producedAmount.isNotZero())
        {
            storage.add(producedAmount);
            addProduct(producedAmount);
            calcStatistics();
        }
    }
    internal override bool canTrade()
    {
        return false;
    }
    public override bool shouldPayAristocratTax()
    {
        return true;
    }

    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Tribal || reform == Government.Democracy)
            && (isStateCulture() || getCountry().minorityPolicy.getValue() == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }

    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            return 1;
        else
            return 0;
    }
    public override void consumeNeeds()
    {
        //life needs First
        consumeWithNaturalEconomy(getRealLifeNeeds());
    }
}
public class Farmers : GrainGetter
{
    public Farmers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Farmers, where, culture)
    { }
    public Farmers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Farmers, iculture, where)
    { }

    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy)
         || targetType == PopType.Tribesmen
         || targetType == PopType.Workers
            )
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == PopType.Aristocrats
          || targetType == PopType.Capitalists && getCountry().isInvented(Invention.Manufactories)
            )
            return true;
        else
            return false;
    }
    public override void produce()
    {
        Storage producedAmount = new Storage(popType.getBasicProduction().getProduct(), getPopulation() * popType.getBasicProduction().get() / 1000f);
        producedAmount.multiply(modEfficiency.getModifier(this), false); // could be negative with bad modifiers, defaults to zero                
        if (producedAmount.isNotZero())
        {
            addProduct(producedAmount);
            storage.add(getGainGoodsThisTurn());
            calcStatistics();
        }
        if (Economy.isMarket.checkIftrue(getCountry()))
        {
            //sentToMarket.set(gainGoodsThisTurn);
            //Game.market.sentToMarket.add(gainGoodsThisTurn);
            sell(getGainGoodsThisTurn());
        }
        else
        {
            if (getCountry().economy.getValue() == Economy.PlannedEconomy)
            {
                getCountry().countryStorageSet.add(getGainGoodsThisTurn());
            }
        }

    }
    override internal bool canSellProducts()
    {
        if (Economy.isMarket.checkIftrue(getCountry()))
            return true;
        else
            return false;
    }
    public override bool shouldPayAristocratTax()
    {
        return true;
    }
    //internal override bool getSayingYes(AbstractReformValue reform)
    //{
    //    if (reform is Government.ReformValue)
    //    {
    //        if (reform == Government.Tribal)
    //        {
    //            var baseOpinion = new Procent(0f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else if (reform == Government.Aristocracy)
    //        {
    //            var baseOpinion = new Procent(0.2f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else if (reform == Government.Democracy)
    //        {
    //            var baseOpinion = new Procent(1f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else if (reform == Government.Despotism)
    //        {
    //            var baseOpinion = new Procent(0.2f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else if (reform == Government.ProletarianDictatorship)
    //        {
    //            var baseOpinion = new Procent(0.3f);
    //            baseOpinion.add(this.loyalty);
    //            return baseOpinion.get() > Options.votingPassBillLimit;
    //        }
    //        else
    //            return false;
    //    }
    //    else if (reform is TaxationForPoor.ReformValue)
    //    {
    //        TaxationForPoor.ReformValue taxReform = reform as TaxationForPoor.ReformValue;
    //        var baseOpinion = new Procent(1f);
    //        baseOpinion.set(baseOpinion.get() - taxReform.tax.get() * 2);
    //        baseOpinion.set(baseOpinion.get() + loyalty.get() - 0.5f);
    //        return baseOpinion.get() > Options.votingPassBillLimit;
    //    }
    //    else
    //        return false;
    //}
    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy)
            && (isStateCulture() || getCountry().minorityPolicy.getValue() == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }

    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            return 1;
        else
            return 0;
    }
}
public class Aristocrats : GrainGetter
{
    public Aristocrats(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Aristocrats, where, culture)
    { }
    public Aristocrats(int iamount, Culture iculture, Province where) : base(iamount, PopType.Aristocrats, iculture, where)
    { }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
            || targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy)
            || targetType == PopType.Tribesmen)
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        return false;
    }
    internal void dealWithMarket()
    {
        if (storage.get() > Options.aristocratsFoodReserv)
        {
            Storage howMuchSend = new Storage(storage.getProduct(), storage.get() - Options.aristocratsFoodReserv);
            storage.send(getSentToMarket(), howMuchSend);
            //sentToMarket.set(howMuchSend);
            Game.market.sentToMarket.add(howMuchSend);
        }
    }
    public override void produce()
    {
        //Aristocrats don't produce anything
    }
    internal override bool canTrade()
    {
        if (getCountry().economy.getValue() == Economy.PlannedEconomy)
            return false;
        else
            return true;
    }
    override internal bool canSellProducts()
    {
        return true;
    }
    public override bool shouldPayAristocratTax()
    {
        return false;
    }
    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy
            || reform == Government.Aristocracy || reform == Government.Tribal)
            && (isStateCulture() || getCountry().minorityPolicy.getValue() == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }
    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            if (reformValue == Government.WealthDemocracy)
                return Options.PopRichStrataVotePower;
            else
                return 1;
        else
            return 0;
    }
    internal override void invest()
    {
        // Aristocrats invests only in resource factories (and banks)
        if (getProvince().getResource() != null && !getProvince().isThereFactoriesInUpgradeMoreThan(Options.maximumFactoriesInUpgradeToBuildNew))
        {
            FactoryType ftype = FactoryType.whoCanProduce(getProvince().getResource());
            if (ftype != null)
            {
                Factory resourceFactoryInHere = getProvince().getExistingResourceFactory();
                if (resourceFactoryInHere == null) //build new factory
                {
                    //Has money / resources?
                    StorageSet resourceToBuild = ftype.getBuildNeeds();
                    // todo remove connection to grain
                    Storage needFood = resourceToBuild.getFirstStorage(Product.Grain);
                    // try to build for food
                    if (storage.isBiggerOrEqual(needFood))
                    {
                        var newFactory = new Factory(getProvince(), this, ftype);
                        storage.send(newFactory.getInputProductsReserve(), needFood);
                        newFactory.constructionNeeds.setZero();
                    }
                    else
                    {
                        Value cost = Game.market.getCost(resourceToBuild);
                        if (canPay(cost))  //try to build for money  
                        {
                            var newFactory = new Factory(getProvince(), this, ftype);  // build new one              
                            pay(newFactory, cost);
                        }// take loans? Aristocrats wouldn't take loans for upgrades
                    }
                }
                else
                {
                    //upgrade existing resource factory
                    // upgrade is only for money? Yes, because its complicated - lots of various products               
                    if (resourceFactoryInHere.getWorkForceFulFilling().isBiggerThan(Options.minWorkforceFullfillingToUpgradeFactory)
                        && resourceFactoryInHere.getMargin().get() >= Options.minMarginToUpgrade
                        && Factory.conditionsUpgrade.isAllTrue(resourceFactoryInHere, this))
                        resourceFactoryInHere.upgrade(this);
                }
            }
        }
        base.invest();
    }
}
public class Soldiers : GrainGetter
{
    public Soldiers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Soldiers, where, culture)
    { }
    public Soldiers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Soldiers, iculture, where)
    { }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (//targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
            //||
            targetType == PopType.Tribesmen
            || targetType == PopType.Workers
            )
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == PopType.Aristocrats // should be officers
         || targetType == PopType.Artisans
         || targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
         )
            return true;
        else
            return false;
    }

    public override bool shouldPayAristocratTax()
    {
        return false;
    }

    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.Junta)
            && (isStateCulture() || getCountry().minorityPolicy.getValue() == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }
    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            return 1;
        else
            return 0;
    }

    public override void produce()
    {

    }

    internal void takePayCheck()
    {
        Value payCheck = new Value(getCountry().getSoldierWage());
        payCheck.multiply(getPopulation() / 1000f);
        if (getCountry().canPay(payCheck))
        {
            getCountry().pay(this, payCheck);
            getCountry().soldiersWageExpenseAdd(payCheck);
        }
        else
        {
            this.didntGetPromisedSalary = true;
            getCountry().failedToPaySoldiers = true;
        }
    }
}
public class Capitalists : GrainGetter
{
    public Capitalists(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Capitalists, where, culture)
    { }
    public Capitalists(int iamount, Culture iculture, Province where) : base(iamount, PopType.Capitalists, iculture, where)
    { }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
            || targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy)
            || targetType == PopType.Artisans
            )
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        return false;
    }
    public override void produce()
    {
        // Caps don't produce products directly
    }
    internal override bool canTrade()
    {
        if (getCountry().economy.getValue() == Economy.PlannedEconomy)
            return false;
        else
            return true;
    }
    public override bool shouldPayAristocratTax()
    {
        return false;
    }
    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy
            || reform == Government.BourgeoisDictatorship)
            && (isStateCulture() || getCountry().minorityPolicy.getValue() == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }
    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            if (reformValue == Government.WealthDemocracy)
                return Options.PopRichStrataVotePower;
            else
                return 1;
        else
            return 0;
    }
    internal override void invest()
    {
        if (Economy.isMarket.checkIftrue(getCountry()) && getCountry().isInvented(Invention.Manufactories))
        {
            //should I build?
            //province.getUnemployed() > Game.minUnemploymentToBuldFactory && 
            if (!getProvince().isThereFactoriesInUpgradeMoreThan(Options.maximumFactoriesInUpgradeToBuildNew))
            {
                FactoryType proposition = FactoryType.getMostTeoreticalProfitable(getProvince());
                if (proposition != null && proposition.canBuildNewFactory(getProvince()) &&
                    (getProvince().getUnemployedWorkers() > Options.minUnemploymentToBuldFactory || getProvince().getAverageFactoryWorkforceFullfilling() > Options.minFactoryWorkforceFullfillingToBuildNew))
                {
                    Value buildCost = Game.market.getCost(proposition.getBuildNeeds());
                    buildCost.add(Options.factoryMoneyReservPerLevel);
                    if (canPay(buildCost))
                    {
                        Factory found = new Factory(getProvince(), this, proposition);
                        payWithoutRecord(found, buildCost);
                    }
                    else
                        if (getBank().giveLackingMoney(this, buildCost))
                    {
                        Factory found = new Factory(getProvince(), this, proposition);
                        payWithoutRecord(found, buildCost);
                    }
                }
                else
                {
                    //upgrade section
                    Factory factory = FactoryType.getMostPracticlyProfitable(getProvince());
                    if (factory != null
                        && factory.canUpgrade() // don't change it to Modifier  - it would prevent loan takes
                        && factory.getMargin().get() >= Options.minMarginToUpgrade
                        && factory.getWorkForceFulFilling().isBiggerThan(Options.minWorkforceFullfillingToUpgradeFactory))
                    {
                        Value upgradeCost = Game.market.getCost(factory.getUpgradeNeeds());
                        if (canPay(upgradeCost))
                            factory.upgrade(this);
                        else if (getBank().giveLackingMoney(this, upgradeCost))
                            factory.upgrade(this);
                    }
                }
            }

        }
        base.invest();
    }
}
public class Artisans : GrainGetter
{
    private ArtisanProduction artisansProduction;
    public Artisans(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Artisans, where, culture)
    {
        changeProductionType();
    }
    public Artisans(int amount, Culture culture, Province where) : base(amount, PopType.Artisans, culture, where)
    {
        changeProductionType();
    }
    override protected void deleteData()
    {
        base.deleteData();
        artisansProduction = null;
    }
    public override bool canThisDemoteInto(PopType targetType)
    {
        if (//|| targetType == PopType.Farmers && !getCountry().isInvented(Invention.Farming)
            targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy)
            || targetType == PopType.Workers
            )
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == PopType.Capitalists && getCountry().isInvented(Invention.Manufactories))
            return true;
        else
            return false;
    }
    public override void produce()
    {
        // artisan shouldn't work with PE
        if (getCountry().economy.getValue() == Economy.PlannedEconomy)
            artisansProduction = null;
        else
        {
            if (Game.Random.Next(Options.ArtisansChangeProductionRate) == 1
               )// && (artisansProduction==null 
                //|| (artisansProduction !=null && needsFulfilled.isSmallerThan(Options.ArtisansChangeProductionLevel))))
                changeProductionType();

            if (artisansProduction != null)
            {
                if (artisansProduction.isAllInputProductsCollected())
                {
                    artisansProduction.produce();
                    if (Economy.isMarket.checkIftrue(getCountry()))
                    {
                        sell(getGainGoodsThisTurn());
                        //sentToMarket.set(gainGoodsThisTurn);
                        //storage.setZero();
                        //Game.market.sentToMarket.add(gainGoodsThisTurn);
                    }
                    else if (getCountry().economy.getValue() == Economy.NaturalEconomy)
                    {
                        // send to market?
                        sell(getGainGoodsThisTurn());
                        //sentToMarket.set(gainGoodsThisTurn);
                        //storage.setZero();
                        //Game.market.sentToMarket.add(gainGoodsThisTurn);
                    }
                    else if (getCountry().economy.getValue() == Economy.PlannedEconomy)
                    {
                        storage.sendAll(getCountry().countryStorageSet);
                    }
                }
                //else
                //   changeProductionType();
            }
        }
    }
    public override void consumeNeeds()
    {
        base.consumeNeeds();
        if (artisansProduction != null)
        {
            payWithoutRecord(artisansProduction, cash);

            // take loan if don't have enough money to buy inputs            
            if (getCountry().isInvented(Invention.Banking) && !artisansProduction.isAllInputProductsCollected())
                if (artisansProduction.getType().getPossibleProfit(getProvince()).isNotZero())
                {
                    var needs = artisansProduction.getRealAllNeeds();
                    if (!artisansProduction.canAfford(needs))
                    {
                        var loanSize = Game.market.getCost(needs); // takes little more than really need, could be fixed
                        if (getBank().canGiveMoney(this, loanSize))
                            getBank().giveMoney(this, loanSize);
                        payWithoutRecord(artisansProduction, cash);
                    }
                }

            artisansProduction.consumeNeeds();
            artisansProduction.payWithoutRecord(this, artisansProduction.cash);

            // consuming made in artisansProduction.consumeNeeds()
            // here is data transfering
            // todo rework data transfering from artisans?
            this.getConsumedInMarket().add(artisansProduction.getConsumedInMarket());
            this.getConsumed().add(artisansProduction.getConsumed());
            this.getConsumedLastTurn().add(artisansProduction.getConsumedLastTurn());
        }
    }
    internal override bool canTrade()
    {
        if (getCountry().economy.getValue() == Economy.PlannedEconomy)
            return false;
        else
            return true;
    }
    override internal bool canSellProducts()
    {
        return true;
    }
    public override bool shouldPayAristocratTax()
    {
        return true;
    }

    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.Polis || reform == Government.WealthDemocracy
            || reform == Government.BourgeoisDictatorship)
            && (isStateCulture() || getCountry().minorityPolicy.getValue() == MinorityPolicy.Equality))
            return true;
        else
            return false;
    }
    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            if (reformValue == Government.WealthDemocracy)
                return Options.PopMiddleStrataVotePower;
            else
                return 1;
        else
            return 0;
    }
    private void changeProductionType()
    {
        KeyValuePair<FactoryType, float> result = new KeyValuePair<FactoryType, float>(null, 0f);
        foreach (FactoryType factoryType in FactoryType.getNonResourceTypes(getCountry()))
        {
            float possibleProfit = factoryType.getPossibleProfit(getProvince()).get();
            if (possibleProfit > result.Value)
                result = new KeyValuePair<FactoryType, float>(factoryType, possibleProfit);
        }
        if (result.Key != null && (artisansProduction == null || artisansProduction != null && result.Key != artisansProduction.getType()))
        {
            artisansProduction = new ArtisanProduction(result.Key, getProvince(), this);
            base.changeProductionType(artisansProduction.getType().basicProduction.getProduct());
        }
    }
    public StorageSet getInputProducts()
    {
        if (artisansProduction == null)
            return null;
        else
            return artisansProduction.getInputProductsReserve();
    }
    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        if (artisansProduction != null)
            artisansProduction.setStatisticToZero();
    }
    public FactoryType getType()
    {
        if (artisansProduction == null)
            return null;
        else
            return artisansProduction.getType();
    }

    internal void checkProfit()
    {
        // todo doesn't include taxes. Should it?
        if (artisansProduction == null || moneyIncomethisTurn.get() - artisansProduction.getExpences() <= 0f)
            changeProductionType();
    }
}
public class Workers : GrainGetter
{
    public Workers(PopUnit pop, int sizeOfNewPop, Province where, Culture culture) : base(pop, sizeOfNewPop, PopType.Workers, where, culture)
    { }
    public Workers(int iamount, Culture iculture, Province where) : base(iamount, PopType.Workers, iculture, where)
    { }

    public override bool canThisDemoteInto(PopType targetType)
    {
        if (targetType == PopType.Tribesmen
            || targetType == PopType.Soldiers && getCountry().isInvented(Invention.ProfessionalArmy))
            return true;
        else
            return false;
    }
    public override bool canThisPromoteInto(PopType targetType)
    {
        if (targetType == PopType.Farmers && getCountry().isInvented(Invention.Farming)
         || targetType == PopType.Artisans
         )
            return true;
        else
            return false;
    }
    public override void produce()
    { }

    public override bool shouldPayAristocratTax()
    {
        return true;
    }

    internal override bool canVote(Government.ReformValue reform)
    {
        if ((reform == Government.Democracy || reform == Government.ProletarianDictatorship) // temporally
            && (isStateCulture() || getCountry().minorityPolicy.getValue() == MinorityPolicy.Equality)
            )
            return true;
        else
            return false;
    }

    internal override int getVotingPower(Government.ReformValue reformValue)
    {
        if (canVote(reformValue))
            return 1;
        else
            return 0;
    }
}
//public class PopLinkageValue
//{
//    public PopType type;
//    public Value amount;
//    internal PopLinkageValue(PopType p, Value a)
//    {
//        type = p;
//        amount = a;
//    }
//}