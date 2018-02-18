using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.ValueSpace;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{

    /// <summary>
    /// represent ability to take loans/deposits
    /// </summary>
    abstract public class Agent : IHasCountry, IStatisticable
    {
        /// <summary> Used to calculate income tax, now it's only for statistics </summary>
        public Value moneyIncomeThisTurn = new Value(0);
        private readonly Value moneyIncomeLastTurn = new Value(0);
        public Value cash = new Value(0);
        /// <summary> could be null</summary>
        private Bank bank;
        public Value loans = new Value(0);
        public Value deposits = new Value(0);

        public Value incomeTaxPayed = new Value(0);

        public abstract void simulate();
        protected Country country;
        protected Agent(float inAmount, Bank bank, Country country)
        {
            cash.Set(inAmount);
            this.bank = bank;
            this.country = country;
        }
        public virtual void SetStatisticToZero()
        {
            moneyIncomeLastTurn.Set(moneyIncomeThisTurn);
            moneyIncomeThisTurn.Set(0f);
            incomeTaxPayed.SetZero();
        }
        /// <summary> Returns difference between moneyIncomeLastTurn and value</summary>    
        protected Value getSpendingLimit(ReadOnlyValue value)
        {
            return moneyIncomeLastTurn.Copy().Subtract(value, false);
        }
        //public Province Province
        //{
        //    return province;
        //}
        public Country Country
        {
            //get { return province.Country; }
            get { return country; }
        }
        public Bank getBank()
        {
            return bank;
        }
        public void setBank(Bank bank)
        {
            this.bank = bank;
        }
        /// <summary> Includes deposits. New value </summary>    
        public Value getMoneyAvailable()
        {
            if (bank == null)
                return cash.Copy();
            else
                return cash.Copy().Add(bank.HowMuchDepositCanReturn(this));//that's new Value
        }
        //new internal bool canPay(Value howMuchPay)
        //{
        //    return getMoneyTotal().isBiggerOrEqual(howMuchPay);
        //}
        ///// <summary>
        ///// depreciated
        ///// </summary>    
        //private bool canPay(float howMuchPay)
        //{
        //    throw new DontUseThatMethod();
        //}
        //internal CountryWallet getCountryWallet()
        //{
        //    if (this is Country)
        //        return wallet as CountryWallet;
        //    else
        //        return null;
        //}
        //todo should be Value
        //public float getLoans()
        //{
        //    if (loans.get() > 0f)
        //        return loans.get();
        //    else
        //        return 0f;
        //}
        //public float getDeposits()
        //{
        //    if (loans.get() > 0f)
        //        return 0f;
        //    else
        //        return loans.get() * -1f;
        //}
        //***************
        //todo redo those methods:
        internal bool canAfford(Storage need)
        {
            Storage realNeed;
            if (need.isAbstractProduct())
                //realNeed = new Storage(Game.market.getCheapestSubstitute(need).Product, need);
                realNeed = Game.market.getCheapestSubstitute(need);
            else
                realNeed = need;

            if (realNeed.get() == howMuchCanAfford(realNeed).get())
                return true;
            else
                return false;
        }

        internal bool canAfford(StorageSet need)
        {
            foreach (Storage stor in need)
            {
                if (howMuchCanAfford(stor).get() < stor.get())
                    return false;
            }
            return true;
        }
        internal bool canAfford(List<Storage> need)
        {
            foreach (Storage stor in need)
                if (howMuchCanAfford(stor).isSmallerThan(stor))
                    return false;
            return true;
        }
        /// <summary> Including deposits </summary>    
        internal Storage howMuchCanAfford(Storage need)
        {
            Value cost = Game.market.getCost(need);
            if (CanPay(cost))
                return new Storage(need);
            else
                return new Storage(need.Product, getMoneyAvailable().Divide(Game.market.getPrice(need.Product)));
        }
        /// <summary>WARNING! Can overflow if money > cost of need. use CanAfford before </summary>
        //internal Value HowMuchCanNotAfford(PrimitiveStorageSet need)
        //{
        //    return new Value(Game.market.getCost(need).get() - this.cash.get());
        //}
        /// <summary>WARNING! Can overflow if money > cost of need. use CanAfford before </summary>
        //internal Value HowMuchCanNotAfford(float need)
        //{
        //    return new Value(need - this.cash.get());
        //}

        /// <summary> Says how much money lack of desiredSum (counting cash and deposits in bank). New value. 
        /// Goes nut if cash + deposits >= desiredSum, no need for extra money, check that outside
        /// </summary>        
        internal Value HowMuchLacksMoneyIncludingDeposits(ReadOnlyValue desiredSum)
        {
            return desiredSum.Copy().Subtract(getMoneyAvailable(), false);
        }
        /// <summary>
        /// Says how much money lack of desiredSum (counting cash only). New value
        /// Says zero if cash >= desiredSum, no need for extra money
        /// </summary>        
        internal Value HowMuchLacksMoneyCashOnly(ReadOnlyValue desiredSum)
        {
            return desiredSum.Copy().Subtract(cash, false);
        }
        //internal Value HowMuchMoneyCanNotPay(Value value)
        //{
        //    return new Value(value.get() - this.cash.get());
        //}
        /// <summary>WARNING! Can overflow if money > cost of need. use CanAfford before </summary>
        //internal Value HowMuchCanNotAfford(Storage need)
        //{
        //    return new Value(Game.market.getCost(need) - this.cash.get());
        //}
        

        //private float get()
        //{
        //    throw new NotImplementedException();
        //}

        //internal bool canPay(Value howMuchPay)
        //{
        //    if (this.cash.get() >= howMuchPay.get())
        //        return true;
        //    else return false;
        //}
        //internal bool canPay(float howMuchPay)
        //{
        //    if (this.cash.get() >= howMuchPay)
        //        return true;
        //    else
        //        return false;
        //}
        /// <summary> Counting deposit and cash </summary>    
        internal bool CanPay(ReadOnlyValue howMuchPay)
        {
            return getMoneyAvailable().isBiggerOrEqual(howMuchPay);
        }
        internal bool CanPayCashOnly(ReadOnlyValue howMuchPay)
        {
            return cash.isBiggerOrEqual(howMuchPay);
        }

        public float getCash()
        {
            return cash.get();
        }

        /// <summary>
        /// Checks inside. Wouldn't pay if can't. Takes back deposits from bank, if needed
        /// Doesn't pay tax, doesn't register transaction
        /// </summary>    
        public bool payWithoutRecord(Agent whom, ReadOnlyValue howMuch, bool showMessageAboutNegativeValue = true)
        {
            if (CanPay(howMuch))// It does has enough cash or deposit
            {
                //if (!canPayCashOnly(howMuch) && bank != null)// checked for bank invention
                //{
                //    bank.giveLackingMoneyInCredit(this, howMuch);
                //    bank.giveLackingMoneyInCredit(this, howMuch.Copy().Multiply(5));
                //}
                if (CanPayCashOnly(howMuch))
                {
                    whom.cash.Add(howMuch);
                    this.cash.Subtract(howMuch);
                }
                else
                    bank.ReturnDeposit(this, HowMuchLacksMoneyCashOnly(howMuch)); 
                return true;
            }
            else
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log("Not enough money to pay in Agent.payWithoutRecord");
                return false;
            }

        }
        /// <summary>
        /// Checks inside. Wouldn't pay if can't. Takes back deposit from bank if needed
        /// Registers moneyIncomeThisTurn, pays tax. Returns true if was able to pay
        /// </summary>    
        public bool Pay(Agent incomeReceiver, Value howMuch, bool showMessageAboutNegativeValue = true)
        {
            if (howMuch.isNotZero())
                if (payWithoutRecord(incomeReceiver, howMuch, showMessageAboutNegativeValue))
                {
                    // income tax calculation
                    Value howMuchPayReally = howMuch.Copy();
                    incomeReceiver.moneyIncomeThisTurn.Add(howMuchPayReally);
                    if (incomeReceiver is Market) // Market wouldn't pay taxes cause it's abstract entity
                        return true;
                    Agent payer = this;

                    if (payer is Market == false //&& incomeReceiver is Market == false
                        && payer.Country != incomeReceiver.Country
                        && payer is Factory) // pay taxes in enterprise jurisdiction only if it's factory
                    {
                        var payed = payer.Country.TakeIncomeTaxFrom(incomeReceiver, howMuchPayReally, false);
                        howMuchPayReally.Subtract(payed);//and reduce taxable base
                    }

                    // in rest cases only pops pay taxes
                    var popReceiver = incomeReceiver as PopUnit;
                    if (popReceiver != null)
                        incomeReceiver.Country.TakeIncomeTaxFrom(popReceiver, howMuchPayReally, popReceiver.Type.isPoorStrata());
                    //else // if it's not Pop than it should by dividends from enterprise..
                    //{ 
                    //    //var countryPayer = incomeReceiver as Country;
                    //    //if (countryPayer != null)
                    //        incomeReceiver.Country.TakeIncomeTaxFrom(incomeReceiver, howMuchPayReally, false);
                    //}
                    return true;
                }
                else
                    return false;
            return true;
        }
        internal void PayAllAvailableMoney(Agent whom)
        {
            if (bank != null)
                bank.ReturnAllDeposits(this);
            Pay(whom, this.cash.Copy());            
        }
        internal void PayAllAvailableMoneyWithoutRecord(Agent whom)
        {
            if (bank != null)
                bank.ReturnAllDeposits(this);
            payWithoutRecord(whom, this.cash.Copy());            
        }
        public void ConvertFromGoldAndAdd(Value gold)
        {
            float coins = gold.get() * Options.goldToCoinsConvert;
            this.cash.Add(coins);
            this.moneyIncomeThisTurn.Add(coins);
            gold.Set(0);
        }

        override public string ToString()
        {
            return cash.get() + " coins";
        }
    }
}