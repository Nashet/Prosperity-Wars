using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Nashet.ValueSpace;
using Nashet.Utils;
using Nashet.MarchingSquares;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// represents the world, doesn't care about Unity's specific API
    /// </summary>
    public class World : MonoBehaviour
    {
        private readonly static List<Province> allProvinces = new List<Province>();
        private readonly static List<Country> allCountries = new List<Country>();
        internal static Country UncolonizedLand;
        private static World thisObject;
        public static World Get
        {
            get { return thisObject; }            
        }
        private void Start()
        {
            thisObject = this;
        }
        static public IEnumerable<Country> getAllExistingCountries()
        {
            foreach (var country in allCountries)
                if (country.isAlive() && country != World.UncolonizedLand)
                    yield return country;
        }
        static public IEnumerable<Province> GetAllProvinces()
        {
            foreach (var item in allProvinces)
                yield return item;
        }
        static public IEnumerable<IInvestable> GetAllAllowedInvestments(Country includingCountry, Agent investor)
        {
            var countriesAllowingInvestments = getAllExistingCountries().Where(x => x.economy.getTypedValue().AllowForeignInvestments || x == includingCountry);
            foreach (var country in countriesAllowingInvestments)
                foreach (var item in country.GetAllInvestmentProjects(investor))
                    yield return item;
        }

        internal static IEnumerable<Factory> GetAllFactories()
        {
            foreach (var item in getAllExistingCountries())
                foreach (var factory in item.getAllFactories())
                    yield return factory;
        }

        public static IEnumerable<Agent> getAllAgents()
        {
            foreach (var country in World.getAllExistingCountries())
            {
                yield return country;
                foreach (var item in country.getAllAgents())
                    yield return item;
            }
        }
        internal static Money GetAllMoney()
        {
            Money allMoney = new Money(0f);
            foreach (Country country in getAllExistingCountries())
            {
                allMoney.Add(country.Cash);
                allMoney.Add(country.Bank.getReservs());
                foreach (Province province in country.ownedProvinces)
                {
                    foreach (var agent in province.getAllAgents())
                    {
                        allMoney.Add(agent.Cash);
                        //var isArtisan = agent as Artisans;
                        //if (isArtisan!=null && isArtisan.)
                    }

                }
            }
            allMoney.Add(Game.market.Cash);
            return allMoney;
        }

        public static Province FindProvince(Color color)
        {
            foreach (Province anyProvince in allProvinces)
                if (anyProvince.getColorID() == color)
                    return anyProvince;
            return null;
        }
        internal static Province FindProvince(int number)
        {
            foreach (var pro in allProvinces)
                if (pro.getID() == number)
                    return pro;
            return null;
        }
        static public void deleteSomeProvinces(List<Province> toDelete, bool addLakes)
        {
            //Province.allProvinces.FindAndDo(x => blockedProvinces.Contains(x.getColorID()), x => x.removeProvince());
            foreach (var item in allProvinces.ToArray())
                if (toDelete.Contains(item))
                {
                    allProvinces.Remove(item);
                    //item.removeProvince();
                }
            //todo move it in seaProvinces
            if (addLakes)
            {
                int howMuchLakes = allProvinces.Count / Options.ProvinceLakeShance + Game.Random.Next(3);
                for (int i = 0; i < howMuchLakes; i++)
                    allProvinces.Remove(allProvinces.Random());
            }
        }

        public static void preReadProvinces(MyTexture image)
        {
            ProvinceNameGenerator nameGenerator = new ProvinceNameGenerator();
            Color currentProvinceColor = image.GetPixel(0, 0);
            int provinceCounter = 0;
            for (int j = 0; j < image.getHeight(); j++) // circle by province        
                for (int i = 0; i < image.getWidth(); i++)
                {
                    if (currentProvinceColor != image.GetPixel(i, j)
                        //&& !blockedProvinces.Contains(currentProvinceColor)
                        && !isProvinceCreated(currentProvinceColor))
                    {
                        allProvinces.Add(new Province(nameGenerator.generateProvinceName(), provinceCounter, currentProvinceColor, Product.getRandomResource(false)));
                        provinceCounter++;

                    }
                    currentProvinceColor = image.GetPixel(i, j);
                    //game.updateStatus("Reading provinces.. x = " + i + " y = " + j);
                }
        }
        public static bool isProvinceCreated(Color color)
        {
            foreach (Province anyProvince in allProvinces)
                if (anyProvince.getColorID() == color)
                    return true;
            return false;
        }

        internal static void makeCountries()
        {
            UncolonizedLand = new Country("Uncolonized lands", new Culture("Ancient tribes"), Color.yellow, null, 0f);
            allCountries.Add(UncolonizedLand);
            UncolonizedLand.government.setValue(Government.Tribal);
            UncolonizedLand.economy.setValue(Economy.NaturalEconomy);

            var countryNameGenerator = new CountryNameGenerator();
            var cultureNameGenerator = new CultureNameGenerator();
            //int howMuchCountries =3;
            int howMuchCountries = allProvinces.Count / Options.ProvincesPerCountry;
            howMuchCountries += Game.Random.Next(6);
            if (howMuchCountries < 8)
                howMuchCountries = 8;

            for (int i = 0; i < howMuchCountries; i++)
            {
                //Game.updateStatus("Making countries.." + i);
                Culture cul = new Culture(cultureNameGenerator.generateCultureName());

                Province province = GetAllProvinces().Where(x => x.Country == null).Random();
                //Province.getRandomProvinceInWorld(x => x.Country == null);
                //&& !Game.seaProvinces.Contains(x));// Country.NullCountry);
                Country country = new Country(countryNameGenerator.generateCountryName(), cul, ColorExtensions.getRandomColor(), province, 100f);
                allCountries.Add(country);
                //count.setBank(count.bank);
                Game.Player = allCountries[1]; // not wild Tribes, DONT touch that
                province.InitialOwner(country);
                country.GiveMoneyFromNoWhere(100);
            }


            foreach (var pro in allProvinces)
                if (pro.Country == null)
                    pro.InitialOwner(World.UncolonizedLand);
        }
        static public void сreateRandomPopulation()
        {
            foreach (Province province in allProvinces)
            {
                if (province.Country == World.UncolonizedLand)
                {
                    new Tribesmen(PopUnit.getRandomPopulationAmount(500, 1000), province.Country.getCulture(), province);
                }
                else
                {
                    PopUnit pop;
                    //if (Game.devMode)
                    //    pop = new Tribesmen(2000, province.Country.getCulture(), province);
                    //else
                    new Tribesmen(PopUnit.getRandomPopulationAmount(3600, 4000), province.Country.getCulture(), province);

                    //if (Game.devMode)
                    //    pop = new Aristocrats(1000, province.Country.getCulture(), province);
                    //else
                    pop = new Aristocrats(PopUnit.getRandomPopulationAmount(800, 1000), province.Country.getCulture(), province);

                    pop.GiveMoneyFromNoWhere(900f);
                    pop.storage.add(new Storage(Product.Grain, 60f));
                    //if (!Game.devMode)
                    //{
                    //pop = new Capitalists(PopUnit.getRandomPopulationAmount(500, 800), Country.getCulture(), province);
                    //pop.Cash.set(9000);

                    pop = new Artisans(PopUnit.getRandomPopulationAmount(500, 800), province.Country.getCulture(), province);
                    pop.GiveMoneyFromNoWhere(900f);

                    pop = new Farmers(PopUnit.getRandomPopulationAmount(1000, 1100), province.Country.getCulture(), province);
                    pop.GiveMoneyFromNoWhere(20f);
                    //}
                    //province.allPopUnits.Add(new Workers(600, PopType.workers, Game.player.culture, province));              
                }
            }
        }
        static public List<Province> getSeaProvinces(MyTexture mapTexture, bool useProvinceColors)
        {
            List<Province> res = new List<Province>();
            if (!useProvinceColors)
            {
                Province seaProvince;
                for (int x = 0; x < mapTexture.getWidth(); x++)
                {
                    seaProvince = FindProvince(mapTexture.GetPixel(x, 0));
                    if (!res.Contains(seaProvince))
                        res.Add(seaProvince);
                    seaProvince = FindProvince(mapTexture.GetPixel(x, mapTexture.getHeight() - 1));
                    if (!res.Contains(seaProvince))
                        res.Add(seaProvince);
                }
                for (int y = 0; y < mapTexture.getHeight(); y++)
                {
                    seaProvince = FindProvince(mapTexture.GetPixel(0, y));
                    if (!res.Contains(seaProvince))
                        res.Add(seaProvince);
                    seaProvince = FindProvince(mapTexture.GetPixel(mapTexture.getWidth() - 1, y));
                    if (!res.Contains(seaProvince))
                        res.Add(seaProvince);
                }

                seaProvince = FindProvince(mapTexture.getRandomPixel());
                if (!res.Contains(seaProvince))
                    res.Add(seaProvince);

                if (Game.Random.Next(3) == 1)
                {
                    seaProvince = FindProvince(mapTexture.getRandomPixel());
                    if (!res.Contains(seaProvince))
                        res.Add(seaProvince);
                    if (Game.Random.Next(20) == 1)
                    {
                        seaProvince = FindProvince(mapTexture.getRandomPixel());
                        if (!res.Contains(seaProvince))
                            res.Add(seaProvince);
                    }
                }
            }
            else
            { // Victoria 2 format
                foreach (var item in World.GetAllProvinces())
                {
                    var color = item.getColorID();
                    if (color.g + color.b >= 200f / 255f + 200f / 255f && color.r < 96f / 255f)
                        //if (color.g + color.b + color.r > 492f / 255f)
                        res.Add(item);
                }
            }
            return res;
        }
        /// <summary>
        /// 
        /// </summary>        
        public static void Create(MyTexture map, bool isMapGenerated)
        {
            //FactoryType.getResourceTypes(); // FORCING FactoryType to initializate?

            // remake it on messages?
            //Game.updateStatus("Reading provinces..");
            World.preReadProvinces(map);
            var seaProvinces = World.getSeaProvinces(map, !isMapGenerated);
            World.deleteSomeProvinces(seaProvinces, isMapGenerated);

            // Game.updateStatus("Making countries..");
            World.makeCountries();

            //Game.updateStatus("Making population..");
            World.сreateRandomPopulation();

            setStartResources();

        }

        static private void setStartResources()
        {
            //Country.allCountries[0] is null country
            //Country.allCountries[1].Capital.setResource(Product.Wood);// player

            //Country.allCountries[0].Capital.setResource(Product.Wood;
            allCountries[2].Capital.setResource(Product.Fruit);
            allCountries[3].Capital.setResource(Product.Gold);
            allCountries[4].Capital.setResource(Product.Cotton);
            allCountries[5].Capital.setResource(Product.Stone);
            allCountries[6].Capital.setResource(Product.MetalOre);
            allCountries[7].Capital.setResource(Product.Wood);
        }


        // temporally
        internal static IEnumerable<KeyValuePair<IShareOwner, Procent>> GetAllShares()
        {
            foreach (var item in World.getAllExistingCountries())
                foreach (var factory in item.getAllFactories())
                    foreach (var record in factory.ownership.GetAllShares())
                        yield return record;
        }
        // temporally
        internal static IEnumerable<KeyValuePair<IShareable, Procent>> GetAllShares(IShareOwner owner)
        {
            foreach (var item in getAllExistingCountries())
                foreach (var factory in item.getAllFactories())
                    foreach (var record in factory.ownership.GetAllShares())
                        if (record.Key == owner)
                            yield return new KeyValuePair<IShareable, Procent>(factory, record.Value);
        }
    }
}