using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// represent ability to take loans/deposits
/// </summary>
abstract public class Agent : IHasCountry
{
    /// <summary>
    /// Must be filled together with wallet
    /// </summary>
    public Value moneyIncomethisTurn = new Value(0);
    public Value moneyIncomeLastTurn = new Value(0);
    public Value cash = new Value(0);
    /// <summary>
    /// could be null
    /// </summary>
    private Bank bank;

    public Value loans = new Value(0);
    public Value deposits = new Value(0);
    public readonly Province province;

    public Agent(float inAmount, Bank bank, Province province)
    {
        cash.set(inAmount);
        this.bank = bank;
        this.province = province;
    }
    virtual public Country getCountry()
    {
        return province.getCountry();
    }
    public Bank getBank()
    {
        return bank;
    }
    public void setBank(Bank bank)
    {
        this.bank = bank;
    }
    /// <summary> Includes deposits </summary>    
    public Value getMoneyAvailable()
    {
        if (bank == null)
            return new Value(cash);
        else
            return cash.addOutside(bank.howMuchDepositCanReturn(this));
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
        if (need.isAbstractProduct())
            need = Game.market.getCheapestSubstitute(need);
        if (need.get() == howMuchCanAfford(need).get())
            return true;
        else
            return false;
    }

    internal bool canAfford(PrimitiveStorageSet need)
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
    /// <summary>WARNING! Can overflow if money > cost of need. use CanAfford before </summary>
    internal Value howMuchMoneyCanNotPay(Value need)
    {
        //return new Value(need - this.cash.get());
        //return need.subtractOutside(cash);
        return need.subtractOutside(getMoneyAvailable());
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
            return new Storage(need.getProduct(), getMoneyAvailable().divideOutside(
                Game.market.findPrice(need.getProduct())
                ));
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
    internal bool canPay(Value howMuchPay)
    {
        return getMoneyAvailable().isBiggerOrEqual(howMuchPay);
    }
    internal bool canPayCashOnly(Value howMuchPay)
    {
        return cash.isBiggerOrEqual(howMuchPay);
    }

    public float getCash()
    {
        return cash.get();
    }

    /// <summary>
    /// checks inside. Wouldn't pay if can't
    /// </summary>    
    public bool payWithoutRecord(Agent whom, Value howMuch, bool showMessageAboutNegativeValue = true)
    {
        if (canPay(howMuch))
        {
            if (!canPayCashOnly(howMuch) && bank != null)// checked for bank inv
            {
                bank.giveLackingMoney(this, howMuch);
                bank.giveLackingMoney(this, howMuch.multiplyOutside(5));
            }
            whom.cash.add(howMuch); // rise warning if have enough money to pay (with deposits) but did't get enough from bank
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
    /// checks inside. Wouldn't pay if can't
    /// </summary>    
    public bool pay(Agent whom, Value howMuch)
    {
        if (payWithoutRecord(whom, howMuch))
        {
            whom.moneyIncomethisTurn.add(howMuch);
            return true;
        }
        else
            return false;
    }
    internal void sendAllAvailableMoney(Agent whom)
    {
        if (bank != null)
            bank.returnAllMoney(this);
        whom.cash.add(this.cash);
        whom.moneyIncomethisTurn.add(this.cash);
        this.cash.set(0);
    }
    internal void sendAllAvailableMoneyWithoutRecord(Agent whom)
    {
        if (bank != null)
            bank.returnAllMoney(this);
        whom.cash.add(this.cash);
        //whom.moneyIncomethisTurn.add(this.cash);
        this.cash.set(0);
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