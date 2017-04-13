using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System;


public class Game
{
    Texture2D mapImage;
    GameObject mapObject;
    //Mesh mapMesh;
    List<int> trianglesList = new List<int>();
    List<Vector3> vertices = new List<Vector3>();
    int triangleCounter = 0;
    public static Country player;
    internal InventionType inventions = new InventionType();
    internal static bool haveToRunSimulation;
    internal static bool haveToStepSimulation;
    public static System.Random random = new System.Random();

    public static Province selectedProvince;
    public static List<PopUnit> popsToShowInPopulationPanel;
    public static List<Factory> factoriesToShowInProductionPanel;

    public static Market market = new Market();

    internal static float minPrice = 0.001f;
    internal static float maxPrice = 999.99f;
    internal static uint familySize = 5;

    internal static Condition alwaysYesCondition = new Condition(new List<ConditionString>() { new ConditionString(delegate (Country forWhom) { return 2==2; }, "Always Yes condition", true) });

    public static uint date;
    float cellMuliplier = 2f;
    internal static float goldToCoinsConvert = 10f;
    internal static float minWorkforceFullfillingToUpgradeFactory = 0.75f;
    internal static Procent BuyInTimeFactoryUpgradeNeeds = new Procent(0.1f);
    internal static int minUnemploymentToBuldFactory = 300;
    internal static int maximumFactoriesInUpgradeToBuildNew = 2;
    internal static byte maxFactoryLevel = 255;
    internal static float minMarginToUpgrade = 0.05f;
    internal static float minLandForTribemen = 1f;
    internal static float minLandForFarmers = 0.25f;
    internal static uint maxDaysUnprofitableBeforeFactoryClosing = 180;
    internal static uint maxDaysBuildingBeforeRemoving = 180; // 180;
    internal static uint maxDaysClosedBeforeRemovingFactory = 180;
    internal static uint minDaysBeforeSlaryCut = 2;
    internal static int howOftenCheckForFactoryReopenning = 30;
    internal static Procent savePopMoneyReserv = new Procent(0.66666f);
    internal static float factoryMoneyReservPerLevel = 20f;
    internal static float minMarginToRiseSalary = 0.1f;
    internal static float factoryEachLevelEfficiencyBonus = 0.05f;
    internal static float factoryHaveResourceInProvinceBonus = 0.2f;
    internal static int maxFactoryFireHireSpeed = 50;
    internal static float minFactoryWorkforceFullfillingToBuildNew = 0.75f;
    internal static float defaultSciencePointMultiplier = 0.001f; //0.00001f;
    internal static uint fabricConstructionTimeWithoutCapitalism = 20;
    internal static float aristocratsFoodReserv = 50;
    internal static float votingPassBillLimit = 0.5f;
    internal static float votingForcedReformPenalty = 0.5f;
    // just to store temporeal junk
    internal static string dumpString;
    internal static GameObject r3dTextPrefab;

    public Game()
    {
        Application.runInBackground = true;
        LoadImages();
        new Product("Food", false, 1f);
        new Product("Wood", true, 2.7f);
        new Product("Lumber", false, 8f);
        new Product("Gold", true, 4f);
        new Product("Metall ore", true, 3f);
        new Product("Metall", false, 6f);
        new Product("Wool", true, 1);
        new Product("Clothes", false, 3);
        new Product("Furniture", false, 7);
        new Product("Stone", true, 1);
        new Product("Cement", false, 2);
        new Product("Fruit", true, 1);
        new Product("Wine", false, 3);
        market.initialize();
        MakeMap();
        var mapWidth =  mapImage.width * cellMuliplier;
        var mapHeight = mapImage.height * cellMuliplier;
        //MainCamera.cameraMy.transform.position = GameObject.FindWithTag("mapObject").transform.position;
        MainCamera.cameraMy.transform.position = new Vector3(mapWidth / 2f, mapHeight /2f, MainCamera.cameraMy.transform.position.z);
            
        FindProvinceCenters();
        r3dTextPrefab = (GameObject)Resources.Load("prefabs/3dTextPrefab", typeof(GameObject));
        foreach (Province pro in Province.allProvinces)
            pro.SetLabel();

        Province.allProvinces[0].setResource(Product.Gold);
        //Province.allProvinces[0].setResource(Product.Wood;
        Province.allProvinces[1].setResource(Product.Wood);
        Province.allProvinces[2].setResource(Product.Fruit);
        Province.allProvinces[3].setResource(Product.Wool);
        Province.allProvinces[4].setResource(Product.Stone);
        Province.allProvinces[5].setResource(Product.MetallOre);




        new FactoryType("Forestry", new Storage(Product.Wood, 2f), null);
        new FactoryType("Gold pit", new Storage(Product.Gold, 2f), null);
        new FactoryType("Metal pit", new Storage(Product.MetallOre, 2f), null);
        new FactoryType("Sheepfold", new Storage(Product.Wool, 2f), null);
        new FactoryType("Quarry", new Storage(Product.Stone, 2f), null);
        new FactoryType("Orchard", new Storage(Product.Fruit, 2f), null);

        PrimitiveStorageSet resourceInput = new PrimitiveStorageSet();
        resourceInput.Set(new Storage(Product.Lumber, 1f));
        new FactoryType("Furniture factory", new Storage(Product.Furniture, 4f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.Set(new Storage(Product.Wood, 1f));
        new FactoryType("Sawmill", new Storage(Product.Lumber, 2f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.Set(new Storage(Product.Wood, 0.5f));
        resourceInput.Set(new Storage(Product.MetallOre, 2f));
        new FactoryType("Metal smelter", new Storage(Product.Metal, 3f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.Set(new Storage(Product.Wool, 1f));
        new FactoryType("Weaver factory", new Storage(Product.Clothes, 2f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.Set(new Storage(Product.Wood, 0.5f));
        resourceInput.Set(new Storage(Product.Stone, 1f));
        new FactoryType("Cement factory", new Storage(Product.Cement, 3f), resourceInput);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.Set(new Storage(Product.Fruit, 0.3333f));
        new FactoryType("Winery", new Storage(Product.Wine, 2f), resourceInput);

        //new Product("Grain");

        //new PopType(PopType.PopTypes.TribeMen, new Storage(Product.findByName("Food"), 1.5f), "Tribemen");
        new PopType(PopType.PopTypes.Tribemen, new Storage(Product.findByName("Food"), 1.0f), "Tribemen");
        new PopType(PopType.PopTypes.Aristocrats, null, "Aristocrats");
        new PopType(PopType.PopTypes.Capitalists, null, "Capitalists");
        new PopType(PopType.PopTypes.Farmers, new Storage(Product.findByName("Food"), 2.0f), "Farmers");
        //new PopType(PopType.PopTypes.Artisans, null, "Artisans");
        //new PopType(PopType.PopTypes.Soldiers, null, "Soldiers");
        new PopType(PopType.PopTypes.Workers, null, "Workers");

        Culture cul = new Culture("Kocopetji");
        player = new Country("Kocopia", cul);
        //player.inventions.MarkInvented(InventionType.capitalism);
        player.inventions.MarkInvented(InventionType.farming);
        //player.inventions.MarkInvented(InventionType.banking);
        player.storageSet.add(new Storage(Product.Food, 200f));

        CreateRandomPopulation();
        Province.allProvinces[0].allPopUnits[0].education.set(1f);
        //new GoldMine(Province.allProvinces[0]);
        //new Forestry(Province.allProvinces[0]);
        //new GoldMine(Province.allProvinces[0]);
        //new Factory(Province.allProvinces[0]);
        //new Furniture(Province.allProvinces[0]);


        //Province.allProvinces[0].allPopUnits.Add(new Workers(1000, PopType.workers, new Culture("Milaus"), Province.allProvinces[0]));

        MainCamera.topPanel.refresh();
    }
    void CreateRandomPopulation()
    {
        uint chanceForA = 85;

        foreach (Province province in Province.allProvinces)
        {
            province.SecedeTo(player);
            Culture culture = new Culture(province + "landers");

            Tribemen f = new Tribemen(2000, PopType.tribeMen, Game.player.culture, province);
            // f.wallet.haveMoney.set(10);
            province.allPopUnits.Add(f
            //    new PopUnit(PopUnit.getRandomPopulationAmount(), PopType.tribeMen, culture, province)
            //new Tribemen(PopUnit.getRandomPopulationAmount(), PopType.tribeMen, culture, province));
            //new Tribemen(2000, PopType.tribeMen, culture, province));
                );
            Aristocrats ar = new Aristocrats(100, PopType.aristocrats, Game.player.culture, province);
            ar.wallet.haveMoney.set(200);
            province.allPopUnits.Add(ar);
            //province.allPopUnits.Add(new Workers(600, PopType.workers, Game.player.culture, province));

            //if (Procent.GetChance(chanceForA))
            //    province.allPopUnits.Add(
            //    new PopUnit(PopUnit.getRandomPopulationAmount(), PopType.aristocrats, culture, province)
            //    );

        }


    }
    /// <summary>
    /// Makes polygonal Stripe and stores it vertices[] and trianglesList[]
    /// </summary>    
    void makePolygonalStripe(float x, float y, float x2, float y2)
    {

        vertices.Add(new Vector3(x, y, 0));
        vertices.Add(new Vector3(x2, y, 0));
        vertices.Add(new Vector3(x2, y2, 0));
        vertices.Add(new Vector3(x, y2, 0));

        //trianglesList.Add(0 + triangleCounter);
        //trianglesList.Add(1 + triangleCounter);
        //trianglesList.Add(2 + triangleCounter);

        //trianglesList.Add(2 + triangleCounter);
        //trianglesList.Add(3 + triangleCounter);
        //trianglesList.Add(0 + triangleCounter);

        trianglesList.Add(0 + triangleCounter);
        trianglesList.Add(2 + triangleCounter);
        trianglesList.Add(1 + triangleCounter);

        trianglesList.Add(2 + triangleCounter);
        trianglesList.Add(0 + triangleCounter);
        trianglesList.Add(3 + triangleCounter);
        triangleCounter += 4;
    }
    void MakeMap()
    {
        ProvinceNameGenerator nameGenerator = new ProvinceNameGenerator();

        mapObject = GameObject.Find("MapObject");
        //mapImage = UtilsMy.FlipTexture(mapImage); ;

        Color currentProvinceColor = mapImage.GetPixel(0, 0);
        Color currentColor, lastColor, lastprovinceColor = mapImage.GetPixel(0, 0); //, stripeColor;
        int provinceCounter = 0;
        for (int j = 0; j < mapImage.height; j++) // cicle by province        
            for (int i = 0; i < mapImage.width; i++)
            {
                // nextColor = mapImage.GetPixel(i, j);
                currentProvinceColor = mapImage.GetPixel(i, j);
                //if (nextColor == currentProvinceColor)
                if ((lastprovinceColor != currentProvinceColor) && !Province.isProvinceCreated(currentProvinceColor))
                { // fill up province's mesh
                    // making mesh by BMP        
                    int stripeLenght = 0;
                    lastColor = mapImage.GetPixel(0, 0);
                    //stripeColor = lastColor;
                    for (int ypos = 0; ypos < mapImage.height; ypos++)
                    {
                        for (int xpos = 0; xpos < mapImage.width; xpos++)
                        {

                            currentColor = mapImage.GetPixel(xpos, ypos);
                            if (currentColor == currentProvinceColor)
                            {
                                stripeLenght++;

                            }
                            else //place for trangle making
                            {
                                if (lastColor == currentProvinceColor)
                                {
                                    makePolygonalStripe((xpos - stripeLenght) * cellMuliplier, ypos * cellMuliplier, xpos * cellMuliplier, (ypos + 1) * cellMuliplier); //should form 2 triangles
                                    stripeLenght = 0;
                                }
                            }
                            lastColor = currentColor;
                        }
                        if (stripeLenght != 0)
                            if (lastColor == currentProvinceColor)
                            {
                                makePolygonalStripe((mapImage.width - 1 - stripeLenght) * cellMuliplier, ypos * cellMuliplier, (mapImage.width - 1) * cellMuliplier, (ypos + 1) * cellMuliplier); //should form 2 triangles
                                stripeLenght = 0;
                            }
                        stripeLenght = 0;
                    }
                    //finished all map search for currentProvince

                    //spawn object
                    GameObject objToSpawn = new GameObject(string.Format("{0}", provinceCounter));

                    //Add Components
                    MeshFilter meshFilter = objToSpawn.AddComponent<MeshFilter>();
                    MeshRenderer meshRenderer = objToSpawn.AddComponent<MeshRenderer>();

                    // in case you want the new gameobject to be a child
                    // of the gameobject that your script is attached to
                    objToSpawn.transform.parent = mapObject.transform;

                    Mesh mesh = meshFilter.mesh;
                    mesh.Clear();

                    mesh.vertices = vertices.ToArray(); //map.getGroundVertices3();
                    mesh.triangles = trianglesList.ToArray();
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();

                    //Renderer rend = GetComponent<Renderer>();
                    meshRenderer.material.shader = Shader.Find("Standard");
                    //meshRenderer.material.SetColor("_SpecColor", currentProvinceColor);
                    meshRenderer.material.color = currentProvinceColor;

                    MeshCollider groundMeshCollider;
                    groundMeshCollider = objToSpawn.AddComponent(typeof(MeshCollider)) as MeshCollider;
                    groundMeshCollider.sharedMesh = mesh;

                    vertices.Clear();
                    trianglesList.Clear();
                    triangleCounter = 0;


                    mesh.name = provinceCounter.ToString();
                    //provinceCounter;

                    //Province newProvince = new Province(string.Format("{0}", provinceCounter),
                    Province newProvince = new Province(nameGenerator.generateProvinceName(),
                        provinceCounter, currentProvinceColor, mesh, meshFilter, objToSpawn, meshRenderer, Product.getRandomResource(false));
                    Province.allProvinces.Add(newProvince);



                    //newProvince.centre = meshRenderer.;
                    //newProvince.centre = objToSpawn.rigidbody.centerOfMass;



                    provinceCounter++;


                }
                else
                {
                    // currentProvinceColor = nextColor;
                }
                lastprovinceColor = currentProvinceColor;
            }
    }


    bool FindProvinceCenters()
    {
        Vector3 accu = new Vector3(0,0,0);
        foreach (Province pro in Province.allProvinces)
        {
            accu.Set(0, 0, 0);
            foreach (var c in pro.mesh.vertices)
                accu+=c;
            accu = accu / pro.mesh.vertices.Length;
            pro.centre = accu;
        }
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
            //                pro.centre = new Vector3((i + 0.5f) * cellMuliplier, (j + 0.5f) * cellMuliplier, 0f);
            //                wroteResult = true;
            //            }
            //        }
            //}
            //return false;
    }
    bool isThereOtherColorsIn4Negbors(int x, int y, short[,] bordersMarkers, short borderDeepLevel)
    {
        Color color = mapImage.GetPixel(x, y);
        //if (x == 0)
        //    return true;
        //else
        //    if (!UtilsMy.isSameColorsWithoutAlpha(mapImage.GetPixel(x - 1, y), color)) return true;

        //if (x == mapImage.width - 1)
        //    return true;
        //else
        //    if (!UtilsMy.isSameColorsWithoutAlpha(mapImage.GetPixel(x + 1, y), color)) return true;
        //if (y == 0)
        //    return true;
        //else
        //    if (!UtilsMy.isSameColorsWithoutAlpha(mapImage.GetPixel(x, y - 1), color)) return true;
        //if (y == mapImage.height - 1)
        //    return true;
        //if (!UtilsMy.isSameColorsWithoutAlpha(mapImage.GetPixel(x, y + 1), color)) return true;
        //return false;
        if (x == 0)
            return true;
        else
          if (mapImage.GetPixel(x - 1, y) != color || bordersMarkers[x - 1, y] != borderDeepLevel) return true;

        if (x == mapImage.width - 1)
            return true;
        else
            if (mapImage.GetPixel(x + 1, y) != color || bordersMarkers[x + 1, y] != borderDeepLevel) return true;
        if (y == 0)
            return true;
        else
            if (mapImage.GetPixel(x, y - 1) != color || bordersMarkers[x, y - 1] != borderDeepLevel) return true;
        if (y == mapImage.height - 1)
            return true;
        if (mapImage.GetPixel(x, y + 1) != color || bordersMarkers[x, y + 1] != borderDeepLevel) return true;
        return false;
    }
    void LoadImages()
    {
        ////Loadin images...
        //BMPLoader loader = new BMPLoader();
        ////loader.ForceAlphaReadWhenPossible = true; // can be uncomment to read alpha

        ////load the image data
        //BMPImage img = loader.LoadBMP("assets\\provinces.bmp");

        //// Convert the Color32 array into a Texture2D
        //mapImage = img.ToTexture2D();
        //mapImage = (Texture2D)Resources.Load("assets\\provinces");
        

        //byte[] bytes = File.ReadAllBytes("assets/provinces.png");
         
        //Texture2D texture = new Texture2D(2, 2);
        //texture.filterMode = FilterMode.Trilinear;
        //texture.LoadImage(bytes);
        //var z
        mapImage = Resources.Load("provinces", typeof(Texture2D))  as Texture2D; ///texture;
        //Texture2D mapImage = new Texture2D(z.width, z.height);
        //mapImage.SetPixels(z.GetPixels());
        //mapImage = mapImage.c
        RawImage ri = GameObject.Find("RawImage").GetComponent<RawImage>();
        //Image ri = GameObject.Find("Image").GetComponent<Image>();
        ri.texture = mapImage;


    }
    internal static void stepSimulation()
    {
        date++;
        // strongly before PrepareForNewTick
        Game.market.simulatePriceChangeBasingOnLastTurnDate();

        PopUnit.PrepareForNewTick();

        // big PRODUCE circle
        foreach (Country country in Country.allCountries)
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
                //That placed here to avoid issues with Aristocrts and clerics
                //Otherwise Arisocrats starts to consume BEFORE they get all what they should
                {
                    if (pop.type.basicProduction != null)// only Farmers and Tribemen
                        pop.produce();

                    //if (!country.isInvented(InventionType.capitalism) && pop.ShouldPayAristocratTax())
                    //    pop.PayTaxToAllAristocrats();
                }
            }
        //Game.market.ForceDSBRecalculation();
        // big CONCUME circle
        foreach (Country country in Country.allCountries)
            foreach (Province province in country.ownedProvinces)//Province.allProvinces)            
            {
                foreach (Factory factory in province.allFactories)
                {
                    factory.consume();
                }

                foreach (PopUnit pop in province.allPopUnits)
                {
                    if (country.serfdom.status == Serfdom.Allowed || country.serfdom.status == Serfdom.Brutal)
                        if (pop.ShouldPayAristocratTax())
                            //if (!country.isInvented(InventionType.capitalism) && pop.ShouldPayAristocratTax())
                            pop.PayTaxToAllAristocrats();
                }
                foreach (PopUnit pop in province.allPopUnits)
                {
                    pop.consume();
                }
            }
        // big AFTER all circle
        foreach (Country country in Country.allCountries)
        {
            foreach (Province province in country.ownedProvinces)//Province.allProvinces)
            {
                //province.BalanceEmployableWorkForce();
                foreach (Factory factory in province.allFactories)
                {
                    factory.getMoneyFromMarket();
                    factory.changeSalary();
                    factory.PayDividend();
                }
                province.allFactories.RemoveAll(item => item.isToRemove());
                foreach (PopUnit pop in province.allPopUnits)
                {
                    //if (pop.type != PopType.tribeMen && !(pop.type == PopType.farmers && !province.owner.isInvented(InventionType.capitalism)))
                    if (pop.type == PopType.aristocrats || pop.type == PopType.capitalists || (pop.type == PopType.farmers && province.owner.economy.isMarket()))
                        pop.getMoneyFromMarket();
                    //becouse income come only after consuming, and only after FULL consumption
                    if (pop.canTrade())
                        pop.payTaxes(); // temp

                    pop.calcLoyalty();
                    // temp
                    pop.calcPromotions();
                    pop.calcDemotions();
                    pop.calcGrowth();

                    pop.Invest();
                }

                foreach (PopUnit pop in PopUnit.tempPopList)
                {
                    PopUnit targetToMerge = province.FindSimularPopUnit(pop);
                    if (targetToMerge == null)
                        province.allPopUnits.Add(pop);
                    else
                        targetToMerge.Merge(pop);
                }
                PopUnit.tempPopList.Clear();
            }
            country.Think();
        }
    }


}
