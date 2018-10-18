using Nashet.Conditions;
using Nashet.ValueSpace;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation.Reforms
{
    public class Government : AbstractReform
    {
        public static readonly GovernmentReformValue Tribal = new GovernmentReformValue("Tribal Federation", " - Democracy-lite; Tribesmen and Aristocrats vote.", 0,
            new DoubleConditionsList(), "Tribe", 10, 0f, TaxationForPoor.PoorTaxValue.TaxRate20);

        public static readonly GovernmentReformValue Aristocracy = new GovernmentReformValue("Aristocracy", " - Aristocrats and Clerics make the rules.", 1,
            new DoubleConditionsList(), "Kingdom", 20, 0.5f, TaxationForPoor.PoorTaxValue.TaxRate0, TaxationForRich.RichTaxValue.TaxRate20);

        public static readonly GovernmentReformValue Polis = new GovernmentReformValue("Polis", " - Landed individuals allowed to vote. Farmers, Aristocrats, and Clerics share equal voting power.", 8,
            new DoubleConditionsList(), "Polis", 5, 1f);

        public static readonly GovernmentReformValue Despotism = new GovernmentReformValue("Despotism", " - Who needs elections? All the power belongs to you!", 2,
            new DoubleConditionsList(), "Empire", 40, 0.25f);

        public static readonly GovernmentReformValue Theocracy = new GovernmentReformValue("Theocracy", " - God decreed only Clerics should have power because of their heavenly connections.", 5,
            new DoubleConditionsList(Condition.IsNotImplemented), "", 40, 0f);

        public static readonly GovernmentReformValue WealthDemocracy = new GovernmentReformValue("Wealth Democracy", " - Landed individuals allowed to vote, such as Farmers, Aristocrats, etc. Wealthy individuals have more votes (5 to 1)", 9,
            new DoubleConditionsList(Condition.IsNotImplemented), "States", 40, 1f);

        public static readonly GovernmentReformValue Democracy = new GovernmentReformValue("Universal Democracy", " - The ideal democracy. Everyone's vote is equal.", 3,
            new DoubleConditionsList(new List<Condition> { Invention.IndividualRights.Invented }), "Republic", 100, 1f);

        public static readonly GovernmentReformValue BourgeoisDictatorship = new GovernmentReformValue("Bourgeois Dictatorship", " - Robber Barons or Captains of Industry? You decide!", 6,
            new DoubleConditionsList(new List<Condition> { Invention.IndividualRights.Invented }), "Oligarchy", 20, 1f, TaxationForPoor.PoorTaxValue.TaxRate10, TaxationForRich.RichTaxValue.TaxRate10);

        public static readonly GovernmentReformValue Junta = new GovernmentReformValue("Junta", " - The military knows what's best for the people...", 7,
            new DoubleConditionsList(Invention.ProfessionalArmy.Invented), "Junta", 20, 0.3f);

        public static readonly GovernmentReformValue ProletarianDictatorship = new GovernmentReformValue("Proletarian Dictatorship", " - Bureaucrats ruling with a terrifying hammer and a friendly sickle.", 4,
            new DoubleConditionsList(new List<Condition> { Invention.Collectivism.Invented, Invention.Manufactures.Invented }), "SSR", 20, 0.5f, TaxationForPoor.PoorTaxValue.TaxRate50, TaxationForRich.RichTaxValue.TaxRate100);

        public static readonly Condition isPolis = new Condition(x => (x as Country).government == Polis, "Government is " + Polis, true);
        public static readonly Condition isTribal = new Condition(x => (x as Country).government == Tribal, "Government is " + Tribal, true);
        public static readonly Condition isAristocracy = new Condition(x => (x as Country).government == Aristocracy, "Government is " + Aristocracy, true);

        public static readonly Condition isDespotism = new Condition(x => (x as Country).government == Despotism, "Government is " + Despotism, true);
        public static readonly Condition isTheocracy = new Condition(x => (x as Country).government == Theocracy, "Government is " + Theocracy, true);
        public static readonly Condition isWealthDemocracy = new Condition(x => (x as Country).government == WealthDemocracy, "Government is " + WealthDemocracy, true);
        public static readonly Condition isDemocracy = new Condition(x => (x as Country).government == Democracy, "Government is " + Democracy, true);
        public static readonly Condition isBourgeoisDictatorship = new Condition(x => (x as Country).government == BourgeoisDictatorship, "Government is " + BourgeoisDictatorship, true);
        public static readonly Condition isJunta = new Condition(x => (x as Country).government == Junta, "Government is " + Junta, true);
        public static readonly Condition isProletarianDictatorship = new Condition(x => (x as Country).government == ProletarianDictatorship, "Government is " + ProletarianDictatorship, true);

        public GovernmentReformValue typedValue { get; protected set; }

        public Government(Country country, int showOrder) : base("Government", " (forms of government)", country, showOrder,
            new List<IReformValue> {Tribal, Aristocracy, Polis, Despotism, Theocracy, WealthDemocracy,
                Democracy, BourgeoisDictatorship, Junta, ProletarianDictatorship})
        {
            SetValue(Aristocracy);
            //typedValue = Aristocracy;
            //value = Aristocracy;
        }
        public float ScienceModifier { get { return typedValue.ScienceModifier; } }
        public int LoyaltySizeLimit { get { return typedValue.MaxSizeLimitForDisloyaltyModifier; } }
        internal string Prefix { get { return typedValue.Prefix; } }

        public override void OnReformEnactedInProvince(Province province)
        {
            var government = province.Country.government;
            if (government == ProletarianDictatorship)
            {
                owner.setSoldierWage(MoneyView.Zero);

                //nationalization
                foreach (var factory in province.AllFactories)
                {
                    owner.Nationilize(factory);

                    // next is for PE only
                    factory.PayAllAvailableMoney(owner, Register.Account.Rest);
                    factory.loans.SetZero();
                    factory.deposits.SetZero();
                    factory.setSubsidized(false);
                    factory.setZeroSalary();
                    factory.setPriorityAutoWithPlannedEconomy();
                    //factory.setStatisticToZero();
                }
                //nationalize banks
                foreach (var item in province.AllPops)
                {
                    item.PayAllAvailableMoney(owner, Register.Account.Rest);
                    item.loans.SetZero();
                    item.deposits.SetZero();
                }
            }
        }
        public override void SetValue(IReformValue selectedReform)
        {
            base.SetValue(selectedReform);
            typedValue = selectedReform as GovernmentReformValue;
            owner.setPrefix();

            if (typedValue == Tribal)
            {
                owner.economy.SetValue(Economy.StateCapitalism);
                //owner.serfdom.setValue(Serfdom.AbolishedAndNationalized);
                owner.minimalWage.SetValue(MinimalWage.None);
                owner.unemploymentSubsidies.SetValue(UnemploymentSubsidies.None);
                owner.minorityPolicy.SetValue(MinorityPolicy.Residency);
                owner.taxationForPoor.SetValue(Tribal.defaultPoorTax);
                //owner.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
            if (typedValue == Aristocracy)
            {
                owner.economy.SetValue(Economy.StateCapitalism);
                //owner.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                owner.minimalWage.SetValue(MinimalWage.None);
                owner.unemploymentSubsidies.SetValue(UnemploymentSubsidies.None);
                owner.minorityPolicy.SetValue(MinorityPolicy.Residency);
                //owner.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                owner.taxationForRich.SetValue(Aristocracy.defaultRichTax);
            }
            else
            if (typedValue == Polis)
            {
                if (owner.economy == Economy.PlannedEconomy)
                    owner.economy.SetValue(Economy.StateCapitalism);
                //owner.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                //owner.minimalWage.setValue(MinimalWage.None);
                //owner.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                //owner.minorityPolicy.setValue(MinorityPolicy.Residency);
                //owner.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                //owner.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
            if (typedValue == Despotism)
            {
                owner.economy.SetValue(Economy.StateCapitalism);
                //owner.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                //owner.minimalWage.setValue(MinimalWage.None);
                //owner.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                //owner.minorityPolicy.setValue(MinorityPolicy.Equality);
                //owner.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                //owner.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
        if (typedValue == Theocracy)
            {
                owner.economy.SetValue(Economy.StateCapitalism);
                //owner.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                //owner.minimalWage.setValue(MinimalWage.None);
                //owner.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                owner.minorityPolicy.SetValue(MinorityPolicy.Equality);
                //owner.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                //owner.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
            if (typedValue == WealthDemocracy)
            {
                owner.economy.SetValue(Economy.Interventionism);
                //owner.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                //owner.minimalWage.setValue(MinimalWage.None);
                //owner.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                //owner.minorityPolicy.setValue(MinorityPolicy.Equality);
                //owner.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                //owner.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
            if (typedValue == Democracy)
            {
                if (owner.economy == Economy.PlannedEconomy)
                    owner.economy.SetValue(Economy.LaissezFaire);
                //owner.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                //owner.minimalWage.setValue(MinimalWage.None);
                //owner.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                //owner.minorityPolicy.setValue(MinorityPolicy.Equality);
                //owner.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                //owner.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
        if (typedValue == BourgeoisDictatorship)
            {
                owner.economy.SetValue(Economy.LaissezFaire);
                //owner.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                owner.minimalWage.SetValue(MinimalWage.None);
                owner.unemploymentSubsidies.SetValue(UnemploymentSubsidies.None);
                owner.minorityPolicy.SetValue(MinorityPolicy.Equality);
                owner.taxationForPoor.SetValue(BourgeoisDictatorship.defaultPoorTax);
                owner.taxationForRich.SetValue(BourgeoisDictatorship.defaultRichTax);
            }
            else
        if (typedValue == Junta)
            {
                owner.economy.SetValue(Economy.StateCapitalism);
                //owner.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                //owner.minimalWage.setValue(MinimalWage.None);
                //owner.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                //owner.minorityPolicy.setValue(MinorityPolicy.Equality);
                //owner.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                //owner.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
        if (typedValue == ProletarianDictatorship)
            {
                owner.economy.SetValue(Economy.PlannedEconomy);
                //owner.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                owner.minimalWage.SetValue(MinimalWage.None);
                owner.unemploymentSubsidies.SetValue(UnemploymentSubsidies.None);
                owner.minorityPolicy.SetValue(MinorityPolicy.Equality);
                owner.taxationForPoor.SetValue(ProletarianDictatorship.defaultPoorTax);
                owner.taxationForRich.SetValue(ProletarianDictatorship.defaultRichTax);

                //nationalization
                owner.Bank.Nationalize();

                foreach (var province in owner.AllProvinces)
                {
                    OnReformEnactedInProvince(province);
                }
            }
            if (owner == Game.Player)
                //MainCamera.refreshAllActive();
                UIEvents.RiseSomethingVisibleToPlayerChangedInWorld(EventArgs.Empty, this);
        }
        public class GovernmentReformValue : NamedReformValue
        {
            public int MaxSizeLimitForDisloyaltyModifier { get; protected set; }
            public string Prefix { get; protected set; }
            public float ScienceModifier { get; protected set; }
            public TaxationForPoor.PoorTaxValue defaultPoorTax;
            public TaxationForRich.RichTaxValue defaultRichTax;

            internal GovernmentReformValue(string name, string description, int id, DoubleConditionsList condition,
                string prefix, int MaxSizeLimitForDisloyaltyModifier, float scienceModifier, TaxationForPoor.PoorTaxValue defaultPoorTax = null, TaxationForRich.RichTaxValue defaultRichTax = null) : base(name, description, id, condition)
            {
                Prefix = prefix;
                this.MaxSizeLimitForDisloyaltyModifier = MaxSizeLimitForDisloyaltyModifier;
                ScienceModifier = scienceModifier;
                this.defaultPoorTax = defaultPoorTax;
                this.defaultRichTax = defaultRichTax;
            }
            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                if (pop.getVotingPower(this) > pop.getVotingPower(pop.Country.government.typedValue))
                {
                    //if (this == Tribal)
                    //    result = new Procent(0.8f);
                    //else
                    result = new Procent(1f);
                }
                else if (this == ProletarianDictatorship)
                    result = new Procent(0.5f);
                else if (this == Despotism && pop.needsFulfilled.get() < 0.1f)
                    result = new Procent(1f);
                else if (this == Tribal)
                    result = new Procent(0.3f);
                else
                    result = new Procent(0f);
                return result;
            }






        }
    }
}