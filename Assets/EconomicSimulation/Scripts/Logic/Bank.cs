using UnityEngine;
using System.Collections;
using System;
using Nashet.ValueSpace;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    public class Bank : Agent, INameable
    {
        private readonly Value givenCredits = new Value(0);
        //private readonly Country country;

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

        public Bank(Country country) : base(country)
        {
            //this.country = country;
        }
        /// <summary>
        /// Gives money to bank (as deposit or loan payment). Checks inside.
        /// Just wouldn't take money if giver hasn't enough money.
        /// Don't provide variables like Cash as argument!! It would default to zero!
        /// </summary>    
        internal void ReceiveMoney(Agent giver, ReadOnlyValue sum)
        {
            if (giver.PayWithoutRecord(this, sum))
                if (giver.loans.isNotZero())  //has debt (meaning has no deposits)
                    if (sum.isBiggerOrEqual(giver.loans)) // cover debt
                    {
                        Value extraMoney = sum.Copy().Subtract(giver.loans);
                        this.givenCredits.Subtract(giver.loans);

                        giver.loans.Set(0f); //put extra money on deposit
                        giver.deposits.Set(extraMoney);
                    }
                    else// not cover debt, just decrease loan
                    {
                        giver.loans.Subtract(sum);
                        this.givenCredits.Subtract(sum);
                    }
                else
                {
                    giver.deposits.Add(sum);
                }
        }

        /// <summary>
        /// Gives money in credit or returns deposit, if possible.
        /// Gives whole sum or gives nothing.
        /// Checks inside. Return false if didn't give credit.
        /// </summary>   
        internal bool GiveCredit(Agent taker, ReadOnlyValue desiredCredit) // todo check
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
                        PayWithoutRecord(taker, restOfTheSum);
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
                    PayWithoutRecord(taker, desiredCredit);
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Gives credit. Checks inside. Just wouldn't give money if can't
        /// </summary>    
        internal bool GiveLackingMoneyInCredit(Agent taker, ReadOnlyValue desirableSum)
        {
            if (taker.Country.Invented(Invention.Banking))// find money in bank?
            {
                Value lackOfSum = desirableSum.Copy().Subtract(taker.Cash);
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
                        PayWithoutRecord(toWhom, returnMoney);

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
        /// includes checks for Cash and deposit. Returns copy
        /// </summary>   
        internal ReadOnlyValue HowMuchDepositCanReturn(Agent agent)
        {
            var howMuchReturn = agent.deposits.Copy();//initialization

            var wantedResrve = Cash.Copy().Subtract(GetMinimalReservs(), false); //defaults to zero if there is no money to give

            if (howMuchReturn.isBiggerThan(wantedResrve))
                howMuchReturn.Set(wantedResrve);

            return howMuchReturn;
        }
        /// <summary>
        /// includes checks for Cash and deposit.
        /// </summary>   
        internal bool CanReturnDeposit(Agent agent, ReadOnlyValue howMuch)
        {
            return HowMuchDepositCanReturn(agent).isBiggerOrEqual(howMuch);
        }

        internal ReadOnlyValue GetGivenCredits()
        {
            return givenCredits;
        }
        /// <summary>
        /// how much money have in Cash. It's copy
        /// </summary>
        internal ReadOnlyValue getReservs()
        {
            return Cash.Copy();
        }


        private ReadOnlyValue GetMinimalReservs()
        {
            //todo improve reserves
            return new Value(100f);
        }


        /// <summary>
        /// Agent refuses to pay debt
        /// </summary>        
        internal void OnLoanerRefusesToPay(Agent agent)
        {
            givenCredits.Subtract(agent.loans);
            agent.loans.Set(0);
        }
        /// <summary>
        /// Assuming all clients already defaulted theirs loans
        /// </summary>    
        internal void Annex(Bank annexingBank)
        {
            annexingBank.PayAllAvailableMoney(this);
            annexingBank.givenCredits.SendAll(this.givenCredits);
        }


        /// <summary>
        /// Checks reserve limits
        /// </summary>    
        internal bool CanGiveCredit(Agent whom, ReadOnlyValue desirableSum)
        {
            return HowBigCreditCanGive(whom).isBiggerOrEqual(desirableSum);
        }
        /// <summary>
        /// How much can
        /// Checks reserve limits. Returns copy
        /// </summary>    
        internal ReadOnlyValue HowBigCreditCanGive(Agent whom)
        {
            Value maxSum = Cash.Copy().Subtract(GetMinimalReservs(), false);
            //if (whom.deposits.isBiggerThan(maxSum))
            //{
            //    maxSum = whom.deposits.Copy(); // sets maxSum to deposits size
            //    if (maxSum.isBiggerThan(Cash)) //decrease maxSum to Cash size
            //        maxSum.Set(Cash);
            //}
            return maxSum;
        }

        internal void destroy(Country byWhom)
        {
            PayAllAvailableMoney(byWhom);
            givenCredits.SetZero();
        }

        public override void simulate()
        {
            throw new NotImplementedException();
        }
        public void Nationalize()
        {
            country.Bank.PayAllAvailableMoney(country);
            country.Bank.givenCredits.SetZero();
            country.loans.SetZero();
            country.deposits.SetZero();
        }
    }
}