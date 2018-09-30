using Nashet.Conditions;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation.Reforms
{
    public class Economy : AbstractReform
    {
        protected EconomyReformValue typedValue;
        public static readonly EconomyReformValue PlannedEconomy = new EconomyReformValue("Planned economy", "No market, no private business, everything is free (except freedom)", 0,
            new DoubleConditionsList(new List<Condition> {
            Invention.Collectivism.Invented, Government.isProletarianDictatorship }), false);

        private static readonly ConditionsList capitalism = new ConditionsList(new List<Condition>
        {
            Invention.IndividualRights.Invented,
            Invention.Banking.Invented,
            Serfdom.IsAbolishedInAnyWay
        });

        public static readonly EconomyReformValue NaturalEconomy = new EconomyReformValue("Natural economy", "No market", 1, new DoubleConditionsList(Condition.IsNotImplemented), false);//new ConditionsList(Condition.AlwaysYes));
        public static readonly EconomyReformValue StateCapitalism = new EconomyReformValue("State capitalism", "Coexistence of the private and government economy", 2, new DoubleConditionsList(capitalism), false, null, TaxationForPoor.PoorTaxValue.TaxRate20);
        public static readonly EconomyReformValue Interventionism = new EconomyReformValue("Limited interventionism", "", 3,
           new DoubleConditionsList(       
            Invention.Keynesianism.Invented,
            Invention.Banking.Invented,
            Serfdom.IsAbolishedInAnyWay)        
        , true);
        public static readonly EconomyReformValue LaissezFaire = new EconomyReformValue("Laissez faire", "No Government Intervention", 4, new DoubleConditionsList(capitalism), true, TaxationForPoor.PoorTaxValue.TaxRate50);

        public static readonly DoubleCondition isNotLFOrMoreConservative = new DoubleCondition((country, newReform) => (country as Country).economy != Economy.LaissezFaire
        || (newReform as IReformValue).IsMoreConservativeThan((country as Country).Politics.GetReform(newReform as AbstractReformValue).Value),
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
                var taxesForRich = y as TaxationForRich.ProcentReformValue;
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
                var taxesForRich = y as ProcentReform.ProcentReformValue;
                return (x as Country).economy != StateCapitalism || taxesForRich.get() >= 0.2f;
            }
        },
            x => "Economy policy is State capitalism and tax is not lower than 20%", false);

        public static Condition isNotMarket = new Condition(x => (x as Country).economy == NaturalEconomy || (x as Country).economy == PlannedEconomy,
          "Economy is not market economy", true);

        public static Condition isMarket = new Condition(x => (x as Country).economy == StateCapitalism || (x as Country).economy == Interventionism
            || (x as Country).economy == LaissezFaire
            , "Economy is market economy", true);

        public Economy(Country country, int showOrder) : base("Economy", "- your economy policy", country, showOrder,
            new List<IReformValue> { NaturalEconomy, StateCapitalism, Interventionism, LaissezFaire, PlannedEconomy })
        {
            SetValue(StateCapitalism);
        }

        public override void OnReformEnactedInProvince(Province province)
        {
            foreach (var factory in province.AllFactories)
            {
                factory.setSubsidized(false);
                factory.ownership.SetToSell(owner, Procent.HundredProcent, false);
            }
        }

        public override void SetValue(IReformValue reformValue)
        {
            base.SetValue(reformValue);

            typedValue = reformValue as EconomyReformValue;
            if (typedValue == LaissezFaire)
            {
                if (owner.taxationForRich.tax.get() > 0.5f)
                    owner.taxationForRich.SetValue(LaissezFaire.maxTax);
                if (owner.taxationForPoor.tax.get() > 0.5f)
                    owner.taxationForPoor.SetValue(LaissezFaire.maxTax);
                owner.Provinces.AllProvinces.PerformAction(x => OnReformEnactedInProvince(x));
            }
            else if (typedValue == StateCapitalism)
            {
                if (owner.taxationForRich.tax.get() < 0.2f)
                    owner.taxationForRich.SetValue(StateCapitalism.minTax);
                if (owner.taxationForPoor.tax.get() < 0.2f)
                    owner.taxationForPoor.SetValue(StateCapitalism.minTax);
            }
        }

        public bool AllowForeignInvestments
        {
            get { return typedValue.AllowForeignInvestments; }
        }
        public class EconomyReformValue : NamedReformValue
        {
            public ProcentReform.ProcentReformValue maxTax;
            public ProcentReform.ProcentReformValue minTax;
            public bool AllowForeignInvestments { get; protected set; }

            internal EconomyReformValue(string name, string description, int id, DoubleConditionsList condition,
                bool allowForeighnIvestments, TaxationForPoor.PoorTaxValue maxTax = null, TaxationForPoor.PoorTaxValue minTax = null) : base(name, description, id, condition)
            {
                AllowForeignInvestments = allowForeighnIvestments;
                this.minTax = minTax;
                this.maxTax = maxTax;
            }
            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                //if (pop.Type == PopType.Capitalists)
                if (pop.Type.isRichStrata())
                {
                    //positive - more liberal
                    int relation = GetRelativeConservatism(pop.Country.economy.typedValue); // ID - pop.Country.economy.value.ID;
                    //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                    if (relation > 0)
                        result = new Procent(1f + relation / 10f);
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
    }
}