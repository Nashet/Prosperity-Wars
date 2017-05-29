using UnityEngine;
using System.Collections;
using System;

public class Bank
{
    /// <summary>
    /// how much money have in cash
    /// </summary>
    Wallet reservs = new Wallet(0);
    Value givenLoans = new Value(0);

    // 
    internal void takeMoney(Owner giver, Value howMuch)
    {
        giver.wallet.pay(reservs, howMuch);
        if (giver.loans.get() > 0f)  //has debt (meaning has no deposits)
            if (howMuch.get() >= giver.loans.get()) // cover debt
            {
                float extraMoney = howMuch.get() - giver.loans.get();
                this.givenLoans.subtract(giver.loans);
                giver.loans.set(0f);
                giver.deposits.set(extraMoney);
            }
            else// not cover debt
            {
                giver.loans.subtract(howMuch);
                this.givenLoans.subtract(howMuch);
            }
        else
            giver.deposits.add(howMuch);
    }

    /// <summary>
    /// checks are outside
    /// </summary>   
    internal void giveMoney(Owner taker, Value howMuch)
    {
        reservs.pay(taker.wallet, howMuch);
        if (taker.deposits.get() > 0f) // has deposit (meaning, has no loans)
            if (howMuch.get() >= taker.deposits.get())// loan is bigger than this deposit
            {
                float notEnoughMoney = howMuch.get() - taker.deposits.get();
                taker.deposits.set(0f);
                taker.loans.set(notEnoughMoney);
                this.givenLoans.add(notEnoughMoney);
            }
            else // not cover
            {
                taker.deposits.subtract(howMuch);
            }
        else
        {
            taker.loans.add(howMuch);
            this.givenLoans.add(howMuch);
        }
    }
    /// <summary>
    /// checks are outside
    /// </summary>
    internal void returnLoan(Producer returner, Value howMuch)
    {
        //reservs.pay(taker.wallet, howMuch);
        returner.wallet.pay(reservs, howMuch);
        returner.loans.subtract(howMuch);
        this.givenLoans.subtract(howMuch);
    }
    internal Value getGivenLoans()
    {
        return new Value(givenLoans.get());
    }
    /// <summary>
    /// how much money have in cash
    /// </summary>
    internal float getReservs()
    {
        return reservs.haveMoney.get();
    }

    internal bool CanITakeThisLoan(Value loan)
    {
        //if there is enough money and enough reserves
        if (reservs.haveMoney.get() - loan.get() >= getMinimalReservs().get())
            return true;
        return false;
    }

    private Value getMinimalReservs()
    {
        //todo improve reserves
        return new Value(100f);
    }

    override public string ToString()
    {
        return reservs.ToString();
    }

    internal void defaultLoaner(Producer producer)
    {
        givenLoans.subtract(producer.loans);
        producer.loans.set(0);
    }
    /// <summary>
    /// Assuming all clients already defaulted theirs loans
    /// </summary>    
    internal void add(Bank annexingBank)
    {
        annexingBank.reservs.sendAll(this.reservs);
        annexingBank.givenLoans.sendAll(this.givenLoans);
    }
}
