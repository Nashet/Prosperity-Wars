using UnityEngine;
using System.Collections;
using System;

public class Bank : Agent
{
    Value givenLoans = new Value(0);

    public Bank() : base(0f, null)
    {
        //setBank(this);
    }
    /// <summary>
    /// checks inside. Just wouldn't take money if giver hasn't enough money
    /// </summary>    
    internal void takeMoney(Agent giver, Value howMuch)
    {
        if (giver.pay(this, howMuch))
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
    ///checks outside 
    /// </summary>   
    internal void giveMoney(Agent taker, Value howMuch)
    {
        payWithoutRecord(taker, howMuch);
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
    /// checks inside. Just wouldn't give money if can't
    /// </summary>    
    internal void giveLackingMoney(Agent agent, Value howMuch)
    {
        Value loan = howMuch.subtractOutside(agent.cash);
        if (canGiveLoan(loan))
            giveMoney(agent, loan);
    }
    /// <summary>
    /// checks inside. Just wouldn't give money if can't
    /// </summary>
    /// //todo - add some cross bank money transfer?
    internal void returnAllMoney(Agent agent)
    {
        if (canGiveLoan(agent.deposits))
            giveMoney(agent, agent.deposits);
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

    internal bool canGiveLoan(Value loan)
    {
        //if there is enough money and enough reserves
        if (cash.get() - loan.get() >= getMinimalReservs().get())
            return true;
        return false;
    }

    private Value getMinimalReservs()
    {
        //todo improve reserves
        return new Value(1000f);
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
    internal Value howMuchCanGive()
    {
        return cash.subtractOutside(getMinimalReservs());
    }
    internal void destroy(Country byWhom)
    {
        cash.sendAll(byWhom.cash);
        givenLoans.setZero();
    }
}
