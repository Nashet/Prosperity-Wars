using System.Collections.Generic;
using System.Linq;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represent anyone who can consume (but can't produce by itself)
    /// Stores data about last consumption
    /// </summary>
    public abstract class Consumer : Agent
    {
        /// <summary> Amount of consumed product (destroyed by consumption) including market and non-market consumption. Used for statistics </summary>
        protected readonly StorageSet consumed = new StorageSet();

        private readonly StorageSet consumedLastTurnAwq = new StorageSet();

        /// <summary> Amount of product bought and consumed (destroyed by consumption). Included only market bought products. Used to calculate prices on market</summary>
        protected readonly List<KeyValuePair<Market, Storage>> consumedInMarket = new List<KeyValuePair<Market, Storage>>();

        /// <summary>
        /// Represents buying and/or consuming needs
        /// </summary>
        public abstract void consumeNeeds();

        public abstract IEnumerable<Storage> getRealAllNeeds();

        protected Consumer(Country country) : base(country)
        {
        }

        /// <summary>
        /// Use for only reads!
        /// </summary>
        public IEnumerable<Storage> getConsumed()
        {
            foreach (var item in consumed)
            {
                yield return item;
            }
            //return consumed;
        }

        /// <summary>
        /// Use for only reads!
        /// </summary>
        public StorageSet getConsumedLastTurn()
        {
            return consumed;
        }

        /// <summary>
        /// Use for only reads!
        /// </summary>
        public IEnumerable<Storage> AllConsumedInMarket(Market market)
        {
            foreach (var item in consumedInMarket.Where(x => x.Key == market))
                yield return item.Value;
        }
        public IEnumerable<KeyValuePair<Market, Storage>> AllConsumedInMarket()
        {
            foreach (var item in consumedInMarket)
                yield return item;
        }


        /// <summary>
        /// Buys, returns actually bought, subsidizations allowed, uses deposits if available
        /// </summary>
        //public Storage Buy(Storage need)// old market.Sell
        //{
        //    if (this.CanAfford(need))
        //    {
        //        return Buy(need);
        //    }
        //    //todo fix that - return subsidies
        //    //else if (subsidizer.GiveFactorySubsidies(toWhom, toWhom.HowMuchLacksMoneyIncludingDeposits(getCost(need))))
        //    //{
        //    //    return Sell(toWhom, need);
        //    //}
        //    else
        //        return new Storage(need.Product, 0f);
        //}
        /// <summary>
        /// returns how much was sold de facto
        /// new version of buy-old,
        /// real deal. If not enough money to buy (including deposits) then buys some part of desired
        /// </summary>
        protected Storage Buy(Storage need) // old Market.Sell
        {
            if (need.isNotZero())
            {
                Market market = Market.GetCheapestMarket(need);
                Storage sale;
                if (need.Product.isAbstract())
                {
                    sale = market.prices.ConvertToRandomCheapestExistingSubstitute(need, Country.market);
                    if (sale == null)//no substitution available on market
                        return new Storage(need.Product);
                    else if (sale.isZero())
                        return sale;
                }
                else
                    sale = need;

                Storage howMuchCanConsume;
                MoneyView price = Country.market.getCost(sale.Product);
                MoneyView cost;

                if (market.HasAvailable(sale))
                {
                    cost = Country.market.getCost(sale);

                    if (this.CanPay(cost))
                    {
                        this.Buy_utility(market, cost, sale);
                        return sale;
                    }
                    else
                    {
                        float val = (float)(this.getMoneyAvailable().Get() / price.Get());
                        howMuchCanConsume = new Storage(sale.Product, val);
                        howMuchCanConsume.Subtract(0.001f, false); // to fix precision bug
                        if (howMuchCanConsume.isZero())
                            return howMuchCanConsume;
                        else
                        {

                            this.Buy_utility(market, Country.market.getCost(howMuchCanConsume), howMuchCanConsume);
                            return howMuchCanConsume;
                        }
                    }
                }
                else
                {
                    // assuming available < buying
                    Storage howMuchAvailable = new Storage(market.HowMuchAvailable(sale));
                    if (howMuchAvailable.isNotZero())
                    {
                        cost = Country.market.getCost(howMuchAvailable);
                        if (this.CanPay(cost))
                        {
                            this.Buy_utility(market, cost, howMuchAvailable);
                            return howMuchAvailable;
                        }
                        else
                        {
                            howMuchCanConsume = new Storage(howMuchAvailable.Product, (float)(this.getMoneyAvailable().Get() / price.Get()));

                            if (howMuchCanConsume.get() > howMuchAvailable.get())
                                howMuchCanConsume.Set(howMuchAvailable.get()); // you don't buy more than there is

                            howMuchCanConsume.Subtract(0.001f, false); // to fix precision bug
                            if (howMuchCanConsume.isNotZero())
                            {
                                this.Buy_utility(market, this.getMoneyAvailable(), howMuchCanConsume);//pay all money cause you don't have more                                                                        
                                return howMuchCanConsume;
                            }
                            else
                                return howMuchCanConsume;
                        }
                    }
                    else
                        return howMuchAvailable;
                }
            }
            else
                return need; // assuming buying is empty here            
        }

        /// <summary>
        /// Buying needs in circle, by Procent in time
        /// return true if buying is zero (bought all what it wanted)
        /// former bool Sell(Producer buyer, StorageSet stillHaveToBuy, Procent buyInTime, List<Storage> ofWhat)
        /// </summary>
        public bool Buy(StorageSet stillHaveToBuy, Procent buyInTime, List<Storage> ofWhat)
        {
            bool buyingIsFinished = true;
            foreach (Storage what in ofWhat)
            {
                Storage consumeOnThisIteration = new Storage(what.Product, what.get() * buyInTime.get());
                if (consumeOnThisIteration.isZero())
                    return true;

                // check if consumeOnThisIteration is not bigger than stillHaveToBuy
                if (!stillHaveToBuy.has(consumeOnThisIteration))
                    consumeOnThisIteration = stillHaveToBuy.getBiggestStorage(what.Product);

                var reallyBought = Buy(consumeOnThisIteration, null);

                stillHaveToBuy.Subtract(reallyBought);

                if (stillHaveToBuy.getBiggestStorage(what.Product).isNotZero())
                    buyingIsFinished = false;
            }
            return buyingIsFinished;
        }

        /// <summary>
        /// Buys, returns actually bought, subsidizations allowed, uses deposits if available
        /// </summary>
        public Storage Buy(Storage need, Country subsidizer)
        {
            if (CanAfford(need) || subsidizer == null)
            {
                return Buy(need);
            }
            //todo fix that
            else
            {
                if (subsidizer.GiveFactorySubsidies(this, HowMuchLacksMoneyIncludingDeposits(Country.market.getCost(need))))
                {
                    return Buy(need);
                }
                else
                    return new Storage(need.Product, 0f);
            }
        }
        // Do I use it where need to? Yes, I do. It called from this.Buy()
        protected virtual void Buy_utility(Market market, MoneyView cost, Storage what)
        {
            this.Pay(market, cost, Register.Account.MarketOperations);
            consumed.Add(what);
            consumedInMarket.Add(new KeyValuePair<Market, Storage>(market, what));
            market.SendGoods(what);


            if (Game.logMarket)
                Debug.Log(this + " consumed from " + market + " " + what + " costing " + Country.market.getCost(what));
        }

        public void consumeFromCountryStorage(List<Storage> what, Country country)
        {
            consumed.Add(what);
            country.countryStorageSet.subtract(what);
        }

        public void consumeFromCountryStorage(Storage what, Country country)
        {
            consumed.Add(what);
            country.countryStorageSet.Subtract(what);
        }

        public override void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            consumed.setZero();
            consumedInMarket.Clear();
        }

    }
}