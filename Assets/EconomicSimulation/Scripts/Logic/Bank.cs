using UnityEngine;
using System.Collections;
using System;
using Nashet.ValueSpace;
namespace Nashet.EconomicSimulation
{
    public class Bank : Agent
    {
        Value givenLoans = new Value(0);

        public Bank() : base(0f, null, null)
        {
            //setBank(this);
        }
        /// <summary>
        /// Returns money to bank. checks inside.
        /// Just wouldn't take money if giver hasn't enough money
        /// Don't provide variables like Cash as argument!! It would default to zero!
        /// </summary>    
        internal void takeMoney(Agent giver, Value howMuchTake)
        {
            if (giver.pay(this, howMuchTake))
                if (giver.loans.isBiggerThan(Value.Zero))  //has debt (meaning has no deposits)
                    if (howMuchTake.isBiggerOrEqual(giver.loans)) // cover debt
                    {
                        Value extraMoney = howMuchTake.Copy().subtract(giver.loans);
                        this.givenLoans.subtract(giver.loans);
                        giver.loans.set(0f);
                        giver.deposits.set(extraMoney);
                    }
                    else// not cover debt
                    {
                        giver.loans.subtract(howMuchTake);
                        this.givenLoans.subtract(howMuchTake);
                    }
                else
                {
                    giver.deposits.Add(howMuchTake);
                }
        }

        /// <summary>
        ///checks outside 
        /// </summary>   
        internal void giveMoney(Agent taker, Value howMuch)
        {
            payWithoutRecord(taker, howMuch);
            if (taker.deposits.isBiggerThan(Value.Zero)) // has deposit (meaning, has no loans)
                if (howMuch.isBiggerOrEqual(taker.deposits))// loan is bigger than this deposit
                {
                    Value notEnoughMoney = howMuch.Copy().subtract(taker.deposits);
                    taker.deposits.set(0f);
                    taker.loans.set(notEnoughMoney);
                    this.givenLoans.Add(notEnoughMoney);
                }
                else // not cover
                {
                    taker.deposits.subtract(howMuch);
                }
            else
            {
                taker.loans.Add(howMuch);
                this.givenLoans.Add(howMuch);
            }
        }
        /// <summary>
        ///checks outside 
        /// </summary>   
        //internal bool giveMoneyIf(Consumer taker, Value howMuch)
        //{
        //    
        //        Value needLoan = howMuch.subtractOutside(taker.cash);
        //        if (this.canGiveMoney(taker, needLoan))
        //        {
        //            this.giveMoney(taker, needLoan);
        //            return true;
        //        }
        //   
        //    return false;
        //}
        /// <summary>
        /// checks inside. Just wouldn't give money if can't
        /// </summary>    
        internal bool giveLackingMoney(Agent taker, Value sum)
        {
            if (taker.GetCountry().isInvented(Invention.Banking))// find money in bank?
            {
                Value lackOfSum = sum.Copy().subtract(taker.cash);
                if (canGiveMoney(taker, lackOfSum))
                {
                    giveMoney(taker, lackOfSum);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Returns deposits only. As much as possible. checks inside. Just wouldn't give money if can't
        /// </summary>
        /// //todo - add some cross bank money transfer?
        internal void returnAllMoney(Agent agent)
        {
            //if (canGiveLoan(agent.deposits))
            //    giveMoney(agent, agent.deposits);

            giveMoney(agent, howMuchDepositCanReturn(agent));
        }

        internal Value getGivenLoans()
        {
            return givenLoans;
        }
        /// <summary>
        /// how much money have in cash. It's copy
        /// </summary>
        internal Value getReservs()
        {
            return cash.Copy();
        }
        /// <summary>
        /// Checks reserve limits and deposits
        /// </summary>    
        internal bool canGiveMoney(Agent agent, Value loan)
        {
            return howMuchCanGive(agent).isBiggerOrEqual(loan);
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

        internal void defaultLoaner(Agent agent)
        {
            givenLoans.subtract(agent.loans);
            agent.loans.set(0);
        }
        /// <summary>
        /// Assuming all clients already defaulted theirs loans
        /// </summary>    
        internal void add(Bank annexingBank)
        {
            annexingBank.cash.sendAll(this.cash);
            annexingBank.givenLoans.sendAll(this.givenLoans);
        }
        bool isItEnoughReserves(Value sum)
        {
            return cash.Copy().subtract(getMinimalReservs()).isNotZero();
        }

        /// <summary>
        /// Checks reserve limits and deposits. Returns copy
        /// </summary>    
        internal Value howMuchCanGive(Agent agent)
        {
            Value wouldGive = cash.Copy().subtract(getMinimalReservs(), false);
            if (agent.deposits.isBiggerThan(wouldGive))
            {
                wouldGive = agent.deposits.Copy(); // increase wouldGive to deposits size
                if (wouldGive.isBiggerThan(cash)) //decrease wouldGive to cash size
                    wouldGive.set(cash);
            }
            return wouldGive;
        }
        /// <summary>
        /// includes checks for cash and deposit size. Returns copy
        /// </summary>   
        internal Value howMuchDepositCanReturn(Agent agent)
        {
            if (cash.isBiggerOrEqual(agent.deposits))
                return agent.deposits.Copy();
            else
                return cash.Copy();
        }
        internal void destroy(Country byWhom)
        {
            cash.sendAll(byWhom.cash);
            givenLoans.setZero();
        }

        public override void simulate()
        {
            throw new NotImplementedException();
        }
    }
}