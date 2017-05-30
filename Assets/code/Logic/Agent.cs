using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// represent ability to take loans/deposits
/// </summary>
public class Agent 
{
    /// <summary>
    /// Must be filled together with wallet
    /// </summary>
    public Value moneyIncomethisTurn = new Value(0);
    public Value cash = new Value(0);
    /// <summary>
    /// could be null
    /// </summary>
    public Bank bank;

    public Value loans = new Value(0);
    public Value deposits = new Value(0);

    //public Agent(Bank bank) : base(0f, bank)
    //{

    //}
    public Agent(float inAmount, Bank bank)
    {
        cash.set(inAmount);
        this.bank = bank;
    }
    public Value getMoneyTotal()
    {
        return cash.addOutside(deposits);
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
    internal bool CanAfford(Storage need)
    {
        if (need.get() == HowMuchCanAfford(need).get())
            return true;
        else
            return false;
    }

    internal bool CanAfford(PrimitiveStorageSet need)
    {
        foreach (Storage stor in need)
        {
            if (HowMuchCanAfford(stor).get() < stor.get())
                return false;
        }
        return true;
    }
    /// <summary>WARNING! Can overflow if money > cost of need. use CanAfford before </summary>

    internal Value HowMuchCanNotAfford(PrimitiveStorageSet need)
    {
        return new Value(Game.market.getCost(need).get() - this.cash.get());
    }
    internal Value HowMuchCanNotAfford(float need)
    {
        return new Value(need - this.cash.get());
    }
    internal Value HowMuchCanNotAfford(Storage need)
    {
        return new Value(Game.market.getCost(need) - this.cash.get());
    }
    internal Storage HowMuchCanAfford(Storage need)
    {
        float price = Game.market.findPrice(need.getProduct()).get();
        float cost = need.get() * price;
        if (cost <= cash.get())
            return new Storage(need.getProduct(), need.get());
        else
            return new Storage(need.getProduct(), cash.get() / price);
    }

    //private float get()
    //{
    //    throw new NotImplementedException();
    //}
    internal Value HowMuchCanNotPay(Value value)
    {
        return new Value(value.get() - this.cash.get());
    }
    internal bool canPay(Value howMuchPay)
    {
        if (this.cash.get() >= howMuchPay.get())
            return true;
        else return false;
    }
    internal bool canPay(float howMuchPay)
    {
        if (this.cash.get() >= howMuchPay)
            return true;
        else
            return false;
    }
    public float getCash()
    {
        return cash.get();
    }

    //internal void pay(Wallet whom, float howMuch)
    //{
    //    if (canPay(howMuch))
    //    {
    //        whom.haveMoney.add(howMuch);
    //        whom.moneyIncomethisTurn.add(howMuch);
    //        this.haveMoney.subtract(howMuch);

    //    }
    //    else
    //        Debug.Log("Failed payment in wallet");
    //}
    internal void payWithoutRecord(Agent whom, Value howMuch)
    {
        if (canPay(howMuch))
        {
            whom.cash.add(howMuch);
            //whom.moneyIncomethisTurn.add(howMuch);
            this.cash.subtract(howMuch);
        }
        else
            Debug.Log("Failed payment in wallet");
    }

    internal void pay(Agent whom, Value howMuch)
    {
        if (canPay(howMuch))
        {
            whom.cash.add(howMuch);
            whom.moneyIncomethisTurn.add(howMuch);
            this.cash.subtract(howMuch);
        }
        else
            Debug.Log("Failed payment in wallet");
    }
    internal void sendAll(Agent whom)
    {
        whom.cash.add(this.cash);
        whom.moneyIncomethisTurn.add(this.cash);
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