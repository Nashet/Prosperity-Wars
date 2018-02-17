using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.ValueSpace;
namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represent anyone who can consume (but can't produce by itself)
    /// Stores data about last consumption
    /// </summary>
    public abstract class Consumer : Agent
    {
        /// <summary> Amount of consumed product (destroyed by consumption) including market and non-market consumption. Used for statistics </summary>
        private readonly StorageSet consumed = new StorageSet();
        private readonly StorageSet consumedLastTurnAwq = new StorageSet();
        /// <summary> Amount of product bought and consumed (destroyed by consumption). Included only market bought products. Used to calculate prices on market</summary>
        private readonly StorageSet consumedInMarket = new StorageSet();

        /// <summary>
        /// Represents buying and/or consuming needs     
        /// </summary>
        public abstract void consumeNeeds();
        public abstract List<Storage> getRealAllNeeds();

        protected Consumer(Bank bank, Country country) : base(0f, bank, country)
        {

        }
        /// <summary>
        /// Use for only reads!
        /// </summary>    
        public StorageSet getConsumed()
        {
            return consumed;
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
        public StorageSet getConsumedInMarket()
        {
            return consumedInMarket;
        }
        // Do I use where need to? Yes, I do. It goes to Market.Buy()
        public void consumeFromMarket(Storage what)
        {
            consumed.Add(what);
            consumedInMarket.Add(what);
            Game.market.sentToMarket.Subtract(what);
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
        override public void SetStatisticToZero()
        {
            base.SetStatisticToZero();            
            consumed.setZero();
            consumedInMarket.setZero();
        }
    }
}