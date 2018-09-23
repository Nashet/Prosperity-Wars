using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections.Generic;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// represent ability to take loans/deposits
    /// </summary>
    public abstract class Agent : IHasCountry, IStatisticable
    {       
        protected readonly Money cash = new Money(0);
        public MoneyView Cash { get { return cash; } }

        public readonly Register Register = new Register();
        public readonly Register FailedPayments = new Register(false); 

        /// <summary> could be null</summary>
        //private Bank bank;

        public Money loans = new Money(0);

        public Money deposits = new Money(0);
       

        public abstract void simulate();

        //private Country country;

        public Country Country
        {
            //get { return province.Country; }
            get;//{ return country; }
            protected set; //{ country = value; 
                
        }

        protected Agent(Country country)
        {
            Country = country;           
        }

        public void OnProvinceOwnerChanged(Country newOner)
        {
            Country = newOner;
        }

        public void GiveMoneyFromNoWhere(decimal money)
        {
            cash.Add(money);
        }

        public virtual void SetStatisticToZero()
        {
            Register.SetStatisticToZero();
            FailedPayments.SetStatisticToZero();
        }

        /// <summary> Returns difference between moneyIncomeLastTurn and value</summary>
        //protected Value getSpendingLimit(ReadOnlyValue value)
        //{
        //    return moneyIncomeLastTurn.Copy().Subtract(value, false);
        //}

        public Bank Bank
        {
            get
            {
                if (Country == null)
                    return null; // to deal with no-country Agents like Market
                else
                    return Country.Bank;
            }
        }

        /// <summary> Includes deposits. New value </summary>
        public MoneyView getMoneyAvailable()
        {
            if (Bank == null)
                return cash;
            else
                return cash.Copy().Add(Bank.HowMuchDepositCanReturn(this));//that's new Value
        }


        /// <summary>
        /// Ignores if need is available on market or not
        /// </summary>
        public bool CanAfford(Storage need)
        {
            Storage realNeed;
            if (need.isAbstractProduct())
                //realNeed = new Storage(Country.market.getCheapestSubstitute(need).Product, need);
                realNeed = Country.market.GetRandomCheapestSubstitute(need);
            else
                realNeed = need;

            return CanPay(Country.market.getCost(realNeed));
            //return realNeed.IsEqual(HowMuchCanAfford(realNeed));
        }

        public bool CanAfford(StorageSet need)
        {
            foreach (Storage stor in need)
            {
                if (!CanAfford(stor))
                    //if (HowMuchCanAfford(stor).get() < stor.get())
                    return false;
            }
            return true;
        }

        public bool CanAfford(IEnumerable<Storage> need)
        {
            foreach (Storage stor in need)
                //if (HowMuchCanAfford(stor).isSmallerThan(stor))
                if (!CanAfford(stor))
                    return false;
            return true;
        }

        /// <summary> Including deposits </summary>
        public Storage HowMuchCanAfford(Storage need)
        {
            MoneyView cost = Country.market.getCost(need);
            if (CanPay(cost))
                return new Storage(need);
            else
                return new Storage(need.Product, (float)getMoneyAvailable().Copy().Divide(Country.market.getCost(need.Product).Get()).Get());
        }

        /// <summary>WARNING! Can overflow if money > cost of need. use CanAfford before </summary>
        //public Value HowMuchCanNotAfford(PrimitiveStorageSet need)
        //{
        //    return new Value(Country.market.getCost(need).get() - this.cash.get());
        //}
        /// <summary>WARNING! Can overflow if money > cost of need. use CanAfford before </summary>
        //public Value HowMuchCanNotAfford(float need)
        //{
        //    return new Value(need - this.cash.get());
        //}

        /// <summary> Says how much money lack of desiredSum (counting cash and deposits in bank). New value.
        /// Goes nut if cash + deposits >= desiredSum, no need for extra money, check that outside
        /// </summary>
        public MoneyView HowMuchLacksMoneyIncludingDeposits(MoneyView desiredSum)
        {
            return desiredSum.Copy().Subtract(getMoneyAvailable(), false);
        }

        /// <summary>
        /// Says how much money lack of desiredSum (counting cash only). New value
        /// Says zero if cash >= desiredSum, no need for extra money
        /// </summary>
        public MoneyView HowMuchLacksMoneyCashOnly(MoneyView desiredSum)
        {
            return desiredSum.Copy().Subtract(cash, false);
        }

        //public Value HowMuchMoneyCanNotPay(Value value)
        //{
        //    return new Value(value.get() - this.cash.get());
        //}
        /// <summary>WARNING! Can overflow if money > cost of need. use CanAfford before </summary>
        //public Value HowMuchCanNotAfford(Storage need)
        //{
        //    return new Value(Country.market.getCost(need) - this.cash.get());
        //}

        //private float get()
        //{
        //    throw new NotImplementedException();
        //}

        //public bool canPay(Value howMuchPay)
        //{
        //    if (this.cash.get() >= howMuchPay.get())
        //        return true;
        //    else return false;
        //}
        //public bool canPay(float howMuchPay)
        //{
        //    if (this.cash.get() >= howMuchPay)
        //        return true;
        //    else
        //        return false;
        //}
        /// <summary> Counting deposit and cash </summary>
        public bool CanPay(MoneyView howMuchPay)
        {
            return getMoneyAvailable().isBiggerOrEqual(howMuchPay);
        }

        public bool CanPayCashOnly(MoneyView howMuchPay)
        {
            return cash.isBiggerOrEqual(howMuchPay);
        }

        //public float getCash()
        //{
        //    return cash.get();
        //}

        /// <summary>
        /// Checks inside. Wouldn't pay if can't. Takes back deposits from bank, if needed
        /// Doesn't pay tax
        /// </summary>
        public bool PayWithoutRecord(Agent whom, MoneyView howMuch, Register.Account account, bool showMessageAboutNegativeValue = true)
        {
            if (CanPay(howMuch))// It does has enough cash or deposit
            {
                if (!CanPayCashOnly(howMuch))
                    Bank.ReturnDeposit(this, HowMuchLacksMoneyCashOnly(howMuch));
                Register.RecordPayment(whom, account, howMuch);
                whom.cash.Add(howMuch);
                cash.Subtract(howMuch);
                return true;
            }
            else
            {
                FailedPayments.RecordIncomeFromNowhere(account, howMuch);
                if (showMessageAboutNegativeValue)
                    Debug.Log(this + " doesn't have " + howMuch + " to pay in Agent.payWithoutRecord2 " + whom
                        + " has " + getMoneyAvailable());
                //PayAllAvailableMoneyWithoutRecord(whom);
                return false;
            }
        }

        /// <summary>
        /// used only to pay to stash, should be removed 
        /// Checks inside. Wouldn't pay if can't. Takes back deposits from bank, if needed
        /// Doesn't pay tax
        /// </summary>
        // todo remove it #510
        public bool PayWithoutRecord(Money whom, MoneyView howMuch, bool showMessageAboutNegativeValue = true)
        {
            if (CanPay(howMuch))// It does has enough cash or deposit
            {
                if (!CanPayCashOnly(howMuch))
                    Bank.ReturnDeposit(this, HowMuchLacksMoneyCashOnly(howMuch));
                whom.Add(howMuch);
                cash.Subtract(howMuch);
                return true;
            }
            else
            {
                if (showMessageAboutNegativeValue)
                    Debug.Log(this + " doesn't have " + howMuch + " to pay in Agent.payWithoutRecord " + whom
                        + " has " + getMoneyAvailable());
                //PayWithoutRecord(whom, getMoneyAvailable());
                return false;
            }
        }

        /// <summary>
        /// Checks inside. Wouldn't pay if can't. Takes back deposit from bank if needed
        /// Registers moneyIncomeThisTurn, pays tax. Returns true if was able to pay
        /// </summary>
        public bool Pay(Agent incomeReceiver, MoneyView howMuch, Register.Account account, bool showMessageAboutNegativeValue = true)
        {
            if (howMuch.isNotZero())
                if (PayWithoutRecord(incomeReceiver, howMuch, account, showMessageAboutNegativeValue)) // pays here
                {
                    Money howMuchPayReally = howMuch.Copy();

                    if (incomeReceiver is Market) // Market wouldn't pay taxes cause it's abstract entity
                        return true;

                    Agent payer = this;

                    // foreigners income tax calculation
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

        public void PayAllAvailableMoney(Agent whom, Register.Account account)
        {
            if (Bank != null)
                Bank.ReturnAllDeposits(this);
            Pay(whom, cash, account);
        }

        public void PayAllAvailableMoneyWithoutRecord(Agent whom, Register.Account account)
        {
            if (Bank != null)
                Bank.ReturnAllDeposits(this);
            PayWithoutRecord(whom, cash, account);
        }

        public override string ToString()
        {
            return "Agent " + cash.Get();
        }
    }
}