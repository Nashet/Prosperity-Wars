using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.ValueSpace;
namespace Nashet.EconomicSimulation
{

    /// <summary>
    /// represent ability to take loans/deposits
    /// </summary>
    abstract public class Agent : IHasGetCountry, IHasStatistics, IHasGetProvince
    {
        /// <summary> Must be filled together with wallet </summary>
        public Value moneyIncomethisTurn = new Value(0);
        private readonly Value moneyIncomeLastTurn = new Value(0);
        public Value cash = new Value(0);
        /// <summary> could be null</summary>
        private Bank bank;
        public Value loans = new Value(0);
        public Value deposits = new Value(0);
        private readonly Province province;
        public Value incomeTaxPayed = new Value(0);

        public abstract void simulate();

        protected Agent(float inAmount, Bank bank, Province province)
        {
            cash.set(inAmount);
            this.bank = bank;
            this.province = province;
        }
        public virtual void SetStatisticToZero()
        {
            moneyIncomeLastTurn.set(moneyIncomethisTurn);
            moneyIncomethisTurn.set(0f);
            incomeTaxPayed.setZero();
        }
        /// <summary> Returns difference between moneyIncomeLastTurn and value</summary>    
        protected Value getSpendingLimit(ReadOnlyValue value)
        {
            return moneyIncomeLastTurn.Copy().subtract(value, false);
        }
        public Province GetProvince()
        {
            return province;
        }
        virtual public Country GetCountry()
        {
            return province.GetCountry();
        }
        public Bank getBank()
        {
            return bank;
        }
        public void setBank(Bank bank)
        {
            this.bank = bank;
        }
        /// <summary> Includes deposits. Is copy </summary>    
        public Value getMoneyAvailable()
        {
            if (bank == null)
                return cash.Copy();
            else
                return cash.Copy().Add(bank.howMuchDepositCanReturn(this));//that's new Value
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
        internal bool canAfford(Storage need)
        {
            Storage realNeed;
            if (need.isAbstractProduct())
                //realNeed = new Storage(Game.market.getCheapestSubstitute(need).getProduct(), need);
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
        /// <summary> Returns how much you lack. Doesn't change data
        /// WARNING! Can overflow if money > cost of need. use CanAfford before </summary>
        internal Value GetLackingMoney(ReadOnlyValue need)
        {
            return need.Copy().subtract(getMoneyAvailable());
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
        /// <summary> Including deposits </summary>    
        internal Storage howMuchCanAfford(Storage need)
        {
            Value cost = Game.market.getCost(need);
            if (canPay(cost))
                return new Storage(need);
            else
                return new Storage(need.getProduct(), getMoneyAvailable().divide(Game.market.getPrice(need.getProduct())));
        }

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
        /// <summary> Includes deposits </summary>    
        internal bool canPay(ReadOnlyValue howMuchPay)
        {
            return getMoneyAvailable().isBiggerOrEqual(howMuchPay);
        }
        internal bool canPayCashOnly(ReadOnlyValue howMuchPay)
        {
            return cash.isBiggerOrEqual(howMuchPay);
        }

        public float getCash()
        {
            return cash.get();
        }

        /// <summary>
        /// checks inside. Wouldn't pay if can't. Takes credits from bank
        /// Doesn't pay tax, doesn't register transaction
        /// </summary>    
        public bool payWithoutRecord(Agent whom, ReadOnlyValue howMuch, bool showMessageAboutNegativeValue = true)
        {
            if (canPay(howMuch))
            {
                if (!canPayCashOnly(howMuch) && bank != null)// checked for bank inv
                {
                    bank.giveLackingMoney(this, howMuch);
                    bank.giveLackingMoney(this, howMuch.Copy().multiply(5));
                }
                whom.cash.Add(howMuch); // rise warning if have enough money to pay (with deposits) but didn't get enough from bank
                this.cash.subtract(howMuch);
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
        /// checks inside. Wouldn't pay if can't. Takes credits from bank
        /// Register moneyIncomethisTurn, pays tax. Returns true if was able pay
        /// </summary>    
        public bool pay(Agent incomeReceiver, Value howMuch, bool showMessageAboutNegativeValue = true)
        {
            if (howMuch.isNotZero())
                if (payWithoutRecord(incomeReceiver, howMuch, showMessageAboutNegativeValue))
                {
                    Value howMuchPayReally = howMuch.Copy();
                    incomeReceiver.moneyIncomethisTurn.Add(howMuchPayReally);
                    if (incomeReceiver is Market) // Market wouldn't pay taxes cause it's abstract entity
                        return true;
                    Agent payer = this;

                    if (payer is Market == false && incomeReceiver is Market == false
                        && payer.GetCountry() != incomeReceiver.GetCountry()
                        && payer is Factory) // pay taxes in enterprise jurisdiction
                    {   // and reduce taxable base
                        var payed = payer.GetCountry().TakeIncomeTaxFrom(incomeReceiver, howMuchPayReally, false);
                        howMuchPayReally.subtract(payed);
                    }

                    var popReceiver = incomeReceiver as PopUnit;
                    if (popReceiver != null)
                        incomeReceiver.GetCountry().TakeIncomeTaxFrom(popReceiver, howMuchPayReally, popReceiver.popType.isPoorStrata());
                    else
                    {
                        var countryPayer = incomeReceiver as Country;
                        if (countryPayer != null)
                            incomeReceiver.GetCountry().TakeIncomeTaxFrom(countryPayer, howMuchPayReally, false);
                    }
                    return true;
                }
                else
                    return false;
            return true;
        }
        internal void PayAllAvailableMoney(Agent whom)
        {
            if (bank != null)
                bank.returnAllMoney(this);
            pay(whom, this.cash.Copy());
            //whom.cash.Add(this.cash);
            //whom.moneyIncomethisTurn.Add(this.cash);
            //this.cash.set(0);
        }
        internal void PayAllAvailableMoneyWithoutRecord(Agent whom)
        {
            if (bank != null)
                bank.returnAllMoney(this);
            payWithoutRecord(whom, this.cash.Copy());
            //whom.cash.Add(this.cash);
            ////whom.moneyIncomethisTurn.add(this.cash);
            //this.cash.set(0);
        }
        public void ConvertFromGoldAndAdd(Value gold)
        {
            float coins = gold.get() * Options.goldToCoinsConvert;
            this.cash.add(coins);
            this.moneyIncomethisTurn.add(coins);
            gold.set(0);
        }

        override public string ToString()
        {
            return cash.get() + " coins";
        }
    }
}