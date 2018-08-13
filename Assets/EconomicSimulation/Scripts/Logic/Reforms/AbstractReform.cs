using System.Collections;
using Nashet.UnityUIUtils;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    public abstract class AbstractReform : Name, IClickable
    {
        private readonly string description;

        protected AbstractReform(string name, string indescription, Country country) : base(name)
        {
            description = indescription;
            //country.reforms.Add(this);
            this.country = country;
        }
        private readonly Country country;
        public abstract bool isAvailable(Country country);

        public abstract IEnumerator GetEnumerator();


        public virtual void setValue(IReformValue selectedReformValue)
        {
            foreach (PopUnit pop in country.Provinces.AllPops)
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
}