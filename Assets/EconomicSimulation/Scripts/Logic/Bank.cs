using UnityEngine;
using System.Collections;
using System;
using Nashet.ValueSpace;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    public class Bank : Agent, INameable
    {
        Value givenCredits = new Value(0);
        private readonly Country country;

        override public string ToString()
        {
            return FullName;
        }
        public string FullName
        {
            get { return "Bank of " + country.ShortName; }
        }

        public string ShortName
        {
            get { return "Bank of " + country.ShortName; }
        }

        public Bank(Country country) : base(0f, null, null)
        {
            this.country = country;
        }
        /// <summary>
        /// Returns money to bank. checks inside.
        /// Just wouldn't take money if giver hasn't enough money
        /// Don't provide variables like Cash as argument!! It would default to zero!
        /// </summary>    
        internal void takeMoney(Agent giver, Value howMuchTake)
        {
            if (giver.Pay(this, howMuchTake))
                if (giver.loans.isBiggerThan(Value.Zero))  //has debt (meaning has no deposits)
                    if (howMuchTake.isBiggerOrEqual(giver.loans)) // cover debt
                    {
                        Value extraMoney = howMuchTake.Copy().Subtract(giver.loans);
                        this.givenCredits.Subtract(giver.loans);
                        giver.loans.Set(0f);
                        giver.deposits.Set(extraMoney);
                    }
                    else// not cover debt
                    {
                        giver.loans.Subtract(howMuchTake);
                        this.givenCredits.Subtract(howMuchTake);
                    }
                else
                {
                    giver.deposits.Add(howMuchTake);
                }
        }

        /// <summary>
        /// Gives money in credit or returns deposit, if possible.
        /// Gives whole sum or gives nothing.
        /// Checks inside. Return false if didn't give credit.
        /// </summary>   
        internal bool GiveCredit(Agent taker, Value desiredCredit) // todo check
        {
            if (taker.deposits.isNotZero()) // has deposit (meaning, has no loans)
            {
                if (desiredCredit.isBiggerThan(taker.deposits))// loan is bigger than this deposit
                {
                    ReadOnlyValue returnedDeposit = ReturnDeposit(taker, taker.deposits);
                    if (returnedDeposit.isSmallerThan(taker.deposits))
                        return false;// if can't return deposit than can't give credit for sure
                                     //returnedMoney = new ReadOnlyValue(0f);

                    Value restOfTheSum = desiredCredit.Copy().Subtract(returnedDeposit);
                    if (CanGiveCredit(taker, restOfTheSum))
                    {
                        taker.loans.Set(restOfTheSum);//important
                        this.givenCredits.Add(restOfTheSum);
                        payWithoutRecord(taker, restOfTheSum);
                        return true;
                    }
                    else
                        return false;
                }
                else // no need for credit, just return deposit
                {
                    // if can't return deposit than can't give credit for sure                     
                    if (CanReturnDeposit(taker, desiredCredit))
                    {
                        ReturnDeposit(taker, desiredCredit);
                        return true;
                    }
                    else
                        return false;
                }
            }
            else
            {
                if (CanGiveCredit(taker, desiredCredit))
                {
                    taker.loans.Add(desiredCredit);
                    this.givenCredits.Add(desiredCredit);
                    payWithoutRecord(taker, desiredCredit);
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Gives credit. Checks inside. Just wouldn't give money if can't
        /// </summary>    
        internal bool giveLackingMoneyInCredit(Agent taker, ReadOnlyValue desirableSum)
        {
            if (taker.Country.Invented(Invention.Banking))// find money in bank?
            {
                Value lackOfSum = desirableSum.Copy().Subtract(taker.cash);
                return GiveCredit(taker, lackOfSum);
            }
            return false;
        }
        /// <summary>
        /// Result is how much deposit was really returned. Checks inside. Just wouldn't give money if can't
        /// Can return less than was prompted
        /// </summary>    
        internal ReadOnlyValue ReturnDeposit(Agent toWhom, ReadOnlyValue howMuchWants)
        {
            if (toWhom.Country.Invented(Invention.Banking))// find money in bank? //todo remove checks, make bank==null if uninvented
            {
                var maxReturnLimit = HowMuchDepositCanReturn(toWhom);
                if (maxReturnLimit.isBiggerOrEqual(howMuchWants))
                {
                    ReadOnlyValue returnMoney;
                    if (howMuchWants.isBiggerThan(maxReturnLimit))
                        returnMoney = maxReturnLimit;
                    else
                        returnMoney = howMuchWants;

                    if (returnMoney.isNotZero())// return deposit
                    {
                        //giveMoney(toWhom, moneyToReturn);
                        toWhom.deposits.Subtract(returnMoney);
                        payWithoutRecord(toWhom, returnMoney);

                    }
                    return returnMoney;
                }
            }
            return Value.Zero;
        }
        /// <summary>
        /// Returns deposits only. As much as possible. checks inside. Just wouldn't give money if can't
        /// </summary>        
        internal void ReturnAllDeposits(Agent toWhom)
        {
            ReturnDeposit(toWhom, HowMuchDepositCanReturn(toWhom));
        }
        /// <summary>
        /// includes checks for cash and deposit. Returns copy
        /// </summary>   
        internal ReadOnlyValue HowMuchDepositCanReturn(Agent agent)
        {
            var howMuchReturn = agent.deposits.Copy();

            if (howMuchReturn.isBiggerThan(cash))
                howMuchReturn.Set(cash);
            if (howMuchReturn.isBiggerThan(GetMinimalReservs()))
                howMuchReturn.Set(GetMinimalReservs());
            return howMuchReturn;
        }
        /// <summary>
        /// includes checks for cash and deposit.
        /// </summary>   
        internal bool CanReturnDeposit(Agent agent, ReadOnlyValue howMuch)
        {
            return HowMuchDepositCanReturn(agent).isBiggerOrEqual(howMuch);
        }

        internal Value getGivenCredits()
        {
            return givenCredits;
        }
        /// <summary>
        /// how much money have in cash. It's copy
        /// </summary>
        internal Value getReservs()
        {
            return cash.Copy();
        }


        private Value GetMinimalReservs()
        {
            //todo improve reserves
            return new Value(100f);
        }



        internal void defaultLoaner(Agent agent)
        {
            givenCredits.Subtract(agent.loans);
            agent.loans.Set(0);
        }
        /// <summary>
        /// Assuming all clients already defaulted theirs loans
        /// </summary>    
        internal void Annex(Bank annexingBank)
        {
            annexingBank.cash.SendAll(this.cash);
            annexingBank.givenCredits.SendAll(this.givenCredits);
        }
        bool isItEnoughReserves(Value sum)
        {
            return cash.Copy().Subtract(GetMinimalReservs()).isNotZero();
        }

        /// <summary>
        /// Checks reserve limits
        /// </summary>    
        internal bool CanGiveCredit(Agent whom, Value desirableSum)
        {
            return HowBigCreditCanGive(whom).isBiggerOrEqual(desirableSum);
        }
        /// <summary>
        /// How much can
        /// Checks reserve limits. Returns copy
        /// </summary>    
        internal Value HowBigCreditCanGive(Agent whom)
        {
            Value maxSum = cash.Copy().Subtract(GetMinimalReservs(), false);
            //if (whom.deposits.isBiggerThan(maxSum))
            //{
            //    maxSum = whom.deposits.Copy(); // sets maxSum to deposits size
            //    if (maxSum.isBiggerThan(cash)) //decrease maxSum to cash size
            //        maxSum.Set(cash);
            //}
            return maxSum;
        }

        internal void destroy(Country byWhom)
        {
            cash.SendAll(byWhom.cash);
            givenCredits.SetZero();
        }

        public override void simulate()
        {
            throw new NotImplementedException();
        }

    }
}