using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represent anyone who can have money (currently - gold only)
/// </summary>
public class Wallet// : Value // : Storage
{
    /// <summary>
    /// Must be filled together with wallet
    /// </summary>
    public Value moneyIncomethisTurn = new Value(0);
    public  Value haveMoney = new Value(0);
    /// <summary>
    /// could be null
    /// </summary>
    public Bank bank;

    public Wallet(float inAmount, Bank bank) //: base (inAmount)//: base(Product.findByName("Gold"), inAmount)
    {
        haveMoney.set(inAmount);
        this.bank = bank;
    }

    ///public Wallet() : base(Product.findByName("Gold"), 0f)
    //public Wallet() : base(Product.findByName("Gold"), 20f) 

    //}

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
        return new Value(Game.market.getCost(need).get() - this.haveMoney.get());
    }
    internal Value HowMuchCanNotAfford(float need)
    {
        return new Value(need - this.haveMoney.get());
    }
    internal Value HowMuchCanNotAfford(Storage need)
    {
        return new Value(Game.market.getCost(need) - this.haveMoney.get());
    }
    internal Storage HowMuchCanAfford(Storage need)
    {
        float price = Game.market.findPrice(need.getProduct()).get();
        float cost = need.get() * price;
        if (cost <= haveMoney.get())
            return new Storage(need.getProduct(), need.get());
        else
            return new Storage(need.getProduct(), haveMoney.get() / price);
    }

    //private float get()
    //{
    //    throw new NotImplementedException();
    //}
    internal Value HowMuchCanNotPay(Value value)
    {
        return new Value(value.get() - this.haveMoney.get());
    }
    internal bool canPay(Value howMuchPay)
    {
        if (this.haveMoney.get() >= howMuchPay.get())
            return true;
        else return false;
    }
    internal bool canPay(float howMuchPay)
    {
        if (this.haveMoney.get() >= howMuchPay)
            return true;
        else
            return false;
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
    internal void payWithoutRecord(Wallet whom, Value howMuch)
    {
        if (canPay(howMuch))
        {
            whom.haveMoney.add(howMuch);
            //whom.moneyIncomethisTurn.add(howMuch);
            this.haveMoney.subtract(howMuch);
        }
        else
            Debug.Log("Failed payment in wallet");
    }  

    internal void pay(Wallet whom, Value howMuch)
    {
        if (canPay(howMuch))
        {
            whom.haveMoney.add(howMuch);
            whom.moneyIncomethisTurn.add(howMuch);
            this.haveMoney.subtract(howMuch);
        }
        else
            Debug.Log("Failed payment in wallet");
    }
    internal void sendAll(Wallet whom)
    {
        whom.haveMoney.add(this.haveMoney);
        whom.moneyIncomethisTurn.add(this.haveMoney);
        this.haveMoney.set(0);
    }
    public void ConvertFromGoldAndAdd(Value gold)
    {
        float coins = gold.get() * Options.goldToCoinsConvert;
        this.haveMoney.add(coins);
        this.moneyIncomethisTurn.add(coins);
        gold.set(0);

    }

    override public string ToString()
    {
        return haveMoney.get() + " coins";
    }
}
