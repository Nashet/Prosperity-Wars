using Nashet.Conditions;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation.Reforms
{
    public class FamilyPlanning : AbstractReform
    {
        protected FamilyPlanningValue typedValue;

        //Economy.isNotPlanned
        public static readonly FamilyPlanningValue None = new FamilyPlanningValue("No birth limits", "", 0, new DoubleConditionsList(new List<Condition> {
            new Condition(x => (x as Country).FamilyPlanning == FourKids, "Previous reform enacted", true)
        }), new Procent(0.020f));

        public static readonly FamilyPlanningValue FourKids = new FamilyPlanningValue("4 kids limit", " per family", 1, new DoubleConditionsList(new List<Condition>
        {
            Invention.Collectivism.Invented, new Condition(x => (x as Country).FamilyPlanning == None || (x as Country).FamilyPlanning == ThreeKids, "Previous reform enacted", true)
        }), new Procent(0.015f));

        public static readonly FamilyPlanningValue ThreeKids = new FamilyPlanningValue("3 kids limit", " per family", 2, new DoubleConditionsList(new List<Condition>
        {
            Invention.Collectivism.Invented, new Condition(x => (x as Country).FamilyPlanning == FourKids || (x as Country).FamilyPlanning == TwoKids, "Previous reform enacted", true)
        }), new Procent(0.010f));

        public static readonly FamilyPlanningValue TwoKids = new FamilyPlanningValue("2 kids limit", " per family", 3, new DoubleConditionsList(new List<Condition>
        {
            Invention.Collectivism.Invented, new Condition(x => (x as Country).FamilyPlanning == ThreeKids || (x as Country).FamilyPlanning == OneKid, "Previous reform enacted", true)
        }), new Procent(0.005f));

        public static readonly FamilyPlanningValue OneKid = new FamilyPlanningValue("1 kid limit", " per family", 4, new DoubleConditionsList(new List<Condition>
        {
            Invention.Collectivism.Invented, new Condition(x => (x as Country).FamilyPlanning == TwoKids, "Previous reform enacted", true)
        }), new Procent(0.0f));


        public FamilyPlanning(Country country, int showOrder) : base("Family planning", " - you can limit amount of kids in family reducing pop growth rate", country, showOrder,
            new List<IReformValue> { None, FourKids, ThreeKids, TwoKids, OneKid })
        {
            SetValue(None);
        }

        public override void SetValue(IReformValue selectedReform)
        {
            base.SetValue(selectedReform);
            typedValue = selectedReform as FamilyPlanningValue;

        }

        public Procent GrowthRate { get { return typedValue.GrowthRate; } }
        //public override string ToString()
        //{
        //    return base.ToString() + " (" + WageSize + ")";
        //}

        public class FamilyPlanningValue : NamedReformValue
        {
            internal FamilyPlanningValue(string inname, string indescription, int id, DoubleConditionsList condition, Procent growthRate)
                : base(inname, indescription, id, condition)
            {
                LifeQualityImpact = new Procent(ID, 10f);
                GrowthRate = growthRate;
            }

            internal Procent GrowthRate { get; private set; }

            public override Procent howIsItGoodForPop(PopUnit pop)
            {
                Procent result;
                if (pop.Type == PopType.Workers)
                {
                    //positive - reform will be better for worker, [-5..+5]
                    int change = GetRelativeConservatism(pop.Country.FamilyPlanning.typedValue); // ID - pop.Country.minimalWage.value.ID;
                                                                                                 //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f);
                    if (change > 0)
                        if (this == OneKid)
                            result = new Procent(0f);
                        else
                            result = new Procent(0.6f);
                    else
                        //result = new Procent((change + PossibleStatuses.Count - 1) * 0.1f /2f);
                        result = new Procent(0f);
                }
                else // rich strata
                {
                    result = new Procent(0f);
                }

                return result;
            }
        }
    }
}