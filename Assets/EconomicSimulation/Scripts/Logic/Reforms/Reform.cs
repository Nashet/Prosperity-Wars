using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Nashet.Conditions;
using Nashet.ValueSpace;
using Nashet.Utils;
using Nashet.UnityUIUtils;
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
    abstract public class AbstractReformStepValue : AbstractReformValue
    {
        //private readonly int totalSteps;
        protected AbstractReformStepValue(string name, string indescription, int ID, DoubleConditionsList condition, int totalSteps)
            : base(name, indescription, ID, condition)
        {

        }

    }
    abstract public class AbstractReformValue : Name
    {
        public readonly static DoubleCondition isNotLFOrMoreConservative = new DoubleCondition((country, newReform) => (country as Country).economy.getValue() != Economy.LaissezFaire
        || (newReform as AbstractReformValue).isMoreConservative(
            (country as Country).getReform((newReform as AbstractReformValue)).getValue()
            ), x => "Economy policy is not Laissez Faire or that is reform rollback", true);
        readonly string description;
        public readonly int ID;
        public readonly DoubleConditionsList allowed;
        readonly public Condition isEnacted;// = new Condition(x => !(x as Country).reforms.isEnacted(this), "Reform is not enacted yet", true);

        abstract internal bool isAvailable(Country country);
        abstract protected Procent howIsItGoodForPop(PopUnit pop);
        static AbstractReformValue()
        {
            //allowed.add();
        }
        protected AbstractReformValue(string name, string indescription, int ID, DoubleConditionsList condition) : base(name)
        {
            this.ID = ID;
            description = indescription;
            this.allowed = condition;
            isEnacted = new Condition(x => !(x as Country).reforms.isEnacted(this), "Reform is not enacted yet", false);
            allowed.add(isEnacted);
            wantsReform = new Modifier(x => this.howIsItGoodForPop(x as PopUnit).get(),
                        "How much is it good for population", 1f, true);
            loyalty = new Modifier(x => this.loyaltyBoostFor(x as PopUnit),
                        "Loyalty", 1f, false);
            modVoting = new ModifiersList(new List<Condition>{
        wantsReform, loyalty, education
        });
        }
        public bool isMoreConservative(AbstractReformValue anotherReform)
        {
            return this.ID < anotherReform.ID;
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
        readonly string description;

        protected AbstractReform(string name, string indescription, Country country) : base(name)
        {
            description = indescription;
            country.reforms.Add(this);
        }

        internal abstract bool isAvailable(Country country);
        public abstract IEnumerator GetEnumerator();
        internal abstract bool canChange();
        internal abstract void setValue(AbstractReformValue selectedReformValue);

        override public string FullName
        {
            get { return description; }
        }

        abstract internal AbstractReformValue getValue();
        abstract internal bool canHaveValue(AbstractReformValue abstractReformValue);

        //abstract internal AbstractReformValue getValue(int value);
        //abstract internal void setValue(int value);
        public void OnClicked()
        {
            MainCamera.politicsPanel.selectReform(this);
            MainCamera.politicsPanel.Refresh();
        }
    }
    public class Government : AbstractReform, IHasCountry
    {
        readonly internal static List<ReformValue> PossibleStatuses = new List<ReformValue>();
        private ReformValue reform;
        private readonly Country country;
        public Country Country
        {
            get { return country; }
        }
        public class ReformValue : AbstractReformValue
        {
            readonly private int MaxiSizeLimitForDisloyaltyModifier;
            readonly private string prefix;
            readonly private float scienceModifier;

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
            internal override bool isAvailable(Country country)
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
                    result = new Procent(1f);
                else if (this == Government.ProletarianDictatorship)
                    result = new Procent(0.4f);
                else if (this == Government.Despotism && pop.needsFulfilled.get() < 0.1f)
                    result = new Procent(1f);
                else
                    result = new Procent(0f);
                return result;
            }
            internal string getPrefix()
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

        readonly internal static ReformValue Tribal = new ReformValue("Tribal democracy", "- Tribesmen and Aristocrats can vote", 0,
            new DoubleConditionsList(), "tribe", 10, 0f);

        readonly internal static ReformValue Aristocracy = new ReformValue("Aristocracy", "- Only Aristocrats and Clerics can vote", 1,
            new DoubleConditionsList(), "kingdom", 20, 0.5f);

        readonly internal static ReformValue Polis = new ReformValue("Polis", "- Landed individuals allowed to vote, such as Farmers, Aristocrats, Clerics; each vote is equal", 8,
            new DoubleConditionsList(), "polis", 5, 1f);

        readonly internal static ReformValue Despotism = new ReformValue("Despotism", "- Despot does what he wants", 2,
            new DoubleConditionsList(), "empire", 40, 0.25f);

        readonly internal static ReformValue Theocracy = new ReformValue("Theocracy", "- Only Clerics have power", 5,
            new DoubleConditionsList(Condition.IsNotImplemented), "", 40, 0f);

        readonly internal static ReformValue WealthDemocracy = new ReformValue("Wealth Democracy", "- Landed individuals allowed to vote, such as Farmers, Aristocrats, etc. Rich classes has more votes (5 to 1)", 9,
            new DoubleConditionsList(Condition.IsNotImplemented), "states", 40, 1f);

        readonly internal static ReformValue Democracy = new ReformValue("Universal Democracy", "- Everyone can vote; each vote is equal", 3,
            new DoubleConditionsList(new List<Condition> { Invention.IndividualRightsInvented }), "republic", 100, 1f);

        readonly internal static ReformValue BourgeoisDictatorship = new ReformValue("Bourgeois dictatorship", "- Only capitalists have power", 6,
            new DoubleConditionsList(new List<Condition> { Invention.IndividualRightsInvented }), "", 20, 1f);

        readonly internal static ReformValue Junta = new ReformValue("Junta", "- Only military guys have power", 7,
            new DoubleConditionsList(new List<Condition> { Invention.ProfessionalArmyInvented }), "junta", 20, 0.3f);

        readonly internal static ReformValue ProletarianDictatorship = new ReformValue("Proletarian dictatorship", "- ProletarianDictatorship is it. Bureaucrats rule you", 4,
            new DoubleConditionsList(new List<Condition> { Invention.CollectivismInvented, Invention.ManufacturesInvented }), "SSR", 20, 0.5f);

        internal readonly static Condition isPolis = new Condition(x => (x as Country).government.getValue() == Government.Polis, "Government is " + Government.Polis.ToString(), true);
        internal readonly static Condition isTribal = new Condition(x => (x as Country).government.getValue() == Government.Tribal, "Government is " + Government.Tribal.ToString(), true);
        internal readonly static Condition isAristocracy = new Condition(x => (x as Country).government.getValue() == Government.Aristocracy, "Government is " + Government.Aristocracy.ToString(), true);

        internal readonly static Condition isDespotism = new Condition(x => (x as Country).government.getValue() == Government.Despotism, "Government is " + Government.Despotism.ToString(), true);
        internal readonly static Condition isTheocracy = new Condition(x => (x as Country).government.getValue() == Government.Theocracy, "Government is " + Government.Theocracy.ToString(), true);
        internal readonly static Condition isWealthDemocracy = new Condition(x => (x as Country).government.getValue() == Government.WealthDemocracy, "Government is " + Government.WealthDemocracy.ToString(), true);
        internal readonly static Condition isDemocracy = new Condition(x => (x as Country).government.getValue() == Government.Democracy, "Government is " + Government.Democracy.ToString(), true);
        internal readonly static Condition isBourgeoisDictatorship = new Condition(x => (x as Country).government.getValue() == Government.BourgeoisDictatorship, "Government is " + Government.BourgeoisDictatorship.ToString(), true);
        internal readonly static Condition isJunta = new Condition(x => (x as Country).government.getValue() == Government.Junta, "Government is " + Government.Junta.ToString(), true);
        internal readonly static Condition isProletarianDictatorship = new Condition(x => (x as Country).government.getValue() == Government.ProletarianDictatorship, "Government is " + Government.ProletarianDictatorship.ToString(), true);

        public Government(Country country) : base("Government", "Form of government", country)
        {
            reform = Aristocracy;
            this.country = country;
        }
        internal string getPrefix()
        {
            return reform.getPrefix();
        }
        internal override AbstractReformValue getValue()
        {
            return reform;
        }
        internal Government.ReformValue getTypedValue()
        {
            return reform;
        }
        //internal override AbstractReformValue getValue(int value)
        //{
        //    return PossibleStatuses[value];
        //}
        internal override bool canChange()
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
            if (government == Government.ProletarianDictatorship)
            {
                country.setSoldierWage(0f);

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
                foreach (var item in province.getAllPopUnits())
                {
                    item.PayAllAvailableMoney(country);
                    item.loans.SetZero();
                    item.deposits.SetZero();
                }
            }
        }
        internal override void setValue(AbstractReformValue selectedReform)
        {
            reform = (ReformValue)selectedReform;
            country.setPrefix();

            if (reform == Government.Tribal)
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
            if (reform == Government.Aristocracy)
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
            if (reform == Government.Polis)
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
            if (reform == Government.Despotism)
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
        if (reform == Government.Theocracy)
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
            if (reform == Government.WealthDemocracy)
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
            if (reform == Government.Democracy)
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
        if (reform == Government.BourgeoisDictatorship)
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
        if (reform == Government.Junta)
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
        if (reform == Government.ProletarianDictatorship)
            {
                country.economy.setValue(Economy.PlannedEconomy);
                //country.serfdom.setValue(Serfdom.AbolishedAndNationalizated);
                country.minimalWage.setValue(MinimalWage.None);
                country.unemploymentSubsidies.setValue(UnemploymentSubsidies.None);
                country.minorityPolicy.setValue(MinorityPolicy.Equality);
                country.taxationForPoor.setValue(TaxationForPoor.PossibleStatuses[5]);
                country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[10]);
                //nationalization
                country.getBank().PayAllAvailableMoney(country);
                country.getBank().getGivenLoans().SetZero();
                country.loans.SetZero();
                country.deposits.SetZero();

                foreach (var province in country.ownedProvinces)
                {
                    onReformEnacted(province);
                }
            }
            if (country == Game.Player)
                MainCamera.refreshAllActive();
        }
        //internal void setValue(AbstractReformValue selectedReform, bool setPrefix)
        //{
        //    setValue(selectedReform);
        //    if (setPrefix)
        //        country.setPrefix();
        //}

        internal override bool isAvailable(Country country)
        {
            return true;
        }
        internal override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }
    public class Economy : AbstractReform, IHasCountry
    {
        private readonly Country country;


        internal readonly static Condition isNotLF = new Condition(delegate (object forWhom) { return (forWhom as Country).economy.status != Economy.LaissezFaire; }, "Economy policy is not Laissez Faire", true);
        internal readonly static Condition isLF = new Condition(delegate (object forWhom) { return (forWhom as Country).economy.status == Economy.LaissezFaire; }, "Economy policy is Laissez Faire", true);

        internal readonly static Condition isNotNatural = new Condition(x => (x as Country).economy.status != Economy.NaturalEconomy, "Economy policy is not Natural Economy", true);
        internal readonly static Condition isNatural = new Condition(x => (x as Country).economy.status == Economy.NaturalEconomy, "Economy policy is Natural Economy", true);

        internal readonly static Condition isNotState = new Condition(x => (x as Country).economy.status != Economy.StateCapitalism, "Economy policy is not State Capitalism", true);
        internal readonly static Condition isStateCapitlism = new Condition(x => (x as Country).economy.status == Economy.StateCapitalism, "Economy policy is State Capitalism", true);

        internal readonly static Condition isNotInterventionism = new Condition(x => (x as Country).economy.status != Economy.Interventionism, "Economy policy is not Limited Interventionism", true);
        internal readonly static Condition isInterventionism = new Condition(x => (x as Country).economy.status == Economy.Interventionism, "Economy policy is Limited Interventionism", true);

        internal readonly static Condition isNotPlanned = new Condition(x => (x as Country).economy.status != Economy.PlannedEconomy, "Economy policy is not Planned Economy", true);
        internal readonly static Condition isPlanned = new Condition(x => (x as Country).economy.status == Economy.PlannedEconomy, "Economy policy is Planned Economy", true);

        internal readonly static DoubleCondition taxesInsideLFLimit = new DoubleCondition(
        delegate (object x, object y)
        {
            //if it's poor taxes
            var taxesForPoor = y as TaxationForPoor.ReformValue;
            if (taxesForPoor != null)
                return (x as Country).economy.status != Economy.LaissezFaire || taxesForPoor.tax.get() <= 0.5f;
            else
            {
                var taxesForRich = y as TaxationForRich.ReformValue;
                return (x as Country).economy.status != Economy.LaissezFaire || taxesForRich.tax.get() <= 0.5f;
            }
        },
            x => "Economy policy is Laissez Faire and tax is not higher than 50%", false);
        internal readonly static DoubleCondition taxesInsideSCLimit = new DoubleCondition(
        delegate (object x, object y)
        {
            //if it's poor taxes
            var taxesForPoor = y as TaxationForPoor.ReformValue;
            if (taxesForPoor != null)
                return (x as Country).economy.status != Economy.StateCapitalism || taxesForPoor.tax.get() >= 0.2f;
            else
            {
                var taxesForRich = y as TaxationForRich.ReformValue;
                return (x as Country).economy.status != Economy.StateCapitalism || taxesForRich.tax.get() >= 0.2f;
            }
        },
            x => "Economy policy is State capitalism and tax is not lower than 20%", false);

        internal static Condition isNotMarket = new Condition(x => (x as Country).economy.status == Economy.NaturalEconomy || (x as Country).economy.status == Economy.PlannedEconomy,
          "Economy is not market economy", true);
        internal static Condition isMarket = new Condition(x => (x as Country).economy.status == Economy.StateCapitalism || (x as Country).economy.status == Economy.Interventionism
            || (x as Country).economy.status == Economy.LaissezFaire
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
                this._allowForeignIvestments = _allowForeighnIvestments;
            }

            internal override bool isAvailable(Country country)
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
                    if (this == Economy.PlannedEconomy)
                        result = new Procent(0f); // that can be achieved only by government reform
                    else if (this == Economy.LaissezFaire)
                    {
                        if (this == Economy.Interventionism)
                            result = new Procent(0.3f);
                        else
                            result = new Procent(0.7f);
                    }
                    else if (this == Economy.Interventionism)
                    {
                        if (this == Economy.LaissezFaire)
                            result = new Procent(0.3f);
                        else
                            result = new Procent(0.8f);
                    }
                    else
                        result = new Procent(0.5f);
                }
                return result;
            }
        }
        static readonly ConditionsList capitalism = new ConditionsList(new List<Condition>()
        {
            Invention.IndividualRightsInvented,
            Invention.BankingInvented,
            Serfdom.IsAbolishedInAnyWay
        });


        private ReformValue status;

        internal static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();
        internal static readonly ReformValue PlannedEconomy = new ReformValue("Planned economy", "", 0, false,
            new DoubleConditionsList(new List<Condition> {
            Invention.CollectivismInvented, Government.isProletarianDictatorship }));
        internal static readonly ReformValue NaturalEconomy = new ReformValue("Natural economy", " ", 1, false, new DoubleConditionsList(Condition.IsNotImplemented));//new ConditionsList(Condition.AlwaysYes)); 
        internal static readonly ReformValue StateCapitalism = new ReformValue("State capitalism", "", 2, false, new DoubleConditionsList(capitalism));
        internal static readonly ReformValue Interventionism = new ReformValue("Limited interventionism", "", 3, true, new DoubleConditionsList(capitalism));
        internal static readonly ReformValue LaissezFaire = new ReformValue("Laissez faire", "", 4, true, new DoubleConditionsList(capitalism));
        /// ////////////
        public Economy(Country country) : base("Economy", "Your economy policy", country)
        {
            status = NaturalEconomy;
            this.country = country;
        }
        internal override AbstractReformValue getValue()
        {
            return status;
        }
        public Country Country
        {
            get { return country; }
        }
        // todo add OnReformEnacted?
        internal override void setValue(AbstractReformValue selectedReform)
        {
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
        internal Economy.ReformValue getTypedValue()
        {
            return status;
        }
        //internal override AbstractReformValue getValue(int value)
        //{
        //    return PossibleStatuses[value];
        //}
        internal override bool canChange()
        {
            return true;
        }
        public override IEnumerator GetEnumerator()
        {
            foreach (ReformValue f in PossibleStatuses)
                yield return f;
        }

        internal override bool isAvailable(Country country)
        {
            return true;
        }
        internal override bool canHaveValue(AbstractReformValue abstractReformValue)
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
            internal override bool isAvailable(Country country)
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

            static Procent br = new Procent(0.2f);
            static Procent al = new Procent(0.1f);
            static Procent nu = new Procent(0.0f);
            internal Procent getTax()
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
        internal static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { Allowed, Brutal, Abolished, AbolishedWithLandPayment, AbolishedAndNationalizated };
        internal static ReformValue Allowed;
        internal static ReformValue Brutal;
        internal static ReformValue Abolished = new ReformValue("Abolished", "- Abolished with no obligations", 2,
            new DoubleConditionsList(new List<Condition>() { Invention.IndividualRightsInvented, Condition.IsNotImplemented }));
        internal static ReformValue AbolishedWithLandPayment = new ReformValue("Abolished with land payment", "- Peasants are personally free now but they have to pay debt for land", 3,
            new DoubleConditionsList(new List<Condition>()
            {
            Invention.IndividualRightsInvented,Invention.BankingInvented, Condition.IsNotImplemented
            }));
        internal static ReformValue AbolishedAndNationalized = new ReformValue("Abolished and nationalized land", "- Aristocrats loose property", 4,
            new DoubleConditionsList(new List<Condition>()
            {
            Government.isProletarianDictatorship, Condition.IsNotImplemented
            }));
        public Serfdom(Country country) : base("Serfdom", "- Aristocrats privileges", country)
        {
            if (Allowed == null)
                Allowed = new ReformValue("Allowed", "- Peasants and other plebes pay 10% of income to Aristocrats", 1,
                    new DoubleConditionsList(new List<Condition>()
                    {
            Economy.isNotMarket,  Condition.IsNotImplemented
                    }));
            if (Brutal == null)
                Brutal = new ReformValue("Brutal", "- Peasants and other plebes pay 20% of income to Aristocrats", 0,
                new DoubleConditionsList(new List<Condition>()
                {
            Economy.isNotMarket, Condition.IsNotImplemented
                }));

            status = Allowed;
        }
        internal override AbstractReformValue getValue()
        {
            return status;
        }
        //internal override AbstractReformValue getValue(int value)
        //{
        //    //return PossibleStatuses.Find(x => x.ID == value);
        //    return PossibleStatuses[value];
        //}
        internal override bool canChange()
        {
            return true;
        }
        public override IEnumerator GetEnumerator()
        {
            foreach (ReformValue f in PossibleStatuses)
                yield return f;
        }

        internal override void setValue(AbstractReformValue selectedReform)
        {
            status = (ReformValue)selectedReform;
        }
        internal override bool isAvailable(Country country)
        {
            return true;
        }
        internal static Condition IsAbolishedInAnyWay = new Condition(x => (x as Country).serfdom.status == Serfdom.Abolished
        || (x as Country).serfdom.status == Serfdom.AbolishedAndNationalized || (x as Country).serfdom.status == Serfdom.AbolishedWithLandPayment,
            "Serfdom is abolished", true);
        internal static Condition IsNotAbolishedInAnyWay = new Condition(x => (x as Country).serfdom.status == Serfdom.Allowed
        || (x as Country).serfdom.status == Serfdom.Brutal,
            "Serfdom is in power", true);
        internal override bool canHaveValue(AbstractReformValue abstractReformValue)
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
            internal override bool isAvailable(Country country)
            {
                return true;
            }

            /// <summary>
            /// Calculates wage basing on consumption cost for 1000 workers
            /// Returns new value
            /// </summary>        
            internal Money getWage()
            {
                if (this == None)
                    return new Money(0f);
                else if (this == Scanty)
                {
                    Money result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    //result.multipleInside(0.5f);
                    return result;
                }
                else if (this == Minimal)
                {
                    Money result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    Money everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men());
                    everyDayCost.Multiply(0.02f);
                    result.Add(everyDayCost);
                    return result;
                }
                else if (this == Trinket)
                {
                    Money result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    Money everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men());
                    everyDayCost.Multiply(0.04f);
                    result.Add(everyDayCost);
                    return result;
                }
                else if (this == Middle)
                {
                    Money result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    Money everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men());
                    everyDayCost.Multiply(0.06f);
                    result.Add(everyDayCost);
                    return result;
                }
                else if (this == Big)
                {
                    Money result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    Money everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men());
                    everyDayCost.Multiply(0.08f);
                    //Value luxuryCost = Game.market.getCost(PopType.workers.luxuryNeedsPer1000);
                    result.Add(everyDayCost);
                    //result.add(luxuryCost);
                    return result;
                }
                else
                    return new Money(0f);
            }
            override public string ToString()
            {
                return base.ToString() + " (" + getWage() + ")";
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

        internal readonly static List<ReformValue> PossibleStatuses = new List<ReformValue>();
        internal readonly static ReformValue None = new ReformValue("No minimal wage", "", 0, new DoubleConditionsList(new List<Condition> { AbstractReformValue.isNotLFOrMoreConservative }));

        internal readonly static ReformValue Scanty = new ReformValue("Scanty minimal wage", "- Half-hungry", 1, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned,
        }));
        internal readonly static ReformValue Minimal = new ReformValue("Tiny minimal wage", "- Just enough to feed yourself", 2, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned,
        }));
        internal readonly static ReformValue Trinket = new ReformValue("Trinket minimal wage", "- You can buy some small stuff", 3, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned,
        }));
        internal readonly static ReformValue Middle = new ReformValue("Middle minimal wage", "- Plenty good wage", 4, new DoubleConditionsList(new List<Condition>
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned,
        }));
        internal readonly static ReformValue Big = new ReformValue("Generous minimal wage", "- Can live almost like a king. Almost..", 5, new DoubleConditionsList(new List<Condition>()
        {
            Invention.WelfareInvented,AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned,
        }));

        public MinimalWage(Country country) : base("Minimal wage", "", country)
        {
            status = None;
        }
        internal bool isThatReformEnacted(int value)
        {
            return status == PossibleStatuses[value];
        }
        internal override AbstractReformValue getValue()
        {
            return status;
        }
        //internal override AbstractReformValue getValue(int value)
        //{
        //    return PossibleStatuses.Find(x => x.ID == value);
        //    //return PossibleStatuses[value];
        //}
        internal override bool canChange()
        {
            return true;
        }
        public override IEnumerator GetEnumerator()
        {
            foreach (ReformValue f in PossibleStatuses)
                yield return f;
        }
        internal override void setValue(AbstractReformValue selectedReform)
        {
            status = (ReformValue)selectedReform;
        }
        internal override bool isAvailable(Country country)
        {
            if (country.Invented(Invention.Welfare))
                return true;
            else
                return false;
        }
        internal override bool canHaveValue(AbstractReformValue abstractReformValue)
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
            internal override bool isAvailable(Country country)
            {
                return true;
            }

            /// <summary>
            /// Calculates Unemployment Subsidies basing on consumption cost for 1000 workers
            /// </summary>        
            internal float getSubsidiesRate()
            {
                if (this == None)
                    return 0f;
                else if (this == Scanty)
                {
                    Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    //result.multipleInside(0.5f);
                    return result.get();
                }
                else if (this == Minimal)
                {
                    Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    Value everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men());
                    everyDayCost.Multiply(0.02f);
                    result.Add(everyDayCost);
                    return result.get();
                }
                else if (this == Trinket)
                {
                    Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    Value everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men());
                    everyDayCost.Multiply(0.04f);
                    result.Add(everyDayCost);
                    return result.get();
                }
                else if (this == Middle)
                {
                    Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    Value everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men());
                    everyDayCost.Multiply(0.06f);
                    result.Add(everyDayCost);
                    return result.get();
                }
                else if (this == Big)
                {
                    Value result = Game.market.getCost(PopType.Workers.getLifeNeedsPer1000Men());
                    Value everyDayCost = Game.market.getCost(PopType.Workers.getEveryDayNeedsPer1000Men());
                    everyDayCost.Multiply(0.08f);
                    //Value luxuryCost = Game.market.getCost(PopType.workers.luxuryNeedsPer1000);
                    result.Add(everyDayCost);
                    //result.add(luxuryCost);
                    return result.get();
                }
                else
                    return 0f;
            }
            override public string ToString()
            {
                return base.ToString() + " (" + getSubsidiesRate() + ")";
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
        internal readonly static List<ReformValue> PossibleStatuses = new List<ReformValue>();
        internal readonly static ReformValue None = new ReformValue("No unemployment subsidies", "", 0, new DoubleConditionsList(new List<Condition>()));
        internal readonly static ReformValue Scanty = new ReformValue("Scanty unemployment subsidies", "- Half-hungry", 1, new DoubleConditionsList(new List<Condition>()
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned,
        }));
        internal readonly static ReformValue Minimal = new ReformValue("Minimal unemployment subsidies", "- Just enough to feed yourself", 2, new DoubleConditionsList(new List<Condition>()
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned,
        }));
        internal readonly static ReformValue Trinket = new ReformValue("Trinket unemployment subsidies", "- You can buy some small stuff", 3, new DoubleConditionsList(new List<Condition>()
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned,
        }));
        internal readonly static ReformValue Middle = new ReformValue("Middle unemployment subsidies", "- Plenty good subsidies", 4, new DoubleConditionsList(new List<Condition>()
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned,
        }));
        internal readonly static ReformValue Big = new ReformValue("Generous unemployment subsidies", "- Can live almost like a king. Almost..", 5, new DoubleConditionsList(new List<Condition>()
        {
            Invention.WelfareInvented, AbstractReformValue.isNotLFOrMoreConservative, Economy.isNotPlanned,
        }));


        public UnemploymentSubsidies(Country country) : base("Unemployment Subsidies", "", country)
        {
            status = None;
        }
        internal bool isThatReformEnacted(int value)
        {
            return status == PossibleStatuses[value];
        }
        internal override AbstractReformValue getValue()
        {
            return status;
        }
        //internal override AbstractReformValue getValue(int value)
        //{
        //    return PossibleStatuses.Find(x => x.ID == value);
        //    //return PossibleStatuses[value];
        //}
        internal override void setValue(AbstractReformValue selectedReform)
        {
            status = (ReformValue)selectedReform;
        }
        internal override bool canChange()
        {
            return true;
        }
        public override IEnumerator GetEnumerator()
        {
            foreach (ReformValue f in PossibleStatuses)
                yield return f;
        }

        internal override bool isAvailable(Country country)
        {
            if (country.Invented(Invention.Welfare))
                return true;
            else
                return false;
        }

        internal override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }


    public class TaxationForPoor : AbstractReform
    {
        public class ReformValue : AbstractReformStepValue
        {
            internal Procent tax;
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

            override public string ToString()
            {
                return tax.ToString() + base.ToString();
            }
            internal override bool isAvailable(Country country)
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
        internal readonly static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { NaturalEconomy, StateCapitalism, PlannedEconomy };
        static TaxationForPoor()
        {
            for (int i = 0; i <= 10; i++)
                PossibleStatuses.Add(new ReformValue(" tax for poor", "", new Procent(i * 0.1f), i, new DoubleConditionsList(new List<Condition> { Economy.isNotPlanned, Economy.taxesInsideLFLimit, Economy.taxesInsideSCLimit })));
        }
        public TaxationForPoor(Country country) : base("Taxation for poor", "", country)
        {
            status = PossibleStatuses[1];
        }
        internal bool isThatReformEnacted(int value)
        {
            return status == PossibleStatuses[value];
        }
        internal override AbstractReformValue getValue()
        {
            return status;
        }
        internal TaxationForPoor.ReformValue getTypedValue()
        {
            return status;
        }
        internal override bool canChange()
        {
            return true;
        }
        public override IEnumerator GetEnumerator()
        {
            foreach (ReformValue f in PossibleStatuses)
                yield return f;
        }
        internal override void setValue(AbstractReformValue selectedReform)
        {
            status = (ReformValue)selectedReform;
        }
        internal override bool isAvailable(Country country)
        {
            return true;
        }
        internal override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }

    public class TaxationForRich : AbstractReform//, ICopyable<TaxationForRich>
    {
        public class ReformValue : AbstractReformStepValue
        {
            internal Procent tax;
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
            override public string ToString()
            {
                return tax.ToString() + base.ToString();
            }
            internal override bool isAvailable(Country country)
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
                        result = new Procent(0.8f);
                    else
                        result = new Procent(0f);
                }
                return result;
            }
        }
        private ReformValue status;
        internal readonly static List<ReformValue> PossibleStatuses = new List<ReformValue>();// { NaturalEconomy, StateCapitalism, PlannedEconomy };
        static TaxationForRich()
        {
            for (int i = 0; i <= 10; i++)
                PossibleStatuses.Add(new ReformValue(" tax for rich", "", new Procent(i * 0.1f), i, new DoubleConditionsList(new List<Condition> { Economy.isNotPlanned, Economy.taxesInsideLFLimit, Economy.taxesInsideSCLimit })));
        }
        public TaxationForRich(Country country) : base("Taxation for rich", "", country)
        {
            status = PossibleStatuses[1];
        }

        internal bool isThatReformEnacted(int value)
        {
            return status == PossibleStatuses[value];
        }
        internal override AbstractReformValue getValue()
        {
            return status;
        }
        internal TaxationForRich.ReformValue getTypedValue()
        {
            return status;
        }
        //internal override AbstractReformValue getValue(int value)
        //{
        //    return PossibleStatuses[value];
        //}
        internal override bool canChange()
        {
            return true;
        }
        public override IEnumerator GetEnumerator()
        {
            foreach (ReformValue f in PossibleStatuses)
                yield return f;
        }
        internal override void setValue(AbstractReformValue selectedReform)
        {
            status = (ReformValue)selectedReform;
        }
        internal override bool isAvailable(Country country)
        {
            return true;
        }
        internal override bool canHaveValue(AbstractReformValue abstractReformValue)
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
            internal override bool isAvailable(Country country)
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
        readonly internal static List<ReformValue> PossibleStatuses = new List<ReformValue>();
        internal static ReformValue Equality; // all can vote
        internal static ReformValue Residency; // state culture only can vote    
        internal readonly static ReformValue NoRights = new ReformValue("No rights for minorities", "- Slavery?", 0, new DoubleConditionsList(Condition.IsNotImplemented));

        //internal readonly static Condition isEquality = new Condition(x => (x as Country).minorityPolicy.getValue() == MinorityPolicy.Equality, "Minority policy is " + MinorityPolicy.Equality.getName(), true);
        //internal static Condition IsResidencyPop;
        public MinorityPolicy(Country country) : base("Minority Policy", "- Minority Policy", country)
        {
            if (Equality == null)
                Equality = new ReformValue("Equality for minorities", "- All cultures have same rights, assimilation is slower", 2,
                    new DoubleConditionsList(new List<Condition>() { Invention.IndividualRightsInvented }));
            if (Residency == null)
                Residency = new ReformValue("Restricted rights for minorities", "- Only state culture can vote, assimilation is on except foreign core provinces", 1, new DoubleConditionsList());

            status = Residency;
            //IsResidencyPop = new Condition(x => (x as PopUnit).province.getOwner().minorityPolicy.status == MinorityPolicy.Residency,
            //Residency.FullName, true);
        }
        internal override AbstractReformValue getValue()
        {
            return status;
        }
        //internal override AbstractReformValue getValue(int value)
        //{
        //    return PossibleStatuses[value];
        //}
        internal override bool canChange()
        {
            return true;
        }
        public override IEnumerator GetEnumerator()
        {
            foreach (ReformValue f in PossibleStatuses)
                yield return f;
        }

        internal override void setValue(AbstractReformValue selectedReform)
        {
            status = (ReformValue)selectedReform;
        }
        internal override bool isAvailable(Country country)
        {
            return true;
        }

        //internal static Condition IsResidency = new Condition(x => (x as Country).minorityPolicy.status == MinorityPolicy.Residency,
        //    Residency.FullName, true);

        //internal static Condition IsEquality = new Condition(x => (x as Country).minorityPolicy.status == MinorityPolicy.Equality,
        //    Equality.FullName, true);
        internal override bool canHaveValue(AbstractReformValue abstractReformValue)
        {
            return PossibleStatuses.Contains(abstractReformValue as ReformValue);
        }
    }

}