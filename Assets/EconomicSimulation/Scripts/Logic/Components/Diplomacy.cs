using Nashet.Conditions;
using Nashet.EconomicSimulation.Reforms;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections.Generic;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Tracks diplomatic relations betweens IDiplomat
    /// </summary>
    public class Diplomacy : Component<IDiplomat>
    {
        public static readonly DoubleConditionsList canAttack = new DoubleConditionsList(new List<Condition>
    {
        new DoubleCondition((province, country)=>(province as Province).AllNeighbors().Any(x => x.Country == country)
        && (province as Province) .Country != country, x=>"Is neighbor province", true),
        new DoubleCondition((province, country)=>!Government.isDemocracy.checkIfTrue(country)
        || !Government.isDemocracy.checkIfTrue((province as Province).Country), x=>"Democracies can't attack each other", true)
    });
        private static readonly Dictionary<IDiplomat, IDiplomat> wars = new Dictionary<IDiplomat, IDiplomat>();

        protected readonly Dictionary<IDiplomat, Date> LastAttackDate = new Dictionary<IDiplomat, Date>();
        protected readonly Dictionary<IDiplomat, Procent> opinionOf = new Dictionary<IDiplomat, Procent>();
       

        protected readonly Procent defaultRelation = new Procent(0.5f);
        protected readonly float relationDecreaseOnAttack = -0.5f;

        public Diplomacy(IDiplomat owner) : base(owner)
        {
           
        }

        public static void DeclareWar(IDiplomat attacker, IDiplomat defender)
        {
            if (!IsInWar(attacker, defender))
                wars.Add(attacker, defender);
        }

        public static bool IsInWar(IDiplomat sideOne, IDiplomat sideTwo)
        {
            if (wars.ContainsKey(sideOne))
                return true;
            else
                return wars.ContainsKey(sideTwo);
        }
        /// <summary>
        /// Returns null if used on itself
        /// </summary>        
        public Procent GetRelationTo(IDiplomat partner)
        {
            if (this == partner)
                return null;
            Procent opinion;
            if (opinionOf.TryGetValue(partner, out opinion))
                return opinion;
            else
            {
                opinion = defaultRelation.Copy();
                opinionOf.Add(partner, opinion);
                return opinion;
            }
        }

        /// <summary>
        /// Changes that country opinion of another country
        /// </summary>
        public void ChangeRelation(IDiplomat another, float change)
        {
            var relation = GetRelationTo(another);
            relation.Add(change, false);
            relation.clamp100();
        }

        public Date GetLastAttackDateOn(IDiplomat someOne)
        {
            if (LastAttackDate.ContainsKey(someOne))
                return LastAttackDate[someOne];
            else
                return Date.Never.Copy();
        }
        public void OnAttack(IDiplomat attacked)
        {
            attacked.Diplomacy.ChangeRelation(owner, relationDecreaseOnAttack);
            if (owner.Diplomacy.LastAttackDate.ContainsKey(attacked))
                owner.Diplomacy.LastAttackDate[attacked].set(Date.Today);
            else
                owner.Diplomacy.LastAttackDate.Add(attacked, Date.Today.Copy());
        }        
    }
}