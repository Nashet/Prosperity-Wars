using Nashet.Conditions;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    public class Invention : Name, IClickable
    {
        protected static readonly List<Invention> allInventions = new List<Invention>();
        protected readonly string description;
        public Value Cost { get; protected set; }

        /// <summary>ICanInvent scope</summary>
        public ConditionsList InventedPreviousTechs { get; protected set; } = new ConditionsList();
        public Condition Invented { get; protected set; }

        public static readonly Invention Farming = new Invention("Farming", "Allows farming and farmers", new Value(100f)),
            Banking = new Invention("Banking",
                "Allows national bank, credits and deposits. Also allows serfdom abolishment with compensation for aristocrats",
                new Value(100f)),

            Manufactures = new Invention("Manufactures", "Allows building manufactures to process raw product", new Value(80f)),
            JohnKayFlyingshuttle = new Invention("John Kay's Flying shuttle", "Allows Weaver factory", new Value(60f)),
            Mining = new Invention("Mining",
                "Allows resource gathering from holes in ground, increasing it's efficiency by 50%",
                new Value(100f)),
            //religion = new InventionType("Religion", "Allows clerics, gives loyalty boost", new Value(100f)),
            Metal = new Invention("Metal", "Allows metal ore and smelting. Allows Cold arms", new Value(100f)),
            // Add here capitalism and link it to serfdom
            IndividualRights = new Invention("Classical liberalism",
                "Allows Laissez faire policy, Universal Democracy, Bourgeois dictatorship",
                new Value(80f)),
            Keynesianism = new Invention("Keynesianism",
                "Allows Limited Interventionism in economy",
                new Value(80f), IndividualRights),

            Collectivism = new Invention("Collectivism", "Allows Proletarian dictatorship & Planned Economy", new Value(100f)),
            SteamPower = new Invention("Steam Power",
                "Allows Machinery & Cement, Increases efficiency of all enterprises by 25%",
                new Value(100f), Metal, Manufactures),

            Welfare = new Invention("Welfare", "Allows Unemployment Benefits and UBI", new Value(90f)),
            Gunpowder = new Invention("Gunpowder", "Allows Artillery & Ammunition", new Value(100f), Metal),
            Firearms = new Invention("Hand-held cannons",
                "Allows Firearms, very efficient in battles", new Value(200f), Gunpowder),

            CombustionEngine = new Invention("Combustion engine",
                "Allows Oil, Fuel, Cars, Rubber, Increases efficiency of all enterprises by 25%",
                new Value(400f), SteamPower),

            Tanks = new Invention("Tanks", "Allows Tanks", new Value(800f), CombustionEngine),
            Airplanes = new Invention("Airplanes", "Allows Airplanes", new Value(1200f), CombustionEngine),
            ProfessionalArmy = new Invention("Professional Army", "Allows soldiers", new Value(200f)),
            Domestication = new Invention("Domestication",
                "Allows barnyard producing cattle. Also allows using horses in army",
                new Value(100f)),

            Electronics = new Invention("Electronics", "Allows Electronics", new Value(1000f), Airplanes),
            Tobacco = new Invention("Tobacco", "Allows Tobacco", new Value(100f)),
            Coal = new Invention("Coal", "Allows coal", new Value(100f), Metal),
            Universities = new Invention("Universities", "Allows building of Universities", new Value(150f));



        protected Invention(string name, string description, Value cost, params Invention[] requiredInventions) : base(name)
        {
            this.description = description;
            this.Cost = cost;
            allInventions.Add(this);
            if (requiredInventions != null)
                foreach (var item in requiredInventions)
                {
                    InventedPreviousTechs.add(new Condition(x => (x as IInventor).Science.IsInvented(item), item.ShortName + " aren't invented", true));
                }
            Invented = new Condition(x => (x as IInventor).Science.IsInvented(this), "Invented " + name, true);
        }

        public static IEnumerable<Invention> All
        {
            get
            {
                foreach (var item in allInventions)
                {
                    yield return item;
                }
            }
        }

        public bool CanInvent(IInventor inventor)
        {
            return InventedPreviousTechs.isAllTrue(inventor);
        }

        public override string FullName
        {
            get { return description; }
        }

        public void OnClicked()
        {
            Game.Player.events.RiseClickedOn(new InventionEventArgs(this));
            //MainCamera.inventionsPanel.selectInvention(this);
            //MainCamera.inventionsPanel.Refresh();
        }
    }
}