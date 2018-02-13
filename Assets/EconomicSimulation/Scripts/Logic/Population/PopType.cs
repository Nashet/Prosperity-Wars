using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Nashet.ValueSpace;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    public class PopType : IEscapeTarget, ISortableName
    {
        private readonly static List<PopType> allPopTypes = new List<PopType>();
        public static readonly PopType Tribesmen, Aristocrats, Farmers, Artisans, Soldiers, Workers, Capitalists;

        public static Predicate<PopType> All = x => true;
        ///<summary> per 1000 men </summary>    
        private readonly List<Storage> lifeNeeds;//= new List<Storage>();
        private readonly List<Storage> everyDayNeeds;// = new List<Storage>();
        private readonly List<Storage> luxuryNeeds;// = new List<Storage>();
        private readonly List<Storage> militaryNeeds;// = new List<Storage>();

        ///<summary> per 1000 men </summary>
        private readonly Storage basicProduction;
        private readonly string name;
        /// <summary>
        /// SHOULD not be zero!
        /// </summary>
        private readonly float strenght;
        private readonly float nameWeight;
        static PopType() // can't be private
        {
            var militaryNeeds = new List<Storage> { new Storage(Product.Food, 0.2f), new Storage(Product.Cattle, 0.2f), new Storage(Product.ColdArms, 0.2f), new Storage(Product.Firearms, 0.4f), new Storage(Product.Ammunition, 0.6f), new Storage(Product.Artillery, 0.2f), new Storage(Product.Cars, 0.2f), new Storage(Product.Tanks, 0.2f), new Storage(Product.Airplanes, 0.2f), new Storage(Product.MotorFuel, 0.6f) };
            var tribemenLifeNeeds = new List<Storage> { new Storage(Product.Food, 1) };
            var tribemenEveryDayNeeds = new List<Storage> { new Storage(Product.Food, 2) };
            var tribemenLuxuryNeeds = new List<Storage> { new Storage(Product.Food, 3) };
            Tribesmen = new PopType("Tribesmen", new Storage(Product.Cattle, 1.0f), 2f,
                militaryNeeds, tribemenLifeNeeds, tribemenEveryDayNeeds, tribemenLuxuryNeeds);
            //***************************************next type***************************
            var aristocratsLifeNeeds = new List<Storage> { new Storage(Product.Food, 1) };
            var aristocratsEveryDayNeeds = new List<Storage> {

            new Storage(Product.ColdArms, 1f),
            new Storage(Product.Clothes, 1f),
            new Storage(Product.Furniture, 1f),
            new Storage(Product.Liquor, 2f),
            new Storage(Product.Electronics, 1f)
            ,};
            var aristocratsLuxuryNeeds = new List<Storage> {
            new Storage(Product.Fruit, 1),
            new Storage(Product.Cars, 1f),
            new Storage(Product.MotorFuel, 1f),
            new Storage(Product.Airplanes, 1f) };
            Aristocrats = new PopType("Aristocrats", null, 4f,
                militaryNeeds, aristocratsLifeNeeds, aristocratsEveryDayNeeds, aristocratsLuxuryNeeds);
            //***************************************next type***************************
            var capitalistsLifeNeeds = new List<Storage> { new Storage(Product.Food, 1) };
            var capitalistsEveryDayNeeds = new List<Storage> {
            new Storage(Product.Clothes, 1f),
            new Storage(Product.Furniture, 1f),
            new Storage(Product.Tobacco, 2f),
            new Storage(Product.Fruit, 1f) };
            var capitalistsLuxuryNeeds = new List<Storage> {
            new Storage(Product.Liquor, 2f),
            new Storage(Product.Firearms, 1f),
            new Storage(Product.Ammunition, 0.5f),
            new Storage(Product.Cars, 1f),
            new Storage(Product.MotorFuel, 1f),
            new Storage(Product.Airplanes, 1f)};
            Capitalists = new PopType("Capitalists", null, 1f,
                militaryNeeds, capitalistsLifeNeeds, capitalistsEveryDayNeeds, capitalistsLuxuryNeeds);
            //***************************************next type***************************
            {
                var artisansLifeNeeds = new List<Storage> { new Storage(Product.Food, 1) };
                var artisansEveryDayNeeds = new List<Storage> {
            new Storage(Product.Fish, 1f),
            new Storage(Product.Clothes, 1f),
            new Storage(Product.Furniture, 1f),
            new Storage(Product.Metal, 1f) };
                var artisansLuxuryNeeds = new List<Storage> {
            new Storage(Product.Liquor, 1f),
            //new Storage(Product.Cars, 1f),
            //new Storage(Product.MotorFuel, 1f),
            new Storage(Product.Electronics, 1f),
            new Storage(Product.Tobacco, 1f)
            };
                Artisans = new PopType("Artisans", null, 1f,
                    militaryNeeds, artisansLifeNeeds, artisansEveryDayNeeds, artisansLuxuryNeeds);
            }
            //***************************************next type***************************
            var farmersLifeNeeds = new List<Storage> { new Storage(Product.Food, 1) };
            var farmersEveryDayNeeds = new List<Storage> {
           //everyDayNeeds.Set(new Storage(Product.Fruit, 1),
            new Storage(Product.Stone, 1f),
            new Storage(Product.Wood, 1f),
            //everyDayNeeds.set(new Storage(Product.Wool, 1),
            new Storage(Product.Lumber, 1f),
            new Storage(Product.Cars, 0.5f),
            new Storage(Product.Fish, 1f),
            new Storage(Product.MotorFuel, 0.5f)};
            var farmersLuxuryNeeds = new List<Storage> {
            new Storage(Product.Clothes, 1),
            new Storage(Product.Furniture, 1),
            new Storage(Product.Liquor, 2)
            //new Storage(Product.Metal, 1),
            //new Storage(Product.Cement, 0.5f)
                                            };
            Farmers = new PopType("Farmers", new Storage(Product.Grain, 1.5f), 1f,
                militaryNeeds, farmersLifeNeeds, farmersEveryDayNeeds, farmersLuxuryNeeds);
            //***************************************next type***************************
            var workersLifeNeeds = new List<Storage> { new Storage(Product.Food, 1) };
            var workersEveryDayNeeds = new List<Storage> {
            new Storage(Product.Clothes, 1f),
            new Storage(Product.Liquor, 2f),
            new Storage(Product.Furniture, 1f),
            new Storage(Product.Cattle, 1)
             };
            var workersLuxuryNeeds = new List<Storage> {
            new Storage(Product.Cars, 0.5f),
            new Storage(Product.Tobacco, 1f),
            new Storage(Product.MotorFuel, 0.5f),
            new Storage(Product.Electronics, 1f)
            };
            Workers = new PopType("Workers", null, 1f,
                militaryNeeds, workersLifeNeeds, workersEveryDayNeeds, workersLuxuryNeeds);
            //***************************************next type***************************
            var soldiersLifeNeeds = new List<Storage> { new Storage(Product.Food, 2) };
            var soldiersEveryDayNeeds = new List<Storage> {
            new Storage(Product.Fruit, 2),
            new Storage(Product.Liquor, 5),
            new Storage(Product.Clothes, 4),
            new Storage(Product.Furniture, 2),
            //new Storage(Product.Wood, 1)
        };
            var soldiersLuxuryNeeds = new List<Storage> {
            new Storage(Product.Tobacco, 1f),
            new Storage(Product.Cars, 1f), // temporally
            new Storage(Product.MotorFuel, 1f),// temporally
            
            };
            Soldiers = new PopType("Soldiers", null, 2f,
                militaryNeeds, soldiersLifeNeeds, soldiersEveryDayNeeds, soldiersLuxuryNeeds);
        }
        private PopType(string name, Storage produces, float strenght, List<Storage> militaryNeeds,
            List<Storage> lifeNeeds, List<Storage> everyDayNeeds, List<Storage> luxuryNeeds)
        {
            nameWeight = name.GetWeight();
            this.militaryNeeds = militaryNeeds;
            this.strenght = strenght;

            this.name = name;
            basicProduction = produces;
            this.lifeNeeds = lifeNeeds;
            this.everyDayNeeds = everyDayNeeds;
            this.luxuryNeeds = luxuryNeeds;
            allPopTypes.Add(this);
        }
        public static IEnumerable<PopType> getAllPopTypes()
        {
            foreach (var item in allPopTypes)
                yield return item;
        }
        /// <summary>
        /// returns new value
        /// </summary>
        /// <returns></returns>
        public Storage getBasicProduction()
        {
            return basicProduction.Copy();
        }
        public bool canMobilize(Staff byWhom)
        {
            if (byWhom is Country)
            {
                if (this == Capitalists)
                    return false;
                else
                    return true;
            }
            else // movement
                return true;
        }
        ///<summary> Returns copy </summary>
        public List<Storage> getMilitaryNeedsPer1000Men(Country country)
        {
            //return militaryNeeds;
            var result = new List<Storage>();
            foreach (var item in militaryNeeds)
                if (item.getProduct() != Product.Cattle || country.Invented(Invention.Domestication))
                    //if (item.getProduct().IsInventedByAnyOne())
                    result.Add(new Storage(item));
            return result;
        }


        ///<summary> Returns copy </summary>
        public List<Storage> getLifeNeedsPer1000Men()
        {
            //List<Storage> result = new List<Storage>();
            //foreach (Storage next in lifeNeeds)        
            //    result.Add(next);
            //return result;
            var result = new List<Storage>();
            foreach (var item in lifeNeeds)
                if (item.getProduct().IsInventedByAnyOne())
                    result.Add(new Storage(item));
            return result;
        }
        ///<summary> Returns copy </summary>
        public List<Storage> getEveryDayNeedsPer1000Men()
        {
            //List<Storage> result = new List<Storage>();
            //foreach (Storage next in everyDayNeeds)            
            //    result.Add(next);
            //return result;
            //return everyDayNeeds;
            var result = new List<Storage>();
            foreach (var item in everyDayNeeds)
                if (item.getProduct().IsInventedByAnyOne())
                    result.Add(new Storage(item));
            return result;
        }
        ///<summary> Returns copy </summary>
        public List<Storage> getLuxuryNeedsPer1000Men()
        {
            //List<Storage> result = new List<Storage>();
            //foreach (Storage next in luxuryNeeds)            
            //    result.Add(next);
            //return result;        
            //return luxuryNeeds;
            var result = new List<Storage>();
            foreach (var item in luxuryNeeds)
                if (item.getProduct().IsInventedByAnyOne())
                    result.Add(new Storage(item));
            return result;
        }
        ///<summary> Returns copy </summary>
        public List<Storage> getAllNeedsPer1000Men()
        {
            var result = getLifeNeedsPer1000Men();
            result.AddRange(getEveryDayNeedsPer1000Men());
            result.AddRange(getLuxuryNeedsPer1000Men());
            return result;
        }
        override public string ToString()
        {
            return name;
        }

        internal bool isPoorStrata()
        {
            return this == Farmers || this == Workers || this == Tribesmen || this == Soldiers;
        }

        internal bool isRichStrata()
        {
            return this == Aristocrats || this == Capitalists || this == Artisans;
        }

        internal float getStrenght()
        {
            return strenght;
        }
        public bool canBeUnemployed()
        {
            return this == Farmers || this == Workers || this == Tribesmen;
        }
        /// <summary>
        /// Returns true if can produce something by himself
        /// </summary>    
        internal bool isProducer()
        {
            return this == Farmers || this == Tribesmen || this == Artisans;
        }
        /// <summary>
        /// Makes sure that pops consume product in cheap-first order
        /// </summary>
        internal static void sortNeeds()
        {
            foreach (var item in allPopTypes)
            {
                item.everyDayNeeds.Sort(Storage.CostOrder);
                item.luxuryNeeds.Sort(Storage.CostOrder);
            }
        }
        //internal bool HasJobsForThatPopTypeIn(Province province)
        //{
        //    return true;
        //}
        private static Procent MigrationUnemploymentLimit = new Procent(0.2f);

        public bool HasJobsFor(PopType type, Province province)
        {
            //if (this == Workers || this == Farmers || this == Tribesmen)
            if (this == Workers)
                return province.getUnemployment(x => x == Workers).isSmallerThan(MigrationUnemploymentLimit);
            else if (this == Farmers || this == Tribesmen)
                return province.GetOverpopulation().isSmallerThan(Procent.HundredProcent);
            else
                return true;
        }
        public float GetNameWeight()
        {
            return nameWeight;
        }
    }
}
