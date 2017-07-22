using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;


public class Province : Name
{
    public enum TerrainTypes
    {
        Plains, Mountains
    };
    public readonly static List<Province> allProvinces = new List<Province>();

    private readonly int ID;
    private readonly Color colorID;

    public readonly List<PopUnit> allPopUnits = new List<PopUnit>();

    //private readonly Dictionary<Province, byte> distances = new Dictionary<Province, byte>();
    private readonly List<Province> neighbors = new List<Province>();
    private Product resource;
    private Vector3 position;
    private Color color;
    //private Mesh landMesh;
    //private MeshStructure meshStructure;

    //private MeshFilter meshFilter;
    private GameObject rootGameObject;
    private MeshRenderer meshRenderer;

    private Country owner;

    public List<Factory> allFactories = new List<Factory>();

    private readonly int fertileSoil;
    readonly List<Country> cores = new List<Country>();
    private readonly Dictionary<Province, MeshRenderer> bordersMeshes = new Dictionary<Province, MeshRenderer>();
    private TerrainTypes terrain;
    private readonly Dictionary<Mod, DateTime> modifiers = new Dictionary<Mod, DateTime>();

    //empty province constructor
    public Province(string name, int iID, Color icolorID, Product resource) : base(name)
    {
        setResource(resource);

        colorID = icolorID;


        ID = iID;

        fertileSoil = 10000;

    }
    public static void preReadProvinces(MyTexture image, Game game)
    {
        ProvinceNameGenerator nameGenerator = new ProvinceNameGenerator();
        Color currentProvinceColor = image.GetPixel(0, 0);
        int provinceCounter = 0;
        for (int j = 0; j < image.getHeight(); j++) // circle by province        
            for (int i = 0; i < image.getWidth(); i++)
            {
                if (currentProvinceColor != image.GetPixel(i, j)
                    //&& !blockedProvinces.Contains(currentProvinceColor)
                    && !Province.isProvinceCreated(currentProvinceColor))
                {
                    allProvinces.Add(new Province(nameGenerator.generateProvinceName(), provinceCounter, currentProvinceColor, Product.getRandomResource(false)));
                    provinceCounter++;

                }
                currentProvinceColor = image.GetPixel(i, j);
                //game.updateStatus("Reading provinces.. x = " + i + " y = " + j);
            }
    }

    internal static void generateUnityData(VoxelGrid grid)
    {
        allProvinces.ForEach(x => x.setUnityAPI(grid.getMesh(x), grid.getBorders()));
        allProvinces.ForEach(x => x.setBorderMaterials(false));
    }
    void setUnityAPI(MeshStructure meshStructure, Dictionary<Province, MeshStructure> neighborBorders)
    {
        //this.meshStructure = meshStructure;

        //spawn object
        rootGameObject = new GameObject(string.Format("{0}", getID()));

        //Add Components
        var meshFilter = rootGameObject.AddComponent<MeshFilter>();
        meshRenderer = rootGameObject.AddComponent<MeshRenderer>();

        // in case you want the new gameobject to be a child
        // of the gameobject that your script is attached to
        rootGameObject.transform.parent = Game.mapObject.transform;

        var landMesh = meshFilter.mesh;
        landMesh.Clear();

        landMesh.vertices = meshStructure.getVertices().ToArray();
        landMesh.triangles = meshStructure.getTriangles().ToArray();
        landMesh.RecalculateNormals();
        landMesh.RecalculateBounds();
        landMesh.name = getID().ToString();

        meshRenderer.material.shader = Shader.Find("Standard");

        meshRenderer.material.color = color;

        MeshCollider groundMeshCollider = rootGameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
        groundMeshCollider.sharedMesh = landMesh;

        position = setProvinceCenter(meshStructure);
        setLabel();

        // setting neighbors
        //making meshes for border
        foreach (var border in neighborBorders)
        {
            //each color is one neighbor (non repeating)
            var neighbor = border.Key;
            if (this.getTerrain() == TerrainTypes.Plains || neighbor.terrain == TerrainTypes.Plains)
                neighbors.Add(neighbor);

            GameObject borderObject = new GameObject("Border with " + neighbor.ToString());

            //Add Components
            meshFilter = borderObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = borderObject.AddComponent<MeshRenderer>();

            borderObject.transform.parent = rootGameObject.transform;

            Mesh borderMesh = meshFilter.mesh;
            borderMesh.Clear();

            borderMesh.vertices = border.Value.getVertices().ToArray();
            borderMesh.triangles = border.Value.getTriangles().ToArray();
            borderMesh.uv = border.Value.getUVmap().ToArray();
            borderMesh.RecalculateNormals();
            borderMesh.RecalculateBounds();
            meshRenderer.material = Game.defaultProvinceBorderMaterial;
            borderMesh.name = "Border with " + neighbor.ToString();

            bordersMeshes.Add(neighbor, meshRenderer);
        }
    }
    internal TerrainTypes getTerrain()
    {
        return terrain;
    }
    public Vector3 getPosition()
    {
        return position;
    }
    public GameObject getRootGameObject()
    {
        return rootGameObject;
    }
    public void setBorderMaterial(Material material)
    {
        foreach (var item in bordersMeshes)
            item.Value.material = material;
    }
    public void setBorderMaterials(bool reWriteSelection)
    {
        foreach (var border in bordersMeshes)
        {
            if (border.Key.isNeighbor(this))
            {
                if (getCountry() == border.Key.getCountry())
                {
                    if (this != Game.selectedProvince || reWriteSelection)
                        border.Value.material = Game.defaultProvinceBorderMaterial;
                    if (border.Key != Game.selectedProvince || reWriteSelection)
                        border.Key.bordersMeshes[this].material = Game.defaultProvinceBorderMaterial;
                }
                else
                {
                    if (this != Game.selectedProvince || reWriteSelection)
                        if (getCountry() == Country.NullCountry)
                            border.Value.material = Game.defaultProvinceBorderMaterial;
                        else
                            border.Value.material = getCountry().getBorderMaterial();
                    if ((border.Key != Game.selectedProvince || reWriteSelection) && border.Key.getCountry() != null)
                        if (border.Key.getCountry() == Country.NullCountry)
                            border.Key.bordersMeshes[this].material = Game.defaultProvinceBorderMaterial;
                        else
                            border.Key.bordersMeshes[this].material = border.Key.getCountry().getBorderMaterial();
                }
            }
            else
            {
                border.Value.material = Game.impassableBorder;
                border.Key.bordersMeshes[this].material = Game.impassableBorder;
            }
        }

        //foreach (var neighbor in neighbors)
        //    if (getCountry() == neighbor.getCountry())
        //    {
        //        this.bordersMeshes[neighbor].material = Game.defaultProvinceBorderMaterial;
        //        neighbor.bordersMeshes[this].material = Game.defaultProvinceBorderMaterial;
        //    }
        //    else
        //    {
        //        {
        //            this.bordersMeshes[neighbor].material = getCountry().getBorderMaterial();
        //            if (neighbor.getCountry() != null)
        //                neighbor.bordersMeshes[this].material = neighbor.getCountry().getBorderMaterial();
        //        }
        //    }
    }


    /// <summary>
    /// returns 
    /// </summary>

    internal Country getCountry()
    {
        //if (owner == null)
        //    return Country.NullCountry;
        //else
        return owner;
    }
    internal int getID()
    { return ID; }
    /// <summary>
    /// called only on map generation
    /// </summary>    
    public void InitialOwner(Country taker)
    {
        // in case if province already have owner..
        //if (this.getCountry() != null)
        //    if (this.getCountry().ownedProvinces != null)
        //        this.getCountry().ownedProvinces.Remove(this);
        owner = taker;

        if (taker.ownedProvinces == null)
            taker.ownedProvinces = new List<Province>();
        taker.ownedProvinces.Add(this);
        color = taker.getColor().getAlmostSameColor();

        if (taker != Country.NullCountry)
            cores.Add(taker);


    }
    public void simulate()
    {
        if (Game.Random.Next(Options.ProvinceChanceToGetCore) == 1)
            if (neighbors.Any(x => x.isCoreFor(getCountry())) && !cores.Contains(getCountry()) && getMajorCulture() == getCountry().getCulture())
                cores.Add(getCountry());
        // modifiers.LastOrDefault()
        //foreach (var item in modifiers)
        //{
        //    if (item.Value.isDatePassed())
        //}
        modifiers.RemoveAll((modifier, date) => date != default(DateTime) && date.isDatePassed());

    }
    public bool isCoreFor(Country country)
    {
        return cores.Contains(country);
    }
    public bool isCoreFor(PopUnit pop)
    {
        return cores.Any(x => x.getCulture() == pop.culture);

    }
    public string getCoresDescription()
    {
        if (cores.Count == 0)
            return "none";
        else
            if (cores.Count == 1)
            return cores[0].getName();
        else
        {
            StringBuilder sb = new StringBuilder();
            cores.ForEach(x => sb.Append(x.getName()).Append("; "));
            return sb.ToString();
        }
    }
    public void secedeTo(Country taker, bool addModifier)
    {
        Country oldCountry = getCountry();
        //refuse loans to old country bank
        foreach (var producer in getProducers())
        {
            if (producer.loans.get() != 0f)
                getCountry().bank.defaultLoaner(producer);
            //take back deposits            
            oldCountry.bank.returnAllMoney(producer);
        }
        //allFactories.Where(x => x.getOwner() == oldCountry)o;
        allFactories.FindAndDo(x => x.getOwner() == oldCountry, x => x.setOwner(taker));
        if (oldCountry.isOneProvince())
            oldCountry.killCountry(taker);
        else
            if (isCapital())
            oldCountry.moveCapitalTo(oldCountry.getRandomOwnedProvince(x => x != this));


        oldCountry.demobilize(x => x.getPopUnit().province == this);

        // add loyalty penalty for conquered province // temp
        foreach (var pop in allPopUnits)
        {
            if (pop.culture == taker.getCulture())
                pop.loyalty.add(Options.PopLoyaltyChangeOnAnnexStateCulture);
            else
                pop.loyalty.subtract(Options.PopLoyaltyChangeOnAnnexNonStateCulture, false);
            pop.loyalty.clamp100();
            Movement.leave(pop);
            //item.setMovement(null);
        }


        if (oldCountry != null)
            if (oldCountry.ownedProvinces != null)
                oldCountry.ownedProvinces.Remove(this);
        owner = taker;

        if (taker.ownedProvinces == null)
            taker.ownedProvinces = new List<Province>();
        taker.ownedProvinces.Add(this);

        color = taker.getColor().getAlmostSameColor();
        meshRenderer.material.color = Game.getProvinceColorAccordingToMapMode(this);

        setBorderMaterials(false);
        if (addModifier)
            if (modifiers.ContainsKey(Mod.recentlyConquered))
                modifiers[Mod.recentlyConquered] = Game.date.AddYears(20);
            else
                modifiers.Add(Mod.recentlyConquered, Game.date.AddYears(20));
        // modifiers.Add(Mod.blockade, default(DateTime));
    }
    public int howFarFromCapital()
    {
        return 0;
    }
    public Dictionary<Mod, DateTime> getModifiers()
    {
        return modifiers;
    }
    internal bool isCapital()
    {
        return getCountry().getCapital() == this;
    }
    public IEnumerable<Country> getCores()
    {
        foreach (var core in cores)
            yield return core;
    }
    internal Country getRandomCore()
    {
        return cores.PickRandom();
    }
    internal Country getRandomCore(Predicate<Country> predicate)
    {
        return cores.FindAll(predicate).PickRandom();
    }
    internal static Province getRandomProvinceInWorld(Predicate<Province> predicate)
    {
        return allProvinces.PickRandom(predicate);
    }
    internal List<Province> getNeigbors(Predicate<Province> predicate)
    {
        return neighbors.FindAll(predicate);

    }

    public IEnumerable<Producer> getProducers()
    //public System.Collections.IEnumerator GetEnumerator()
    {
        foreach (Factory f in allFactories)
            yield return f;
        foreach (PopUnit f in allPopUnits)
            //if (f.type == PopType.farmers || f.type == PopType.aristocrats)
            yield return f;
    }
    public static Vector3 setProvinceCenter(MeshStructure meshStructure)
    {
        Vector3 accu = new Vector3(0, 0, 0);
        foreach (var c in meshStructure.getVertices())
            accu += c;
        accu = accu / meshStructure.verticesCount;
        return accu;

    }

    internal Culture getMajorCulture()
    {
        Dictionary<Culture, int> cultures = new Dictionary<Culture, int>();

        foreach (var pop in allPopUnits)
            //if (cultures.ContainsKey(pop.culture))
            //    cultures[pop.culture] += pop.getPopulation();
            //else
            //    cultures.Add(pop.culture, pop.getPopulation());
            cultures.AddMy(pop.culture, pop.getPopulation());
        ///allPopUnits.ForEach(x=>cultures.Add(x.culture, x.getPopulation()));
        return cultures.MaxBy(y => y.Value).Key as Culture;
    }

    public int getMenPopulation()
    {
        int result = 0;
        foreach (PopUnit pop in allPopUnits)
            result += pop.getPopulation();
        return result;
    }

    internal bool isBelongsTo(Country country)
    {
        return this.getCountry() == country;
    }
    internal bool isNeighbor(Country country)
    {
        return neighbors.Any(x => x.getCountry() == country);
    }
    internal bool isNeighbor(Province province)
    {
        return neighbors.Contains(province);
    }

    public int getFamilyPopulation()
    {
        return getMenPopulation() * Options.familySize;
    }

    internal float getIncomeTax()
    {
        float res = 0f;
        allPopUnits.ForEach(x => res += x.incomeTaxPayed.get());
        return res;
    }

    public Procent getAverageLoyalty()
    {
        Procent result = new Procent(0f);
        int calculatedPopulation = 0;
        foreach (PopUnit pop in allPopUnits)
        {
            result.addPoportionally(calculatedPopulation, pop.getPopulation(), pop.loyalty);
            calculatedPopulation += pop.getPopulation();
        }
        return result;
    }

    internal void mobilize()
    {
        getCountry().mobilize(new List<Province> { this });
    }

    public static bool isProvinceCreated(Color color)
    {
        foreach (Province anyProvince in allProvinces)
            if (anyProvince.colorID == color)
                return true;
        return false;
    }

    public List<PopUnit> getAllPopUnits(PopType ipopType)
    {
        List<PopUnit> result = new List<PopUnit>();
        foreach (PopUnit pop in allPopUnits)
            if (pop.popType == ipopType)
                result.Add(pop);
        return result;
    }


    public static Province find(Color color)
    {
        foreach (Province anyProvince in allProvinces)
            if (anyProvince.colorID == color)
                return anyProvince;
        return null;
    }
    internal static Province find(int number)
    {
        foreach (var pro in allProvinces)
            if (pro.ID == number)
                return pro;
        return null;
    }

    public int getPopulationAmountByType(PopType ipopType)
    {
        List<PopUnit> list = getAllPopUnits(ipopType);
        int result = 0;
        foreach (PopUnit pop in list)
            if (pop.popType == ipopType)
                result += pop.getPopulation();
        return result;
    }
    //not called with capitalism
    internal void shareWithAllAristocrats(Storage fromWho, Value taxTotalToPay)
    {
        List<PopUnit> allAristocratsInProvince = getAllPopUnits(PopType.Aristocrats);
        int aristoctratAmount = 0;
        foreach (PopUnit pop in allAristocratsInProvince)
            aristoctratAmount += pop.getPopulation();
        foreach (Aristocrats aristocrat in allAristocratsInProvince)
        {
            Value howMuch = new Value(taxTotalToPay.get() * (float)aristocrat.getPopulation() / (float)aristoctratAmount);
            fromWho.send(aristocrat.storageNow, howMuch);
            aristocrat.gainGoodsThisTurn.add(howMuch);
            aristocrat.dealWithMarket();
            //aristocrat.sentToMarket.set(aristocrat.gainGoodsThisTurn);
            //Game.market.tmpMarketStorage.add(aristocrat.gainGoodsThisTurn);
        }
    }

    internal void updateColor(Color color)
    {
        meshRenderer.material.color = color;
    }

    ///<summary> Similar by popType & culture</summary>    
    public PopUnit getSimilarPopUnit(PopUnit target)
    {
        foreach (PopUnit pop in allPopUnits)
            if (pop.popType == target.popType && pop.culture == target.culture && pop.isAlive())
                return pop;
        return null;
    }

    internal Color getColorID()
    {
        return colorID;
    }
    internal Color getColor()
    {
        return color;
    }

    public Value getAverageNeedsFulfilling(PopType type)
    {
        Value result = new Value(0);
        int allPopulation = 0;
        List<PopUnit> localPops = getAllPopUnits(type);
        if (localPops.Count > 0)
        {
            foreach (PopUnit pop in localPops)
            // get middle needs fulfilling according to pop weight            
            {
                allPopulation += pop.getPopulation();
                result.add(pop.needsFullfilled.multipleOutside(pop.getPopulation()));
            }
            if (allPopulation > 0)
                return result.divideOutside(allPopulation);
            else
                return new Value(1f);
        }
        else
        {
            return new Value(1f);
        }
    }
    public void BalanceEmployableWorkForce()
    {
        List<PopUnit> workforceList = this.getAllPopUnits(PopType.Workers);
        int totalWorkForce = workforceList.Sum(x => x.getPopulation());
        int factoryWants = 0;
        //int factoryWantsTotal = 0;

        //foreach (PopUnit pop in workforceList)
        //    totalWorkForce += pop.getPopulation();

        int popsLeft = totalWorkForce;
        if (totalWorkForce > 0)
        {
            // workforceList = workforceList.OrderByDescending(o => o.population).ToList();
            allFactories = allFactories.OrderByDescending(o => o.getSalary()).ToList();
            //foreach (Factory shownFactory in allFactories)
            //    factoryWantsTotal += shownFactory.HowMuchWorkForceWants();
            //if (factoryWantsTotal > 0)
            foreach (Factory factory in allFactories)
            {
                if (factory.isWorking() && factory.getSalary() > 0f)
                {
                    factoryWants = factory.HowMuchWorkForceWants();
                    if (factoryWants > popsLeft)
                        factoryWants = popsLeft;

                    //if (factoryWants > 0)
                    //shownFactory.HireWorkforce(totalWorkForce * factoryWants / factoryWantsTotal, workforceList);

                    //popsLeft -= factoryWants;                    
                    popsLeft -= factory.hireWorkforce(factoryWants, workforceList);

                    //if (popsLeft <= 0) break;
                }
                else
                {
                    factory.hireWorkforce(0, null);
                }
            }
        }
    }
    internal void setResource(Product inres)
    {
        resource = inres;
        if (resource == Product.Stone || resource == Product.Gold || resource == Product.MetallOre)
            terrain = TerrainTypes.Mountains;
        else
            terrain = TerrainTypes.Plains;
    }
    internal Product getResource()
    {
        //if (getOwner().isInvented(resource))
        if (resource.isInventedByAnyOne())
            return resource;
        else
            return null;
    }
    internal Factory getResourceFactory()
    {
        foreach (Factory f in allFactories)
            if (f.type.basicProduction.getProduct() == resource)
                return f;
        return null;
    }

    internal List<FactoryType> whatFactoriesCouldBeBuild()
    {
        List<FactoryType> result = new List<FactoryType>();
        foreach (FactoryType ft in FactoryType.allTypes)
            if (CanBuildNewFactory(ft))
                result.Add(ft);
        return result;
    }


    internal bool CanBuildNewFactory(FactoryType ft)
    {
        if (HaveFactory(ft))
            return false;
        if ((ft.isResourceGathering() && ft.basicProduction.getProduct() != this.resource) || !ft.basicProduction.getProduct().isInvented(getCountry()))
            return false;

        return true;
    }
    internal bool CanUpgradeFactory(FactoryType ft)
    {
        if (!HaveFactory(ft))
            return false;
        // if (ft.isResourceGathering() && ft.basicProduction.getProduct() != this.resource)
        //     return false;

        return true;
    }
    internal bool HaveFactory(FactoryType ft)
    {
        foreach (Factory f in allFactories)
            if (f.type == ft)
                return true;
        return false;
    }
    public Procent getUnemployment()
    {
        Procent result = new Procent(0f);
        int calculatedBase = 0;
        foreach (var item in allPopUnits)
        {
            if (item.popType.canBeUnemployed())
                result.addPoportionally(calculatedBase, item.getPopulation(), item.getUnemployedProcent());
            calculatedBase += item.getPopulation();
        }
        return result;
    }
    internal int getUnemployedWorkers()
    {
        //int result = 0;
        //List<PopUnit> list = this.FindAllPopUnits(PopType.workers);
        //foreach (PopUnit pop in list)
        //    result += pop.getUnemployed();
        int totalWorkforce = this.getPopulationAmountByType(PopType.Workers);
        if (totalWorkforce == 0) return 0;
        int employed = 0;

        foreach (Factory factory in allFactories)
            employed += factory.getWorkForce();
        return totalWorkforce - employed;
    }
    internal bool isThereFactoriesInUpgradeMoreThan(int limit)
    {
        int counter = 0;
        foreach (Factory factory in allFactories)
            if (factory.isUpgrading())
            {
                counter++;
                if (counter == limit)
                    return true;
            }
        return false;
    }

    internal void setLabel()
    {
        LODGroup group = rootGameObject.AddComponent<LODGroup>();

        // Add 4 LOD levels
        LOD[] lods = new LOD[1];
        Transform txtMeshTransform = GameObject.Instantiate(Game.r3dTextPrefab).transform;
        txtMeshTransform.SetParent(this.rootGameObject.transform, false);
        Renderer[] renderers = new Renderer[1];
        renderers[0] = txtMeshTransform.GetComponent<Renderer>();
        lods[0] = new LOD(0.25F, renderers);

        txtMeshTransform.position = this.getPosition();
        TextMesh txtMesh = txtMeshTransform.GetComponent<TextMesh>();

        txtMesh.text = this.ToString();
        txtMesh.color = Color.red; // Set the text's color to red
        group.SetLODs(lods);
#if UNITY_WEBGL
        group.size = 30; // for webgl
#else
        group.size = 20; // for others
#endif
        //group.RecalculateBounds();
    }

    internal Factory findFactory(FactoryType proposition)
    {
        foreach (Factory f in allFactories)
            if (f.type == proposition)
                return f;
        return null;
    }
    internal bool isProducingOnFactories(PrimitiveStorageSet resourceInput)
    {
        foreach (Storage stor in resourceInput)
            foreach (Factory factory in allFactories)
                if (factory.isWorking() && factory.type.basicProduction.getProduct() == stor.getProduct())
                    return true;
        return false;
    }
    /// <summary>
    /// Adjusted to use in modifiers 
    /// </summary>    
    internal float getOverpopulationAdjusted()
    {
        float res = getOverpopulation();
        res -= 1f;
        if (res <= 0f) res = 0f;
        return res;
    }
    internal float getOverpopulation()
    {
        float usedLand = 0f;
        foreach (PopUnit pop in allPopUnits)
            if (pop.popType == PopType.TribeMen)
                usedLand += pop.getPopulation() * Options.PopMinLandForTribemen;
            else if (pop.popType == PopType.Farmers)
                usedLand += pop.getPopulation() * Options.PopMinLandForFarmers;
            else
                usedLand += pop.getPopulation() * Options.PopMinLandForTownspeople;

        return usedLand / fertileSoil;
    }
    /// <summary>Returns salary of a factory with lowest salary in province. If only one factory in province, then returns Country.minsalary
    /// \nCould auto-drop salary on minSalary of there is problems with inputs</summary>
    internal float getLocalMinSalary()
    {
        float res;
        if (allFactories.Count <= 1)
            res = getCountry().getMinSalary();
        else
        {
            float minSalary;
            minSalary = getLocalMaxSalary();

            foreach (Factory fact in allFactories)
                if (fact.isWorking() && !fact.isJustHiredPeople())
                {
                    if (fact.getSalary() < minSalary)
                        minSalary = fact.getSalary();
                }
            res = minSalary;
        }
        if (res == 0f) res = Options.FactoryMinPossibleSallary;
        return res;
    }

    internal void addNeigbor(Province found)
    {
        //if (found != this && !distances.ContainsKey(found))
        //    distances.Add(found, 1);
        //if (!neighbors.Contains(found))
        //    neighbors.Add(found);

    }
    /// <summary>
    /// for debug reasons
    /// </summary>
    /// <returns></returns>
    //internal string getNeigborsList()
    //{
    //    StringBuilder sb = new StringBuilder();
    //    foreach (var t in distances)
    //        sb.Append("\n").Append(t.Key.ToString());
    //    return sb.ToString();
    //}
    /// <summary>Returns salary of a factory with maximum salary in province. If no factory in province, then returns Country.minsalary
    ///</summary>
    internal float getLocalMaxSalary()
    {
        if (allFactories.Count <= 1)
            return getCountry().getMinSalary();
        else
        {
            float maxSalary;
            maxSalary = allFactories.First().getSalary();

            foreach (Factory fact in allFactories)
                if (fact.isWorking())
                {
                    if (fact.getSalary() > maxSalary)
                        maxSalary = fact.getSalary();
                }
            return maxSalary;
        }
    }
    internal float getAverageFactoryWorkforceFullfilling()
    {
        int workForce = 0;
        int capacity = 0;
        foreach (Factory fact in allFactories)
            if (fact.isWorking())
            {
                workForce += fact.getWorkForce();
                capacity += fact.getMaxWorkforceCapacity();
            }
        if (capacity == 0) return 0f;
        else
            return workForce / (float)capacity;
    }
    public void consolidatePops()
    {
        if (allPopUnits.Count > 14)
        //get some small pop and merge it into bigger
        {
            PopUnit popToMerge = getRandomPop((x) => x.getPopulation() < Options.PopSizeConsolidationLimit);
            //PopUnit popToMerge = getSmallerPop((x) => x.getPopulation() < Options.PopSizeConsolidationLimit);
            if (popToMerge != null)
            {
                PopUnit targetPop = this.getBiggerPop(x => x.isStateCulture() == popToMerge.isStateCulture()
                   && x.popType == popToMerge.popType
                   && x != popToMerge);
                if (targetPop != null)
                    targetPop.mergeIn(popToMerge);
            }

        }
    }

    private PopUnit getBiggerPop(Predicate<PopUnit> predicate)
    {
        return allPopUnits.FindAll(predicate).MaxBy(x => x.getPopulation());
    }
    private PopUnit getSmallerPop(Predicate<PopUnit> predicate)
    {
        return allPopUnits.FindAll(predicate).MinBy(x => x.getPopulation());
    }

    private PopUnit getRandomPop(Predicate<PopUnit> predicate)
    {
        return allPopUnits.PickRandom(predicate);
    }
    private PopUnit getRandomPop()
    {
        return allPopUnits.PickRandom();
    }

    internal bool hasAnotherPop(PopType type)
    {
        int result = 0;
        foreach (PopUnit pop in allPopUnits)
        {
            if (pop.popType == type)
            {
                result++;
                if (result == 2)
                    return true;
            }
        }
        return false;
    }
    public bool hasModifier(Mod modifier)
    {
        return modifiers.ContainsKey(modifier);
    }
}
public class Mod : Name
{
    static readonly public Mod recentlyConquered = new Mod("Recently conquered");
    static readonly public Mod blockade = new Mod("Blockade");

    //private readonly DateTime expireDate;
    public Mod(string name) : base(name)
    { }

    //public Mod(string name, int years) : base(name)
    //{
    //    expireDate = Game.date;
    //    expireDate.AddYears(years);
    //}
}
