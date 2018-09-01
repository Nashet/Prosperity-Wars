using Nashet.Utils;
using System.Collections.Generic;
using System.Text;

namespace Nashet.EconomicSimulation
{
    public class Register : IStatisticable
    {
        protected Dictionary<Account, decimal> moneyFlow = new Dictionary<Account, decimal>();
        public void SetStatisticToZero()
        {
            moneyFlow.Clear();
            //foreach (var item in moneyFlow)
            //{
            //    item.Value = 0m;                    
            //}
        }
        public void RecordPayment(Agent receiver, Account account, decimal sum)
        {
            receiver.Register.moneyFlow.AddAndSum(account, sum);
            moneyFlow.AddAndSum(account, sum * -1m);//giver
        }

        private readonly StringBuilder incomeText = new StringBuilder();
        private readonly StringBuilder expensesText = new StringBuilder();
        public override string ToString()
        {
            incomeText.Clear().Append("Income:");
            expensesText.Clear().Append("\nExpenses");

            foreach (var item in moneyFlow)
            {
                if (item.Value > 0m)
                    incomeText.AppendLine(" " + item.Key.ToString() + ": " + item.Value);
                else if (item.Value < 0m)
                    expensesText.AppendLine(" " + item.Key.ToString() + ": " + item.Value);
            }
            return incomeText.Append(expensesText).ToString();
        }
        public class Account : Name
        {
            public static readonly Account RichIncomeTax = new Account("RichIncomeTax");
            public static readonly Account ForeignIncomeTax = new Account("ForeignIncomeTax");
            public static readonly Account PoorIncomeTax = new Account("PoorIncomeTax");
            public static readonly Account UnemploymentSubsidies = new Account("UnemploymentSubsidies");
            public static readonly Account MarketOperations = new Account("MarketOperations");

            public static readonly Account UBISubsidies = new Account("UBISubsidies");
            public static readonly Account PovertyAid = new Account("PovertyAid");
            public static readonly Account Wage = new Account("Wage");

            public static readonly Account BuyingProperty = new Account("BuyingProperty");
            public static readonly Account Dividends = new Account("Dividends ");
            public static readonly Account MinedGoldTax = new Account("MinedGoldTax");
            public static readonly Account MinedGold = new Account("MinedGold");

            /// <summary>nationalization / closing business </summary>
            public static readonly Account Rest = new Account("Rest");
            public static readonly Account DontRecord = new Account("DontRecord");

            protected Account(string name) : base(name)
            {
            }
        }
    }
}