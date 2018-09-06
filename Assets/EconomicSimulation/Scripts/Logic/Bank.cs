using Nashet.Utils;
using Nashet.ValueSpace;
using System;

namespace Nashet.EconomicSimulation
{
    public class Bank : Agent, INameable
    {
        private readonly Money givenCredits = new Money(0);
        //private readonly Country country;

        public override string ToString()
        {
            return FullName;
        }

        public string FullName
        {
            get { return "Bank of " + Country.ShortName; }
        }

        public string ShortName
        {
            get { return "Bank of " + Country.ShortName; }
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
        public void ReceiveMoney(Agent giver, MoneyView sum)
        {
            if (giver.PayWithoutRecord(this, sum, Register.Account.BankOperation))
                if (giver.loans.isNotZero())  //has debt (meaning has no deposits)
                    if (sum.isBiggerOrEqual(giver.loans)) // cover debt
                    {
                        MoneyView extraMoney = sum.Copy().Subtract(giver.loans);
                        givenCredits.Subtract(giver.loans);

                        giver.loans.SetZero(); //put extra money on deposit
                        giver.deposits.Set(extraMoney);
                    }
                    else// not cover debt, just decrease loan
                    {
                        giver.loans.Subtract(sum);
                        givenCredits.Subtract(sum);
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
        public bool GiveCredit(Agent taker, MoneyView desiredCredit) // todo check
        {
            if (taker.deposits.isNotZero()) // has deposit (meaning, has no loans)
            {
                if (desiredCredit.isBiggerThan(taker.deposits))// loan is bigger than this deposit
                {
                    MoneyView returnedDeposit = ReturnDeposit(taker, taker.deposits);
                    if (returnedDeposit.isSmallerThan(taker.deposits))
                        return false;// if can't return deposit than can't give credit for sure
                                     //returnedMoney = new ReadOnlyValue(0f);

                    MoneyView restOfTheSum = desiredCredit.Copy().Subtract(returnedDeposit);
                    if (CanGiveCredit(taker, restOfTheSum))
                    {
                        taker.loans.Set(restOfTheSum);//important
                        givenCredits.Add(restOfTheSum);
                        PayWithoutRecord(taker, restOfTheSum, Register.Account.BankOperation);
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
                    givenCredits.Add(desiredCredit);
                    PayWithoutRecord(taker, desiredCredit, Register.Account.BankOperation);
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Gives credit. Checks inside. Just wouldn't give money if can't
        /// </summary>
        public bool GiveLackingMoneyInCredit(Agent taker, MoneyView desirableSum)
        {
            if (taker.Country.Science.IsInvented(Invention.Banking))// find money in bank?
            {
                MoneyView lackOfSum = desirableSum.Copy().Subtract(taker.getMoneyAvailable(), false);
                if (lackOfSum.isNotZero())
                    return GiveCredit(taker, lackOfSum);
                else
                    return false;
            }
            return false;
        }

        /// <summary>
        /// Result is how much deposit was really returned. Checks inside. Just wouldn't give money if can't
        /// Can return less than was prompted
        /// </summary>
        public MoneyView ReturnDeposit(Agent toWhom, MoneyView howMuchWants)
        {
            if (toWhom.Country.Science.IsInvented(Invention.Banking))// find money in bank? //todo remove checks, make bank==null if uninvented
            {
                var maxReturnLimit = HowMuchDepositCanReturn(toWhom);
                if (maxReturnLimit.isBiggerOrEqual(howMuchWants))
                {
                    MoneyView returnMoney;
                    if (howMuchWants.isBiggerThan(maxReturnLimit))
                        returnMoney = maxReturnLimit;
                    else
                        returnMoney = howMuchWants;

                    if (returnMoney.isNotZero())// return deposit
                    {
                        //giveMoney(toWhom, moneyToReturn);
                        toWhom.deposits.Subtract(returnMoney);
                        PayWithoutRecord(toWhom, returnMoney, Register.Account.BankOperation);
                    }
                    return returnMoney;
                }
            }
            return MoneyView.Zero;
        }

        /// <summary>
        /// Returns deposits only. As much as possible. checks inside. Just wouldn't give money if can't
        /// </summary>
        public void ReturnAllDeposits(Agent toWhom)
        {
            ReturnDeposit(toWhom, HowMuchDepositCanReturn(toWhom));
        }

        /// <summary>
        /// includes checks for Cash and deposit. Returns copy
        /// </summary>
        public MoneyView HowMuchDepositCanReturn(Agent agent)
        {
            var howMuchReturn = agent.deposits.Copy();//initialization

            var wantedResrve = Cash.Copy().Subtract(GetMinimalReservs(), false); //defaults to zero if there is no money to give
                                                                                 // doesn't account bank's deposits
            if (howMuchReturn.isBiggerThan(wantedResrve))
                howMuchReturn.Set(wantedResrve);

            return howMuchReturn;
        }

        /// <summary>
        /// includes checks for Cash and deposit.
        /// </summary>
        public bool CanReturnDeposit(Agent agent, MoneyView howMuch)
        {
            return HowMuchDepositCanReturn(agent).isBiggerOrEqual(howMuch);
        }

        public MoneyView GetGivenCredits()
        {
            return givenCredits;
        }

        /// <summary>
        /// how much money have in Cash.
        /// </summary>
        //public ReadOnlyValue getReservs()
        //{
        //    return Cash;
        //}

        private MoneyView GetMinimalReservs()
        {
            //todo improve reserves
            return new MoneyView(100m);
        }

        /// <summary>
        /// Agent refuses to pay debt
        /// </summary>
        public void OnLoanerRefusesToPay(Agent agent)
        {
            givenCredits.Subtract(agent.loans);
            agent.loans.SetZero();
        }

        /// <summary>
        /// Assuming all clients already defaulted theirs loans
        /// </summary>
        public void Annex(Bank annexingBank)
        {
            annexingBank.PayAllAvailableMoney(this, Register.Account.Rest);
            //annexingBank.givenCredits.SendAll(this.givenCredits);
            givenCredits.Add(annexingBank.givenCredits);
            annexingBank.givenCredits.SetZero();
        }

        /// <summary>
        /// Checks reserve limits
        /// </summary>
        public bool CanGiveCredit(Agent whom, MoneyView desirableSum)
        {
            return HowBigCreditCanGive(whom).isBiggerOrEqual(desirableSum);
        }

        /// <summary>
        /// How much can
        /// Checks reserve limits.
        /// new value
        /// </summary>
        public MoneyView HowBigCreditCanGive(Agent whom)
        {
            MoneyView maxSum = Cash.Copy().Subtract(GetMinimalReservs(), false); // don't take in account banks deposits
            //if (whom.deposits.isBiggerThan(maxSum))
            //{
            //    maxSum = whom.deposits.Copy(); // sets maxSum to deposits size
            //    if (maxSum.isBiggerThan(Cash)) //decrease maxSum to Cash size
            //        maxSum.Set(Cash);
            //}
            return maxSum;
        }

        public void destroy(Country byWhom)
        {
            PayAllAvailableMoney(byWhom, Register.Account.Rest);
            givenCredits.SetZero();
        }

        public override void simulate()
        {
            throw new NotImplementedException();
        }

        public void Nationalize()
        {
            Country.Bank.PayAllAvailableMoney(Country, Register.Account.Rest);
            Country.Bank.givenCredits.SetZero();
            Country.loans.SetZero();
            Country.deposits.SetZero();
        }
    }
}