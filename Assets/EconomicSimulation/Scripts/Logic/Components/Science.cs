using Nashet.Conditions;
using Nashet.EconomicSimulation.Reforms;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Represents ability to invent Inventions
    /// </summary>
    public class Science : Component<ICanInvent>
    {
        public static readonly ModifiersList modSciencePoints = new ModifiersList(new List<Condition>
        {
        //new Modifier(Government.isTribal, 0f, false),
        //new Modifier(Government.isTheocracy, 0f, false),
        new Modifier(Government.isDespotism, Government.Despotism.getScienceModifier(), false),
        new Modifier(Government.isJunta, Government.Junta.getScienceModifier(), false),
        new Modifier(Government.isAristocracy, Government.Aristocracy.getScienceModifier(), false),
        new Modifier(Government.isProletarianDictatorship, Government.ProletarianDictatorship.getScienceModifier(), false),
        new Modifier(Government.isDemocracy, Government.Democracy.getScienceModifier(), false),
        new Modifier(Government.isPolis, Government.Polis.getScienceModifier(), false),
        new Modifier(Government.isWealthDemocracy, Government.WealthDemocracy.getScienceModifier(), false),
        new Modifier(Government.isBourgeoisDictatorship, Government.BourgeoisDictatorship.getScienceModifier(), false),
        new Modifier(x=>(x as Country).Provinces.AllPops.GetAverageProcent(y=>y.Education).RawUIntValue, "Education", 1f / Procent.Precision, false)
    });
        protected readonly Dictionary<Invention, bool> inventions = new Dictionary<Invention, bool>();
        public float Points { get; protected set; }

        public Science(ICanInvent owner) : base(owner)
        {
            foreach (var each in Invention.All)
                inventions.Add(each, false);
        }

        public IEnumerable<KeyValuePair<Invention, bool>> AllAvailableInventions()
        {
            foreach (var invention in inventions)
                if (invention.Key.IsInvented(owner))
                    yield return invention;
        }

        public IEnumerable<Invention> AllUninvented()
        {
            foreach (var invention in inventions)
                if (invention.Value == false && invention.Key.IsInvented(owner))
                    yield return invention.Key;
        }

        public IEnumerable<Invention> AllInvented()
        {
            foreach (var invention in inventions)
                if (invention.Value && invention.Key.IsInvented(owner))
                    yield return invention.Key;
        }

        public void Invent(Invention type)
        {
            inventions[type] = true;
            Points -= type.getCost().get();
            if (Points < 0f)
                Points = 0f;
        }

        public bool IsInvented(Invention type)
        {
            bool result = false;
            inventions.TryGetValue(type, out result);
            return result;
        }

        public bool IsInvented(Product product)
        {
            if (product.isAbstract())
                return true;
            if (
                ((product == Product.Metal || product == Product.MetalOre || product == Product.ColdArms) && !IsInvented(Invention.Metal))
                || (!IsInvented(Invention.SteamPower) && (product == Product.Machinery))//|| product == Product.Cement))
                || ((product == Product.Artillery || product == Product.Ammunition) && !IsInvented(Invention.Gunpowder))
                || (product == Product.Firearms && !IsInvented(Invention.Firearms))
                || (product == Product.Coal && !IsInvented(Invention.Coal))
                //|| (product == Cattle && !country.isInvented(Invention.Domestication))
                || (!IsInvented(Invention.CombustionEngine) && (product == Product.Oil || product == Product.MotorFuel || product == Product.Rubber || product == Product.Cars))
                || (!IsInvented(Invention.Tanks) && product == Product.Tanks)
                || (!IsInvented(Invention.Airplanes) && product == Product.Airplanes)
                || (product == Product.Tobacco && !IsInvented(Invention.Tobacco))
                || (product == Product.Electronics && !IsInvented(Invention.Electronics))
                //|| (!isResource() && !country.isInvented(Invention.Manufactories))
                || (product == Product.Education && !IsInvented(Invention.Universities))
                )
                return false;
            else
                return true;
        }

        public bool IsInventedFactory(ProductionType production)
        {
            //if (!Invented(production.basicProduction.Product)
            // || production.IsResourceProcessing() && !Invented(Invention.Manufactures)
            // || (production.basicProduction.Product == Product.Cattle && !Invented(Invention.Domestication))
            if (!IsInventedArtisanship(production)
                 || production.IsResourceProcessing() && !IsInvented(Invention.Manufactures)
             )
                return false;
            else
                return true;
        }

        public bool IsInventedArtisanship(ProductionType production)
        {
            if (!IsInvented(production.basicProduction.Product)
             || (production.basicProduction.Product == Product.Cattle && !IsInvented(Invention.Domestication))
             )
                return false;
            else
                return true;
        }

        public void AddPoints(float points)
        {
            Points += points;
        }
    }
}