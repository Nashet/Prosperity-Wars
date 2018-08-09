using System.Collections.Generic;
using Nashet.Conditions;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public class Invention : Name, IClickable
    {
        private static readonly List<Invention> allInventions = new List<Invention>();
        private string description;
        private Value cost;
        private string inventedPhrase;

        public static readonly Invention Farming = new Invention("Farming", "Allows farming and farmers", new Value(100f)),
            Banking = new Invention("Banking", "Allows national bank, credits and deposits. Also allows serfdom abolishment with compensation for aristocrats", new Value(100f)),
            Manufactures = new Invention("Manufactures", "Allows building manufactures to process raw product", new Value(70f)),
            Mining = new Invention("Mining", "Allows resource gathering from holes in ground, increasing it's efficiency by 50%", new Value(100f)),
            //religion = new InventionType("Religion", "Allows clerics, gives loyalty boost", new Value(100f)),
            Metal = new Invention("Metal", "Allows metal ore and smelting. Allows Cold arms", new Value(100f)),
            IndividualRights = new Invention("Individual rights",
                "Allows Limited interventionism, Laissez faire, Universal Democracy, Bourgeois dictatorship",
                new Value(80f)),//Allows Capitalism, Serfdom & Slavery abolishments
            Collectivism = new Invention("Collectivism", "Allows Proletarian dictatorship & Planned Economy", new Value(100f)),
            SteamPower = new Invention("Steam Power", "Allows Machinery & cement, Increases efficiency of all enterprises by 25%", new Value(100f)),
            Welfare = new Invention("Welfare", "Allows min wage and.. other", new Value(90f)),
            Gunpowder = new Invention("Gunpowder", "Allows Artillery & Ammunition", new Value(100f)),
            Firearms = new Invention("Hand-held cannons", "Allows Firearms, very efficient in battles", new Value(200f)),
            CombustionEngine = new Invention("Combustion engine", "Allows Oil, Fuel, Cars, Rubber, Increases efficiency of all enterprises by 25%", new Value(400f)),
            Tanks = new Invention("Tanks", "Allows Tanks", new Value(800f)),
            Airplanes = new Invention("Airplanes", "Allows Airplanes", new Value(1200f)),
            ProfessionalArmy = new Invention("Professional Army", "Allows soldiers", new Value(200f)),

            Domestication = new Invention("Domestication", "Allows barnyard producing cattle. Also allows using horses in army", new Value(100f)),
            Electronics = new Invention("Electronics", "Allows Electronics", new Value(1000f)),
            Tobacco = new Invention("Tobacco", "Allows Tobacco", new Value(100f)),
            Coal = new Invention("Coal", "Allows coal", new Value(100f)),
            Universities = new Invention("Universities", "Allows building of Universities", new Value(150f))
            ;

        public static readonly Condition ProfessionalArmyInvented = new Condition(x => (x as Country).Inventions.IsInvented(ProfessionalArmy), "Professional Army is invented", true);
        public static readonly Condition SteamPowerInvented = new Condition(x => (x as Country).Inventions.IsInvented(SteamPower), "Steam Power is invented", true);
        public static readonly Condition CombustionEngineInvented = new Condition(x => (x as Country).Inventions.IsInvented(CombustionEngine), "Combustion Engine is invented", true);
        public static readonly Condition IndividualRightsInvented = new Condition(x => (x as Country).Inventions.IsInvented(IndividualRights), "Individual Rights are invented", true);
        public static readonly Condition BankingInvented = new Condition(x => (x as Country).Inventions.IsInvented(Banking), "Banking is invented", true);
        public static readonly Condition WelfareInvented = new Condition(x => (x as Country).Inventions.IsInvented(Welfare), "Welfare is invented", true);
        public static readonly Condition CollectivismInvented = new Condition(x => (x as Country).Inventions.IsInvented(Collectivism), "Collectivism is invented", true);
        public static readonly Condition ManufacturesInvented = new Condition(x => (x as Country).Inventions.IsInvented(Manufactures), "Manufactures are invented", true);
        public static readonly Condition ManufacturesUnInvented = new Condition(x => !(x as Country).Inventions.IsInvented(Manufactures), "Manufactures aren't invented", true);

        public Invention(string name, string description, Value cost) : base(name)
        {
            //this.name = name;
            this.description = description;
            this.cost = cost;
            inventedPhrase = "Invented " + name;
            allInventions.Add(this);
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

        public string getInventedPhrase()
        {
            return inventedPhrase;
        }

        public bool IsInvented(Country country)
        {
            if (//this == Collectivism
                //||
                (this == Gunpowder && !country.Inventions.IsInvented(Metal))
                || (this == Coal && !country.Inventions.IsInvented(Metal))
                || (this == SteamPower && (!country.Inventions.IsInvented(Metal) || !country.Inventions.IsInvented(Manufactures)))
                || (this == Firearms && !country.Inventions.IsInvented(Gunpowder))
                || (this == CombustionEngine && !country.Inventions.IsInvented(SteamPower))
                || (this == Tanks && !country.Inventions.IsInvented(CombustionEngine))
                || (this == Airplanes && !country.Inventions.IsInvented(CombustionEngine))
                || (this == Electronics && !country.Inventions.IsInvented(Airplanes))
                )
                return false;
            else
                return true;
        }

        public override string FullName
        {
            get { return description; }
        }

        public Value getCost()
        {
            return cost;
        }

        public void OnClicked()
        {
            MainCamera.inventionsPanel.selectInvention(this);
            MainCamera.inventionsPanel.Refresh();
        }
    }
}