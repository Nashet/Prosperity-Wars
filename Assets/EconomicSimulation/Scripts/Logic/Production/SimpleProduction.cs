using System;
using System.Collections.Generic;
using Nashet.EconomicSimulation.Reforms;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Contains common mechanics for Factory and ArtisanProduction
    /// </summary>
    public abstract class SimpleProduction : Producer
    {
        //private Agent owner;
        private readonly ProductionType type;

        public ProductionType Type { get { return type; } }
        private readonly StorageSet inputProductsReserve = new StorageSet();

        protected SimpleProduction(ProductionType type, Province province) : base(province)
        {
            this.type = type;
            //gainGoodsThisTurn = new Storage(this.Type.basicProduction.Product);
            //storage = new Storage(this.Type.basicProduction.Product);
            //sentToMarket = new Storage(this.Type.basicProduction.Product);
            changeProductionType(this.type.basicProduction.Product);
        }

        //public Agent getOwner()
        //{
        //    return owner;
        //}
        //public void setOwner(Agent agent)
        //{
        //    owner = agent;
        //}
        public StorageSet getInputProductsReserve()
        {
            return inputProductsReserve;
        }

        public override string ToString()
        {
            return "crafting " + type.basicProduction;
        }

        public override void simulate()
        {
            throw new NotImplementedException();
        }

        public override void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            storage.Set(0f);
        }

        /// <summary>
        /// could be negative
        /// </summary>        
        public decimal getProfit()

        {
            //return (float)(moneyIncomeThisTurn.Get() - getExpences().Get());
            if (Country.economy == Economy.PlannedEconomy)
                return 0m;
            else
                //return base.getProfit() - (float)getSalaryCost().Get();
                return Register.Balance;
        }

        /// <summary>
        /// Fills storageNow and gainGoodsThisTurn. Don't not to confuse with Producer.produce()
        /// </summary>
        protected void produce(Value multiplier)
        {
            addProduct(type.basicProduction.Copy().Multiply(multiplier));
            if (getGainGoodsThisTurn().isNotZero())
            {
                storage.add(getGainGoodsThisTurn());
                calcStatistics();
            }
            //consume Input Resources
            if (!type.isResourceGathering())
                foreach (Storage next in getRealAllNeeds())
                    if (next.isAbstractProduct())
                    {
                        var substitute = getInputProductsReserve().convertToBiggestStorage(next);
                        if (substitute.isNotZero())
                            getInputProductsReserve().Subtract(substitute, false); // could be zero reserves if isJustHiredPeople()
                    }
                    else
                        getInputProductsReserve().Subtract(next, false);
        }

        public abstract Procent getInputFactor();

        protected Procent getInputFactor(Procent multiplier)
        {
            if (multiplier.isZero())
                return Procent.ZeroProcent.Copy();
            if (type.isResourceGathering())
                return Procent.HundredProcent.Copy();

            List<Storage> reallyNeedResources = new List<Storage>();
            //Storage available;

            // how much we really want
            foreach (Storage input in type.resourceInput)
            {
                reallyNeedResources.Add(input.Copy().Multiply(multiplier));
            }

            // checking if there is enough in market
            //old DSB
            //foreach (Storage input in realInput)
            //{
            //    available = Country.market.HowMuchAvailable(input);
            //    if (available.get() < input.get())
            //        input.set(available);
            //}

            // check if we have enough resources
            foreach (Storage resource in reallyNeedResources)
            {
                Storage haveResource = getInputProductsReserve().getBiggestStorage(resource.Product);
                //if (!getInputProductsReserve().has(resource))
                if (haveResource.isSmallerThan(resource))
                {
                    // what we really have
                    resource.set(haveResource);
                }
            }
            //old last turn consumption checking thing
            //foreach (Storage input in realInput)
            //{
            //    //if (Country.market.getDemandSupplyBalance(input.Product) >= 1f)
            //    //available = input

            //    available = consumedLastTurn.findStorage(input.Product);
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
            var inputFactor = Procent.HundredProcent.Copy();
            // searching lowest factor
            foreach (Storage need in reallyNeedResources)
            {
                Value denominator = type.resourceInput.GetFirstSubstituteStorage(need.Product).Copy().Multiply(multiplier);
                if (denominator.isNotZero())
                {
                    var newfactor = new Procent(need, denominator);
                    if (newfactor.isSmallerThan(inputFactor))
                        inputFactor = newfactor;
                }
                else // no resources
                    inputFactor.Set(0f);
            }
            return inputFactor;
        }

        public abstract List<Storage> getHowMuchInputProductsReservesWants();

        protected List<Storage> getHowMuchInputProductsReservesWants(Value multiplier)
        {
            //Value multiplier = new Value(getWorkForceFulFilling() * getLevel() * Options.FactoryInputReservInDays);
            if (type.isResourceGathering())
                return null;
            List<Storage> result = new List<Storage>();

            foreach (Storage next in type.resourceInput)
            {
                Storage howMuchWantToBuy = new Storage(next);
                howMuchWantToBuy.Multiply(multiplier);
                Storage howMuchHave = getInputProductsReserve().getBiggestStorage(next.Product);
                if (howMuchWantToBuy.isBiggerThan(howMuchHave))
                {
                    howMuchWantToBuy.subtract(howMuchHave);
                    result.Add(howMuchWantToBuy);
                }//else  - there is enough reserves, you shouldn't buy than
            }
            return result;
        }

        // Should remove market availability assumption since its goes to double- calculation?
        //public List<Storage> getRealNeeds()
        //{
        //    Value multiplier = new Value(getEfficiency(false).get() * getLevel());

        //    List<Storage> result = new List<Storage>();

        //    foreach (Storage next in type.resourceInput)
        //    {
        //        Storage nStor = new Storage(next.Product, next.get());
        //        nStor.multiple(multiplier);
        //        result.Add(nStor);
        //    }
        //    return result;
        //}

        protected IEnumerable<Storage> getRealNeeds(Value multiplier)
        {
            //Value multiplier = new Value(getEfficiency(false).get() * getLevel());
            if (type.isResourceGathering())
                yield return null;
            //List<Storage> result = new List<Storage>();

            foreach (Storage next in type.resourceInput)
            {
                Storage nStor = new Storage(next.Product, next.get());
                nStor.Multiply(multiplier);
                //result.Add(nStor);
                yield return nStor;
            }
            //return result;
        }
        protected IEnumerable<Storage> getRealNeeds(float multiplier)
        {
            //Value multiplier = new Value(getEfficiency(false).get() * getLevel());
            if (type.isResourceGathering())
                yield return null;
            //List<Storage> result = new List<Storage>();

            foreach (Storage next in type.resourceInput)
            {
                Storage nStor = new Storage(next.Product, next.get());
                nStor.Multiply(multiplier);
                //result.Add(nStor);
                yield return nStor;
            }
            //return result;
        }

        /// <summary>  Return in pieces basing on current prices and needs  /// </summary>
        protected float getLocalEffectiveDemand(Product product, Procent multiplier)
        {
            // need to know how much i Consumed inside my needs
            Storage need = type.resourceInput.GetFirstSubstituteStorage(product).Copy();
            if (need.isZero())
                return 0f;
            else
            {
                Storage realNeed = need.Multiply(multiplier.get());
                Storage canAfford = HowMuchCanAfford(realNeed);
                return canAfford.get();
            }
        }

        /// <summary>
        ///new value
        /// </summary>
        public virtual MoneyView getExpences()
        {
            return Country.market.getCost(getConsumed());
        }

        public bool isAllInputProductsCollected()
        {
            var realNeeds = getRealAllNeeds();
            foreach (var item in realNeeds)
            {
                if (!inputProductsReserve.has(item))
                    return false;
            }
            return true;
        }
        override protected void Buy_utility(Market market, MoneyView cost, Storage what)
        {
            base.Buy_utility(market, cost, what);
            getInputProductsReserve().Add(what);
        }
    }
}