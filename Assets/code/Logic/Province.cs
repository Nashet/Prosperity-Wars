using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

public class Province
{
    readonly Color colorID;
    Color color;
    readonly public Mesh landMesh;
    Mesh borderMesh;
    readonly MeshFilter meshFilter;
    readonly internal GameObject rootGameObject;
    readonly MeshRenderer meshRenderer;
    //public static int maxTribeMenCapacity = 2000;
    private readonly string name;
    private readonly int ID;
    Country owner;
    public readonly List<PopUnit> allPopUnits = new List<PopUnit>();
    public Vector3 centre;

    public readonly static List<Province> allProvinces = new List<Province>();

    public List<Factory> allFactories = new List<Factory>();
    private readonly Dictionary<Province, byte> distances = new Dictionary<Province, byte>();
    private readonly List<Province> neighbors = new List<Province>();
    Product resource;
    readonly internal int fertileSoil;
    readonly List<Country> cores = new List<Country>();
    List<EdgeHelpers.Edge> edges;
    Dictionary<Province, MeshRenderer> bordersMeshes = new Dictionary<Province, MeshRenderer>();
    public Province(string iname, int iID, Color icolorID, Mesh imesh, MeshFilter imeshFilter, GameObject igameObject, MeshRenderer imeshRenderer, Product inresource)
    {
        // List<int> trianglesList = new List<int>();
        //List<Vector3> vertices = new List<Vector3>();
        //int triangleCounter = 0;

        //rootGameObject = mapObject;
        //    //spawn object
        //    GameObject objToSpawn = new GameObject(string.Format("{0}", iID));

        //    // in case you want the new gameobject to be a child
        //    // of the gameobject that your script is attached to
        //    objToSpawn.transform.parent = mapObject.transform;

        //    //Add Components
        //    meshFilter = objToSpawn.AddComponent<MeshFilter>();
        //    meshRenderer = objToSpawn.AddComponent<MeshRenderer>();



        //    landMesh = meshFilter.mesh;
        //    landMesh.Clear();

        //    landMesh.vertices = meshToCopy.vertices;
        //    landMesh.triangles = meshToCopy.triangles;
        //    landMesh.RecalculateNormals();
        //    landMesh.RecalculateBounds();

        //    meshRenderer.material.shader = Shader.Find("Standard");
        //    meshRenderer.material.color = colorID;

        //    MeshCollider groundMeshCollider;
        //    groundMeshCollider = objToSpawn.AddComponent(typeof(MeshCollider)) as MeshCollider;
        //    groundMeshCollider.sharedMesh = landMesh;

        //    //vertices.Clear();
        //    //trianglesList.Clear();
        //    //triangleCounter = 0;

        //    landMesh.name = iID.ToString();


        allProducers = getProducers();
        resource = inresource;
        colorID = icolorID; landMesh = imesh; name = iname; meshFilter = imeshFilter;
        ID = iID;
        rootGameObject = igameObject;
        meshRenderer = imeshRenderer;

        fertileSoil = 10000;
        setProvinceCenter();
        SetLabel();

        //makeBordersMesh();
        //var lr = rootGameObject.AddComponent<LineRenderer>();
        ////lr.loop = true;
        //var perimeterVerices = landMesh.getPerimeterVerices(true);
        ////perimeterVerices.ForEach(x => x.);
        ////foreach (var item in perimeterVerices)
        ////{
        ////    item.Set(item.x, item.y, item.z - 0.5f);
        ////}
        //lr.positionCount = perimeterVerices.Length;
        //lr.SetPositions(perimeterVerices);
        //// lineRenderer.SetVertexCount(2);
        ////.SetColors(c1, c2);

        ////lineRenderer.SetWidth(1, 1);
        ////Debug.Log("rendered line");
    }
    public static void makeAllBordersMesh()
    {
        allProvinces.ForEach(x => x.edges = EdgeHelpers.GetEdges(x.landMesh.triangles).FindBoundary());
        allProvinces.ForEach(x => x.makeBordersMesh());
    }
    MeshRenderer makeBorderMesh(Province neghbor, List<EdgeHelpers.Edge> edges)
    {
        float borderWidth = 0.8f;
        float borderWidth2 = -0.8f;
        float borderHeight = 0.1f;
        // GameObject objToSpawn = new GameObject(string.Format("{0} border", getID()));
        GameObject objToSpawn = new GameObject("Border with " + neghbor.ToString());

        //Add Components
        MeshFilter meshFilter = objToSpawn.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = objToSpawn.AddComponent<MeshRenderer>();

        objToSpawn.transform.parent = rootGameObject.transform;

        borderMesh = meshFilter.mesh;
        borderMesh.Clear();

        //var perimeterVertices = landMesh.getPerimeterVerices(false);
        Vector3[] borderVertices = new Vector3[edges.Count * 4];
        Vector2[] UVmap = new Vector2[edges.Count * 4];
        int[] borderTriangles = new int[edges.Count * 6];
        int vertexCounter = 0;
        int i = 0;
        foreach (var item in edges)
        {
            borderVertices[i * 2 + 0] = landMesh.vertices[item.v1] + Vector3.back * borderHeight;
            UVmap[i * 2 + 0] = new Vector2(0f, 1f);

            borderVertices[i * 2 + 1] = MeshExtensions.makeArrow(landMesh.vertices[item.v1], landMesh.vertices[item.v2], borderWidth) + Vector3.back * borderHeight;
            UVmap[i * 2 + 1] = new Vector2(1f, 1f);

            borderVertices[i * 2 + 2] = landMesh.vertices[item.v2] + Vector3.back * borderHeight;
            UVmap[i * 2 + 2] = new Vector2(0f, 0f);

            borderVertices[i * 2 + 3] = MeshExtensions.makeArrow(landMesh.vertices[item.v2], landMesh.vertices[item.v1], borderWidth2) + Vector3.back * borderHeight;
            UVmap[i * 2 + 3] = new Vector2(1f, 0f);

            borderTriangles[i * 3 + 0] = 0 + vertexCounter;
            borderTriangles[i * 3 + 1] = 2 + vertexCounter;
            borderTriangles[i * 3 + 2] = 1 + vertexCounter;

            borderTriangles[i * 3 + 3] = 2 + vertexCounter;
            borderTriangles[i * 3 + 4] = 3 + vertexCounter;
            borderTriangles[i * 3 + 5] = 1 + vertexCounter;

            vertexCounter += 4;
            i += 2;
        }

        borderMesh.vertices = borderVertices;
        borderMesh.triangles = borderTriangles;
        borderMesh.uv = UVmap;
        borderMesh.RecalculateNormals();
        borderMesh.RecalculateBounds();


        meshRenderer.material = Game.defaultProvinceBorderMaterial;
        borderMesh.name = "Border with " + neghbor.ToString();
        return meshRenderer;
    }
    void makeBordersMesh()
    {

        //if (Game.Random.Next(10) == 1)

        foreach (var neighbor in neighbors)
        {
            var filtredEdges = filterNeighborEdges(neighbor);
            bordersMeshes.Add(neighbor, makeBorderMesh(neighbor, filtredEdges));
        }

    }
    public List<EdgeHelpers.Edge> filterNeighborEdges(Province neighbor)
    {
        List<EdgeHelpers.Edge> res = new List<EdgeHelpers.Edge>();

        foreach (var checkingEdge in edges)
        {
            //if neighbor has checkingEdge add it in res
            foreach (var comparingEdge in neighbor.edges)
                if (MeshExtensions.isTwoLinesTouchEachOther(landMesh.vertices[checkingEdge.v1],
                    landMesh.vertices[checkingEdge.v2],
            neighbor.landMesh.vertices[comparingEdge.v1],
            neighbor.landMesh.vertices[comparingEdge.v2])
            )
                    res.Add(checkingEdge);
        }

        //foreach (var checkingEdge in edges)
        //{
        //    //if (!mesh.hasDuplicateOfEdge(item.v1, item.v2))
        //    // check only in edges!
        //    // need vector by vector comprasion
        //    int foundDuplicates = 0;
        //    foreach (var comparingEdge in edges)
        //    {
        //        //if (checkingEdge == comparingEdge)
        //        if (mesh.isSameEdge(checkingEdge.v1, checkingEdge.v2, comparingEdge.v1, comparingEdge.v2))
        //        {
        //            foundDuplicates++;
        //            if (foundDuplicates > 1) // 1 - is edge itself
        //                break;
        //        }
        //    }
        //    if (foundDuplicates < 2)
        //        res.Add(checkingEdge);
        //}
        return res;
    }
    /// <summary>
    /// returns 
    /// </summary>
    /// <returns></returns>
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
        if (this.getCountry() != null)
            if (this.getCountry().ownedProvinces != null)
                this.getCountry().ownedProvinces.Remove(this);
        owner = taker;

        if (taker.ownedProvinces == null)
            taker.ownedProvinces = new List<Province>();
        taker.ownedProvinces.Add(this);
        color = taker.getColor().getAlmostSameColor();
        meshRenderer.material.color = color;
        if (taker != Country.NullCountry)
            cores.Add(taker);

        foreach (var neighbor in neighbors)
            if (getCountry() == neighbor.getCountry())
            {
                this.bordersMeshes[neighbor].material = Game.defaultProvinceBorderMaterial;
                neighbor.bordersMeshes[this].material = Game.defaultProvinceBorderMaterial;
            }
            else
            {
                // if (getCountry() != Country.NullCountry && neighbor.getCountry() != Country.NullCountry)
                {
                    this.bordersMeshes[neighbor].material = getCountry().getBorderMaterial();
                    if (neighbor.getCountry() != null)
                        neighbor.bordersMeshes[this].material = neighbor.getCountry().getBorderMaterial();
                }
            }
    }
    public void think()
    {
        if (Game.Random.Next(Options.ProvinceChanceToGetCore) == 1)
            if (neighbors.Any(x => x.isCoreFor(getCountry())) && !cores.Contains(getCountry()) && getMajorCulture() == getCountry().getCulture())
                cores.Add(getCountry());
    }
    public bool isCoreFor(Country country)
    {
        return cores.Contains(country);
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
    public void secedeTo(Country taker)
    {
        Country oldCountry = getCountry();
        //refuse loans to old country bank
        foreach (var producer in allProducers)
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


        oldCountry.staff.demobilize(x => x.getPopUnit().province == this);

        // add loyalty penalty for conquered province // temp
        allPopUnits.ForEach(x => x.loyalty.set(0f));

        if (oldCountry != null)
            if (oldCountry.ownedProvinces != null)
                oldCountry.ownedProvinces.Remove(this);
        owner = taker;

        if (taker.ownedProvinces == null)
            taker.ownedProvinces = new List<Province>();
        taker.ownedProvinces.Add(this);

        color = taker.getColor().getAlmostSameColor();
        meshRenderer.material.color = Game.getProvinceColorAccordingToMapMode(this);

        foreach (var neighbor in neighbors)
            if (getCountry() == neighbor.getCountry())
            {
                this.bordersMeshes[neighbor].material = Game.defaultProvinceBorderMaterial;
                neighbor.bordersMeshes[this].material = Game.defaultProvinceBorderMaterial;
            }
            else
            {
                // if (getCountry() != Country.NullCountry && neighbor.getCountry() != Country.NullCountry)
                {
                    this.bordersMeshes[neighbor].material = getCountry().getBorderMaterial();
                    if (neighbor.getCountry() != null)
                        neighbor.bordersMeshes[this].material = neighbor.getCountry().getBorderMaterial();
                }
            }
    }

    internal bool isCapital()
    {
        return getCountry().getCapital() == this;
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
    internal IEnumerable<Producer> allProducers;
    IEnumerable<Producer> getProducers()
    //public System.Collections.IEnumerator GetEnumerator()
    {
        foreach (Factory f in allFactories)
            yield return f;
        foreach (PopUnit f in allPopUnits)
            //if (f.type == PopType.farmers || f.type == PopType.aristocrats)
            yield return f;
    }
    public void setProvinceCenter()
    {
        Vector3 accu = new Vector3(0, 0, 0);
        //foreach (Province pro in Province.allProvinces)
        //{
        //e accu.Set(0, 0, 0);
        foreach (var c in this.landMesh.vertices)
            accu += c;
        accu = accu / this.landMesh.vertices.Length;
        this.centre = accu;
        // }
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
    internal bool isNeghbour(Country country)
    {
        return neighbors.Any(x => x.getCountry() == country);
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

    public Procent getMiddleLoyalty()
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
        getCountry().staff.mobilize(new List<Province> { this });
    }

    public static bool isProvinceCreated(Color color)
    {
        foreach (Province anyProvince in allProvinces)
            if (anyProvince.colorID == color)
                return true;
        return false;
    }
    public static Province findProvince(Color color)
    {
        foreach (Province anyProvince in allProvinces)
            if (anyProvince.colorID == color)
                return anyProvince;
        return null;
    }
    public List<PopUnit> getAllPopUnits(PopType ipopType)
    {
        List<PopUnit> result = new List<PopUnit>();
        foreach (PopUnit pop in allPopUnits)
            if (pop.type == ipopType)
                result.Add(pop);
        return result;
    }



    internal static Province findByID(int number)
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
            if (pop.type == ipopType)
                result += pop.getPopulation();
        return result;
    }
    //not called with capitalism
    internal void shareWithAllAristocrats(Storage fromWho, Value taxTotalToPay)
    {
        List<PopUnit> allAristocratsInProvince = getAllPopUnits(PopType.aristocrats);
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
            if (pop.type == target.type && pop.culture == target.culture)
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

    public Value getMiddleNeedsFulfilling(PopType type)
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
            return result.divideOutside(allPopulation);
        }
        else/// add default population - no, don't, we now fixed it
        {
            //PopUnit.PopListToAddToGeneralList.Add(PopUnit.Instantiate(Province.defaultPopulationSpawn, type, this.getOwner().culture, this));
            //return new Value(float.MaxValue);// meaning always convert in type if does not exist yet
            return new Value(1f);
        }
    }
    public void BalanceEmployableWorkForce()
    {
        List<PopUnit> workforceList = this.getAllPopUnits(PopType.workers);
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
                if (factory.isWorking())
                {
                    factoryWants = factory.HowMuchWorkForceWants();
                    if (factoryWants > popsLeft)
                        factoryWants = popsLeft;

                    //if (factoryWants > 0)
                    //shownFactory.HireWorkforce(totalWorkForce * factoryWants / factoryWantsTotal, workforceList);
                    if (factoryWants > 0 && factory.getWorkForce() == 0)
                        factory.justHiredPeople = true;
                    else
                        factory.justHiredPeople = false;
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

    internal List<FactoryType> WhatFactoriesCouldBeBuild()
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
    override public string ToString()
    {
        return name;
    }
    public Procent getUnemployment()
    {
        Procent result = new Procent(0f);
        int calculatedBase = 0;
        foreach (var item in allPopUnits)
        {
            if (item.type.canBeUnemployed())
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
        int totalWorkforce = this.getPopulationAmountByType(PopType.workers);
        if (totalWorkforce == 0) return 0;
        int employed = 0;

        foreach (Factory factory in allFactories)
            employed += factory.getWorkForce();
        return totalWorkforce - employed;
    }
    internal bool isThereMoreThanFactoriesInUpgrade(int limit)
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

    internal void SetLabel()
    {

        Transform txtMeshTransform = GameObject.Instantiate(Game.r3dTextPrefab).transform;
        txtMeshTransform.SetParent(this.rootGameObject.transform, false);

        //newProvince.centre = (meshRenderer.bounds.max + meshRenderer.bounds.center) / 2f;
        txtMeshTransform.position = this.centre;

        TextMesh txtMesh = txtMeshTransform.GetComponent<TextMesh>();
        txtMesh.text = this.ToString();
        txtMesh.color = Color.red; // Set the text's color to red

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
    internal float getOverpopulation()
    {
        float usedLand = 0f;
        foreach (PopUnit pop in allPopUnits)
            switch (pop.type.type)
            {
                case PopType.PopTypes.Tribemen:
                    usedLand += pop.getPopulation() * Options.PopMinLandForTribemen;
                    break;
                case PopType.PopTypes.Farmers:
                    usedLand += pop.getPopulation() * Options.PopMinLandForFarmers;
                    break;
                default:
                    usedLand += pop.getPopulation() * Options.PopMinLandForTownspeople;
                    break;
            }
        return usedLand / fertileSoil;
    }
    /// <summary>Returns salary of a factory with lowest salary in province. If only one factory in province, then returns Country.minsalary
    /// \nCould auto-drop salary on minSalary of there is problems with inputs</summary>
    internal float getLocalMinSalary()
    {
        if (allFactories.Count <= 1)
            return getCountry().getMinSalary();
        else
        {
            float minSalary;
            minSalary = getLocalMaxSalary();

            foreach (Factory fact in allFactories)
                if (fact.isWorking() && !fact.justHiredPeople)
                {
                    if (minSalary > fact.getSalary())
                        minSalary = fact.getSalary();
                }
            return minSalary;
        }
    }

    internal void addNeigbor(Province found)
    {
        if (found != this && !distances.ContainsKey(found))
            distances.Add(found, 1);
        if (!neighbors.Contains(found))
            neighbors.Add(found);

    }
    /// <summary>
    /// for debug reasons
    /// </summary>
    /// <returns></returns>
    internal string getNeigborsList()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var t in distances)
            sb.Append("\n").Append(t.Key.ToString());
        return sb.ToString();
    }
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
    internal float getMiddleFactoryWorkforceFullfilling()
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
                   && x.type == popToMerge.type
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
            if (pop.type == type)
            {
                result++;
                if (result == 2)
                    return true;
            }
        }
        return false;
    }

}
