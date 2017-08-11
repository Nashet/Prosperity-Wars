using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System;
using System.Linq;

public class Game : ThreadedJob
{
    private static readonly bool readMapFormFile = false;
    static MyTexture map;
    public static GameObject mapObject;
    internal static GameObject r3dTextPrefab;

    public static Country Player;

    static bool haveToRunSimulation;
    static bool haveToStepSimulation;
    internal static int howMuchPausedWindowsOpen = 0;

    public static System.Random Random = new System.Random();

    public static Province selectedProvince;
    public static List<PopUnit> popsToShowInPopulationPanel = new List<PopUnit>();
    public static List<Factory> factoriesToShowInProductionPanel;

    internal static List<BattleResult> allBattles = new List<BattleResult>();
    internal readonly static Stack<Message> MessageQueue = new Stack<Message>();
    public readonly static Market market = new Market();

    internal static StringBuilder threadDangerSB = new StringBuilder();

    public static DateTime date = new DateTime(50, 1, 1);
    internal static bool devMode = false;
    private static int mapMode;
    private static bool surrended = true;
    internal static Material defaultCountryBorderMaterial, defaultProvinceBorderMaterial, selectedProvinceBorderMaterial,
        impassableBorder;
    private readonly Rect mapBorders;

    internal static List<Province> seaProvinces;
    static VoxelGrid grid;

    public Game()
    {
        if (readMapFormFile)
        {
            Texture2D mapImage = Resources.Load("provinces", typeof(Texture2D)) as Texture2D; ///texture;                
            map = new MyTexture(mapImage);
        }
        else
            generateMapImage();
        mapBorders = new Rect(0f, 0f, map.getWidth() * Options.cellMultiplier, map.getHeight() * Options.cellMultiplier);
    }
    public void initialize()
    {

        market.initialize();
        makeFactoryTypes();

        updateStatus("Reading provinces..");
        Province.preReadProvinces(Game.map, this);
        seaProvinces = getSeaProvinces();
        deleteSomeProvinces();

        updateStatus("Making grid..");
        grid = new VoxelGrid(map.getWidth(), map.getHeight(), Options.cellMultiplier * map.getWidth(), map, Game.seaProvinces, this, Province.allProvinces);

        updateStatus("Making countries..");
        Country.makeCountries(this);

        updateStatus("Making population..");
        CreateRandomPopulation();

        setStartResources();
        makeHelloMessage();
        updateStatus("Finishing generation..");
        

    }
    public static void setUnityAPI()
    {
        // Assigns a material named "Assets/Resources/..." to the object.
        defaultCountryBorderMaterial = Resources.Load("materials/CountryBorder", typeof(Material)) as Material;
        defaultProvinceBorderMaterial = Resources.Load("materials/ProvinceBorder", typeof(Material)) as Material;
        selectedProvinceBorderMaterial = Resources.Load("materials/SelectedProvinceBorder", typeof(Material)) as Material;
        impassableBorder = Resources.Load("materials/ImpassableBorder", typeof(Material)) as Material;
        r3dTextPrefab = (GameObject)Resources.Load("prefabs/3dProvinceNameText", typeof(GameObject));

        mapObject = GameObject.Find("MapObject");
        Province.generateUnityData(grid);
        Country.setUnityAPI();
        seaProvinces = null;
        grid = null;
        map = null;
        // Annex all countries to P)layer
        //foreach (var item in Country.allCountries)
        //{
        //    item.annexTo(Game.Player);
        //}
    }
    public Rect getMapBorders()
    {
        return mapBorders;
    }
    static List<Province> getSeaProvinces()
    {
        List<Province> res = new List<Province>();
        if (!readMapFormFile)
        {
            Province seaProvince;
            for (int x = 0; x < map.getWidth(); x++)
            {
                seaProvince = Province.find(map.GetPixel(x, 0));
                if (!res.Contains(seaProvince))
                    res.Add(seaProvince);
                seaProvince = Province.find(map.GetPixel(x, map.getHeight() - 1));
                if (!res.Contains(seaProvince))
                    res.Add(seaProvince);
            }
            for (int y = 0; y < map.getHeight(); y++)
            {
                seaProvince = Province.find(map.GetPixel(0, y));
                if (!res.Contains(seaProvince))
                    res.Add(seaProvince);
                seaProvince = Province.find(map.GetPixel(map.getWidth() - 1, y));
                if (!res.Contains(seaProvince))
                    res.Add(seaProvince);
            }

            seaProvince = Province.find(map.getRandomPixel());
            if (!res.Contains(seaProvince))
                res.Add(seaProvince);

            if (Game.Random.Next(3) == 1)
            {
                seaProvince = Province.find(map.getRandomPixel());
                if (!res.Contains(seaProvince))
                    res.Add(seaProvince);
                if (Game.Random.Next(20) == 1)
                {
                    seaProvince = Province.find(map.getRandomPixel());
                    if (!res.Contains(seaProvince))
                        res.Add(seaProvince);
                }
            }
        }
        else
        {
            foreach (var item in Province.allProvinces)
            {
                var color = item.getColorID();
                if (color.g + color.b >= 200f / 255f + 200f / 255f && color.r < 96f / 255f)
                    //if (color.g + color.b + color.r > 492f / 255f)
                    res.Add(item);

            }
        }
        return res;
    }
    internal static void takePlayerControlOfThatCountry(Country country)
    {
        if (country != Country.NullCountry)
        {
            surrended = false;
            Player = country;
            MainCamera.refreshAllActive();
        }
    }

    public static void givePlayerControlToAI()
    {
        surrended = true;
    }
    static private void deleteSomeProvinces()
    {
        //Province.allProvinces.FindAndDo(x => blockedProvinces.Contains(x.getColorID()), x => x.removeProvince());
        foreach (var item in Province.allProvinces.ToArray())
            if (seaProvinces.Contains(item))
            {
                Province.allProvinces.Remove(item);
                //item.removeProvince();
            }
        //todo move it in seaProvinces
        if (!readMapFormFile)
        {
            int howMuchLakes = Province.allProvinces.Count / Options.ProvinceLakeShance + Game.Random.Next(3);
            for (int i = 0; i < howMuchLakes; i++)
                Province.allProvinces.Remove(Province.allProvinces.PickRandom());
        }
    }

    static private void setStartResources()
    {
        //Country.allCountries[0] is null country
        //Country.allCountries[1].getCapital().setResource(Product.Wood);// player

        //Country.allCountries[0].getCapital().setResource(Product.Wood;
        Country.allCountries[2].getCapital().setResource(Product.Fruit);
        Country.allCountries[3].getCapital().setResource(Product.Gold);
        Country.allCountries[4].getCapital().setResource(Product.Wool);
        Country.allCountries[5].getCapital().setResource(Product.Stone);
        Country.allCountries[6].getCapital().setResource(Product.MetallOre);
        Country.allCountries[7].getCapital().setResource(Product.Wood);
    }

    internal static int getMapMode()
    {
        return mapMode;
    }
    public static Color getProvinceColorAccordingToMapMode(Province province)
    {
        switch (mapMode)
        {
            case 0: //political mode                
                return province.getColor();
            case 1: //culture mode
                return Country.allCountries.Find(x => x.getCulture() == province.getMajorCulture()).getColor();
            case 2: //cores mode
                if (Game.selectedProvince == null)
                {
                    if (province.isCoreFor(province.getCountry()))
                        return province.getCountry().getColor();
                    else
                    {
                        var c = province.getRandomCore();
                        if (c == null)
                            return Color.yellow;
                        else
                            return c.getColor();
                    }
                }
                else
                {
                    if (province.isCoreFor(Game.selectedProvince.getCountry()))
                        return Game.selectedProvince.getCountry().getColor();
                    else
                    {
                        if (province.isCoreFor(province.getCountry()))
                            return province.getCountry().getColor();
                        else
                        {
                            var so = province.getRandomCore(x => x.isAlive());
                            if (so != null)
                                return so.getColor();
                            else
                            {
                                var c = province.getRandomCore();
                                if (c == null)
                                    return Color.yellow;
                                else
                                    return c.getColor();
                            }
                        }
                    }
                }
            default:
                return default(Color);
        }
    }
    public static void redrawMapAccordingToMapMode(int newMapMode)
    {
        mapMode = newMapMode;
        foreach (var item in Province.allProvinces)
            item.updateColor(getProvinceColorAccordingToMapMode(item));
    }

    internal static void continueSimulation()
    {
        haveToRunSimulation = true;
    }

    internal static bool isRunningSimulation()
    {
        return haveToRunSimulation || haveToStepSimulation;
    }
    internal static void pauseSimulation()
    {
        haveToRunSimulation = false;
    }
    internal static void makeOneStepSimulation()
    {
        haveToStepSimulation = true;
    }

    static void makeFactoryTypes()
    {
        new FactoryType("Forestry", new Storage(Product.Wood, 2f), null, false);
        new FactoryType("Gold pit", new Storage(Product.Gold, 2f), null, true);
        new FactoryType("Metal pit", new Storage(Product.MetallOre, 2f), null, true);
        new FactoryType("Sheepfold", new Storage(Product.Wool, 2f), null, false);
        new FactoryType("Quarry", new Storage(Product.Stone, 2f), null, true);
        new FactoryType("Orchard", new Storage(Product.Fruit, 2f), null, false);

        PrimitiveStorageSet resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Lumber, 1f));
        new FactoryType("Furniture factory", new Storage(Product.Furniture, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Wood, 1f));
        new FactoryType("Sawmill", new Storage(Product.Lumber, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Wood, 0.5f));
        resourceInput.set(new Storage(Product.MetallOre, 2f));
        new FactoryType("Metal smelter", new Storage(Product.Metal, 4f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Wool, 1f));
        new FactoryType("Weaver factory", new Storage(Product.Clothes, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Wood, 0.5f));
        resourceInput.set(new Storage(Product.Stone, 2f));
        new FactoryType("Cement factory", new Storage(Product.Cement, 4f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Fruit, 1f));
        new FactoryType("Winery", new Storage(Product.Wine, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Smithery", new Storage(Product.ColdArms, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Stone, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Ammunition factory", new Storage(Product.Ammunition, 4f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Lumber, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Firearms factory", new Storage(Product.Firearms, 4f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Lumber, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Artillery factory", new Storage(Product.Artillery, 4f), resourceInput, false);


        new FactoryType("Oil rig", new Storage(Product.Oil, 2f), null, true);
        new FactoryType("Rubber plantation", new Storage(Product.Rubber, 1f), null, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Oil, 1f));
        new FactoryType("Oil refinery", new Storage(Product.Fuel, 2f), resourceInput, false);


        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Metal, 1f));
        new FactoryType("Machinery factory", new Storage(Product.Machinery, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Machinery, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        resourceInput.set(new Storage(Product.Rubber, 1f));
        new FactoryType("Car factory", new Storage(Product.Cars, 6f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Machinery, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        resourceInput.set(new Storage(Product.Artillery, 1f));
        new FactoryType("Tank factory", new Storage(Product.Tanks, 6f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.set(new Storage(Product.Lumber, 1f));
        resourceInput.set(new Storage(Product.Metal, 1f));
        resourceInput.set(new Storage(Product.Machinery, 1f));
        new FactoryType("Airplane factory", new Storage(Product.Airplanes, 6f), resourceInput, false);
    }


    internal static Value getAllMoneyInWorld()
    {
        Value allMoney = new Value(0f);
        foreach (Country country in Country.allCountries)
        {
            allMoney.add(country.cash);
            allMoney.add(country.bank.getReservs());
            foreach (Province province in country.ownedProvinces)
            {
                foreach (var factory in province.getAgents())
                    allMoney.add(factory.cash);
            }
        }
        allMoney.add(Game.market.cash);
        return allMoney;
    }
    static void CreateRandomPopulation()
    {

        foreach (Province province in Province.allProvinces)
        {
            if (province.getCountry() == Country.NullCountry)
            {
                Tribemen f = new Tribemen(PopUnit.getRandomPopulationAmount(500, 1000), province.getCountry().getCulture(), province);
            }
            else
            {
                PopUnit pop;
                if (Game.devMode)
                    pop = new Tribemen(2000, province.getCountry().getCulture(), province);
                else
                    pop = new Tribemen(PopUnit.getRandomPopulationAmount(1800, 2000), province.getCountry().getCulture(), province);


                if (province.getCountry() == Game.Player)
                {
                    //pop = new Tribesmen(20900, PopType.tribeMen, province.getOwner().culture, province);
                    //province.allPopUnits.Add(pop);
                }
                if (Game.devMode)
                    pop = new Aristocrats(100, province.getCountry().getCulture(), province);
                else
                    pop = new Aristocrats(PopUnit.getRandomPopulationAmount(800, 1000), province.getCountry().getCulture(), province);


                pop.cash.set(9000);
                pop.storageNow.add(new Storage(Product.Food, 60f));
                if (!Game.devMode)
                {
                    //pop = new Capitalists(PopUnit.getRandomPopulationAmount(500, 800), getCountry().getCulture(), province);
                    //pop.cash.set(9000);

                    pop = new Artisans(PopUnit.getRandomPopulationAmount(500, 800), province.getCountry().getCulture(), province);
                    pop.cash.set(900);

                    pop = new Farmers(PopUnit.getRandomPopulationAmount(10000, 12000), province.getCountry().getCulture(), province);
                    pop.cash.set(20);

                }
                //province.allPopUnits.Add(new Workers(600, PopType.workers, Game.player.culture, province));              

            }

        }
    }

    internal static bool isPlayerSurrended()
    {
        return surrended;
    }

    static void generateMapImage()
    {
        //Texture2D mapImage = new Texture2D(100, 100);
#if UNITY_WEBGL
        int mapSize = 20000;//30000;
#else
        int mapSize = 60000;
#endif
        int width = 150 + Random.Next(150);
        Texture2D mapImage = new Texture2D(width, mapSize / width);        // standard for webGL
        //Texture2D mapImage = new Texture2D(180 + Random.Next(100), 180 + Random.Next(100));

        Color emptySpaceColor = Color.black;//.setAlphaToZero();
        mapImage.setColor(emptySpaceColor);
        int amountOfProvince;
        //if (Game.devMode)
        //    amountOfProvince = 7;
        //else
        //    amountOfProvince = 12 + Game.Random.Next(8);
        //amountOfProvince = 40 + Game.Random.Next(20);
        //amountOfProvince = 160 + Game.Random.Next(20);
        amountOfProvince = mapImage.width * mapImage.height / 140 + Game.Random.Next(5);
        //amountOfProvince = 400 + Game.Random.Next(100);
        for (int i = 0; i < amountOfProvince; i++)
            mapImage.SetPixel(mapImage.getRandomX(), mapImage.getRandomY(), ColorExtensions.getRandomColor());

        int emptyPixels = int.MaxValue;
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
        map = new MyTexture(mapImage);
        Texture2D.Destroy(mapImage);
    }

    static bool FindProvinceCenters()
    {
        //Vector3 accu = new Vector3(0, 0, 0);
        //foreach (Province pro in Province.allProvinces)
        //{
        //    accu.Set(0, 0, 0);
        //    foreach (var c in pro.mesh.vertices)
        //        accu += c;
        //    accu = accu / pro.mesh.vertices.Length;
        //    pro.centre = accu;
        //}
        return true;

        //short[,] bordersMarkers = new short[mapImage.width, mapImage.height];

        //int foundedProvinces = 0;
        //Color currentColor;
        //short borderDeepLevel = 0;
        //short alphaChangeForLevel = 1;
        //float defaultApha = 1f;
        //int placedMarkers = 456;//random number
        ////while (Province.allProvinces.Count != foundedProvinces)

        //foreach (Province pro in Province.allProvinces)
        //{
        //    borderDeepLevel = -1;
        //    placedMarkers = int.MaxValue;
        //    int emergencyExit = 200;
        //    while (placedMarkers != 0)
        //    {
        //        emergencyExit--;
        //        if (emergencyExit == 0)
        //            break;
        //        placedMarkers = 0;
        //        borderDeepLevel += alphaChangeForLevel;
        //        for (int j = 0; j < mapImage.height; j++) // cicle by province        
        //            for (int i = 0; i < mapImage.width; i++)
        //            {

        //                currentColor = mapImage.GetPixel(i, j);
        //                //if (UtilsMy.isSameColorsWithoutAlpha(currentColor, pro.colorID) && currentColor.a == defaultApha && isThereOtherColorsIn4Negbors(i, j))
        //                // && bordersMarkers[i, j] == borderDeepLevel-1
        //                if (currentColor == pro.colorID  && isThereOtherColorsIn4Negbors(i, j, bordersMarkers, (short)(borderDeepLevel)))
        //                {
        //                    //currentColor.a = borderDeepLevel;
        //                    //mapImage.SetPixel(i, j, currentColor);
        //                    borderDeepLevel ++;
        //                    bordersMarkers[i, j] = borderDeepLevel;
        //                    borderDeepLevel--;
        //                    placedMarkers++;

        //                }
        //            }

        //        //if (placedMarkers == 0) 
        //        //    ;
        //    }
        //    //// found centers!
        //    bool wroteResult = false;
        //    //
        //    for (int j = 0; j < mapImage.height && !wroteResult; j++) // cicle by province, looking where is my centre        
        //        //&& !wroteResult
        //        for (int i = 0; i < mapImage.width && !wroteResult; i++)
        //        {
        //            currentColor = mapImage.GetPixel(i, j);
        //            //if (currentColor.a == borderDeepLevel)
        //            if (currentColor == pro.colorID && bordersMarkers[i, j] == borderDeepLevel - 1)
        //            {
        //                pro.centre = new Vector3((i + 0.5f) * Options.cellMuliplier, (j + 0.5f) * Options.cellMuliplier, 0f);
        //                wroteResult = true;
        //            }
        //        }
        //}
        //return false;
    }

    public static void prepareForNewTick()
    {
        Game.market.sentToMarket.setZero();
        foreach (Country country in Country.getExisting())
        // if (country != Country.NullCountry)
        {
            //country.wallet.moneyIncomethisTurn.set(0);
            country.storageSet.setStatisticToZero();
            country.setStatisticToZero();
            country.setStatisticToZero();
            foreach (Province province in country.ownedProvinces)
            {
                province.BalanceEmployableWorkForce();
                {
                    foreach (var item in province.getAgents())
                        item.setStatisticToZero();
                }
            }
        }
    }
    static void makeHelloMessage()
    {
        new Message("Tutorial", "Hi, this is VERY early demo of game-like economy simulator" +
            "\n\nCurrently there is: "
            + "\n\tpopulation agents \\ factories \\ countries \\ national banks"
            + "\n\tbasic trade & production \n\tbasic warfare \n\tbasic inventions"
            + " \n\tbasic reforms (population can vote for reforms)"
            + "\n\tpopulation demotion \\ promotion to other classes \n\tmigration \\ immigration \\ assimilation"
            + "\n\tpolitical \\ culture \\ core map mode"
            + "\n\tmovements and rebellions"
            + "\n\nYou play as " + Game.Player.getDescription() + " You can try to growth economy or conquer the world."
            + "\nOr, You can give control to AI and watch it"
            + "\n\nTry arrows or WASD for scrolling map and mouse wheel for scale"
            + "\n'Enter' key to close top window, space - to pause \\ unpause"
            , "Ok");
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
                            attackerArmy.getDestination().secedeTo(attacker as Country, true);
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

        date = date.AddYears(1);
        // strongly before PrepareForNewTick
        Game.market.simulatePriceChangeBasingOnLastTurnDate();

        Game.calcBattles(); // should be before PrepareForNewTick cause PrepareForNewTick hires dead workers on factories
        prepareForNewTick(); // including workforce balancing

        // big PRODUCE circle
        foreach (Country country in Country.getExisting())
            foreach (Province province in country.ownedProvinces)//Province.allProvinces)
            {
                //Now factories time!               
                foreach (Factory fact in province.allFactories)
                {
                    fact.produce();
                    fact.payTaxes(); // empty for now
                    fact.paySalary(); // workers get gold or food here                   
                }
                foreach (PopUnit pop in province.allPopUnits)
                //That placed here to avoid issues with Aristocrats and clerics
                //Otherwise Aristocrats starts to consume BEFORE they get all what they should
                {
                    if (pop.popType.isProducer())// only Farmers and Tribesmen and Artisans
                        pop.produce();
                    pop.takeUnemploymentSubsidies();
                    if (country.isInvented(Invention.ProfessionalArmy))
                    {
                        var soldier = pop as Soldiers;
                        if (soldier != null)
                            soldier.takePayCheck();
                    }
                }
            }
        //Game.market.ForceDSBRecalculation();
        // big CONCUME circle
        PopType.sortNeeds();
        foreach (Country country in Country.getExisting())
            foreach (Province province in country.ownedProvinces)//Province.allProvinces)            
            {
                foreach (Factory factory in province.allFactories)
                {
                    factory.buyNeeds();
                }

                foreach (PopUnit pop in province.allPopUnits)
                {
                    if (country.serfdom.status == Serfdom.Allowed || country.serfdom.status == Serfdom.Brutal)
                        if (pop.ShouldPayAristocratTax())
                            pop.PayTaxToAllAristocrats();
                }
                foreach (PopUnit pop in province.allPopUnits)
                {
                    pop.buyNeeds();
                }
            }
        // big AFTER all circle
        foreach (Country country in Country.getExisting())
        {
            foreach (Province province in country.ownedProvinces)//Province.allProvinces)
            {
                foreach (Factory factory in province.allFactories)
                {
                    factory.getMoneyForSoldProduct();
                    factory.changeSalary();
                    factory.payDividend();
                }
                province.allFactories.RemoveAll(item => item.isToRemove());
                foreach (PopUnit pop in province.allPopUnits)
                {                    
                    //if (pop.popType == PopType.Aristocrats || (pop.popType == PopType.Farmers && Economy.isMarket.checkIftrue(getCountry())))
                    if (pop.canSellProducts())
                        pop.getMoneyForSoldProduct();
                    // this is no good cause it changes production type only if someone is unprofitable
                    // alternative is in Artisans.produce
                    //if (Game.Random.Next(Options.ArtisansChangeProductionRate) == 1)
                    //{
                    //    var artisan = pop as Artisans;
                    //    if (artisan != null)
                    //        artisan.checkProfit();
                    //}
                    //because income come only after consuming, and only after FULL consumption
                    if (pop.canBuyProducts() && pop.hasToPayGovernmentTaxes())
                        // POps who can't trade will pay tax BEFORE consumption, not after
                        // Otherwise pops who can't trade avoid tax
                        pop.payTaxes();

                    pop.calcLoyalty();

                    //if (Game.Random.Next(10) == 1)
                    {
                        pop.calcGrowth();
                        pop.calcPromotions();
                        pop.calcDemotions();
                        pop.calcMigrations();
                        pop.calcImmigrations();
                        pop.calcAssimilations();
                    }
                    pop.invest();
                    if (Game.Random.Next(20) == 1)
                        pop.putExtraMoneyInBank();
                }
                //if (Game.random.Next(3) == 0)
                //    province.consolidatePops();                
                foreach (PopUnit pop in PopUnit.PopListToAddToGeneralList)
                {
                    PopUnit targetToMerge = pop.province.getSimilarPopUnit(pop);
                    if (targetToMerge == null)
                        pop.province.allPopUnits.Add(pop);
                    else
                        targetToMerge.mergeIn(pop);
                }
                province.allPopUnits.RemoveAll(x => !x.isAlive());
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
        initialize();
    }
}
