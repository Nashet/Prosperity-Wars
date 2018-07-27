using System.Collections;
using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public static class ReformExtensions
    {
        public static bool isEnacted(this List<AbstractReform> list, AbstractReformValue reformValue)
        {
            foreach (var item in list)
                if (item.getValue() == reformValue)
                    return true;
            return false;
        }
    }

    public abstract class AbstractReformStepValue : AbstractReformValue
    {
        //private readonly int totalSteps;
        protected AbstractReformStepValue(string name, string indescription, int ID, DoubleConditionsList condition, int totalSteps)
            : base(name, indescription, ID, condition)
        {
        }
    }

    public abstract class AbstractReformValue : Name
    {
        public static readonly DoubleCondition isNotLFOrMoreConservative = new DoubleCondition((country, newReform) => (country as Country).economy.getValue() != Economy.LaissezFaire
        || (newReform as AbstractReformValue).isMoreConservative(
            (country as Country).getReform((newReform as AbstractReformValue)).getValue()
            ), x => "Economy policy is not Laissez Faire or that is reform rollback", true);

        private readonly string description;
        public readonly int ID; // covert inti liberal_weight
        public readonly DoubleConditionsList allowed;
        public readonly Condition isEnacted;// = new Condition(x => !(x as Country).reforms.isEnacted(this), "Reform is not enacted yet", true);

        public abstract bool isAvailable(Country country);

        protected abstract Procent howIsItGoodForPop(PopUnit pop);

        static AbstractReformValue()
        {
            //allowed.add();
        }

        protected AbstractReformValue(string name, string indescription, int ID, DoubleConditionsList condition) : base(name)
        {
            this.ID = ID;
            description = indescription;
            allowed = condition;
            isEnacted = new Condition(x => !(x as Country).reforms.isEnacted(this), "Reform not enacted yet", false);
            allowed.add(isEnacted);
            wantsReform = new Modifier(x => howIsItGoodForPop(x as PopUnit).get(),
                        "Benefit to population", 1f, true);
            loyalty = new Modifier(x => loyaltyBoostFor(x as PopUnit),
                        "Loyalty", 1f, false);
            modVoting = new ModifiersList(new List<Condition>{
        wantsReform, loyalty, education
        });
        }

        public bool isMoreConservative(AbstractReformValue anotherReform)
        {
            return ID < anotherReform.ID;
        }

        private float loyaltyBoostFor(PopUnit popUnit)
        {
            float result;
            if (howIsItGoodForPop(popUnit).get() > 0.5f)
                result = popUnit.loyalty.get() / 4f;
            else
                result = popUnit.loyalty.get50Centre() / 4f;
            return result;
        }

        public override string FullName
        {
            get { return description; }
        }

        private readonly Modifier loyalty;
        private readonly Modifier education = new Modifier(Condition.IsNotImplemented, 0f, false);
        private readonly Modifier wantsReform;
        public readonly ModifiersList modVoting;
    }

    public abstract class AbstractReform : Name, IClickable
    {
        private readonly string description;

        protected AbstractReform(string name, string indescription, Country country) : base(name)
        {
            description = indescription;
            country.reforms.Add(this);
            this.country = country;
        }
        private readonly Country country;
        public abstract bool isAvailable(Country country);

        public abstract IEnumerator GetEnumerator();

        public abstract bool canChange();

        public virtual void setValue(AbstractReformValue selectedReformValue)
        {
            foreach (PopUnit pop in country.GetAllPopulation())
                if (pop.getSayingYes(selectedReformValue))
                {
                    pop.loyalty.Add(Options.PopLoyaltyBoostOnDiseredReformEnacted);
                    pop.loyalty.clamp100();
                }
            var isThereSuchMovement = country.movements.Find(x => x.getGoal() == selectedReformValue);
            if (isThereSuchMovement != null)
            {
                isThereSuchMovement.onRevolutionWon(false);                
            }
        }

        public override string FullName
        {
            get { return description; }
        }

        public abstract AbstractReformValue getValue();

        public abstract bool canHaveValue(AbstractReformValue abstractReformValue);

        //abstract public AbstractReformValue getValue(int value);
        //abstract public void setValue(int value);
        public void OnClicked()
        {
            MainCamera.politicsPanel.selectReform(this);
            MainCamera.politicsPanel.Refresh();
        }
    }

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
                if (ID == 4 && !country.Invented(Invention.Collectivism))
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
                foreach (var factory in province.getAllFactories())
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
                foreach (var item in province.GetAllPopulation())
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

                foreach (var province in country.AllProvinces())
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

    public class Economy : AbstractReform, IHasCountry
    {
        private readonly Country country;

        public static readonly Condition isNotLF = new Condition(delegate (object forWhom) { return (forWhom as Country).economy.status != LaissezFaire; }, "Economy policy is not Laissez Faire", true);
        public static readonly Condition isLF = new Condition(delegate (object forWhom) { return (forWhom as Country).economy.status == LaissezFaire; }, "Economy policy is Laissez Faire", true);

        public static readonly Condition isNotNatural = new Condition(x => (x as Country).economy.status != NaturalEconomy, "Economy policy is not Natural Economy", true);
        public static readonly Condition isNatural = new Condition(x => (x as Country).economy.status == NaturalEconomy, "Economy policy is Natural Economy", true);

        public static readonly Condition isNotState = new Condition(x => (x as Country).economy.status != StateCapitalism, "Economy policy is not State Capitalism", true);
        public static readonly Condition isStateCapitlism = new Condition(x => (x as Country).economy.status == StateCapitalism, "Economy policy is State Capitalism", true);

        public static readonly Condition isNotInterventionism = new Condition(x => (x as Country).economy.status != Interventionism, "Economy policy is not Limited Interventionism", true);
        public static readonly Condition isInterventionism = new Condition(x => (x as Country).economy.status == Interventionism, "Economy policy is Limited Interventionism", true);

        public static readonly Condition isNotPlanned = new Condition(x => (x as Country).economy.status != PlannedEconomy, "Economy policy is not Planned Economy", true);
        public static readonly Condition isPlanned = new Condition(x => (x as Country).economy.status == PlannedEconomy, "Economy policy is Planned Economy", true);

        public static readonly DoubleCondition taxesInsideLFLimit = new DoubleCondition(
        delegate (object x, object y)
        {
            //if it's poor taxes
            var taxesForPoor = y as TaxationForPoor.ReformValue;
            if (taxesForPoor != null)
                return (x as Country).economy.status != LaissezFaire || taxesForPoor.tax.get() <= 0.5f;
            else
            {
                var taxesForRich = y as TaxationForRich.ReformValue;
                return (x as Country).economy.status != LaissezFaire || taxesForRich.tax.get() <= 0.5f;
            }
        },
            x => "Economy policy is Laissez Faire and tax is not higher than 50%", false);

        public static readonly DoubleCondition taxesInsideSCLimit = new DoubleCondition(
        delegate (object x, object y)
        {
            //if it's poor taxes
            var taxesForPoor = y as TaxationForPoor.ReformValue;
            if (taxesForPoor != null)
                return (x as Country).economy.status != StateCapitalism || taxesForPoor.tax.get() >= 0.2f;
            else
            {
                var taxesForRich = y as TaxationForRich.ReformValue;
                return (x as Country).economy.status != StateCapitalism || taxesForRich.tax.get() >= 0.2f;
            }
        },
            x => "Economy policy is State capitalism and tax is not lower than 20%", false);

        public static Condition isNotMarket = new Condition(x => (x as Country).economy.status == NaturalEconomy || (x as Country).economy.status == PlannedEconomy,
          "Economy is not market economy", true);

        public static Condition isMarket = new Condition(x => (x as Country).economy.status == StateCapitalism || (x as Country).economy.status == Interventionism
            || (x as Country).economy.status == LaissezFaire
            , "Economy is market economy", true);

        public class ReformValue : AbstractReformValue
        {
            private readonly bool _allowForeignIvestments;

            public bool AllowForeignInvestments
            {
                get { return _allowForeignIvestments; }
            }

            public ReformValue(string name, string description, int id, bool _allowForeighnIvestments, DoubleConditionsList condition) : base(name, description, id, condition)
            {
                PossibleStatuses.Add(this);
                _allowForeignIvestments = _allowForeighnIvestments;
            }

            public override bool isAvailable(Country country)
            {
                ReformValue requested = this;
                if (requested.ID == 0)
                    return true;
                else
                if (requested.ID == 1)
                    return true;
                else
                if (requested.ID == 2 && country.Invented(Invention.Collectivism))
                    return true;
                else
                if (requested.ID == 3)
                    return true;
                else
                    return false;
            }

            protected override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                //if (pop.Type == PopType.Capitalists)
                if (pop.Type.isRichStrata())
                {
                    //positive - more liberal
                    int change = ID - pop.Country.economy.status.ID;
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                    if (change > 0)
                        result = new Procent(1f + change / 10f);
                    else
                        //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                        result = new Procent(0f);
                }
                else
                {
                    if (this == PlannedEconomy)
                        result = new Procent(0f); // that can be achieved only by government reform
                    else if (this == LaissezFaire)
                    {
                        result = new Procent(0.6f);
                    }
                    else if (this == Interventionism)
                    {
                        result = new Procent(0.7f);
                    }
                    else
                        result = new Procent(0.5f);
                }
                return result;
            }
        }

        private static readonly ConditionsList capitalism = new ConditionsList(new List<Condition>
        {
            Invention.IndividualRightsInvented,
            Invention.BankingInvented,
            Serfdom.IsAbolishedInAnyWay
        });

        private ReformValue status;

        public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();

        public static readonly ReformValue PlannedEconomy = new ReformValue("Planned economy", "", 0, false,
            new DoubleConditionsList(new List<Condition> {
            Invention.CollectivismInvented, Government.isProletarianDictatorship }));

        public static readonly ReformValue NaturalEconomy = new ReformValue("Natural economy", " ", 1, false, new DoubleConditionsList(Condition.IsNotImplemented));//new ConditionsList(Condition.AlwaysYes));
        public static readonly ReformValue StateCapitalism = new ReformValue("State capitalism", "", 2, false, new DoubleConditionsList(capitalism));
        public static readonly ReformValue Interventionism = new ReformValue("Limited interventionism", "", 3, true, new DoubleConditionsList(capitalism));
        public static readonly ReformValue LaissezFaire = new ReformValue("Laissez faire", "", 4, true, new DoubleConditionsList(capitalism));

        /// ////////////
        public Economy(Country country) : base("Economy", "Your economy policy", country)
        {
            status = NaturalEconomy;
            this.country = country;
        }

        public override AbstractReformValue getValue()
        {
            return status;
        }

        public Country Country
        {
            get { return country; }
        }

        // todo add OnReformEnacted?
        public override void setValue(AbstractReformValue selectedReform)
        {
            base.setValue(selectedReform);
            status = (ReformValue)selectedReform;
            if (status == LaissezFaire)
            {
                if (Country.taxationForRich.getTypedValue().tax.get() > 0.5f)
                    Country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[5]);
                if (Country.taxationForPoor.getTypedValue().tax.get() > 0.5f)
                    Country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                Country.getAllFactories().PerformAction(
                     x =>
                     {
                         x.setSubsidized(false);
                         x.ownership.SetToSell(Country, Procent.HundredProcent, false);
                     });
            }
            else
                if (status == StateCapitalism)
            {
                if (Country.taxationForRich.getTypedValue().tax.get() < 0.2f)
                    Country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[2]);
                if (Country.taxationForPoor.getTypedValue().tax.get() < 0.2f)
                    Country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[2]);
            }
        }

        public ReformValue getTypedValue()
        {
            return status;
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

        public override bool isAvailable(Country country)
        {
            return true;
        }

        public override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }

    public class Serfdom : AbstractReform
    {
        public class ReformValue : AbstractReformValue
        {
            public ReformValue(string inname, string indescription, int idin, DoubleConditionsList condition) : base(inname, indescription, idin, condition)
            {
                //if (!PossibleStatuses.Contains(this))
                PossibleStatuses.Add(this);
                // this.allowed = condition;
            }

            public override bool isAvailable(Country country)
            {
                ReformValue requested = this;

                if ((requested.ID == 4) && country.Invented(Invention.Collectivism) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 4))
                    return true;
                else
                if ((requested.ID == 3) && country.Invented(Invention.Banking) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 3))
                    return true;
                else
                if ((requested.ID == 2) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1 || country.serfdom.status.ID == 2))
                    return true;
                else
                    if ((requested.ID == 1) && (country.serfdom.status.ID == 0 || country.serfdom.status.ID == 1))
                    return true;
                else
                if ((requested.ID == 0))
                    return true;
                else
                    return false;
            }

            private static Procent br = new Procent(0.2f);
            private static Procent al = new Procent(0.1f);
            private static Procent nu = new Procent(0.0f);

            public Procent getTax()
            {
                if (this == Brutal)
                    return br;
                else
                    if (this == Allowed)
                    return al;
                else
                    return nu;
            }

            protected override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                int change = ID - pop.Country.serfdom.status.ID; //positive - more liberal
                if (pop.Type == PopType.Aristocrats)
                {
                    if (change > 0)
                        result = new Procent(0f);
                    else
                        result = new Procent(1f);
                }
                else
                {
                    if (change > 0)
                        result = new Procent(1f);
                    else
                        result = new Procent(0f);
                }
                return result;
            }
        }

        public ReformValue status;
        public static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { Allowed, Brutal, Abolished, AbolishedWithLandPayment, AbolishedAndNationalizated };
        public static ReformValue Allowed;
        public static ReformValue Brutal;

        public static ReformValue Abolished = new ReformValue("Abolished", "- Abolished with no obligations", 2,
            new DoubleConditionsList(new List<Condition> { Invention.IndividualRightsInvented, Condition.IsNotImplemented }));

        public static ReformValue AbolishedWithLandPayment = new ReformValue("Abolished with land payment", "- Peasants are personally free now but they have to pay debt for land", 3,
            new DoubleConditionsList(new List<Condition>
            {
            Invention.IndividualRightsInvented,Invention.BankingInvented, Condition.IsNotImplemented
            }));

        public static ReformValue AbolishedAndNationalized = new ReformValue("Abolished and Nationalized land", "- Aristocrats loose property", 4,
            new DoubleConditionsList(new List<Condition>
            {
            Government.isProletarianDictatorship, Condition.IsNotImplemented
            }));

        public Serfdom(Country country) : base("Serfdom", "- Aristocratic Privileges", country)
        {
            if (Allowed == null)
                Allowed = new ReformValue("Allowed", "- Peasants and other plebes pay 10% of income to Aristocrats", 1,
                    new DoubleConditionsList(new List<Condition>
                    {
            Economy.isNotMarket,  Condition.IsNotImplemented
                    }));
            if (Brutal == null)
                Brutal = new ReformValue("Brutal", "- Peasants and other plebes pay 20% of income to Aristocrats", 0,
                new DoubleConditionsList(new List<Condition>
                {
            Economy.isNotMarket, Condition.IsNotImplemented
                }));

            status = Allowed;
        }

        public override AbstractReformValue getValue()
        {
            return status;
        }

        //public override AbstractReformValue getValue(int value)
        //{
        //    //return PossibleStatuses.Find(x => x.ID == value);
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

        public override void setValue(AbstractReformValue selectedReform)
        {
            base.setValue(selectedReform);
            status = (ReformValue)selectedReform;
        }

        public override bool isAvailable(Country country)
        {
            return true;
        }

        public static Condition IsAbolishedInAnyWay = new Condition(x => (x as Country).serfdom.status == Abolished
        || (x as Country).serfdom.status == AbolishedAndNationalized || (x as Country).serfdom.status == AbolishedWithLandPayment,
            "Serfdom is abolished", true);

        public static Condition IsNotAbolishedInAnyWay = new Condition(x => (x as Country).serfdom.status == Allowed
        || (x as Country).serfdom.status == Brutal,
            "Serfdom is in power", true);

        public override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }

    public class MinimalWage : AbstractReform
    {
        public class ReformValue : AbstractReformStepValue
        {
            public ReformValue(string inname, string indescription, int id, DoubleConditionsList condition)
                : base(inname, indescription, id, condition, 6)
            {
                // if (!PossibleStatuses.Contains(this))
                PossibleStatuses.Add(this);
                var totalSteps = 6;
                var previousID = ID - 1;
                var nextID = ID + 1;
                if (previousID >= 0 && nextID < totalSteps)
                    condition.add(new Condition(x => (x as Country).minimalWage.isThatReformEnacted(previousID)
                    || (x as Country).minimalWage.isThatReformEnacted(nextID), "Previous reform enacted", true));
                else
                {
                    if (nextID < totalSteps)
                        condition.add(new Condition(x => (x as Country).minimalWage.isThatReformEnacted(nextID), "Previous reform enacted", true));
                    else
                    {
                        if (previousID >= 0)
                            condition.add(new Condition(x => (x as Country).minimalWage.isThatReformEnacted(previousID), "Previous reform enacted", true));
                    }
                }
            }

            public override bool isAvailable(Country country)
            {
                return true;
            }

            /// <summary>
            /// Calculates wage basing on consumption cost for 1000 workers
            /// Returns new value
            /// </summary>
            public MoneyView getWage(Market market)
            {
                if (this == None)
                    return Money.Zero;
                else if (this == Scanty)
                {
                    MoneyView result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    //result.multipleInside(0.5f);
                    return result;
                }
                else if (this == Minimal)
                {
                    Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
                    Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
                    everyDayCost.Multiply(0.02m);
                    result.Add(everyDayCost);
                    return result;
                }
                else if (this == Trinket)
                {
                    Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
                    Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
                    everyDayCost.Multiply(0.04m);
                    result.Add(everyDayCost);
                    return result;
                }
                else if (this == Middle)
                {
                    Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
                    Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
                    everyDayCost.Multiply(0.06m);
                    result.Add(everyDayCost);
                    return result;
                }
                else if (this == Big)
                {
                    Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
                    Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
                    everyDayCost.Multiply(0.08m);
                    //Value luxuryCost = Country.market.getCost(PopType.workers.luxuryNeedsPer1000);
                    result.Add(everyDayCost);
                    //result.add(luxuryCost);
                    return result;
                }
                else
                    return new Money(0m);
            }

            public override string ToString()
            {
                return base.ToString() + " (" + "getwage back" + ")";//getWage()
            }

            protected override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                if (pop.Type == PopType.Workers)
                {
                    //positive - reform will be better for worker, [-5..+5]
                    int change = ID - pop.Country.minimalWage.status.ID;
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                    if (change > 0)
                        result = new Procent(1f);
                    else
                        //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                        result = new Procent(0f);
                }
                else if (pop.Type.isPoorStrata())
                    result = new Procent(0.5f);
                else // rich strata
                {
                    //positive - reform will be better for rich strata, [-5..+5]
                    int change = pop.Country.minimalWage.status.ID - ID;
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                    if (change > 0)
                        result = new Procent(1f);
                    else
                        //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                        result = new Procent(0f);
                }
                return result;
            }
        }

        private ReformValue status;

        public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();
        public static readonly ReformValue None = new ReformValue("No Minimum Wage", "", 0, new DoubleConditionsList(new List<Condition> { AbstractReformValue.isNotLFOrMoreConservative }));

        public static readonly ReformValue Scanty = new ReformValue("Scant Minimum Wage", "- Half-hungry", 1, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly ReformValue Minimal = new ReformValue("Subsistence Minimum Wage", "- Just enough to feed yourself", 2, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly ReformValue Trinket = new ReformValue("Mid-Level Minimum Wage", "- You can buy some small stuff", 3, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly ReformValue Middle = new ReformValue("Social Security", "- Minimum Wage & Retirement benefits", 4, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly ReformValue Big = new ReformValue("Generous Minimum Wage", "- Can live almost like a king. Almost..", 5, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented,AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public MinimalWage(Country country) : base("Minimum wage", "", country)
        {
            status = None;
        }

        public bool isThatReformEnacted(int value)
        {
            return status == PossibleStatuses[value];
        }

        public override AbstractReformValue getValue()
        {
            return status;
        }

        //public override AbstractReformValue getValue(int value)
        //{
        //    return PossibleStatuses.Find(x => x.ID == value);
        //    //return PossibleStatuses[value];
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

        public override void setValue(AbstractReformValue selectedReform)
        {
            base.setValue(selectedReform);
            status = (ReformValue)selectedReform;
        }

        public override bool isAvailable(Country country)
        {
            if (country.Invented(Invention.Welfare))
                return true;
            else
                return false;
        }

        public override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }

    public class UnemploymentSubsidies : AbstractReform
    {
        public class ReformValue : AbstractReformStepValue
        {
            public ReformValue(string inname, string indescription, int idin, DoubleConditionsList condition)
                : base(inname, indescription, idin, condition, 6)
            {
                //if (!PossibleStatuses.Contains(this))
                PossibleStatuses.Add(this);
                var totalSteps = 6;
                var previousID = ID - 1;
                var nextID = ID + 1;
                if (previousID >= 0 && nextID < totalSteps)
                    condition.add(new Condition(x => (x as Country).unemploymentSubsidies.isThatReformEnacted(previousID)
                    || (x as Country).unemploymentSubsidies.isThatReformEnacted(nextID), "Previous reform enacted", true));
                else
                if (nextID < totalSteps)
                    condition.add(new Condition(x => (x as Country).unemploymentSubsidies.isThatReformEnacted(nextID), "Previous reform enacted", true));
                else
                if (previousID >= 0)
                    condition.add(new Condition(x => (x as Country).unemploymentSubsidies.isThatReformEnacted(previousID), "Previous reform enacted", true));
            }

            public override bool isAvailable(Country country)
            {
                return true;
            }

            /// <summary>
            /// Calculates Unemployment Subsidies basing on consumption cost for 1000 workers
            /// </summary>
            public MoneyView getSubsidiesRate(Market market)
            {
                if (this == None)
                    return MoneyView.Zero;
                else if (this == Scanty)
                {
                    MoneyView result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    //result.multipleInside(0.5f);
                    return result;
                }
                else if (this == Minimal)
                {
                    Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
                    Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
                    everyDayCost.Multiply(0.02m);
                    result.Add(everyDayCost);
                    return result;
                }
                else if (this == Trinket)
                {
                    Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
                    Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
                    everyDayCost.Multiply(0.04m);
                    result.Add(everyDayCost);
                    return result;
                }
                else if (this == Middle)
                {
                    Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
                    Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
                    everyDayCost.Multiply(0.06m);
                    result.Add(everyDayCost);
                    return result;
                }
                else if (this == Big)
                {
                    Money result = market.getCost(PopType.Workers.getLifeNeedsPer1000Men()).Copy();
                    Money everyDayCost = market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men()).Copy();
                    everyDayCost.Multiply(0.08m);
                    //Value luxuryCost = Country.market.getCost(PopType.workers.luxuryNeedsPer1000);
                    result.Add(everyDayCost);
                    //result.add(luxuryCost);
                    return result;
                }
                else
                {
                    Debug.Log("Unknown reform");
                    return MoneyView.Zero;
                }
            }

            public override string ToString()
            {
                return base.ToString() + " (" + "get back getSubsidiesRate()" + ")";//getSubsidiesRate()
            }

            protected override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                //positive - higher subsidies
                int change = ID - pop.Country.unemploymentSubsidies.status.ID;
                if (pop.Type.isPoorStrata())
                {
                    if (change > 0)
                        result = new Procent(1f);
                    else
                        result = new Procent(0f);
                }
                else
                {
                    if (change > 0)
                        result = new Procent(0f);
                    else
                        result = new Procent(1f);
                }
                return result;
            }
        }

        private ReformValue status;
        public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();
        public static readonly ReformValue None = new ReformValue("No Unemployment Benefits", "", 0, new DoubleConditionsList(new List<Condition>()));

        public static readonly ReformValue Scanty = new ReformValue("Bread Lines", "-The people are starving. Let them eat bread.", 1, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly ReformValue Minimal = new ReformValue("Food Stamps", "- Let the people buy what they need.", 2, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly ReformValue Trinket = new ReformValue("Housing & Food Assistance", "- Affordable Housing for the Unemployed.", 3, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly ReformValue Middle = new ReformValue("Welfare Ministry", "- Now there is a minister granting greater access to benefits.", 4, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public static readonly ReformValue Big = new ReformValue("Full State Unemployment Benefits", "- Full State benefits for the downtrodden.", 5, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned
        }));

        public UnemploymentSubsidies(Country country) : base("Unemployment Subsidies", "", country)
        {
            status = None;
        }

        public bool isThatReformEnacted(int value)
        {
            return status == PossibleStatuses[value];
        }

        public override AbstractReformValue getValue()
        {
            return status;
        }

        //public override AbstractReformValue getValue(int value)
        //{
        //    return PossibleStatuses.Find(x => x.ID == value);
        //    //return PossibleStatuses[value];
        //}
        public override void setValue(AbstractReformValue selectedReform)
        {
            base.setValue(selectedReform);
            status = (ReformValue)selectedReform;
        }

        public override bool canChange()
        {
            return true;
        }

        public override IEnumerator GetEnumerator()
        {
            foreach (ReformValue f in PossibleStatuses)
                yield return f;
        }

        public override bool isAvailable(Country country)
        {
            if (country.Invented(Invention.Welfare))
                return true;
            else
                return false;
        }

        public override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }

    public class TaxationForPoor : AbstractReform
    {
        public class ReformValue : AbstractReformStepValue
        {
            public Procent tax;

            public ReformValue(string name, string description, Procent tarrif, int ID, DoubleConditionsList condition) : base(name, description, ID, condition, 11)
            {
                tax = tarrif;
                var totalSteps = 11;
                var previousID = ID - 1;
                var nextID = ID + 1;
                if (previousID >= 0 && nextID < totalSteps)
                    condition.add(new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(previousID)
                    || (x as Country).taxationForPoor.isThatReformEnacted(nextID), "Previous reform enacted", true));
                else
                if (nextID < totalSteps)
                    condition.add(new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(nextID), "Previous reform enacted", true));
                else
                if (previousID >= 0)
                    condition.add(new Condition(x => (x as Country).taxationForPoor.isThatReformEnacted(previousID), "Previous reform enacted", true));
            }

            public override string ToString()
            {
                return tax + base.ToString();
            }

            public override bool isAvailable(Country country)
            {
                //if (ID == 2 && !country.isInvented(InventionType.collectivism))
                //    return false;
                //else
                return true;
            }

            protected override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                //positive mean higher tax
                int change = ID - pop.Country.taxationForPoor.status.ID;
                if (pop.Type.isPoorStrata())
                {
                    if (change > 0)
                        result = new Procent(0f);
                    else
                        result = new Procent(1f);
                }
                else
                {
                    result = new Procent(0.5f);
                }
                return result;
            }
        }

        private ReformValue status;
        public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();// { NaturalEconomy, StateCapitalism, PlannedEconomy };

        static TaxationForPoor()
        {
            for (int i = 0; i <= 10; i++)
                PossibleStatuses.Add(new ReformValue(" tax for poor", "", new Procent(i * 0.1f), i, new DoubleConditionsList(new List<Condition> { Economy.isNotPlanned, Economy.taxesInsideLFLimit, Economy.taxesInsideSCLimit })));
        }

        public TaxationForPoor(Country country) : base("Taxation for poor", "", country)
        {
            status = PossibleStatuses[1];
        }

        public bool isThatReformEnacted(int value)
        {
            return status == PossibleStatuses[value];
        }

        public override AbstractReformValue getValue()
        {
            return status;
        }

        public ReformValue getTypedValue()
        {
            return status;
        }

        public override bool canChange()
        {
            return true;
        }

        public override IEnumerator GetEnumerator()
        {            
            foreach (ReformValue f in PossibleStatuses)
                yield return f;
        }

        public override void setValue(AbstractReformValue selectedReform)
        {
            base.setValue(selectedReform);
            status = (ReformValue)selectedReform;
        }

        public override bool isAvailable(Country country)
        {
            return true;
        }

        public override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }

    public class TaxationForRich : AbstractReform//, ICopyable<TaxationForRich>
    {
        public class ReformValue : AbstractReformStepValue
        {
            public Procent tax;

            public ReformValue(string inname, string indescription, Procent intarrif, int idin, DoubleConditionsList condition) : base(inname, indescription, idin, condition, 11)
            {
                tax = intarrif;
                var totalSteps = 11;
                var previousID = ID - 1;
                var nextID = ID + 1;
                if (previousID >= 0 && nextID < totalSteps)
                    condition.add(new Condition(x => (x as Country).taxationForRich.isThatReformEnacted(previousID)
                    || (x as Country).taxationForRich.isThatReformEnacted(nextID), "Previous reform enacted", true));
                else
                if (nextID < totalSteps)
                    condition.add(new Condition(x => (x as Country).taxationForRich.isThatReformEnacted(nextID), "Previous reform enacted", true));
                else
                if (previousID >= 0)
                    condition.add(new Condition(x => (x as Country).taxationForRich.isThatReformEnacted(previousID), "Previous reform enacted", true));
            }

            public override string ToString()
            {
                return tax + base.ToString();
            }

            public override bool isAvailable(Country country)
            {
                //if (ID == 2 && !country.isInvented(InventionType.collectivism))
                //    return false;
                //else
                return true;
            }

            protected override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                int change = ID - pop.Country.taxationForRich.status.ID;//positive mean higher tax
                if (pop.Type.isRichStrata())
                {
                    if (change > 0)
                        result = new Procent(0f);
                    else
                        result = new Procent(1f);
                }
                else
                {
                    if (change > 0)
                        if (tax.get() > 0.6f)
                            result = new Procent(0.4f);
                        else
                            result = new Procent(0.5f);
                    else
                        result = new Procent(0.0f);
                }
                return result;
            }
        }

        private ReformValue status;
        public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();// { NaturalEconomy, StateCapitalism, PlannedEconomy };

        static TaxationForRich()
        {
            for (int i = 0; i <= 10; i++)
                PossibleStatuses.Add(new ReformValue(" tax for rich", "", new Procent(i * 0.1f), i, new DoubleConditionsList(new List<Condition> { Economy.isNotPlanned, Economy.taxesInsideLFLimit, Economy.taxesInsideSCLimit })));
        }

        public TaxationForRich(Country country) : base("Taxation for rich", "", country)
        {
            status = PossibleStatuses[1];
        }

        public bool isThatReformEnacted(int value)
        {
            return status == PossibleStatuses[value];
        }

        public override AbstractReformValue getValue()
        {
            return status;
        }

        public ReformValue getTypedValue()
        {
            return status;
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

        public override void setValue(AbstractReformValue selectedReform)
        {
            base.setValue(selectedReform);
            status = (ReformValue)selectedReform;
        }

        public override bool isAvailable(Country country)
        {
            return true;
        }

        public override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }

        //public TaxationForRich Copy()
        //{
        //    return new TaxationForRich(this);
        //}
    }

    public class MinorityPolicy : AbstractReform
    {
        public class ReformValue : AbstractReformValue
        {
            public ReformValue(string inname, string indescription, int idin, DoubleConditionsList condition) : base(inname, indescription, idin, condition)
            {
                PossibleStatuses.Add(this);
            }

            public override bool isAvailable(Country country)
            {
                ReformValue requested = this;
                if ((requested.ID == 4) && country.Invented(Invention.Collectivism) && (country.serfdom.getValue().ID == 0 || country.serfdom.getValue().ID == 1 || country.serfdom.getValue().ID == 4))
                    return true;
                else
                if ((requested.ID == 3) && country.Invented(Invention.Banking) && (country.serfdom.getValue().ID == 0 || country.serfdom.getValue().ID == 1 || country.serfdom.getValue().ID == 3))
                    return true;
                else
                if ((requested.ID == 2) && (country.serfdom.getValue().ID == 0 || country.serfdom.getValue().ID == 1 || country.serfdom.getValue().ID == 2))
                    return true;
                else
                    if ((requested.ID == 1) && (country.serfdom.getValue().ID == 0 || country.serfdom.getValue().ID == 1))
                    return true;
                else
                if ((requested.ID == 0))
                    return true;
                else
                    return false;
            }

            protected override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                if (pop.isStateCulture())
                {
                    result = new Procent(0f);//0.5f);
                }
                else
                {
                    //positive - more rights for minorities
                    int change = ID - pop.Country.minorityPolicy.status.ID;
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                    if (change > 0)
                        result = new Procent(0.3f);// 1f);
                    else
                        //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                        result = new Procent(0f);
                }
                return result;
            }
        }

        private ReformValue status;
        public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();
        public static ReformValue Equality; // all can vote
        public static ReformValue Residency; // state culture only can vote
        public static readonly ReformValue NoRights = new ReformValue("No Rights for Minorities", "-Slavery?", 0, new DoubleConditionsList(Condition.IsNotImplemented));

        //public readonly static Condition isEquality = new Condition(x => (x as Country).minorityPolicy.getValue() == MinorityPolicy.Equality, "Minority policy is " + MinorityPolicy.Equality.getName(), true);
        //public static Condition IsResidencyPop;
        public MinorityPolicy(Country country) : base("Minority Policy", "- Minority Policy", country)
        {
            if (Equality == null)
                Equality = new ReformValue("Equality for Minorities", "- All cultures have same rights, assimilation is slower.", 2,
                    new DoubleConditionsList(new List<Condition> { Invention.IndividualRightsInvented }));
            if (Residency == null)
                Residency = new ReformValue("Restricted Rights for Minorities", "- Only state culture can vote, assimilation occurs except foreign core provinces", 1, new DoubleConditionsList());

            status = Residency;
            //IsResidencyPop = new Condition(x => (x as PopUnit).province.getOwner().minorityPolicy.status == MinorityPolicy.Residency,
            //Residency.FullName, true);
        }

        public override AbstractReformValue getValue()
        {
            return status;
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

        public override void setValue(AbstractReformValue selectedReform)
        {
            base.setValue(selectedReform);
            status = (ReformValue)selectedReform;
        }

        public override bool isAvailable(Country country)
        {
            return true;
        }

        //public static Condition IsResidency = new Condition(x => (x as Country).minorityPolicy.status == MinorityPolicy.Residency,
        //    Residency.FullName, true);

        //public static Condition IsEquality = new Condition(x => (x as Country).minorityPolicy.status == MinorityPolicy.Equality,
        //    Equality.FullName, true);
        public override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }
}