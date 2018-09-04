using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections.Generic;
using System.Text;

namespace Nashet.EconomicSimulation
{
    public class Register : IStatisticable
    {
        protected Dictionary<Account, decimal> moneyFlow = new Dictionary<Account, decimal>();

        protected Money income = new Money(0m);
        public MoneyView Income { get { return income; } }

        protected Money incomeLastTurn = new Money(0m);
        public MoneyView IncomeLastTurn { get { return incomeLastTurn; } }

        protected Money expenses = new Money(0m);
        public MoneyView Expenses { get { return expenses; } }

        public decimal Balance { get { return income.Get() - expenses.Get(); } }

        public void SetStatisticToZero()
        {
            incomeLastTurn.Set(income);
            income.SetZero();
            expenses.SetZero();
            moneyFlow.Clear();
            //foreach (var item in moneyFlow)
            //{
            //    item.Value = 0m;                    
            //}
        }
        public void RecordPayment(Agent receiver, Account account, decimal sum)
        {
            receiver.Register.moneyFlow.AddAndSum(account, sum);
            receiver.Register.income.Add(sum);

            this.moneyFlow.AddAndSum(account, sum * -1m);//giver is this
            this.expenses.Add(sum);
        }
        public void RecordIncomeFromNowhere(Account account, decimal sum)
        {
            this.moneyFlow.AddAndSum(account, sum);//giver is this
            this.income.Add(sum);
        }

        private readonly StringBuilder incomeText = new StringBuilder();
        private readonly StringBuilder expensesText = new StringBuilder();
        public override string ToString()
        {
            incomeText.Clear().Append("Income: " + income);
            expensesText.Clear().Append("\nExpenses " + expenses);

            foreach (var item in moneyFlow)
            {
                if (item.Value > 0m)
                    incomeText.Append("\n " + item.Key.ToString() + ": " + Money.DecimalToString(item.Value));
                else if (item.Value < 0m)
                    expensesText.Append("\n " + item.Key.ToString() + ": " + Money.DecimalToString(item.Value * -1m));
            }
            return incomeText.Append(expensesText).ToString();
        }

        public string GetIncomeTest()
        {
            incomeText.Clear().Append("Income: " + income);

            foreach (var item in moneyFlow)
            {
                if (item.Value > 0m)
                    incomeText.Append("\n " + item.Key.ToString() + ": " + Money.DecimalToString(item.Value));
            }
            return incomeText.ToString();
        }

        public string GetExpensesText()
        {
            expensesText.Clear().Append("Expenses: " + expenses);

            foreach (var item in moneyFlow)
            {
                if (item.Value < 0m)
                    expensesText.Append("\n " + item.Key.ToString() + ": " + Money.DecimalToString(item.Value * -1m));
            }
            return expensesText.ToString();
        }

        public class Account : Name
        {
            public static readonly Account RichIncomeTax = new Account("RichIncomeTax");
            public static readonly Account ForeignIncomeTax = new Account("ForeignIncomeTax");
            public static readonly Account PoorIncomeTax = new Account("PoorIncomeTax");

            public static readonly Account UnemploymentSubsidies = new Account("UnemploymentSubsidies");
            public static readonly Account EnterpriseSubsidies = new Account("EnterpriseSubsidies");
            public static readonly Account UBISubsidies = new Account("UBISubsidies");
            public static readonly Account PovertyAid = new Account("PovertyAid");

            public static readonly Account MarketOperations = new Account("MarketOperations");
            public static readonly Account Wage = new Account("Wage");
            public static readonly Account Dividends = new Account("Dividends");
            public static readonly Account BankOperation = new Account("BankOperation");

            public static readonly Account BuyingProperty = new Account("BuyingProperty");
            public static readonly Account Construction = new Account("Construction");
            
            public static readonly Account MinedGoldTax = new Account("MinedGoldTax");
            public static readonly Account MinedGold = new Account("MinedGold");

            /// <summary>nationalization / closing business / pop's merge in / pop's separation</summary>
            public static readonly Account Rest = new Account("Rest");
            //public static readonly Account DontRecord = new Account("DontRecord");


            protected Account(string name) : base(name)
            {
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