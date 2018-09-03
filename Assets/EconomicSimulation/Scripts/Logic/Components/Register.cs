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

        protected Money expenses = new Money(0m);
        public MoneyView Expenses { get { return expenses; } }

        public decimal Balance { get { return income.Get() - expenses.Get(); } }

        public void SetStatisticToZero()
        {
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
            expensesText.Clear().Append("\nExpenses " + expenses);

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

            /// <summary>nationalization / closing business </summary>
            public static readonly Account Rest = new Account("Rest");
            //public static readonly Account DontRecord = new Account("DontRecord");


            protected Account(string name) : base(name)
            {
            }
        }
    }
}