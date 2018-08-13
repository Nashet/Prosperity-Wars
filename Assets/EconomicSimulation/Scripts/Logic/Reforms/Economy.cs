using System.Collections;
using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.Utils;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Economy : AbstractReform
    {
        protected EconomyReformValue typedValue;
        public static readonly EconomyReformValue PlannedEconomy = new EconomyReformValue("Planned economy", "", 0,
            new DoubleConditionsList(new List<Condition> {
            Invention.CollectivismInvented, Government.isProletarianDictatorship }), false);

        private static readonly ConditionsList capitalism = new ConditionsList(new List<Condition>
        {
            Invention.IndividualRightsInvented,
            Invention.BankingInvented,
            Serfdom.IsAbolishedInAnyWay
        });

        public static readonly EconomyReformValue NaturalEconomy = new EconomyReformValue("Natural economy", " ", 1, new DoubleConditionsList(Condition.IsNotImplemented), false);//new ConditionsList(Condition.AlwaysYes));
        public static readonly EconomyReformValue StateCapitalism = new EconomyReformValue("State capitalism", "", 2, new DoubleConditionsList(capitalism), false, null, new Procent(0.2f));
        public static readonly EconomyReformValue Interventionism = new EconomyReformValue("Limited interventionism", "", 3, new DoubleConditionsList(capitalism), true);
        public static readonly EconomyReformValue LaissezFaire = new EconomyReformValue("Laissez faire", "", 4, new DoubleConditionsList(capitalism), true, new Procent(0.5f));

        public static readonly DoubleCondition isNotLFOrMoreConservative = new DoubleCondition((country, newReform) => (country as Country).economy != Economy.LaissezFaire
        || (newReform as IReformValue).isMoreConservative((country as Country).economy), 
            x => "Economy policy is not Laissez Faire or that is reform rollback", true);

        public static readonly Condition isNotLF = new Condition(delegate (object forWhom) { return (forWhom as Country).economy != LaissezFaire; }, "Economy policy is not Laissez Faire", true);
        public static readonly Condition isLF = new Condition(delegate (object forWhom) { return (forWhom as Country).economy == LaissezFaire; }, "Economy policy is Laissez Faire", true);

        public static readonly Condition isNotNatural = new Condition(x => (x as Country).economy != NaturalEconomy, "Economy policy is not Natural Economy", true);
        public static readonly Condition isNatural = new Condition(x => (x as Country).economy == NaturalEconomy, "Economy policy is Natural Economy", true);

        public static readonly Condition isNotState = new Condition(x => (x as Country).economy != StateCapitalism, "Economy policy is not State Capitalism", true);
        public static readonly Condition isStateCapitlism = new Condition(x => (x as Country).economy == StateCapitalism, "Economy policy is State Capitalism", true);

        public static readonly Condition isNotInterventionism = new Condition(x => (x as Country).economy != Interventionism, "Economy policy is not Limited Interventionism", true);
        public static readonly Condition isInterventionism = new Condition(x => (x as Country).economy == Interventionism, "Economy policy is Limited Interventionism", true);

        public static readonly Condition isNotPlanned = new Condition(x => (x as Country).economy != PlannedEconomy, "Economy policy is not Planned Economy", true);
        public static readonly Condition isPlanned = new Condition(x => (x as Country).economy == PlannedEconomy, "Economy policy is Planned Economy", true);

        public static readonly DoubleCondition taxesInsideLFLimit = new DoubleCondition(
        delegate (object x, object y)
        {
            // todo return it
            //if it's poor taxes
            //var taxesForPoor = y as TaxationForPoor.ReformValue;
            //if (taxesForPoor != null)
            //    return (x as Country).economy != LaissezFaire || taxesForPoor.tax.get() <= 0.5f;
            //else
            {
                var taxesForRich = y as ProcentReform .ProcentReformVal;
                return (x as Country).economy != LaissezFaire || taxesForRich.get() <= 0.5f;
            }
        },
            x => "Economy policy is Laissez Faire and tax is not higher than 50%", false);

        public static readonly DoubleCondition taxesInsideSCLimit = new DoubleCondition(
        delegate (object x, object y)
        {
            // todo return it
            //if it's poor taxes
            //var taxesForPoor = y as TaxationForPoor.ReformValue;
            //if (taxesForPoor != null)
            //    return (x as Country).economy != StateCapitalism || taxesForPoor.tax.get() >= 0.2f;
            //else
            {
                var taxesForRich = y as ProcentReform.ProcentReformVal;
                return (x as Country).economy != StateCapitalism || taxesForRich.get() >= 0.2f;
            }
        },
            x => "Economy policy is State capitalism and tax is not lower than 20%", false);

        public static Condition isNotMarket = new Condition(x => (x as Country).economy == NaturalEconomy || (x as Country).economy == PlannedEconomy,
          "Economy is not market economy", true);

        public static Condition isMarket = new Condition(x => (x as Country).economy == StateCapitalism || (x as Country).economy == Interventionism
            || (x as Country).economy == LaissezFaire
            , "Economy is market economy", true);

        public Economy(Country country) : base("Economy", "Your economy policy", country, new List<IReformValue> { })
        {
        }

        public override void OnReformEnactedInProvince(Province province)
        {
            throw new System.NotImplementedException();
        }

       

        public void SetValue(EconomyReformValue reformValue)
        {
            base.SetValue(reformValue);
            typedValue = reformValue;
        }

        public bool AllowForeignInvestments
        {
            get { return typedValue.AllowForeignInvestments; }
        }
        public class EconomyReformValue : NamedReformValue
        {
            public Procent maxPoorTax;
            public Procent minPoorTax;
            public bool AllowForeignInvestments
            {
                get; protected set;
            }

            public EconomyReformValue(string name, string description, int id, DoubleConditionsList condition,
                bool allowForeighnIvestments, Procent maxPoorTax = null, Procent minPoorTax = null) : base(name, description, id, condition)
            {
                AllowForeignInvestments = allowForeighnIvestments;
                this.minPoorTax = minPoorTax;
                this.maxPoorTax = maxPoorTax;
            }
            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                //if (pop.Type == PopType.Capitalists)
                if (pop.Type.isRichStrata())
                {
                    //positive - more liberal
                    int change = ID - pop.Country.economy.value.ID;
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

            public override bool IsAllowed(object firstObject, object secondObject, out string description)
            {
                throw new System.NotImplementedException();
            }

            public override bool IsAllowed(object firstObject, object secondObject)
            {
                throw new System.NotImplementedException();
            }
        }
    }
    //public class Ecccconomy : AbstractReform, IHasCountry
    //{
    //    private readonly Country country;


    //    public static readonly Condition isNotLF = new Condition(delegate (object forWhom) { return (forWhom as Country).economy != LaissezFaire; }, "Economy policy is not Laissez Faire", true);
    //    public static readonly Condition isLF = new Condition(delegate (object forWhom) { return (forWhom as Country).economy == LaissezFaire; }, "Economy policy is Laissez Faire", true);

    //    public static readonly Condition isNotNatural = new Condition(x => (x as Country).economy != NaturalEconomy, "Economy policy is not Natural Economy", true);
    //    public static readonly Condition isNatural = new Condition(x => (x as Country).economy == NaturalEconomy, "Economy policy is Natural Economy", true);

    //    public static readonly Condition isNotState = new Condition(x => (x as Country).economy != StateCapitalism, "Economy policy is not State Capitalism", true);
    //    public static readonly Condition isStateCapitlism = new Condition(x => (x as Country).economy == StateCapitalism, "Economy policy is State Capitalism", true);

    //    public static readonly Condition isNotInterventionism = new Condition(x => (x as Country).economy != Interventionism, "Economy policy is not Limited Interventionism", true);
    //    public static readonly Condition isInterventionism = new Condition(x => (x as Country).economy == Interventionism, "Economy policy is Limited Interventionism", true);

    //    public static readonly Condition isNotPlanned = new Condition(x => (x as Country).economy != PlannedEconomy, "Economy policy is not Planned Economy", true);
    //    public static readonly Condition isPlanned = new Condition(x => (x as Country).economy == PlannedEconomy, "Economy policy is Planned Economy", true);

    //    public static readonly DoubleCondition taxesInsideLFLimit = new DoubleCondition(
    //    delegate (object x, object y)
    //    {
    //        // todo return it
    //        //if it's poor taxes
    //        //var taxesForPoor = y as TaxationForPoor.ReformValue;
    //        //if (taxesForPoor != null)
    //        //    return (x as Country).economy != LaissezFaire || taxesForPoor.tax.get() <= 0.5f;
    //        //else
    //        {
    //            var taxesForRich = y as TaxationForRich.ReformValue;
    //            return (x as Country).economy != LaissezFaire || taxesForRich.tax.get() <= 0.5f;
    //        }
    //    },
    //        x => "Economy policy is Laissez Faire and tax is not higher than 50%", false);

    //    public static readonly DoubleCondition taxesInsideSCLimit = new DoubleCondition(
    //    delegate (object x, object y)
    //    {
    //        // todo return it
    //        //if it's poor taxes
    //        //var taxesForPoor = y as TaxationForPoor.ReformValue;
    //        //if (taxesForPoor != null)
    //        //    return (x as Country).economy != StateCapitalism || taxesForPoor.tax.get() >= 0.2f;
    //        //else
    //        {
    //            var taxesForRich = y as TaxationForRich.ReformValue;
    //            return (x as Country).economy != StateCapitalism || taxesForRich.tax.get() >= 0.2f;
    //        }
    //    },
    //        x => "Economy policy is State capitalism and tax is not lower than 20%", false);

    //    public static Condition isNotMarket = new Condition(x => (x as Country).economy == NaturalEconomy || (x as Country).economy == PlannedEconomy,
    //      "Economy is not market economy", true);

    //    public static Condition isMarket = new Condition(x => (x as Country).economy == StateCapitalism || (x as Country).economy == Interventionism
    //        || (x as Country).economy == LaissezFaire
    //        , "Economy is market economy", true);

    //    public class ReformValue : AbstractReformValue
    //    {
    //        private readonly bool _allowForeignIvestments;
    //        public Procent maxPoorTax;
    //        public Procent minPoorTax;
    //        public bool AllowForeignInvestments
    //        {
    //            get { return _allowForeignIvestments; }
    //        }

    //        public ReformValue(string name, string description, int id, bool _allowForeighnIvestments, DoubleConditionsList condition, Procent maxPoorTax = null, Procent minPoorTax = null) : base(name, description, id, condition)
    //        {
    //            this.maxPoorTax = maxPoorTax;
    //            this.minPoorTax = minPoorTax;
    //            PossibleStatuses.Add(this);
    //            _allowForeignIvestments = _allowForeighnIvestments;
    //        }

    //        public override bool isAvailable(Country country)
    //        {
    //            ReformValue requested = this;
    //            if (requested.ID == 0)
    //                return true;
    //            else
    //            if (requested.ID == 1)
    //                return true;
    //            else
    //            if (requested.ID == 2 && country.Science.IsInvented(Invention.Collectivism))
    //                return true;
    //            else
    //            if (requested.ID == 3)
    //                return true;
    //            else
    //                return false;
    //        }

    //        protected override Procent howIsItGoodForPop(PopUnit pop)
    //        {
    //            Procent result;
    //            //if (pop.Type == PopType.Capitalists)
    //            if (pop.Type.isRichStrata())
    //            {
    //                //positive - more liberal
    //                int change = ID - pop.Country.economy.ID;
    //                //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
    //                if (change > 0)
    //                    result = new Procent(1f + change / 10f);
    //                else
    //                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
    //                    result = new Procent(0f);
    //            }
    //            else
    //            {
    //                if (this == PlannedEconomy)
    //                    result = new Procent(0f); // that can be achieved only by government reform
    //                else if (this == LaissezFaire)
    //                {
    //                    result = new Procent(0.6f);
    //                }
    //                else if (this == Interventionism)
    //                {
    //                    result = new Procent(0.7f);
    //                }
    //                else
    //                    result = new Procent(0.5f);
    //            }
    //            return result;
    //        }
    //    }

    //    private static readonly ConditionsList capitalism = new ConditionsList(new List<Condition>
    //    {
    //        Invention.IndividualRightsInvented,
    //        Invention.BankingInvented,
    //        Serfdom.IsAbolishedInAnyWay
    //    });

    //    private ReformValue status;

    //    public static readonly List<ReformValue> PossibleStatuses = new List<ReformValue>();

    //    public static readonly ReformValue PlannedEconomy = new ReformValue("Planned economy", "", 0, false,
    //        new DoubleConditionsList(new List<Condition> {
    //        Invention.CollectivismInvented, Gov.isProletarianDictatorship }));

    //    public static readonly ReformValue NaturalEconomy = new ReformValue("Natural economy", " ", 1, false, new DoubleConditionsList(Condition.IsNotImplemented));//new ConditionsList(Condition.AlwaysYes));
    //    public static readonly ReformValue StateCapitalism = new ReformValue("State capitalism", "", 2, false, new DoubleConditionsList(capitalism), null, new Procent(0.2f));
    //    public static readonly ReformValue Interventionism = new ReformValue("Limited interventionism", "", 3, true, new DoubleConditionsList(capitalism));
    //    public static readonly ReformValue LaissezFaire = new ReformValue("Laissez faire", "", 4, true, new DoubleConditionsList(capitalism), new Procent(0.5f));

    //    /// ////////////
    //    public Ecccconomy(Country country) : base("Economy", "Your economy policy", country)
    //    {
    //        status = NaturalEconomy;
    //        this.country = country;
    //    }

    //    public override AbstractReformValue getValue()
    //    {
    //        return status;
    //    }

    //    public Country Country
    //    {
    //        get { return country; }
    //    }

    //    // todo add OnReformEnacted?
    //    public override void setValue(AbstractReformValue selectedReform)
    //    {
    //        base.setValue(selectedReform);
    //        status = (ReformValue)selectedReform;
    //        if (status == LaissezFaire)
    //        {
    //            if (Country.taxationForRich.getTypedValue().tax.get() > 0.5f)
    //                Country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[5]);
    //            if (Country.taxationForPoor.tax.get() > 0.5f)
    //                Country.taxationForPoor.SetValue(LaissezFaire.maxPoorTax);
    //            Country.Provinces.AllFactories.PerformAction(
    //                 x =>
    //                 {
    //                     x.setSubsidized(false);
    //                     x.ownership.SetToSell(Country, Procent.HundredProcent, false);
    //                 });
    //        }
    //        else if (status == StateCapitalism)
    //        {
    //            if (Country.taxationForRich.getTypedValue().tax.get() < 0.2f)
    //                Country.taxationForRich.setValue(TaxationForRich.PossibleStatuses[2]);
    //            if (Country.taxationForPoor.tax.get() < 0.2f)
    //                Country.taxationForPoor.SetValue(StateCapitalism.minPoorTax);
    //        }
    //    }

    //    public ReformValue getTypedValue()
    //    {
    //        return status;
    //    }

    //    //public override AbstractReformValue getValue(int value)
    //    //{
    //    //    return PossibleStatuses[value];
    //    //}

    //    public override IEnumerator GetEnumerator()
    //    {
    //        foreach (ReformValue f in PossibleStatuses)
    //            yield return f;
    //    }

    //    public override bool isAvailable(Country country)
    //    {
    //        return true;
    //    }

    //    public override bool canHaveValue(AbstractReformValue abstractReformValue)
    //    {
    //        return PossibleStatuses.Contains(abstractReformValue as ReformValue);
    //    }
    //}
}