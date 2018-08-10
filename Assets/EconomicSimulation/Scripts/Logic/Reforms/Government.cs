using System.Collections;
using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Government : AbstractReform, IHasCountry
    {
        public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();
        private ReformValue reform;
        private readonly Country country;

        public Country Country
        {
            get { return country; }
        }

        public class ReformValue : AbstractReformValue
        {
            private readonly int MaxiSizeLimitForDisloyaltyModifier;
            private readonly string prefix;
            private readonly float scienceModifier;

            public ReformValue(string inname, string indescription, int idin, DoubleConditionsList condition, string prefix, int MaxiSizeLimitForDisloyaltyModifier, float scienceModifier)
        : base(inname, indescription, idin, condition)
            {
                this.scienceModifier = scienceModifier;
                this.MaxiSizeLimitForDisloyaltyModifier = MaxiSizeLimitForDisloyaltyModifier;
                // (!PossibleStatuses.Contains(this))
                PossibleStatuses.Add(this);
                this.prefix = prefix;
            }

            //public void onEnacted()
            //{ }
            public override bool isAvailable(Country country)
            {
                if (ID == 4 && !country.Science.IsInvented(Invention.Collectivism))
                    return false;
                else
                    return true;
            }

            protected override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                if (pop.getVotingPower(this) > pop.getVotingPower(pop.Country.government.getTypedValue()))
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

            public string getPrefix()
            {
                return prefix;
            }

            public int getLoyaltySizeLimit()
            {
                return MaxiSizeLimitForDisloyaltyModifier;
            }

            public float getScienceModifier()
            {
                return scienceModifier;
            }

            public override string FullName
            {
                get
                {
                    return base.FullName;// + ". Max size before loyalty penalty applied: " + getLoyaltySizeLimit()
                                         //+ ". Science points modifier: " + scienceModifier;
                }
            }
        }

        public static readonly ReformValue Tribal = new ReformValue("Tribal Federation", "- Democracy-lite; Tribesmen and Aristocrats vote.", 0,
            new DoubleConditionsList(), "Tribe", 10, 0f);

        public static readonly ReformValue Aristocracy = new ReformValue("Aristocracy", "- Aristocrats and Clerics make the rules.", 1,
            new DoubleConditionsList(), "Kingdom", 20, 0.5f);

        public static readonly ReformValue Polis = new ReformValue("Polis", "- Landed individuals allowed to vote. Farmers, Aristocrats, and Clerics share equal voting power.", 8,
            new DoubleConditionsList(), "Polis", 5, 1f);

        public static readonly ReformValue Despotism = new ReformValue("Despotism", "- Who needs elections? All the power belongs to you!", 2,
            new DoubleConditionsList(), "Empire", 40, 0.25f);

        public static readonly ReformValue Theocracy = new ReformValue("Theocracy", "- God decreed only Clerics should have power because of their heavenly connections.", 5,
            new DoubleConditionsList(Condition.IsNotImplemented), "", 40, 0f);

        public static readonly ReformValue WealthDemocracy = new ReformValue("Wealth Democracy", "- Landed individuals allowed to vote, such as Farmers, Aristocrats, etc. Wealthy individuals have more votes (5 to 1)", 9,
            new DoubleConditionsList(Condition.IsNotImplemented), "States", 40, 1f);

        public static readonly ReformValue Democracy = new ReformValue("Universal Democracy", "- The ideal democracy. Everyone's vote is equal.", 3,
            new DoubleConditionsList(new List<Condition> { Invention.IndividualRightsInvented }), "Republic", 100, 1f);

        public static readonly ReformValue BourgeoisDictatorship = new ReformValue("Bourgeois Dictatorship", "- Robber Barons or Captains of Industry? You decide!", 6,
            new DoubleConditionsList(new List<Condition> { Invention.IndividualRightsInvented }), "Oligarchy", 20, 1f);

        public static readonly ReformValue Junta = new ReformValue("Junta", "- The military knows what's best for the people...", 7,
            new DoubleConditionsList(new List<Condition> { Invention.ProfessionalArmyInvented }), "Junta", 20, 0.3f);

        public static readonly ReformValue ProletarianDictatorship = new ReformValue("Proletarian Dictatorship", "- Bureaucrats ruling with a terrifying hammer and a friendly sickle.", 4,
            new DoubleConditionsList(new List<Condition> { Invention.CollectivismInvented, Invention.ManufacturesInvented }), "SSR", 20, 0.5f);

        public static readonly Condition isPolis = new Condition(x => (x as Country).government.getValue() == Polis, "Government is " + Polis, true);
        public static readonly Condition isTribal = new Condition(x => (x as Country).government.getValue() == Tribal, "Government is " + Tribal, true);
        public static readonly Condition isAristocracy = new Condition(x => (x as Country).government.getValue() == Aristocracy, "Government is " + Aristocracy, true);

        public static readonly Condition isDespotism = new Condition(x => (x as Country).government.getValue() == Despotism, "Government is " + Despotism, true);
        public static readonly Condition isTheocracy = new Condition(x => (x as Country).government.getValue() == Theocracy, "Government is " + Theocracy, true);
        public static readonly Condition isWealthDemocracy = new Condition(x => (x as Country).government.getValue() == WealthDemocracy, "Government is " + WealthDemocracy, true);
        public static readonly Condition isDemocracy = new Condition(x => (x as Country).government.getValue() == Democracy, "Government is " + Democracy, true);
        public static readonly Condition isBourgeoisDictatorship = new Condition(x => (x as Country).government.getValue() == BourgeoisDictatorship, "Government is " + BourgeoisDictatorship, true);
        public static readonly Condition isJunta = new Condition(x => (x as Country).government.getValue() == Junta, "Government is " + Junta, true);
        public static readonly Condition isProletarianDictatorship = new Condition(x => (x as Country).government.getValue() == ProletarianDictatorship, "Government is " + ProletarianDictatorship, true);

        public Government(Country country) : base("Government", "Form of government", country)
        {
            reform = Aristocracy;
            this.country = country;
        }

        public string getPrefix()
        {
            return reform.getPrefix();
        }

        public override AbstractReformValue getValue()
        {
            return reform;
        }

        public ReformValue getTypedValue()
        {
            return reform;
        }

        //public override AbstractReformValue getValue(int value)
        //{
        //    return PossibleStatuses[value];
        //}
        public override bool canChange()
        {
            return true;
        }

        public override IEnumerator GetEnumerator()
        {
            foreach (ReformValue f in PossibleStatuses)
                yield return f;
        }

        public void onReformEnacted(Province province)
        {
            var government = province.Country.government.getValue();
            if (government == ProletarianDictatorship)
            {
                country.setSoldierWage(MoneyView.Zero);

                //nationalization
                foreach (var factory in province.AllFactories)
                {
                    country.Nationilize(factory);

                    // next is for PE only
                    factory.PayAllAvailableMoney(country);
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
                    item.PayAllAvailableMoney(country);
                    item.loans.SetZero();
                    item.deposits.SetZero();
                }
            }
        }

        public override void setValue(AbstractReformValue selectedReform)
        {
            base.setValue(selectedReform);
            reform = (ReformValue)selectedReform;
            country.setPrefix();

            if (reform == Tribal)
            {
                country.economy.setValue(Economy.StateCapitalism);
                //country.serfdom.setValue(Serfdom.AbolishedAndNationalized);
                country.minimalWage.setValue(MinimalWage.None);
                country.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                country.minorityPolicy.setValue(MinorityPolicy.Residency);
                country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[2]);
                //country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
            if (reform == Aristocracy)
            {
                country.economy.setValue(Economy.StateCapitalism);
                //country.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                country.minimalWage.setValue(MinimalWage.None);
                country.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                country.minorityPolicy.setValue(MinorityPolicy.Residency);
                //country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[2]);
            }
            else
            if (reform == Polis)
            {
                if (country.economy.getValue() == Economy.PlannedEconomy)
                    country.economy.setValue(Economy.StateCapitalism);
                //country.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                //country.minimalWage.setValue(MinimalWage.None);
                //country.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                //country.minorityPolicy.setValue(MinorityPolicy.Residency);
                //country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                //country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
            if (reform == Despotism)
            {
                country.economy.setValue(Economy.StateCapitalism);
                //country.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                //country.minimalWage.setValue(MinimalWage.None);
                //country.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                //country.minorityPolicy.setValue(MinorityPolicy.Equality);
                //country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                //country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
        if (reform == Theocracy)
            {
                country.economy.setValue(Economy.StateCapitalism);
                //country.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                //country.minimalWage.setValue(MinimalWage.None);
                //country.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                country.minorityPolicy.setValue(MinorityPolicy.Equality);
                //country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                //country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
            if (reform == WealthDemocracy)
            {
                country.economy.setValue(Economy.Interventionism);
                //country.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                //country.minimalWage.setValue(MinimalWage.None);
                //country.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                //country.minorityPolicy.setValue(MinorityPolicy.Equality);
                //country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                //country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
            if (reform == Democracy)
            {
                if (country.economy.getValue() == Economy.PlannedEconomy)
                    country.economy.setValue(Economy.LaissezFaire);
                //country.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                //country.minimalWage.setValue(MinimalWage.None);
                //country.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                //country.minorityPolicy.setValue(MinorityPolicy.Equality);
                //country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                //country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
        if (reform == BourgeoisDictatorship)
            {
                country.economy.setValue(Economy.LaissezFaire);
                //country.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                country.minimalWage.setValue(MinimalWage.None);
                country.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                country.minorityPolicy.setValue(MinorityPolicy.Equality);
                country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[1]);
                country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[1]);
            }
            else
        if (reform == Junta)
            {
                country.economy.setValue(Economy.StateCapitalism);
                //country.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                //country.minimalWage.setValue(MinimalWage.None);
                //country.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                //country.minorityPolicy.setValue(MinorityPolicy.Equality);
                //country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                //country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
            }
            else
        if (reform == ProletarianDictatorship)
            {
                country.economy.setValue(Economy.PlannedEconomy);
                //country.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                country.minimalWage.setValue(MinimalWage.None);
                country.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                country.minorityPolicy.setValue(MinorityPolicy.Equality);
                country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);

                //nationalization
                country.Bank.Nationalize();

                foreach (var province in country.AllProvinces)
                {
                    onReformEnacted(province);
                }
            }
            if (country == Game.Player)
                MainCamera.refreshAllActive();
        }

        //public void setValue(AbstractReformValue selectedReform, bool setPrefix)
        //{
        //    setValue(selectedReform);
        //    if (setPrefix)
        //        country.setPrefix();
        //}

        public override bool isAvailable(Country country)
        {
            return true;
        }

        public override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }
}