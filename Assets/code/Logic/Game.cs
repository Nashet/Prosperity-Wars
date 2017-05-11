using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System;

public class Game
{
    Texture2D mapImage;
    GameObject mapObject;
    internal static GameObject r3dTextPrefab;

    List<int> trianglesList = new List<int>();
    List<Vector3> vertices = new List<Vector3>();
    int triangleCounter = 0;

    public static Country player;
    internal InventionType inventions = new InventionType();

    internal static bool haveToRunSimulation;
    internal static bool haveToStepSimulation;
    internal static int howMuchPausedWindowsOpen = 0;

    public static System.Random random = new System.Random();

    public static Province selectedProvince;
    public static List<PopUnit> popsToShowInPopulationPanel;
    public static List<Factory> factoriesToShowInProductionPanel;

    internal static List<BattleResult> allBattles = new List<BattleResult>();
    internal static Stack<Message> MessageQueue = new Stack<Message>();
    public static Market market = new Market();

    internal static StringBuilder threadDangerSB = new StringBuilder();

    public static int date;
    internal static bool devMode = false;
    public Game()
    {
        Application.runInBackground = true;
        //LoadImages();        
        generateMapImage();
        makeProducts();
        market.initialize();
        r3dTextPrefab = (GameObject)Resources.Load("prefabs/3dProvinceNameText", typeof(GameObject));
        makeProvinces();
        roundMesh();
        deleteEdgeProvinces();
        findNeighborprovinces();
        var mapWidth = mapImage.width * Options.cellMultiplier;
        var mapHeight = mapImage.height * Options.cellMultiplier;

        
        makeFactoryTypes();
        makePopTypes();

        var countryNameGenerator = new CountryNameGenerator();
        int extraCountries = random.Next(6);
        for (int i = 0; i < 16 + extraCountries; i++)
            makeCountry(countryNameGenerator);

        foreach (var pro in Province.allProvinces)
            if (pro.getOwner() == null)
                pro.InitialOwner(Country.NullCountry);

        CreateRandomPopulation();
        //Province.allProvinces[0].allPopUnits[0].education.set(1f);
        setStartResources();
        MainCamera.topPanel.refresh();
        makeHelloMessage();
        //MainCamera.cameraMy.transform.position = new Vector3(mapWidth / 2f, mapHeight / 2f, MainCamera.cameraMy.transform.position.z);
        MainCamera.cameraMy.transform.position = new Vector3(Game.player.getCapital().centre.x, Game.player.getCapital().centre.y, MainCamera.cameraMy.transform.position.z);
    }

    private void deleteEdgeProvinces()
    {
        for (int x = 0; x < mapImage.width; x++)
        {
            removeProvince(x, 0);
            removeProvince(x, mapImage.height - 1);
        }
        for (int y = 0; y < mapImage.height; y++)
        {
            removeProvince(0, y);
            removeProvince(mapImage.width - 1, y);
        }

    }
    void removeProvince(int x, int y)
    {
        var toremove = Province.findProvince(mapImage.GetPixel(x, y));
        if (Province.allProvinces.Contains(toremove))
        {
            UnityEngine.Object.Destroy(toremove.gameObject);
            Province.allProvinces.Remove(toremove);
        }
    }

    private void setStartResources()
    {
        //Country.allCountries[0] is null country
        Country.allCountries[1].getCapital().setResource(Product.Fruit);
        
        //Country.allCountries[0].getCapital().setResource(Product.Wood;
        Country.allCountries[2].getCapital().setResource(Product.Wood);
        Country.allCountries[3].getCapital().setResource(Product.Gold);
        Country.allCountries[4].getCapital().setResource(Product.Wool);
        Country.allCountries[5].getCapital().setResource(Product.Stone);
        Country.allCountries[6].getCapital().setResource(Product.MetallOre);
    }

    private void makePopTypes()
    {
        //new PopType(PopType.PopTypes.TribeMen, new Storage(Product.findByName("Food"), 1.5f), "Tribesmen");
        new PopType(PopType.PopTypes.Tribemen, new Storage(Product.findByName("Food"), 1.0f), "Tribesmen", 2f);
        new PopType(PopType.PopTypes.Aristocrats, null, "Aristocrats", 4f);
        new PopType(PopType.PopTypes.Capitalists, null, "Capitalists", 1f);
        new PopType(PopType.PopTypes.Farmers, new Storage(Product.findByName("Food"), 2.0f), "Farmers", 1f);
        //new PopType(PopType.PopTypes.Artisans, null, "Artisans");
        //new PopType(PopType.PopTypes.Soldiers, null, "Soldiers");
        new PopType(PopType.PopTypes.Workers, null, "Workers", 1f);
    }

    void makeCountry(CountryNameGenerator name)
    {
        Culture cul = new Culture("Ridvans");

        Province province = Province.getRandomProvinceInWorld((x) => x.getOwner() == null);// Country.NullCountry);
        Country count = new Country(name.generateCountryName(), cul, new CountryWallet(0f), UtilsMy.getRandomColor(), province);
        player = Country.allCountries[1]; // not wild Tribes DONT touch that
        province.InitialOwner(count);
        count.moveCapitalTo(province);

        count.storageSet.add(new Storage(Product.Food, 200f));
        count.wallet.haveMoney.add(100f);
    }
    void makeFactoryTypes()
    {
        new FactoryType("Forestry", new Storage(Product.Wood, 2f), null, false);
        new FactoryType("Gold pit", new Storage(Product.Gold, 2f), null, true);
        new FactoryType("Metal pit", new Storage(Product.MetallOre, 2f), null, true);
        new FactoryType("Sheepfold", new Storage(Product.Wool, 2f), null, false);
        new FactoryType("Quarry", new Storage(Product.Stone, 2f), null, true);
        new FactoryType("Orchard", new Storage(Product.Fruit, 2f), null, false);

        PrimitiveStorageSet resourceInput = new PrimitiveStorageSet();
        resourceInput.Set(new Storage(Product.Lumber, 1f));
        new FactoryType("Furniture factory", new Storage(Product.Furniture, 4f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.Set(new Storage(Product.Wood, 1f));
        new FactoryType("Sawmill", new Storage(Product.Lumber, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.Set(new Storage(Product.Wood, 0.5f));
        resourceInput.Set(new Storage(Product.MetallOre, 2f));
        new FactoryType("Metal smelter", new Storage(Product.Metal, 3f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.Set(new Storage(Product.Wool, 1f));
        new FactoryType("Weaver factory", new Storage(Product.Clothes, 2f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.Set(new Storage(Product.Wood, 0.5f));
        resourceInput.Set(new Storage(Product.Stone, 1f));
        new FactoryType("Cement factory", new Storage(Product.Cement, 3f), resourceInput, false);

        resourceInput = new PrimitiveStorageSet();
        resourceInput.Set(new Storage(Product.Fruit, 0.3333f));
        new FactoryType("Winery", new Storage(Product.Wine, 2f), resourceInput, false);
    }

    void makeProducts()
    {
        new Product("Food", false, 0.4f);
        new Product("Wood", true, 2.7f);
        new Product("Lumber", false, 8f);
        new Product("Gold", true, 4f);
        new Product("Metal ore", true, 3f);
        new Product("Metal", false, 6f);
        new Product("Wool", true, 1);
        new Product("Clothes", false, 3);
        new Product("Furniture", false, 7);
        new Product("Stone", true, 1);
        new Product("Cement", false, 2);
        new Product("Fruit", true, 1);
        new Product("Wine", false, 3);
    }
    internal static float getAllMoneyInWorld()
    {
        float allMoney = 0f;
        foreach (Country co in Country.allCountries)
        {
            allMoney += co.wallet.haveMoney.get();
            allMoney += co.bank.getReservs();
            foreach (Province pr in co.ownedProvinces)
            {
                foreach (var factory in pr.allProducers)
                    allMoney += factory.wallet.haveMoney.get();
            }
        }
        return allMoney;
    }
    void CreateRandomPopulation()
    {
        int chanceForA = 85;

        foreach (Province province in Province.allProvinces)
        {
            Culture culture = new Culture(province + "landers");
            if (province.getOwner() == Country.NullCountry)
            {
                Tribemen f = new Tribemen(PopUnit.getRandomPopulationAmount(500, 1000), PopType.tribeMen, province.getOwner().culture, province);
                province.allPopUnits.Add(f);
            }
            else
            {
                PopUnit pop;
                if (!Game.devMode)
                    pop = new Tribemen(PopUnit.getRandomPopulationAmount(1800, 2000), PopType.tribeMen, province.getOwner().culture, province);
                else
                    pop = new Tribemen(2000, PopType.tribeMen, province.getOwner().culture, province);
                province.allPopUnits.Add(pop);

                if (province.getOwner() == Game.player)
                {
                    //pop = new Tribesmen(20900, PopType.tribeMen, province.getOwner().culture, province);
                    //province.allPopUnits.Add(pop);
                }
                if (!Game.devMode)
                    pop = new Aristocrats(PopUnit.getRandomPopulationAmount(80, 100), PopType.aristocrats, province.getOwner().culture, province);
                else
                    pop = new Aristocrats(100, PopType.aristocrats, province.getOwner().culture, province);

                pop.wallet.haveMoney.set(9000);
                pop.storageNow.add(60f);
                province.allPopUnits.Add(pop);
                if (!Game.devMode)
                {
                    pop = new Capitalists(PopUnit.getRandomPopulationAmount(30, 50), PopType.capitalists, province.getOwner().culture, province);
                    pop.wallet.haveMoney.set(9000);
                    province.allPopUnits.Add(pop);

                    pop = new Farmers(PopUnit.getRandomPopulationAmount(500, 600), PopType.farmers, province.getOwner().culture, province);
                    pop.wallet.haveMoney.set(20);
                    province.allPopUnits.Add(pop);

                }
                //province.allPopUnits.Add(new Workers(600, PopType.workers, Game.player.culture, province));

                //if (Procent.GetChance(chanceForA))
                //    province.allPopUnits.Add(
                //    new PopUnit(PopUnit.getRandomPopulationAmount(), PopType.aristocrats, culture, province)
                //    );

            }

        }
    }
    /// <summary>
    /// Makes polygonal Stripe and stores it vertices[] and trianglesList[]
    /// </summary>    
    void makePolygonalStripe(float x, float y, float x2, float y2, int xpos1, int ypos1, int xpos2, int ypos2)
    //void makePolygonalStripe(float x, float y, float x2, float y2)
    {
        //float x = xpos1 * Options.cellMuliplier;
        //float y = ypos1 * Options.cellMuliplier;
        //float x2 = xpos2 * Options.cellMuliplier;
        //float y2 = ypos1 * Options.cellMuliplier;

        //if (mapImage.isLeftTopCorner(xpos1, ypos1))
        //    x -= Options.cellMuliplier;
        //if (mapImage.isRightTopCorner(xpos1, ypos1))
        //    x += Options.cellMuliplier;

        //if (mapImage.isLeftBottomCorner(xpos1, ypos1))
        //    vertices.Add(new Vector3(x + Options.cellMuliplier, y, 0));
        //else
        vertices.Add(new Vector3(x, y, 0));

        //if (mapImage.isRightBottomCorner(xpos2 - 1, ypos1))
        //    vertices.Add(new Vector3(x2 - Options.cellMuliplier, y, 0));
        //else
        vertices.Add(new Vector3(x2, y, 0));

        //if (mapImage.isRightTopCorner(xpos2 - 1, ypos1))
        //    vertices.Add(new Vector3(x2 - Options.cellMuliplier, y2, 0));
        //else
        vertices.Add(new Vector3(x2, y2, 0));

        //if (mapImage.isLeftTopCorner(xpos1, ypos1))
        //    vertices.Add(new Vector3(x + Options.cellMuliplier, y2, 0));
        //else
        vertices.Add(new Vector3(x, y2, 0));

        trianglesList.Add(0 + triangleCounter);
        trianglesList.Add(2 + triangleCounter);
        trianglesList.Add(1 + triangleCounter);

        trianglesList.Add(2 + triangleCounter);
        trianglesList.Add(0 + triangleCounter);
        trianglesList.Add(3 + triangleCounter);
        triangleCounter += 4;
    }
    void checkCoordinateForNeighbors(Province province, int x1, int y1, int x2, int y2)
    {
        if (mapImage.coordinatesExist(x2, y2) && mapImage.isDifferentColor(x1, y1, x2, y2))
        {
            Province found;
            found = Province.findProvince(mapImage.GetPixel(x2, y2));
            if (found != null) // for remove edge provinces
                province.addNeigbor(found);
        }
    }
    void findNeighborprovinces()
    {
        int f = 0;
        foreach (var province in Province.allProvinces)
        {
            f++;
            for (int j = 0; j < mapImage.height; j++)
                for (int i = 0; i < mapImage.width; i++)
                {
                    Color currentColor = mapImage.GetPixel(i, j);
                    if (currentColor == province.getColorID())
                    {
                        checkCoordinateForNeighbors(province, i, j, i + 1, j);
                        checkCoordinateForNeighbors(province, i, j, i - 1, j);
                        checkCoordinateForNeighbors(province, i, j, i, j + 1);
                        checkCoordinateForNeighbors(province, i, j, i, j - 1);
                    }
                }
        }
    }
    void generateMapImage()
    {
        mapImage = new Texture2D(200, 100);
        //mapImage = new Texture2D(100, 50);
        Color emptySpaceColor = Color.black;//.setAlphaToZero();
        mapImage.setColor(emptySpaceColor);
        int amountOfProvince;
        if (Game.devMode)
            amountOfProvince = 10;
        else
            amountOfProvince = 12 + Game.random.Next(8);
        amountOfProvince = 60 + Game.random.Next(20);
        amountOfProvince = 160 + Game.random.Next(20);
        for (int i = 0; i < amountOfProvince; i++)
            mapImage.SetPixel(mapImage.getRandomX(), mapImage.getRandomY(), UtilsMy.getRandomColor());

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
    }
    Mesh getMeshID(Color color)
    {
        foreach (var all in Province.allProvinces)
            if (color == all.getColorID())
                return all.mesh;
        return null;
    }
    Mesh getMeshID(int xpos, int ypos)
    {
        Color color = mapImage.GetPixel(xpos, ypos);
        foreach (var all in Province.allProvinces)
            if (color == all.getColorID())
                return all.mesh;
        return null;
    }
    private bool movePointRight(Mesh mesh, int xpos, int ypos, int xMove, int yMove)
    {
        Vector3[] editingVertices = mesh.vertices;
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 sr = new Vector3(xpos * Options.cellMultiplier, (ypos + 1) * Options.cellMultiplier, 0f);
            //Vector3 sr2 = new Vector3(xpos * Options.cellMuliplier, (ypos ) * Options.cellMuliplier, 0f);
            //if (editingVertices[i] == sr || editingVertices[i] == sr2)
            if (editingVertices[i] == sr)
            {
                editingVertices[i].x += Options.cellMultiplier * xMove / 2;
                mesh.vertices = editingVertices;
                mesh.RecalculateBounds();
                return true;
            }
        }
        return false;
    }
    private bool movePointLeft(Mesh mesh, int xpos, int ypos, int xMove, int yMove)
    {
        Vector3[] mesh1Vertices = mesh.vertices;

        Mesh mesh2 = getMeshID(xpos + 1, ypos);
        Vector3[] mesh2Vertices = mesh2.vertices;
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            Vector3 sr = new Vector3(xpos * Options.cellMultiplier, (ypos + 2) * Options.cellMultiplier, 0f);
            //Vector3 sr2 = new Vector3(xpos * Options.cellMuliplier, (ypos ) * Options.cellMuliplier, 0f);
            //if (editingVertices[i] == sr || editingVertices[i] == sr2)
            if (mesh1Vertices[i] == sr)
            {
                mesh1Vertices[i].x += Options.cellMultiplier * xMove / 2;
                mesh.vertices = mesh1Vertices;
                mesh.RecalculateBounds();
                return true;
            }
        }
        return false;
    }

    void roundMesh()
    {
        for (int ypos = 0; ypos < mapImage.height; ypos++)
        {
            for (int xpos = 0; xpos < mapImage.width; xpos++)
            {
                if (mapImage.isRightTopCorner(xpos, ypos))
                {
                    movePointLeft(getMeshID(xpos, ypos), xpos + 1, ypos - 1, -1, 0);
                    movePointLeft(getMeshID(xpos + 1, ypos), xpos + 1, ypos - 1, -1, 0);
                }
                else
                if (mapImage.isLeftTopCorner(xpos, ypos))
                {
                    movePointRight(getMeshID(mapImage.GetPixel(xpos, ypos)), xpos, ypos, 1, 0);
                    movePointRight(getMeshID(mapImage.GetPixel(xpos - 1, ypos)), xpos, ypos, 1, 0);
                }
                else
                if (mapImage.isLeftBottomCorner(xpos, ypos))
                {
                    movePointRight(getMeshID(mapImage.GetPixel(xpos, ypos)), xpos, ypos - 1, 1, 0);
                    movePointRight(getMeshID(mapImage.GetPixel(xpos - 1, ypos)), xpos, ypos - 1, 1, 0);
                }
                else
                if (mapImage.isRightBottomCorner(xpos, ypos))
                {
                    movePointLeft(getMeshID(mapImage.GetPixel(xpos, ypos)), xpos + 1, ypos - 2, -1, 0);
                    movePointLeft(getMeshID(mapImage.GetPixel(xpos + 1, ypos)), xpos + 1, ypos - 2, -1, 0);
                }
            }
        }
    }

    void makeProvinces()
    {
        ProvinceNameGenerator nameGenerator = new ProvinceNameGenerator();

        mapObject = GameObject.Find("MapObject");

        Color currentProvinceColor = mapImage.GetPixel(0, 0);
        Color currentColor, lastColor, lastprovinceColor = mapImage.GetPixel(0, 0); //, stripeColor;
        int provinceCounter = 0;
        for (int j = 0; j < mapImage.height; j++) // cicle by province        
            for (int i = 0; i < mapImage.width; i++)
            {
                currentProvinceColor = mapImage.GetPixel(i, j);
                if ((lastprovinceColor != currentProvinceColor) && !Province.isProvinceCreated(currentProvinceColor))
                { // fill up province's mesh
                    // making mesh from texture
                    int stripeLenght = 0;
                    lastColor = Color.black.setAlphaToZero(); // unexisting color

                    for (int ypos = 0; ypos < mapImage.height; ypos++)
                    {
                        lastColor = Color.black.setAlphaToZero(); // unexisting color
                        for (int xpos = 0; xpos < mapImage.width; xpos++)
                        {
                            currentColor = mapImage.GetPixel(xpos, ypos);
                            if (currentColor == currentProvinceColor)
                                stripeLenght++;
                            else //place for trangle making
                            {
                                if (lastColor == currentProvinceColor)
                                {
                                    makePolygonalStripe((xpos - stripeLenght) * Options.cellMultiplier, ypos * Options.cellMultiplier, xpos * Options.cellMultiplier, (ypos + 1) * Options.cellMultiplier,
                                        (xpos - stripeLenght), ypos, xpos, (ypos + 1)); //should form 2 triangles
                                    //makePolygonalStripe((xpos - stripeLenght), ypos, xpos, (ypos + 1)); //should form 2 triangles
                                    stripeLenght = 0;
                                }
                            }
                            lastColor = currentColor;
                        }
                        if (stripeLenght != 0)
                            if (lastColor == currentProvinceColor)
                            {
                                makePolygonalStripe((mapImage.width - stripeLenght) * Options.cellMultiplier, ypos * Options.cellMultiplier, (mapImage.width) * Options.cellMultiplier, (ypos + 1) * Options.cellMultiplier,
                                    (mapImage.width - stripeLenght), ypos, (mapImage.width), (ypos + 1)); //should form 2 triangles
                                //makePolygonalStripe((mapImage.width - stripeLenght), ypos, (mapImage.width), (ypos + 1)); //should form 2 triangles
                                stripeLenght = 0;
                            }
                        stripeLenght = 0;
                    }
                    //finished all map search for currentProvince
                    makeProvince(provinceCounter, currentProvinceColor, nameGenerator.generateProvinceName());
                    provinceCounter++;
                }
                lastprovinceColor = currentProvinceColor;
            }
    }

    void makeProvince(int provinceID, Color colorID, string name)
    {//spawn object
        GameObject objToSpawn = new GameObject(string.Format("{0}", provinceID));

        //Add Components
        MeshFilter meshFilter = objToSpawn.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = objToSpawn.AddComponent<MeshRenderer>();

        // in case you want the new gameobject to be a child
        // of the gameobject that your script is attached to
        objToSpawn.transform.parent = mapObject.transform;

        Mesh mesh = meshFilter.mesh;
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = trianglesList.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshRenderer.material.shader = Shader.Find("Standard");
        meshRenderer.material.color = colorID;

        MeshCollider groundMeshCollider;
        groundMeshCollider = objToSpawn.AddComponent(typeof(MeshCollider)) as MeshCollider;
        groundMeshCollider.sharedMesh = mesh;

        vertices.Clear();
        trianglesList.Clear();
        triangleCounter = 0;

        mesh.name = provinceID.ToString();

        Province newProvince = new Province(name,
            provinceID, colorID, mesh, meshFilter, objToSpawn, meshRenderer, Product.getRandomResource(false));
        Province.allProvinces.Add(newProvince);

    }
    bool FindProvinceCenters()
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
    void makeHelloMessage()
    {
        new Message("Tutorial", "Hi, this is VERY early demo of game-like economy simulator" +
            "\n\nCurrently there is: "
            + "\n\npopulation agents \nbasic trade & production \nbasic warfare \ntechnologies \nbasic reforms (voting is not implemented)"
            + "\n\nYou play as " + Game.player.name + " country yet there is no much gameplay for now. You can try to growth economy or conquer the world."
            + "\nTry arrows or WASD for scrolling map and mouse wheel for scale"            
            , "Ok");
        ;

    }
    void LoadImages()
    {
        mapImage = Resources.Load("provinces", typeof(Texture2D)) as Texture2D; ///texture;        
        RawImage ri = GameObject.Find("RawImage").GetComponent<RawImage>();
        ri.texture = mapImage;
    }
    private static void calcBattles()
    {
        foreach (Country country in Country.allExisting)
        {
            foreach (var attackerArmy in country.allArmies)
            {
                if (attackerArmy.getDestination() != null)
                {
                    var result = attackerArmy.attack(attackerArmy.getDestination());
                    if (result.isAttackerWon())
                    {
                        attackerArmy.getDestination().secedeTo(country);
                    }
                    if (result.getAttacker() == Game.player || result.getDefender() == Game.player)
                    {
                        result.createMessage();
                        //new Message("2th message", "", "");
                        //new Message("3th message", "", "");
                        //new Message("4th message", "", "");
                    }
                    attackerArmy.moveTo(null); // go home
                }
            }
            country.allArmies.consolidate(country);
        }
    }
    internal static void stepSimulation()
    {
        date++;
        // strongly before PrepareForNewTick
        Game.market.simulatePriceChangeBasingOnLastTurnDate();

        Game.calcBattles(); // should be before PrepareForNewTick cause PrepareForNewTick hires dead workers on factories
        PopUnit.PrepareForNewTick();

        // big PRODUCE circle
        foreach (Country country in Country.allExisting)
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
                    if (pop.type.basicProduction != null)// only Farmers and Tribesmen
                        pop.produce();
                    pop.takeUnemploymentSubsidies();
                }
            }
        //Game.market.ForceDSBRecalculation();
        // big CONCUME circle
        foreach (Country country in Country.allExisting)
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
                            pop.PayTaxToAllAristocrats();
                }
                foreach (PopUnit pop in province.allPopUnits)
                {
                    pop.consume();
                }
            }
        // big AFTER all circle
        foreach (Country country in Country.allExisting)
        {
            foreach (Province province in country.ownedProvinces)//Province.allProvinces)
            {
                foreach (Factory factory in province.allFactories)
                {
                    factory.getMoneyFromMarket();
                    factory.changeSalary();
                    factory.PayDividend();
                }
                province.allFactories.RemoveAll(item => item.isToRemove());
                foreach (PopUnit pop in province.allPopUnits)
                {
                    if (pop.type == PopType.aristocrats || pop.type == PopType.capitalists || (pop.type == PopType.farmers && Economy.isMarket.checkIftrue(province.getOwner())))
                        pop.getMoneyFromMarket();

                    //becouse income come only after consuming, and only after FULL consumption
                    if (pop.canTrade() && pop.hasToPayGovernmentTaxes())
                        // POps who can't trade will pay tax BEFORE consumption, not after
                        // Otherwise pops who can't trade avoid tax
                        pop.payTaxes();

                    pop.calcLoyalty();
                    pop.calcPromotions();
                    pop.calcDemotions();
                    pop.calcGrowth();
                    pop.Invest();
                }
                foreach (PopUnit pop in PopUnit.PopListToAddToGeneralList)
                {
                    PopUnit targetToMerge = province.FindSimularPopUnit(pop);
                    if (targetToMerge == null)
                        province.allPopUnits.Add(pop);
                    else
                        targetToMerge.merge(pop);
                }
                PopUnit.PopListToAddToGeneralList.Clear();
            }
            country.Think();
        }
    }
}
