using UnityEngine;
using System.Collections;
using System;

public class Bank : Agent
{
    /// <summary>
    /// how much money have in cash
    /// </summary>
    /// That's in base(Wallet) now
    //Wallet reserves = new Wallet(0);
    Value givenLoans = new Value(0);

    public Bank() : base(0f, null)
    {
        //setBank(this);
    }
    internal void takeMoney(Agent giver, Value howMuch)
    {
        giver.pay(this, howMuch);
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
    internal void giveMoney(Agent taker, Value howMuch)
    {
        this.payWithoutRecord(taker, howMuch);
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
    /// Assuming Bank have enough money 
    /// </summary>    
    internal void giveLackingMoney(Agent agent, Value howMuch)
    {
        giveMoney(agent, howMuch.subtractOutside(agent.cash));
    }
    internal void returnAllMoney(Agent agent)
    {
        giveMoney(agent, agent.deposits);
    }
    /// <summary>
    /// checks are outside
    /// </summary>
    internal void returnLoan(Producer returner, Value howMuch)
    {
        //reservs.pay(taker.wallet, howMuch);
        returner.pay(this, howMuch);
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
        return cash.get();
    }

    internal bool CanITakeThisLoan(Value loan)
    {
        //if there is enough money and enough reserves
        if (cash.get() - loan.get() >= getMinimalReservs().get())
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
        return cash.ToString();
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
        annexingBank.cash.sendAll(this.cash);
        annexingBank.givenLoans.sendAll(this.givenLoans);
    }

    
}
