using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// represents ability to register money flow. Could be optimized
    /// </summary>
    public class Register : IStatisticable
    {
        //protected Dictionary<Account, Money> incomeList = new Dictionary<Account, Money>();
        //protected Dictionary<Account, Money> expensesList = new Dictionary<Account, Money>();
        protected bool enabled = true;

        protected Money income = new Money(0m);
        public MoneyView Income { get { return income; } }

        protected Money incomeLastTurn = new Money(0m);
        public MoneyView IncomeLastTurn { get { return incomeLastTurn; } }

        protected Money expenses = new Money(0m);


        public MoneyView Expenses { get { return expenses; } }

        /// <summary> Can be negative</summary>
        public decimal Balance { get { return income.Get() - expenses.Get(); } }



        public void SetStatisticToZero()
        {
            if (enabled)
            {
                incomeLastTurn.Set(income);
                income.SetZero();
                expenses.SetZero();
                foreach (var account in Account.AllAccounts)
                {
                    account.GetIncomeAccount(this).SetZero();
                    account.GetExpenseAccount(this).SetZero();
                }
            }
            //incomeList.Clear();
            //expensesList.Clear();
            //foreach (var item in incomeList)
            //    item.Value.SetZero();
            //foreach (var item in expensesList)
            //    item.Value.SetZero();

        }

        public void Disable()
        {
            enabled = false;
        }

        internal void Enable()
        {
            enabled = true;
        }

        public void RecordPayment(Agent receiver, Account account, MoneyView sum)
        {
            if (enabled)
            {
                account.GetIncomeAccount(receiver.Register).Add(sum);
                //receiver.Register.incomeList.AddAndSum(account, sum);
                receiver.Register.income.Add(sum);

                account.GetExpenseAccount(this).Add(sum);
                //this.expensesList.AddAndSum(account, sum);//giver is this
                if (account != Account.Dividends)
                    this.expenses.Add(sum);
            }
        }
        public void RecordIncomeFromNowhere(Account account, MoneyView sum)
        {
            if (enabled)
            {
                account.GetIncomeAccount(this).Add(sum);
                //this.incomeList.AddAndSum(account, sum);
                this.income.Add(sum);
            }
        }

        private readonly StringBuilder incomeText = new StringBuilder();
        private readonly StringBuilder expensesText = new StringBuilder();

        private readonly Money foreignTaxIncome = new Money(0);
        private readonly Money foreignTaxExpense = new Money(0);
        private readonly Money poorTaxIncome = new Money(0);
        private readonly Money poorTaxExpense = new Money(0);
        private readonly Money unemploymentIncome = new Money(0);
        private readonly Money unemploymentExpense = new Money(0);
        private readonly Money enterpriseSubsidiesIncome = new Money(0);
        private readonly Money enterpriseSubsidiesExpense = new Money(0);
        private readonly Money povertyAidIncome = new Money(0);
        private readonly Money povertyAidExpense = new Money(0);
        private readonly Money marketOperationsIncome = new Money(0);
        private readonly Money marketOperationsExpense = new Money(0);
        private readonly Money wageIncome = new Money(0);
        private readonly Money wageExpense = new Money(0);
        private readonly Money dividendsIncome = new Money(0);
        private readonly Money dividendsExpense = new Money(0);
        private readonly Money bankIncome = new Money(0);
        private readonly Money bankExpense = new Money(0);
        private readonly Money sellingProperty = new Money(0);
        private readonly Money buyingProperty = new Money(0);
        private readonly Money constructionIncome = new Money(0);
        private readonly Money constructionExpense = new Money(0);
        private readonly Money minedGoldIncome = new Money(0);
        private readonly Money minedGoldExpense = new Money(0);
        private readonly Money restIncome = new Money(0);
        private readonly Money restExpense = new Money(0);

        private readonly Money UBIIncome = new Money(0);
        private readonly Money UBIExpense = new Money(0);

        private readonly Money richTaxIncome = new Money(0);
        private readonly Money richTaxExpense = new Money(0);

        public Register(bool enabled = true)
        {
            this.enabled = enabled;
        }

        public override string ToString()
        {
            return "Income: " + income + GetIncomeText() + "\n" + GetExpensesText();
        }

        public string GetIncomeText()
        {
            incomeText.Clear();//.Append("Income: " + income);

            foreach (var account in Account.AllAccounts)
            {
                var money = account.GetIncomeAccount(this);
                if (money.isNotZero())
                    incomeText.Append("\n " + account.IncomeText + ": " + money);
            }
            return incomeText.ToString();
        }

        public string GetExpensesText()
        {
            expensesText.Clear().Append("Expenses: " + expenses);

            foreach (var account in Account.AllAccounts)
            {
                var money = account.GetExpenseAccount(this);
                if (money.isNotZero())
                    expensesText.Append("\n " + account.ExpenseText + ": " + money);
            }
            return expensesText.ToString();
        }

        public class Account
        {
            protected static List<Account> allAccounts = new List<Account>();
            public static IEnumerable<Account> AllAccounts
            {
                get
                {
                    foreach (var item in allAccounts)
                        yield return item;
                }
            }

            public static readonly Account RichIncomeTax = new Account("Income tax for rich", "Income tax for rich", x => x.richTaxIncome, x => x.richTaxExpense);
            public static readonly Account ForeignIncomeTax = new Account("Income tax for foreigners", "Income tax for foreigners", x => x.foreignTaxIncome, x => x.foreignTaxExpense);
            public static readonly Account PoorIncomeTax = new Account("Income tax for poor", "Income tax for poor", x => x.poorTaxIncome, x => x.poorTaxExpense);

            public static readonly Account UnemploymentSubsidies = new Account("Unemployment Subsidies", "Unemployment Subsidies", x => x.unemploymentIncome, x => x.unemploymentExpense);
            public static readonly Account EnterpriseSubsidies = new Account("Enterprise Subsidies", "Enterprise Subsidies", x => x.enterpriseSubsidiesIncome, x => x.enterpriseSubsidiesExpense);
            public static readonly Account UBISubsidies = new Account("UBI Subsidies", "UBI Subsidies", x => x.UBIIncome, x => x.UBIExpense);
            public static readonly Account PovertyAid = new Account("Poverty Aid", "Poverty Aid", x => x.povertyAidIncome, x => x.povertyAidExpense);

            public static readonly Account MarketOperations = new Account("Selling goods", "Buying goods", x => x.marketOperationsIncome, x => x.marketOperationsExpense);
            public static readonly Account Wage = new Account("Wage", "Wages", x => x.wageIncome, x => x.wageExpense);
            public static readonly Account Dividends = new Account("Dividends", "Dividends", x => x.dividendsIncome, x => x.dividendsExpense);
            public static readonly Account BankOperation = new Account("Took from bank", "Sent to bank", x => x.bankIncome, x => x.bankExpense);

            public static readonly Account BuyingProperty = new Account("Selling property", "Buying property", x => x.sellingProperty, x => x.buyingProperty);
            public static readonly Account Construction = new Account("Construction", "Construction", x => x.constructionIncome, x => x.constructionExpense);

            //public static readonly Account MinedGoldTax = new Account("MinedGoldTax");
            public static readonly Account MinedGold = new Account("Mined Gold", "Mined Gold", x => x.minedGoldIncome, x => x.minedGoldExpense);

            /// <summary>nationalization / closing business / pop's merge in / pop's separation</summary>
            public static readonly Account Rest = new Account("Rest", "Rest", x => x.restIncome, x => x.restExpense);
            //public static readonly Account DontRecord = new Account("DontRecord");

            public string IncomeText { get; protected set; }
            public string ExpenseText { get; protected set; }

            internal readonly Func<Register, Money> GetIncomeAccount;
            internal readonly Func<Register, Money> GetExpenseAccount;

            protected Account(string positiveTransaction, string negativeTransaction, Func<Register, Money> incomeAccount, Func<Register, Money> expenseAccount)
            {
                GetIncomeAccount = incomeAccount;
                GetExpenseAccount = expenseAccount;
                IncomeText = positiveTransaction;
                ExpenseText = negativeTransaction;
                allAccounts.Add(this);
            }

            public override string ToString()
            {
                if (IncomeText == ExpenseText)
                    return IncomeText;
                else
                    return IncomeText + "//" + ExpenseText;
            }
        }
    }
    //protected readonly Money incomeTaxStaticticPoor = new Money(0m);
    //public MoneyView IncomeTaxStaticticPoor { get { return incomeTaxStaticticPoor; } }

    //protected readonly Money incomeTaxStatisticRich = new Money(0m);
    //public MoneyView IncomeTaxStatisticRich { get { return incomeTaxStatisticRich; } }

    //protected readonly Money incomeTaxForeigner = new Money(0m);
    //public MoneyView IncomeTaxForeigner { get { return incomeTaxForeigner; } }

    //protected readonly Money goldMinesIncome = new Money(0m);/// <summary> Assignment mean that value ADDED to this property </summary>
    //public MoneyView GoldMinesIncome
    //{
    //    get { return goldMinesIncome; }
    //    set { goldMinesIncome.Add(value); }
    //}

    //protected readonly Money ownedFactoriesIncome = new Money(0m);/// <summary> Assignment mean that value ADDED to this property </summary>
    //public MoneyView OwnedFactoriesIncome
    //{
    //    get { return ownedFactoriesIncome; }
    //    set { ownedFactoriesIncome.Add(value); }
    //}

    //protected readonly Money unemploymentSubsidiesExpense = new Money(0m);/// <summary> Assignment mean that value ADDED to this property </summary>
    //public MoneyView UnemploymentSubsidiesExpense
    //{
    //    get { return unemploymentSubsidiesExpense; }
    //    set { unemploymentSubsidiesExpense.Add(value); }
    //}

    //protected readonly Money ubiSubsidiesExpense = new Money(0m);/// <summary> Assignment mean that value ADDED to this property </summary>
    //public MoneyView UBISubsidiesExpense
    //{
    //    get { return ubiSubsidiesExpense; }
    //    set { ubiSubsidiesExpense.Add(value); }
    //}

    //protected readonly Money povertyAidExpense = new Money(0m);/// <summary> Assignment mean that value ADDED to this property </summary>
    //public MoneyView PovertyAidExpense
    //{
    //    get { return povertyAidExpense; }
    //    set { povertyAidExpense.Add(value); }
    //}

    //protected readonly Money soldiersWageExpense = new Money(0m);/// <summary> Assignment mean that value ADDED to this property </summary>
    //public MoneyView SoldiersWageExpense
    //{
    //    get { return soldiersWageExpense; }
    //    set { soldiersWageExpense.Add(value); }
    //}

    //protected readonly Money factorySubsidiesExpense = new Money(0m);
    //public MoneyView FactorySubsidiesExpense { get { return factorySubsidiesExpense; } }

    //protected readonly Money storageBuyingExpense = new Money(0m);/// <summary> Assignment mean that value ADDED to this property </summary>
    //public MoneyView StorageBuyingExpense
    //{
    //    get { return storageBuyingExpense; }
    //    set { storageBuyingExpense.Add(value); }
    //}
    //public MoneyView GetRegisteredExpenses()
    //{
    //    Money result = MoneyView.Zero.Copy();
    //    result.Add(unemploymentSubsidiesExpense);
    //    result.Add(factorySubsidiesExpense);
    //    result.Add(storageBuyingExpense);
    //    result.Add(soldiersWageExpense);

    //    result.Add(ubiSubsidiesExpense);
    //    result.Add(povertyAidExpense);
    //    return result;
    //}

    //public Money GetRegisteredIncome()
    //{
    //    Money result = new Money(0m);
    //    result.Add(incomeTaxStaticticPoor);
    //    result.Add(incomeTaxStatisticRich);
    //    result.Add(incomeTaxForeigner);
    //    result.Add(goldMinesIncome);
    //    result.Add(ownedFactoriesIncome);
    //    result.Add(getCostOfAllSellsByGovernment());
    //    return result;
    //}
}