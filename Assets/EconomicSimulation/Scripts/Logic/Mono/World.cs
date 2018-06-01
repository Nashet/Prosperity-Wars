using System;
using System.Collections.Generic;
using System.Linq;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// represents the world, doesn't care about Unity's specific API
    /// </summary>
    public class World : MonoBehaviour
    {
        private static readonly List<Province> allProvinces = new List<Province>();
        private static readonly List<Country> allCountries = new List<Country>();
        private static readonly List<Culture> allCultures = new List<Culture>();

        internal static readonly Country UncolonizedLand;


        private static bool haveToRunSimulation;
        private static bool haveToStepSimulation;

        internal static List<BattleResult> allBattles = new List<BattleResult>();

        public static Market market;
        public Graph graph;


        private static World thisObject;
        public static World Get
        {
            get { return thisObject; }
        }
        private void Start()
        {
            thisObject = this;
        }

        static World()
        {
            //Product.init(); // to avoid crash based on initialization order
            market = new Market();

            var culture = new Culture("Ancient tribes", Color.yellow);
            allCultures.Add(culture);
            UncolonizedLand = new Country("Uncolonized lands", culture, culture.getColor(), null, 0f);
            allCountries.Add(UncolonizedLand);
            UncolonizedLand.government.setValue(Government.Tribal);
            UncolonizedLand.economy.setValue(Economy.NaturalEconomy);
        }

        public static IEnumerable<Army> AllArmies()
        {
            foreach (var country in World.getAllExistingCountries())
            {
                foreach (var army in country.getAllArmies())
                {
                    yield return army;
                }
                foreach (var movement in country.movements)
                    foreach (var army in movement.getAllArmies())
                    {
                        yield return army;
                    }
            }
            foreach (var army in UncolonizedLand.getAllArmies())
            {
                yield return army;
            }
        }

        public static IEnumerable<Country> getAllExistingCountries()
        {
            foreach (var country in allCountries)
                if (country.isAlive() && country != UncolonizedLand)
                    yield return country;
        }

        public static IEnumerable<Province> GetAllProvinces()
        {
            foreach (var item in allProvinces)
                yield return item;
        }

        /// <summary>
        /// Gives list of allowed IInvestable with pre-calculated Margin in Value. Doesn't check if it's invented
        /// </summary>
        public static IEnumerable<KeyValuePair<IInvestable, Procent>> GetAllAllowedInvestments(Agent investor)
        {
            Country includingCountry = investor.Country;
            var countriesAllowingInvestments = getAllExistingCountries().Where(x => x.economy.getTypedValue().AllowForeignInvestments || x == includingCountry);
            foreach (var country in countriesAllowingInvestments)
                foreach (var item in country.allInvestmentProjects.Get())//investor
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
            foreach (var country in getAllExistingCountries())
            {
                yield return country;
                foreach (var item in country.getAllAgents())
                    yield return item;
            }
        }

        internal static Money GetAllMoney()
        {
            Money allMoney = new Money(0m);
            foreach (Country country in getAllExistingCountries())
            {
                allMoney.Add(country.Cash);
                foreach (var agent in country.getAllAgents())
                {
                    allMoney.Add(agent.Cash);
                    //var isArtisan = agent as Artisans;
                    //if (isArtisan!=null && isArtisan.)
                }
            }
            allMoney.Add(World.market.Cash);
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

        public static void deleteSomeProvinces(List<Province> toDelete, bool addLakes)
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
                int howMuchLakes = allProvinces.Count / Options.ProvinceLakeShance + Rand.Get.Next(3);
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

        internal void ResumeSimulation()
        {
            haveToRunSimulation = true;
        }

        internal bool IsRunning
        {
            get { return (haveToRunSimulation || haveToStepSimulation); }// && !MessagePanel.IsOpenAny();
        }

        internal void PauseSimulation()
        {
            haveToRunSimulation = false;
        }

        internal void MakeOneStepSimulation()
        {
            haveToStepSimulation = true;
        }

        public static bool isProvinceCreated(Color color)
        {
            foreach (Province anyProvince in allProvinces)
                if (anyProvince.getColorID() == color)
                    return true;
            return false;
        }

        internal static void CreateCountries()
        {
            var countryNameGenerator = new CountryNameGenerator();
            var cultureNameGenerator = new CultureNameGenerator();
            //int howMuchCountries =3;
            int howMuchCountries = allProvinces.Count / Options.ProvincesPerCountry;
            howMuchCountries += Rand.Get.Next(6);
            if (howMuchCountries < 8)
                howMuchCountries = 8;

            for (int i = 0; i < howMuchCountries; i++)
            {
                //Game.updateStatus("Making countries.." + i);

                Culture culture = new Culture(cultureNameGenerator.generateCultureName(), ColorExtensions.getRandomColor());
                allCultures.Add(culture);

                Province province = GetAllProvinces().Where(x => x.Country == UncolonizedLand).Random();

                Country country = new Country(countryNameGenerator.generateCountryName(), culture, culture.getColor(), province, 100f);
                allCountries.Add(country);
                //count.setBank(count.bank);

                country.GiveMoneyFromNoWhere(100);
            }
            Game.Player = allCountries[1]; // not wild Tribes, DONT touch that

            allCountries.Random().SetName("Zacharia");
            //foreach (var pro in allProvinces)
            //    if (pro.Country == null)
            //        pro.InitialOwner(World.UncolonizedLand);
        }

        public static void CreateRandomPopulation()
        {
            foreach (Province province in allProvinces)
            {
                if (province.Country == UncolonizedLand)
                {
                    //1500-2000
                    //new Tribesmen(PopUnit.getRandomPopulationAmount(300, 400), province.Country.getCulture(), province);
                    //new Aristocrats(PopUnit.getRandomPopulationAmount(300, 400), province.Country.getCulture(), province);
                    new Tribesmen(PopUnit.getRandomPopulationAmount(1500, 2000), province.Country.getCulture(), province);
                    //new Tribesmen(PopUnit.getRandomPopulationAmount(2000, 2500), province.Country.getCulture(), province);
                }
                else
                {
                    PopUnit pop;
                    //if (Game.devMode)
                    //    pop = new Tribesmen(2000, province.Country.getCulture(), province);
                    //else
                    //new Tribesmen(PopUnit.getRandomPopulationAmount(11000, 12000), province.Country.getCulture(), province);
                    //new Tribesmen(PopUnit.getRandomPopulationAmount(3100, 3200), province.Country.getCulture(), province);
                    new Tribesmen(PopUnit.getRandomPopulationAmount(200, 300), province.Country.getCulture(), province);

                    //if (Game.devMode)
                    //    pop = new Aristocrats(1000, province.Country.getCulture(), province);
                    //else
                    pop = new Aristocrats(PopUnit.getRandomPopulationAmount(500, 1000), province.Country.getCulture(), province);

                    pop.GiveMoneyFromNoWhere(900m);
                    pop.storage.add(new Storage(Product.Grain, 60f));
                    //if (!Game.devMode)
                    //{
                    //pop = new Capitalists(PopUnit.getRandomPopulationAmount(500, 800), Country.getCulture(), province);
                    //pop.Cash.set(9000);

                    pop = new Artisans(PopUnit.getRandomPopulationAmount(400, 500), province.Country.getCulture(), province);
                    pop.GiveMoneyFromNoWhere(900m);

                    pop = new Farmers(PopUnit.getRandomPopulationAmount(8200, 9000), province.Country.getCulture(), province);
                    pop.GiveMoneyFromNoWhere(20m);

                    new Workers(PopUnit.getRandomPopulationAmount(500, 800), province.Country.getCulture(), province);
                    //}
                    //province.allPopUnits.Add(new Workers(600, PopType.workers, Game.player.culture, province));
                    //break;
                }
            }
        }

        public static List<Province> getSeaProvinces(MyTexture mapTexture, bool useProvinceColors)
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

                if (Rand.Get.Next(3) == 1)
                {
                    seaProvince = FindProvince(mapTexture.getRandomPixel());
                    if (!res.Contains(seaProvince))
                        res.Add(seaProvince);
                    if (Rand.Get.Next(20) == 1)
                    {
                        seaProvince = FindProvince(mapTexture.getRandomPixel());
                        if (!res.Contains(seaProvince))
                            res.Add(seaProvince);
                    }
                }
            }
            else
            { // Victoria 2 format
                foreach (var item in GetAllProvinces())
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
        /// Could run in threads
        /// </summary>
        public static void Create(MyTexture map, bool isMapGenerated)
        {
            //FactoryType.getResourceTypes(); // FORCING FactoryType to initialize?

            // remake it on messages?
            //Game.updateStatus("Reading provinces..");
            preReadProvinces(map);
            var seaProvinces = getSeaProvinces(map, !isMapGenerated);
            deleteSomeProvinces(seaProvinces, isMapGenerated);

            // Game.updateStatus("Making countries..");
            CreateCountries();

            //Game.updateStatus("Making population..");
            CreateRandomPopulation();

            setStartResources();
            //foreach (var item in World.getAllExistingCountries())
            //{
            //    item.Capital.OnSecedeTo(item, false);
            //}
        }

        private static void setStartResources()
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
            foreach (var item in getAllExistingCountries())
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

        public static IEnumerable<PopUnit> GetAllPopulation()
        {
            foreach (var country in getAllExistingCountries())
            {
                foreach (var item in country.GetAllPopulation())
                    yield return item;
            }
        }

        /// <summary>
        /// Returns last escape type - demotion, migration or immigration
        /// </summary>
        public IEnumerable<KeyValuePair<IWayOfLifeChange, int>> getAllPopulationChanges()
        {
            foreach (var item in GetAllPopulation())
                foreach (var record in item.getAllPopulationChanges())
                    yield return record;
        }

        //private static void calcBattles()
        //{
        //    foreach (Staff attacker in Staff.getAllStaffs().ToList())
        //    {
        //        foreach (var attackerArmy in attacker.getAttackingArmies().ToList())
        //        {
        //            var movement = attacker as Movement;
        //            if (movement == null || movement.isValidGoal()) // movements attack only if goal is still valid
        //            {
        //                var result = attackerArmy.attack(attackerArmy.getDestination());
        //                if (result.isAttackerWon())
        //                {
        //                    if (movement == null)
        //                        (attacker as Country).TakeProvince(attackerArmy.getDestination(), true);
        //                    //attackerArmy.getDestination().secedeTo(attacker as Country, true);
        //                    else
        //                    {
        //                        if (movement.getReformType() == null)//separatists
        //                            movement.onRevolutionWon();
        //                        else
        //                            movement.getReformType().setValue(movement.getGoal());

        //                    }
        //                }
        //                else if (result.isDefenderWon())
        //                {
        //                    if (movement != null)
        //                        movement.onRevolutionLost();
        //                }
        //                if (result.getAttacker() == Game.Player || result.getDefender() == Game.Player)
        //                    result.createMessage();
        //            }
        //            attackerArmy.sendTo(null); // go home
        //        }
        //        //attacker.consolidateArmies();
        //    }
        //}

        public static void prepareForNewTick()
        {
            World.market.SetStatisticToZero();

            foreach (Country country in World.getAllExistingCountries())
            {
                country.SetStatisticToZero();
                foreach (Province province in country.getAllProvinces())
                {
                    province.BalanceEmployableWorkForce();
                    {
                        foreach (var item in province.getAllAgents())
                            item.SetStatisticToZero();
                    }
                }
            }
            PopType.sortNeeds();
            Product.sortSubstitutes();
        }

        internal static void simulate()
        {
            if (haveToStepSimulation)
                haveToStepSimulation = false;

            Date.Simulate();
            if (Game.devMode)
                Debug.Log("New date! - " + Date.Today);
            // strongly before PrepareForNewTick
            World.market.simulatePriceChangeBasingOnLastTurnData();

            // rise event on day passed
            EventHandler handler = DayPassed;
            if (handler != null)
            {
                handler(World.Get, EventArgs.Empty);
            }

            // should be before PrepareForNewTick cause PrepareForNewTick hires dead workers on factories
            //calcBattles();

            // includes workforce balancing
            // and sets statistics to zero. Should go after price calculation
            prepareForNewTick();

            // big PRODUCE circle
            foreach (Country country in World.getAllExistingCountries())
                foreach (Province province in country.getAllProvinces())
                    foreach (var producer in province.getAllProducers())
                        producer.produce();

            // big CONCUME circle
            foreach (Country country in World.getAllExistingCountries())
            {
                country.consumeNeeds();
                if (country.economy.getValue() == Economy.PlannedEconomy)
                {
                    //consume in PE order
                    foreach (Factory factory in country.getAllFactories())
                        factory.consumeNeeds();

                    if (country.Invented(Invention.ProfessionalArmy))
                        foreach (var item in country.GetAllPopulation(PopType.Soldiers))
                            item.consumeNeeds();

                    foreach (var item in country.GetAllPopulation(PopType.Workers))
                        item.consumeNeeds();

                    foreach (var item in country.GetAllPopulation(PopType.Farmers))
                        item.consumeNeeds();

                    foreach (var item in country.GetAllPopulation(PopType.Tribesmen))
                        item.consumeNeeds();
                }
                else  //consume in regular order
                    foreach (Province province in country.getAllProvinces())//Province.allProvinces)
                    {
                        foreach (Factory factory in province.getAllFactories())
                        {
                            factory.consumeNeeds();
                        }
                        foreach (PopUnit pop in province.GetAllPopulation())
                        {
                            //That placed here to avoid issues with Aristocrats and Clerics
                            //Otherwise Aristocrats starts to consume BEFORE they get all what they should
                            if (country.serfdom.getValue() == Serfdom.Allowed || country.serfdom.getValue() == Serfdom.Brutal)
                                if (pop.shouldPayAristocratTax())
                                    pop.payTaxToAllAristocrats();
                        }
                        foreach (PopUnit pop in province.GetAllPopulation())
                        {
                            pop.consumeNeeds();
                        }
                    }
            }
            //force DSB recalculation
            World.market.getDemandSupplyBalance(null, true);
            if (Game.logMarket)
            {
                ValueSpace.Money res = new ValueSpace.Money(0m);
                foreach (var product in Product.getAll())

                    res.Add(World.market.getCost(World.market.getMarketSupply(product, true)).Copy().Multiply((decimal)World.market.getDemandSupplyBalance(product, false))
                        );
                if (!World.market.moneyIncomeThisTurn.IsEqual(res))
                {
                    Debug.Log("Market income: " + World.market.moneyIncomeThisTurn + " total: " + World.market.Cash);
                    Debug.Log("Should pay: " + res);
                }
            }
            // big AFTER all and get money for sold circle
            foreach (Country country in World.getAllExistingCountries())
            {
                country.getMoneyForSoldProduct();
                foreach (Province province in country.getAllProvinces())//Province.allProvinces)
                {
                    foreach (Factory factory in province.getAllFactories())
                    {
                        if (country.economy.getValue() == Economy.PlannedEconomy)
                        {
                            if (country.isAI() && factory.IsClosed && !factory.isBuilding())
                                Rand.Call(() => factory.open(country, false), Options.howOftenCheckForFactoryReopenning);
                        }
                        else
                        {
                            factory.getMoneyForSoldProduct();
                            factory.ChangeSalary();
                            factory.paySalary(); // workers get gold or food here
                            factory.payDividend(); // also pays taxes inside
                            factory.CloseUnprofitable();
                            factory.ownership.CalcMarketPrice();
                            Rand.Call(() =>
                            {
                                factory.ownership.SellLowMarginShares();
                            }, 20);
                        }
                    }
                    province.DestroyAllMarkedfactories();
                    // get pop's income section:
                    foreach (PopUnit pop in province.GetAllPopulation())
                    {
                        if (pop.Type == PopType.Workers)
                            pop.LearnByWork();
                        if (pop.canSellProducts())
                            pop.getMoneyForSoldProduct();
                        pop.takeUnemploymentSubsidies();
                        if (country.Invented(Invention.ProfessionalArmy) && country.economy.getValue() != Economy.PlannedEconomy)
                        // don't need salary with PE
                        {
                            var soldier = pop as Soldiers;
                            if (soldier != null)
                                soldier.takePayCheck();
                        }
                        //because income come only after consuming, and only after FULL consumption
                        //if (pop.canTrade() && pop.hasToPayGovernmentTaxes())
                        // POps who can't trade will pay tax BEFORE consumption, not after
                        // Otherwise pops who can't trade avoid tax
                        // pop.Country.TakeIncomeTax(pop, pop.moneyIncomethisTurn, pop.Type.isPoorStrata());//pop.payTaxes();
                        pop.calcLoyalty();

                        if (Rand.Chance(Options.PopPopulationChangeChance))
                            pop.Growth();

                        if (Rand.Chance(Options.PopPopulationChangeChance))
                            pop.Promote();

                        if (pop.needsFulfilled.isSmallerOrEqual(Options.PopNeedsEscapingLimit))
                            if (Rand.Chance(Options.PopPopulationChangeChance))
                                pop.ChangeLife(pop.GetAllPossibleDemotions().Where(x => x.Value.isBiggerThan(pop.needsFulfilled, Options.PopNeedsEscapingBarrier)).MaxBy(x => x.Value.get()).Key, Options.PopDemotingSpeed);

                        if (Rand.Chance(Options.PopPopulationChangeChance))
                            pop.ChangeLife(pop.GetAllPossibleMigrations().Where(x => x.Value.isBiggerThan(pop.needsFulfilled, Options.PopNeedsEscapingBarrier)).MaxBy(x => x.Value.get()).Key, Options.PopMigrationSpeed);

                        if (Rand.Chance(Options.PopPopulationChangeChance))
                            pop.Assimilate();
                    }
                }
            }
            //investments circle. Needs to be separate, otherwise cashed  investments can conflict
            foreach (Country country in World.getAllExistingCountries())
            {
                foreach (var province in country.getAllProvinces())
                {
                    foreach (var pop in province.GetAllPopulation())
                    {
                        if (country.economy.getValue() != Economy.PlannedEconomy)
                            Rand.Call(() => pop.invest(), Options.PopInvestRate);
                    }

                    if (country.isAI())
                        country.invest(province);
                    //if (Rand.random2.Next(3) == 0)
                    //    province.consolidatePops();
                    province.RemoveDeadPops();
                    foreach (PopUnit pop in PopUnit.PopListToAddToGeneralList)
                    {
                        PopUnit targetToMerge = pop.Province.getSimilarPopUnit(pop);
                        if (targetToMerge == null)
                            pop.Province.RegisterPop(pop);
                        else
                            targetToMerge.mergeIn(pop);
                    }

                    PopUnit.PopListToAddToGeneralList.Clear();
                    province.simulate();
                }
                country.simulate();
                if (country.isAI())
                    country.AIThink();
            }
        }
        public static event EventHandler DayPassed;

    }
}