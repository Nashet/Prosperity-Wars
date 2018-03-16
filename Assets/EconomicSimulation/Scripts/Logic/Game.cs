using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Nashet.UnityUIUtils;
using Nashet.MarchingSquares;
using Nashet.ValueSpace;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    public class Game : ThreadedJob
    {
        static private readonly bool readMapFormFile = false;
        static private MyTexture mapTexture;
        static internal GameObject r3dTextPrefab;

        static public Country Player;

        static bool haveToRunSimulation;
        static bool haveToStepSimulation;

        static public System.Random Random = new System.Random();

        static public Province selectedProvince;
        static public Province previoslySelectedProvince;

        static internal List<BattleResult> allBattles = new List<BattleResult>();

        static public readonly Market market;

        static internal bool devMode = false;
        static private int mapMode;
        static private bool surrended = devMode;
        static internal Material defaultCountryBorderMaterial, defaultProvinceBorderMaterial, selectedProvinceBorderMaterial,
            impassableBorder;

        static private VoxelGrid grid;
        private readonly Rect mapBorders;

        static Game()
        {
            Product.init(); // to avoid crash based on initialization order
            market = new Market();
        }
        public Game()
        {
            if (readMapFormFile)
            {
                Texture2D mapImage = Resources.Load("provinces", typeof(Texture2D)) as Texture2D; ///texture;                
                mapTexture = new MyTexture(mapImage);
            }
            else
                generateMapImage();
            mapBorders = new Rect(0f, 0f, mapTexture.getWidth() * Options.cellMultiplier, mapTexture.getHeight() * Options.cellMultiplier);
        }
        public void InitializeNonUnityData()
        {
            market.initialize();

            World.Create(mapTexture, !readMapFormFile);
            //Game.updateStatus("Making grid..");
            grid = new VoxelGrid(mapTexture.getWidth(), mapTexture.getHeight(), Options.cellMultiplier * mapTexture.getWidth(), mapTexture, World.GetAllProvinces());

            if (!devMode)
                makeHelloMessage();
            updateStatus("Finishing generation..");
        }

        /// <summary>
        /// Separate method to call Unity API. WOULDN'T WORK IN MULTYTHREADING!
        /// Called after initialization of non-Unity data
        /// </summary>
        public static void setUnityAPI()
        {
            // Assigns a material named "Assets/Resources/..." to the object.
            //defaultCountryBorderMaterial = Resources.Load("materials/CountryBorder", typeof(Material)) as Material;
            defaultCountryBorderMaterial = GameObject.Find("CountryBorderMaterial").GetComponent<MeshRenderer>().material;

            //defaultProvinceBorderMaterial = Resources.Load("materials/ProvinceBorder", typeof(Material)) as Material;
            defaultProvinceBorderMaterial = GameObject.Find("ProvinceBorderMaterial").GetComponent<MeshRenderer>().material;

            //selectedProvinceBorderMaterial = Resources.Load("materials/SelectedProvinceBorder", typeof(Material)) as Material;
            selectedProvinceBorderMaterial = GameObject.Find("SelectedProvinceBorderMaterial").GetComponent<MeshRenderer>().material;

            //impassableBorder = Resources.Load("materials/ImpassableBorder", typeof(Material)) as Material;
            impassableBorder = GameObject.Find("ImpassableBorderMaterial").GetComponent<MeshRenderer>().material;

            //r3dTextPrefab = (GameObject)Resources.Load("prefabs/3dProvinceNameText", typeof(GameObject));
            r3dTextPrefab = GameObject.Find("3dProvinceNameText");


            World.GetAllProvinces().PerformAction(x => x.setUnityAPI(grid.getMesh(x), grid.getBorders()));
            World.GetAllProvinces().PerformAction(x => x.setBorderMaterials(false));
            Country.setUnityAPI();
            //seaProvinces = null;
            // todo clear resources
            grid = null;
            mapTexture = null;
            // Annex all countries to P)layer
            //foreach (var item in World.getAllExistingCountries().Where(x => x != Game.Player))
            //{
            //    item.annexTo(Game.Player);
            //}
        }
        public Rect getMapBorders()
        {
            return mapBorders;
        }

        internal static void GivePlayerControlOf(Country country)
        {
            //if (country != Country.NullCountry)
            {
                surrended = false;
                Player = country;
                MainCamera.politicsPanel.selectReform(null);
                MainCamera.inventionsPanel.selectInvention(null);

                // not necessary since it will change automatically on province selection
                MainCamera.buildPanel.selectFactoryType(null);

                MainCamera.refreshAllActive();
            }
        }

        public static void GivePlayerControlToAI()
        {
            surrended = true;
        }




        internal static int getMapMode()
        {
            return mapMode;
        }

        public static void redrawMapAccordingToMapMode(int newMapMode)
        {
            mapMode = newMapMode;
            foreach (var item in World.GetAllProvinces())
                item.updateColor(item.getColorAccordingToMapMode());
        }

        internal static void continueSimulation()
        {
            haveToRunSimulation = true;
        }

        internal static bool isRunningSimulation()
        {
            return (haveToRunSimulation || haveToStepSimulation);// && !MessagePanel.IsOpenAny();
        }
        internal static void pauseSimulation()
        {
            haveToRunSimulation = false;
        }
        internal static void makeOneStepSimulation()
        {
            haveToStepSimulation = true;
        }

        internal static bool isPlayerSurrended()
        {
            return surrended;
        }

        static void generateMapImage()
        {
            int mapSize;
            int width;
            //#if UNITY_WEBGL
            if (devMode)
            {
                mapSize = 20000;
                width = 150 + Random.Next(60);
            }
            else
            {
                //mapSize = 25000;
                //width = 170 + Random.Next(65);
                //mapSize = 30000;
                //width = 180 + Random.Next(65);
                mapSize = 40000;
                width = 250 + Random.Next(40);
            }
            // 140 is sqrt of 20000
            //int width = 30 + Random.Next(12);   // 140 is sqrt of 20000
            //#else
            //        int mapSize = 40000;
            //        int width = 200 + Random.Next(80);
            //#endif          
            Texture2D mapImage = new Texture2D(width, mapSize / width);        // standard for webGL


            Color emptySpaceColor = Color.black;//.setAlphaToZero();
            mapImage.setColor(emptySpaceColor);
            int amountOfProvince;

            amountOfProvince = mapImage.width * mapImage.height / 140 + Game.Random.Next(5);
            //amountOfProvince = 400 + Game.Random.Next(100);
            for (int i = 0; i < amountOfProvince; i++)
                mapImage.SetPixel(mapImage.getRandomX(), mapImage.getRandomY(), ColorExtensions.getRandomColor());

            int emptyPixels = 1;//non zero
            Color currentColor = mapImage.GetPixel(0, 0);
            int emergencyExit = 0;
            while (emptyPixels != 0 && emergencyExit < 100)
            {
                emergencyExit++;
                emptyPixels = 0;
                for (int j = 0; j < mapImage.height; j++) // circle by province        
                    for (int i = 0; i < mapImage.width; i++)
                    {
                        currentColor = mapImage.GetPixel(i, j);
                        if (currentColor == emptySpaceColor)
                            emptyPixels++;
                        else if (currentColor.a == 1f)
                        {
                            mapImage.drawRandomSpot(i, j, currentColor);
                        }
                    }
                mapImage.setAlphaToMax();
            }
            mapImage.Apply();
            mapTexture = new MyTexture(mapImage);
            Texture2D.Destroy(mapImage);
        }


        public static void prepareForNewTick()
        {
            Game.market.sentToMarket.setZero();
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
        static void makeHelloMessage()
        {
            Message.NewMessage("Tutorial", "Hi, this is VERY early demo of game-like economy simulator" +
                "\n\nCurrently there is: "
                + "\n\tpopulation agents \\ factories \\ countries \\ national banks"
                + "\n\tbasic trade \\ production \\ consumption \n\tbasic warfare \n\tbasic inventions"
                + "\n\tbasic reforms (population can vote for reforms)"
                + "\n\tpopulation demotion \\ promotion to other classes \n\tmigration \\ immigration \\ assimilation"
                + "\n\tpolitical \\ culture \\ core \\ resource map mode"
                + "\n\tmovements and rebellions"
                + "\n\nYou play as " + Game.Player.FullName + " You can try to growth economy or conquer the world."
                + "\n\nOr, You can give control to AI and watch it"
                + "\n\nTry arrows or WASD for scrolling map and mouse wheel for scale"
                + "\n'Enter' key to close top window, space - to pause \\ unpause"
                + "\n\n\nI have now Patreon page where I post about that game development. Try red button below!"
                + "\nAlso I would be thankful if you will share info about this project"
                , "Ok", false);
            //, Game.Player.Capital.getPosition()
        }

        private static void calcBattles()
        {
            foreach (Staff attacker in Staff.getAllStaffs().ToList())
            {
                foreach (var attackerArmy in attacker.getAttackingArmies().ToList())
                {
                    var movement = attacker as Movement;
                    if (movement == null || movement.isValidGoal()) // movements attack only if goal is still valid
                    {
                        var result = attackerArmy.attack(attackerArmy.getDestination());
                        if (result.isAttackerWon())
                        {
                            if (movement == null)
                                (attacker as Country).TakeProvince(attackerArmy.getDestination(), true);
                            //attackerArmy.getDestination().secedeTo(attacker as Country, true);
                            else
                                movement.onRevolutionWon();
                        }
                        else if (result.isDefenderWon())
                        {
                            if (movement != null)
                                movement.onRevolutionLost();
                        }
                        if (result.getAttacker() == Game.Player || result.getDefender() == Game.Player)
                            result.createMessage();
                    }
                    attackerArmy.sendTo(null); // go home
                }
                attacker.consolidateArmies();
            }


        }
        internal static void simulate()
        {
            if (Game.haveToStepSimulation)
                Game.haveToStepSimulation = false;

            Date.Simulate();
            // strongly before PrepareForNewTick
            Game.market.simulatePriceChangeBasingOnLastTurnData();

            // should be before PrepareForNewTick cause PrepareForNewTick hires dead workers on factories
            Game.calcBattles();

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
            // big AFTER all and get money for sold circle
            foreach (Country country in World.getAllExistingCountries())
            {
                country.getMoneyForSoldProduct();
                foreach (Province province in country.getAllProvinces())//Province.allProvinces)
                {
                    foreach (Factory factory in province.getAllFactories())
                    {
                        if (country.economy.getValue() == Economy.PlannedEconomy)
                            factory.OpenFactoriesPE();
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
                        if (Rand.Chance(Options.PopPopulationChangeChance))
                            if (pop.needsFulfilled.isSmallerOrEqual(Options.PopNeedsEscapingLimit))
                                pop.FindBetterLife();
                        if (Rand.Chance(Options.PopPopulationChangeChance))
                            pop.Assimilate();

                        if (country.economy.getValue() != Economy.PlannedEconomy)
                            Rand.Call(() => pop.invest(), Options.PopInvestRate);
                    }
                    if (country.isAI())
                        country.invest(province);
                    //if (Game.random.Next(3) == 0)
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

        protected override void ThreadFunction()
        {
            InitializeNonUnityData();
        }
    }
}