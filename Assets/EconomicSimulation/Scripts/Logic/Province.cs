using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using Nashet.UnityUIUtils;
using Nashet.MarchingSquares;
using Nashet.Conditions;
using Nashet.ValueSpace;
using Nashet.Utils;
namespace Nashet.EconomicSimulation
{
    public class Province : Name, IWayOfLifeChange, IHasCountry, IClickable, ISortableName, INameable
    {
        public enum TerrainTypes
        {
            Plains, Mountains
        };
        public static readonly DoubleConditionsList canGetIndependence = new DoubleConditionsList(new List<Condition>
    {
        new DoubleCondition((province, country)=>(province as Province).hasCore(x=>x!=country), x=>"Has another core", true),
        new DoubleCondition((province, country)=>(province as Province).Country==country, x=>"That's your province", true),
    });
        public static readonly DoubleCondition doesCountryOwn =
        new DoubleCondition((country, province) => (province as Province).isBelongsTo(country as Country),
            x =>
            {
                if ((x as Country) == Game.Player)
                    return "You (" + (x as Country).FullName + ") own that province";
                else
                    return (x as Country).FullName + " owns that province";
            }
        , true);



        public static readonly Predicate<Province> All = x => true;


        private Province here
        {
            get { return this; }
        }
        private readonly int ID;
        private readonly Color colorID;

        private readonly List<PopUnit> allPopUnits = new List<PopUnit>();

        //private readonly Dictionary<Province, byte> distances = new Dictionary<Province, byte>();
        private readonly List<Province> neighbors = new List<Province>();
        private Product resource;
        private Vector3 position;
        private Color color;

        private GameObject rootGameObject;
        private MeshRenderer meshRenderer;

        private Country country;

        private readonly List<Factory> allFactories = new List<Factory>();

        private readonly int fertileSoil;
        private readonly List<Country> cores = new List<Country>();
        private readonly Dictionary<Province, MeshRenderer> bordersMeshes = new Dictionary<Province, MeshRenderer>();
        private TerrainTypes terrain;
        private readonly Dictionary<TemporaryModifier, Date> modifiers = new Dictionary<TemporaryModifier, Date>();
        //private readonly float nameWeight;
        //empty province constructor
        public Province(string name, int iID, Color icolorID, Product resource) : base(name)
        {
            country = World.UncolonizedLand;
            color = country.getColor().getAlmostSameColor();
            setResource(resource);
            colorID = icolorID;
            ID = iID;
            fertileSoil = 10000;
        }


        public void setUnityAPI(MeshStructure meshStructure, Dictionary<Province, MeshStructure> neighborBorders)
        {

            //this.meshStructure = meshStructure;

            //spawn object
            rootGameObject = new GameObject(string.Format("{0}", getID()));

            //Add Components
            var meshFilter = rootGameObject.AddComponent<MeshFilter>();
            meshRenderer = rootGameObject.AddComponent<MeshRenderer>();

            // in case you want the new gameobject to be a child
            // of the gameobject that your script is attached to
            rootGameObject.transform.parent = World.Get.transform;

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
                if (!(this.getTerrain() == TerrainTypes.Mountains && neighbor.terrain == TerrainTypes.Mountains))
                    //this.getTerrain() == TerrainTypes.Plains || neighbor.terrain == TerrainTypes.Plains)
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
                    if (Country == border.Key.Country)
                    {
                        if (this != Game.selectedProvince || reWriteSelection)
                            border.Value.material = Game.defaultProvinceBorderMaterial;
                        if (border.Key != Game.selectedProvince || reWriteSelection)
                            border.Key.bordersMeshes[this].material = Game.defaultProvinceBorderMaterial;
                    }
                    else
                    {
                        if (this != Game.selectedProvince || reWriteSelection)
                            if (Country == World.UncolonizedLand)
                                border.Value.material = Game.defaultProvinceBorderMaterial;
                            else
                                border.Value.material = Country.getBorderMaterial();
                        if ((border.Key != Game.selectedProvince || reWriteSelection) && border.Key.Country != null)
                            if (border.Key.Country == World.UncolonizedLand)
                                border.Key.bordersMeshes[this].material = Game.defaultProvinceBorderMaterial;
                            else
                                border.Key.bordersMeshes[this].material = border.Key.Country.getBorderMaterial();
                    }
                }
                else
                {
                    border.Value.material = Game.impassableBorder;
                    border.Key.bordersMeshes[this].material = Game.impassableBorder;
                }
            }

            //foreach (var neighbor in neighbors)
            //    if (Country == neighbor.Country)
            //    {
            //        this.bordersMeshes[neighbor].material = Game.defaultProvinceBorderMaterial;
            //        neighbor.bordersMeshes[this].material = Game.defaultProvinceBorderMaterial;
            //    }
            //    else
            //    {
            //        {
            //            this.bordersMeshes[neighbor].material = Country.getBorderMaterial();
            //            if (neighbor.Country != null)
            //                neighbor.bordersMeshes[this].material = neighbor.Country.getBorderMaterial();
            //        }
            //    }
        }


        /// <summary>
        /// returns 
        /// </summary>
        public Country Country
        {
            get { return country; }
        }
        internal int getID()
        { return ID; }
        /// <summary>
        /// called only on map generation
        /// </summary>    
        //public void InitialOwner(Country taker)
        //{
        //    //owner = taker;           
        //    //taker.ownedProvinces.Add(this);
        //    //color = taker.getColor().getAlmostSameColor();

        //    //if (taker != World.UncolonizedLand)
        //    //    cores.Add(taker);



        //}
        public void setInitial(Country ini)
        {
            if (ini != World.UncolonizedLand)
                cores.Add(country);
        }
        public void simulate()
        {
            if (Game.Random.Next(Options.ProvinceChanceToGetCore) == 1)
                if (neighbors.Any(x => x.isCoreFor(Country)) && !cores.Contains(Country) && getMajorCulture() == Country.getCulture())
                    cores.Add(Country);
            // modifiers.LastOrDefault()
            //foreach (var item in modifiers)
            //{
            //    if (item.Value.isDatePassed())
            //}
            modifiers.RemoveAll((modifier, date) => date != null && date.isPassed());
        }
        /// <summary>
        /// returns true if ANY of cores matches  predicate
        /// </summary>    
        public bool hasCore(Func<Country, bool> predicate)
        {
            return cores.Any(predicate);
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
                return cores[0].ShortName;
            else
            {
                StringBuilder sb = new StringBuilder();
                cores.ForEach(x => sb.Append(x.ShortName).Append("; "));
                return sb.ToString();
            }
        }
        public IEnumerable<Country> getAllCores()
        {
            foreach (var core in cores)
                yield return core;
        }
        internal Country getRandomCore()
        {
            return cores.Random();
        }
        internal Country getRandomCore(Predicate<Country> predicate)
        {
            return cores.FindAll(predicate).Random();
        }
        /// <summary>
        /// Secedes province to Taker. Also kills old province owner if it was last province
        /// Call it only from Country.TakeProvince()
        /// </summary>    
        public void OnSecedeTo(Country taker, bool addModifier)
        {
            Country oldCountry = Country;
            // transfer government owned factories
            // don't do government property revoking for now            
            allFactories.PerformAction(x => x.ownership.TransferAll(oldCountry, taker, false));
            oldCountry.demobilize(x => x.getPopUnit().Province == this);

            // add loyalty penalty for conquered province // temp
            foreach (var pop in allPopUnits)
            {
                if (pop.culture == taker.getCulture())
                    pop.loyalty.Add(Options.PopLoyaltyChangeOnAnnexStateCulture);
                else
                    pop.loyalty.Subtract(Options.PopLoyaltyChangeOnAnnexNonStateCulture, false);
                pop.loyalty.clamp100();
                Movement.leave(pop);
            }

            //refuse pay back loans to old country bank
            foreach (var agent in getAllAgents())
            {
                if (agent.loans.isNotZero())
                    agent.Bank.OnLoanerRefusesToPay(agent);
                //take back deposits            
                oldCountry.Bank.ReturnAllDeposits(agent);
                //agent.setBank(taker.Bank);
                agent.OnProvinceOwnerChanged(taker);
            }
            //transfer province
            //oldCountry.ownedProvinces.Remove(this);
            //taker.ownedProvinces.Add(this);

            country = taker;
            if (addModifier)
                if (modifiers.ContainsKey(TemporaryModifier.recentlyConquered))
                    modifiers[TemporaryModifier.recentlyConquered].set(Date.Today.getNewDate(20));
                else
                    modifiers.Add(TemporaryModifier.recentlyConquered, Date.Today.getNewDate(20));
        }
        public void OnSecedeGraphic(Country taker)
        {
            //graphic stuff
            color = taker.getColor().getAlmostSameColor();
            meshRenderer.material.color = this.getColorAccordingToMapMode();
            setBorderMaterials(false);
        }
        public int howFarFromCapital()
        {
            return 0;
        }
        public Dictionary<TemporaryModifier, Date> getModifiers()
        {
            return modifiers;
        }
        //internal bool isCapital()
        //{
        //    return Country.Capital == this;
        //}


        internal IEnumerable<Province> getAllNeighbors()
        {
            foreach (var item in neighbors)
                yield return item;

        }
        public IEnumerable<Producer> getAllProducers()
        {
            foreach (Factory factory in allFactories)
                yield return factory;
            foreach (PopUnit pop in allPopUnits)
                if (pop.Type.isProducer())
                    yield return pop;
        }
        public IEnumerable<Producer> getAllBuyers()
        {
            foreach (Factory factory in allFactories)
                // if (!factory.Type.isResourceGathering()) // every fabric is buyer (upgrading)
                yield return factory;
            foreach (PopUnit pop in allPopUnits)
                if (pop.canTrade())
                    yield return pop;
        }
        public IEnumerable<Producer> getAllAgents()
        {
            foreach (Factory factory in allFactories)
                yield return factory;
            foreach (PopUnit pop in allPopUnits)
                yield return pop;
        }
        public IEnumerable<Factory> getAllFactories()
        {
            for (int i = 0; i < allFactories.Count; i++)
            {
                yield return allFactories[i];
            }
            //foreach (Factory factory in allFactories)
            //    yield return factory;
        }
        //public IEnumerable<Factory> getAllFactories(Predicate<Factory> predicate)
        //{
        //    foreach (Factory factory in allFactories)
        //        if (predicate(factory))
        //            yield return factory;
        //}
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

        //public int getMenPopulation()
        //{
        //    int result = 0;
        //    foreach (PopUnit pop in allPopUnits)
        //        result += pop.getPopulation();
        //    return result;
        //}
        //public int getMenPopulationEmployable()
        //{
        //    int result = 0;
        //    foreach (PopUnit pop in allPopUnits)
        //        if (pop.Type.canBeUnemployed())
        //            result += pop.getPopulation();
        //    return result;
        //}

        internal bool isBelongsTo(Country country)
        {
            return this.Country == country;
        }
        //internal bool isNeighborButNotOwn(Country country)
        //{
        //    return this.Country != country && neighbors.Any(x => x.Country == country);
        //}
        internal bool isNeighbor(Province province)
        {
            return neighbors.Contains(province);
        }

        public int getFamilyPopulation()
        {
            //return getMenPopulation() * Options.familySize;
            return GetAllPopulation().Sum(x => x.getPopulation()) * Options.familySize;
        }

        internal MoneyView getIncomeTax()
        {
            decimal res = 0m;
            allPopUnits.ForEach(x => res += x.incomeTaxPayed.Get());
            return new MoneyView(res);
        }

        internal void mobilize()
        {
            Country.mobilize(new List<Province> { this });
        }

        public IEnumerable<PopUnit> GetAllPopulation(PopType popType)
        {
            foreach (PopUnit pop in allPopUnits)
                if (pop.Type == popType)
                    yield return pop;
        }
        public IEnumerable<PopUnit> GetAllPopulation()
        {
            foreach (PopUnit pop in allPopUnits)
                yield return pop;
        }



        //not called with capitalism
        internal void shareWithAllAristocrats(Storage fromWho, Value taxTotalToPay)
        {
            int aristoctratAmount = 0;
            foreach (Aristocrats aristocrats in GetAllPopulation(PopType.Aristocrats))
                aristoctratAmount += aristocrats.getPopulation();
            foreach (Aristocrats aristocrat in GetAllPopulation(PopType.Aristocrats))
            {
                Storage howMuch = new Storage(fromWho.Product, taxTotalToPay.get() * (float)aristocrat.getPopulation() / (float)aristoctratAmount);
                fromWho.send(aristocrat.storage, howMuch);
                aristocrat.addProduct(howMuch);
                aristocrat.dealWithMarket();
                //aristocrat.sentToMarket.set(aristocrat.gainGoodsThisTurn);            
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
                if (pop.Type == target.Type && pop.culture == target.culture)
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

        /// <summary>
        /// Returns result divided on groups of factories (List) each with own level of salary or priority given in orderMethod(Factory)
        /// </summary>    
        private IEnumerable<List<Factory>> getAllFactoriesDescendingOrder(Func<Factory, float> orderMethod)
        {
            var sortedfactories = allFactories.OrderByDescending(o => orderMethod(o));
            var iterator = sortedfactories.GetEnumerator();
            // Pre read first element
            if (iterator.MoveNext())
            {
                List<Factory> result = new List<Factory>();
                var previousFactory = iterator.Current;
                result.Add(previousFactory);

                while (iterator.MoveNext())
                {
                    if (orderMethod(iterator.Current) == orderMethod(previousFactory))
                        result.Add(iterator.Current);
                    else
                    {
                        yield return result; // same salary sequence ended
                        result = new List<Factory> { iterator.Current };
                    }
                    previousFactory = iterator.Current;
                }
                yield return result; // final sequence ended
            }
        }
        public void BalanceEmployableWorkForce()
        {
            // List<PopUnit> workforceList = this.GetAllPopulation(PopType.Workers).ToList();
            int unemplyedWorkForce = GetAllPopulation(PopType.Workers).Sum(x => x.getPopulation());

            if (unemplyedWorkForce > 0)
            {
                // workforceList = workforceList.OrderByDescending(o => o.population).ToList();            
                Func<Factory, float> order;
                if (Country.economy.getValue() == Economy.PlannedEconomy)
                    order = (x => x.getPriority());
                else
                    order = (x => (float)x.getSalary().Get());

                foreach (List<Factory> factoryGroup in getAllFactoriesDescendingOrder(order))
                {
                    // if there is no enough workforce to fill all factories in group then
                    // workforce should be distributed proportionally
                    int factoriesInGroupWantsTotal = 0;
                    foreach (Factory factory in factoryGroup)
                    {
                        factoriesInGroupWantsTotal += factory.howMuchWorkForceWants();
                        //factory.clearWorkforce();
                    }

                    int hiredInThatGroup = 0;
                    foreach (var factory in factoryGroup)
                        if (factory.getSalary().isNotZero() || Country.economy.getValue() == Economy.PlannedEconomy)
                        {
                            int factoryWants = factory.howMuchWorkForceWants();

                            int toHire;
                            if (factoriesInGroupWantsTotal == 0 || unemplyedWorkForce == 0 || factoryWants == 0)
                                toHire = 0;
                            else
                                toHire = unemplyedWorkForce * factoryWants / factoriesInGroupWantsTotal;
                            if (toHire > factoryWants)
                                toHire = factoryWants;
                            hiredInThatGroup += factory.hireWorkforce(toHire, GetAllPopulation(PopType.Workers));

                            //if (popsLeft <= 0) break;
                            // don't do breaks to clear old workforce records
                        }
                        else
                        {
                            factory.hireWorkforce(0, null);
                        }
                    unemplyedWorkForce -= hiredInThatGroup;
                }
            }
        }

        internal void DestroyAllMarkedfactories()
        {
            allFactories.RemoveAll(x => x.isToRemove());
        }

        internal void setResource(Product inres)
        {
            resource = inres;
            if (resource == Product.Stone || resource == Product.Gold || resource == Product.MetalOre || resource == Product.Coal)
                terrain = TerrainTypes.Mountains;
            else
                terrain = TerrainTypes.Plains;
        }
        internal Product getResource()
        {
            if (resource.IsInventedByAnyOne())
                return resource;
            else
                return null;
        }
        internal Factory getExistingResourceFactory()
        {
            foreach (Factory factory in allFactories)
                if (factory.Type.basicProduction.Product == resource)
                    return factory;
            return null;
        }

        //internal IEnumerable<FactoryType> getAllBuildableFactories()
        //{
        //    List<FactoryType> result = new List<FactoryType>();
        //    foreach (FactoryType type in FactoryType.allTypes)
        //        if (type.canBuildNewFactory(this))
        //            result.Add(type);
        //    return result;
        //}

        /// <summary>
        /// check type for null outside
        /// </summary>



        internal bool hasFactory(ProductionType type)
        {
            foreach (Factory f in allFactories)
                if (f.Type == type)
                    return true;
            return false;
        }
        /// <summary>
        /// Gets average unemployment from all pop units
        /// </summary>
        /// <returns></returns>
        //public Procent getUnemployment(Predicate<PopType> predicate)
        //{
        //    Procent result = new Procent(0f);
        //    int calculatedBase = 0;
        //    foreach (var item in allPopUnits)
        //    {
        //        if (predicate(item.Type))
        //        {
        //            if (item.Type.canBeUnemployed())
        //                result.AddPoportionally(calculatedBase, item.getPopulation(), item.getUnemployedProcent());
        //            calculatedBase += item.getPopulation();
        //        }
        //    }
        //    return result;
        //}

        internal void DestroyFactory(Factory factory)
        {
            allFactories.Remove(factory);
        }

        /// <summary>
        /// Very heavy method
        /// </summary>        
        internal int getUnemployedWorkers()
        {
            int totalWorkforce = GetAllPopulation(PopType.Workers).Sum(x => x.getPopulation());
            if (totalWorkforce == 0)
                return 0;
            int employed = allFactories.Sum(x => x.getWorkForce());

            //foreach (Factory factory in allFactories)
            //    employed += factory.getWorkForce();
            return totalWorkforce - employed;
        }
        internal bool isThereFactoriesInUpgradeMoreThan(int limit)
        {
            int counter = 0;
            foreach (Factory factory in allFactories)
                if (factory.isUpgrading() || factory.isBuilding())
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
            txtMesh.color = Color.black; // Set the text's color to red
            group.SetLODs(lods);
#if UNITY_WEBGL
            group.size = 20; //was 30 for webgl
#else
            group.size = 20; // for others
#endif
            //group.RecalculateBounds();
        }

        internal Factory findFactory(ProductionType proposition)
        {
            foreach (Factory f in allFactories)
                if (f.Type == proposition)
                    return f;
            return null;
        }
        internal bool isProducingOnEnterprises(StorageSet resourceInput)
        {
            foreach (Storage inputNeed in resourceInput)
                foreach (Factory provinceFactory in allFactories)
                    if (provinceFactory.getGainGoodsThisTurn().isNotZero() && provinceFactory.Type.basicProduction.Product.isSameProduct(inputNeed.Product)
                          )
                        return true;
            return false;
        }
        /// <summary>
        /// Adjusted to use in modifiers 
        /// </summary>    
        internal float getOverpopulationAdjusted(PopUnit pop)
        {
            if (pop.Type == PopType.Tribesmen || pop.Type == PopType.Farmers)
            {
                float res = GetOverpopulation().get();
                res -= 1f;
                if (res <= 0f)
                    res = 0f;
                return res;
            }
            else
                return 0f;
        }
        /// <summary>
        /// New value
        /// </summary>        
        internal Procent GetOverpopulation()
        {
            float usedLand = 0f;
            foreach (PopUnit pop in allPopUnits)
                if (pop.Type == PopType.Tribesmen)
                    usedLand += pop.getPopulation() * Options.PopMinLandForTribemen;
                else if (pop.Type == PopType.Farmers)
                    usedLand += pop.getPopulation() * Options.PopMinLandForFarmers;
                else
                    usedLand += pop.getPopulation() * Options.PopMinLandForTownspeople;

            return new Procent(usedLand, fertileSoil);
        }
        /// <summary> call it BEFORE opening enterprise
        /// Returns salary of a factory with lowest salary in province. If only one factory in province, then returns Country.minsalary
        /// \nCould auto-drop salary on minSalary of there is problems with inputs 
        /// Returns new value</summary>

        internal MoneyView getLocalMinSalary()
        {
            MoneyView res;
            if (allFactories.Count <= 1) // first enterprise in province
                res = Country.getMinSalary();
            else
            {
                Money minSalary = getLocalMaxSalary() .Copy();

                foreach (Factory factory in allFactories)
                    if (factory.IsOpen && factory.HasAnyWorforce())//&& !factory.isJustHiredPeople()
                    {
                        if (factory.getSalary().isSmallerThan(minSalary))
                            minSalary = factory.getSalary();
                    }
                minSalary.Add(0.012m); //connected to ChangeSalary()
                res = minSalary;
            }
            //if (res == 0f)
            //    res = Options.FactoryMinPossibleSalary;
            return res;
        }

        /// <summary>Returns salary of a factory with maximum salary in province. If no factory in province, then returns Country.minSalary
        /// New value
        ///</summary>
        internal MoneyView getLocalMaxSalary()
        {
            if (allFactories.Count <= 1)
                return Country.getMinSalary();
            else
            {
                Money maxSalary;
                maxSalary = allFactories.First().getSalary();

                foreach (Factory fact in allFactories)
                    if (fact.IsOpen)
                    {
                        if (fact.getSalary().isBiggerThan(maxSalary))
                            maxSalary = fact.getSalary();
                    }
                return maxSalary;
            }
        }

        //public void consolidatePops()
        //{
        //    if (allPopUnits.Count > 14)
        //    //get some small pop and merge it into bigger
        //    {
        //        PopUnit popToMerge = GetAllPopulation().Where(x => x.getPopulation() < Options.PopSizeConsolidationLimit).Random();
        //        //PopUnit popToMerge = getSmallerPop((x) => x.getPopulation() < Options.PopSizeConsolidationLimit);
        //        if (popToMerge != null)
        //        {
        //            PopUnit targetPop = this.getBiggerPop(x => x.isStateCulture() == popToMerge.isStateCulture()
        //               && x.Type == popToMerge.Type
        //               && x != popToMerge);
        //            if (targetPop != null)
        //                targetPop.mergeIn(popToMerge);
        //        }

        //    }
        //}

        private PopUnit getBiggerPop(Predicate<PopUnit> predicate)
        {
            return allPopUnits.FindAll(predicate).MaxBy(x => x.getPopulation());
        }
        private PopUnit getSmallerPop(Predicate<PopUnit> predicate)
        {
            return allPopUnits.FindAll(predicate).MinBy(x => x.getPopulation());
        }




        internal bool hasAnotherPop(PopType type)
        {
            int result = 0;
            foreach (PopUnit pop in allPopUnits)
            {
                if (pop.Type == type)
                {
                    result++;
                    if (result == 2)
                        return true;
                }
            }
            return false;
        }
        public bool hasModifier(TemporaryModifier modifier)
        {
            return modifiers.ContainsKey(modifier);
        }
        public Color getColorAccordingToMapMode()
        {
            switch (Game.getMapMode())
            {
                case 0: //political mode                
                    return getColor();
                case 1: //culture mode
                    //return World.getAllExistingCountries().FirstOrDefault(x => x.getCulture() == getMajorCulture()).getColor();
                    return getMajorCulture().getColor();
                case 2: //cores mode
                    if (Game.selectedProvince == null)
                    {
                        if (isCoreFor(Country))
                            return Country.getColor();
                        else
                        {
                            var c = getRandomCore();
                            if (c == null)
                                return Color.yellow;
                            else
                                return c.getColor();
                        }
                    }
                    else
                    {
                        if (isCoreFor(Game.selectedProvince.Country))
                            return Game.selectedProvince.Country.getColor();
                        else
                        {
                            if (isCoreFor(Country))
                                return Country.getColor();
                            else
                            {
                                var so = getRandomCore(x => x.isAlive());
                                if (so != null)
                                    return so.getColor();
                                else
                                {
                                    var c = getRandomCore();
                                    if (c == null)
                                        return Color.yellow;
                                    else
                                        return c.getColor();
                                }
                            }
                        }
                    }
                case 3: //resource mode                
                    {
                        if (getResource() == null)
                            return Color.gray;
                        else
                            return getResource().getColor();
                    }
                case 4: //population change mode                
                    {

                        if (Game.selectedProvince == null)
                        {
                            float maxColor = 3000;
                            //can improve performance
                            var change = Country.GetAllPopulation().Sum(x => x.getAllPopulationChanges()
                             .Where(y => y.Key == null || y.Key is Province || y.Key is Staff).Sum(y => y.Value));
                            if (change > 0)
                                return Color.Lerp(Color.grey, Color.green, change / maxColor);
                            else if (change == 0)
                                return Color.gray;
                            else
                                return Color.Lerp(Color.grey, Color.red, -1f * change / maxColor);
                        }
                        else
                        {
                            float maxColor = 500;
                            var change = GetAllPopulation().Sum(x => x.getAllPopulationChanges()
                            .Where(y => y.Key == null || y.Key is Province || y.Key is Staff).Sum(y => y.Value));
                            if (change > 0)
                                return Color.Lerp(Color.grey, Color.green, change / maxColor);
                            else if (change == 0)
                                return Color.gray;
                            else
                                return Color.Lerp(Color.grey, Color.red, -1f * change / maxColor);
                        }
                    }
                case 5: //population density mode                
                    {
                        float maxPopultion = 25000;
                        var population = GetAllPopulation().Sum(x => x.getPopulation());


                        //return Color.Lerp(new Color(255f / 0xFF, 255f / 0xFD, 255f / 0xDD), new Color(255f / 0x7F, 255f / 0x33, 255f / 0x00), population / maxPopultion);
                        return Color.Lerp(Color.white, Color.red, population / maxPopultion);


                    }
                default:
                    return default(Color);
            }
        }
        public MoneyView getGDP()
        {
            Money result = new Money(0m);
            foreach (var producer in getAllAgents())
                if (producer.getGainGoodsThisTurn().get() > 0f)
                    result.Add(Game.market.getCost(producer.getGainGoodsThisTurn())); //- Game.market.getCost(producer.getConsumedTotal()).get());
            return result;
        }
        //public Procent GetAveragePop(Func<PopUnit, Procent> selector)
        //{
        //    Procent result = new Procent(0f);
        //    int calculatedPopulation = 0;
        //    foreach (PopUnit pop in allPopUnits)
        //    {
        //        result.AddPoportionally(calculatedPopulation, pop.getPopulation(), selector(pop));
        //        calculatedPopulation += pop.getPopulation();
        //    }
        //    return result;
        //}
        //internal float getAverageFactoryWorkforceFulfilling()
        //{
        //    int workForce = 0;
        //    int capacity = 0;
        //    foreach (Factory fact in allFactories)
        //        if (fact.IsOpen)
        //        {
        //            workForce += fact.getWorkForce();
        //            capacity += fact.getMaxWorkforceCapacity();
        //        }
        //    if (capacity == 0) return 0f;
        //    else
        //        return workForce / (float)capacity;
        //}
        /// <summary>
        /// If type is null than return average value for ALL Pops. New value
        /// </summary>    
        public Value getAverageNeedsFulfilling(PopType type)
        {
            var list = GetAllPopulation().Where(x => x.Type == type).ToList();
            if (list.Count == 0)
                if (Rand.Chance(Options.PopMigrationToUnknowAreaChance))
                    return Procent.HundredProcent.Copy();
                else
                    return Procent.ZeroProcent.Copy();
            else
                return list.GetAverageProcent(x => x.needsFulfilled);

            //Value result = new Value(0);
            //int allPopulation = 0;
            //IEnumerable<PopUnit> selector;
            //if (type == null)
            //    selector = GetAllPopulation();
            //else
            //    selector = GetAllPopulation(type);

            //foreach (PopUnit pop in GetAllPopulation(type))
            //// get middle needs fulfilling according to pop weight            
            //{
            //    allPopulation += pop.getPopulation();
            //    result.Add(pop.needsFulfilled.Copy().Multiply(pop.getPopulation()));
            //}
            //if (allPopulation > 0)
            //    return result.Divide(allPopulation);
            //else
            //    return Procent.HundredProcent.Copy();
        }
        public void OnClicked()
        {
            MainCamera.selectProvince(this.getID());
        }
        public IEnumerable<Owners> GetSales()
        {
            foreach (var item in allFactories)
            {
                // sales go on only on owner's permission
                if (item.ownership.IsOnSale())
                    yield return item.ownership;
            }
        }
        /// <summary>
        /// Don't use it for aristocrats
        /// Shouldn't exist
        /// </summary>
        public IEnumerable<IInvestable> getAllInvestmentProjects(Agent investor)
        {
            var upgradeInvestments = getAllFactories().Where(x =>
                    canUpgradeFactory(x.Type)
                    && x.GetWorkForceFulFilling().isBiggerThan(Options.minFactoryWorkforceFulfillingToInvest)
                    );
            foreach (var item in upgradeInvestments)
                yield return item;

            var buildInvestments = ProductionType.getAllInventedFactories(Country).Where(x => x.canBuildNewFactory(this, investor));
            foreach (var item in buildInvestments)
                yield return new NewFactoryProject(this, item);

            foreach (var item in GetSales())
                yield return item;

            var reopenEnterprises = getAllFactories().Where(x => x.IsClosed && !x.isBuilding());
            foreach (var item in reopenEnterprises)
                yield return item;
        }

        //public IEnumerable<IInvestable> getAllInvestmentsProjects()
        //{
        //    //var listA = Enumerable.Range(0, 10).Select(i => new TestClassA());
        //    //var listB = Enumerable.Range(0, 10).Select(i => new TestClassB());

        //    //var combinedEnumerable = listA.Cast<ITestInterface>().Concat(listB.Cast<ITestInterface>());
        //    //Debug.Log("\n\n Testing:");
        //    //foreach (var item in combinedEnumerable)
        //    //{
        //    //    Debug.Log(item.GetText());
        //    //}
        //    /////////////////////////////////////////
        //    //if (owner == Game.Player)
        //    //    Debug.Log("\nnew Testing: " + this);

        //    var upgradeInvetments = getAllFactories().Where(x =>
        //    canUpgradeFactory(x.Type)
        //    && x.GetWorkForceFulFilling().isBiggerThan(Options.minFactoryWorkforceFulfillingToInvest)
        //    ).Cast<IInvestable>();
        //    //if (owner == Game.Player)
        //    //    upgradeInvetments.PerformAction(x => Debug.Log("upgrade old: " + x.ToString() + " " + x.GetType()));

        //    var buildInvestments = FactoryType.getAllInventedTypes(Country, x => x.canBuildNewFactory(this)).Cast<IInvestable>();
        //    //if (owner == Game.Player)
        //    //    buildInvestments.PerformAction(x => Debug.Log("new project: " + x.ToString() + " " + x.GetType()));

        //    var buyInvestments = GetSales().Cast<IInvestable>();

        //    var reopenEnterprises = getAllFactories().Where(x => x.IsClosed && !x.isBuilding()).Cast<IInvestable>();

        //    var combined = upgradeInvetments.Concat(buildInvestments).Concat(buyInvestments).Concat(reopenEnterprises);




        //    //if (owner == Game.Player)
        //    //{
        //    //    Debug.Log("Combined:");
        //    //    combined.PerformAction(x => Debug.Log(x.ToString() + " " + x.GetType()));
        //    //}
        //    return combined;
        //}

        //    //Factory.conditionsUpgrade.isAllTrue // don't change it to Modifier  - it would prevent loan takes
        //    //FactoryType.conditionsBuild.isAllTrue
        //public List<Factory> getAllInvestmentsProjects(Predicate<Factory> predicate)
        //{
        //    List<Factory> res = new List<Factory>();
        //    getAllFactories(x => canUpgradeFactory(x.Type) && predicate(x)).PerformAction(x => res.Add(x));
        //    FactoryType.getAllInventedTypes(Country, x => x.canBuildNewFactory(this) && predicate(x)).PerformAction(x => res.Add(x));
        //    return res;
        //}
        internal bool canUpgradeFactory(ProductionType type)
        {
            var factory = findFactory(type);
            if (factory == null)
                return false;
            if (factory.canUpgrade())
                return true;
            else
                return false;
        }
        public bool HasJobsFor(PopType popType)
        {
            if (!allFactories.Any(x => x.IsOpen))
                return false;
            if (popType == PopType.Workers)
                return this.GetAllPopulation().Where(x => x.Type == PopType.Workers)
                        .GetAverageProcent(x => x.getUnemployment()).isSmallerThan(Options.PopMigrationUnemploymentLimit);
            else if (popType == PopType.Farmers || popType == PopType.Tribesmen)
                return this.GetOverpopulation().isSmallerThan(Procent.HundredProcent);
            else
                return true;
        }
        public Factory BuildFactory(IShareOwner investor, ProductionType type, MoneyView cost)
        {
            if (getAllFactories().Any(x => x.Type == type)) //temporally
            {
                throw new Exception("Can't have 2 same factory types");
            }
            else
            {
                var res = new Factory(this, investor, type, cost);
                allFactories.Add(res);
                return res;
            }
        }
        public void RegisterPop(PopUnit pop)
        {
            if (GetAllPopulation().Any(x => x.Type == pop.Type && x.culture == pop.culture)) //temporally
            {
                throw new Exception("Can't have 2 same popunits");
            }
            else
                allPopUnits.Add(pop);
        }
        public void RemoveDeadPops()
        {
            allPopUnits.RemoveAll(x => !x.isAlive());
        }
        //public PopUnit BithPop(int amount, PopType type, Culture culture)
        //{
        //    if (GetAllPopulation().Any(x => x.Type == type && x.culture == culture)) //temporally
        //    {
        //        throw new Exception("Can't have 2 same popunits");
        //    }
        //    else
        //    {
        //        var res = new PopUnit(int amount, PopType type, Culture culture, this);
        //        allFactories.Add(res);
        //        return res;
        //    }
        //}

        public override string FullName
        {
            get { return this + ", " + Country; }
        }
        public ReadOnlyValue getLifeQuality(PopUnit thisPop, PopType proposedType)
        {
            if (!this.HasJobsFor(proposedType))
                return ReadOnlyValue.Zero;
            else
            {
                var lifeQuality = getAverageNeedsFulfilling(proposedType);

                if (!lifeQuality.isBiggerThan(thisPop.needsFulfilled, Options.PopNeedsEscapingBarrier))
                    return ReadOnlyValue.Zero;

                // checks for same culture and type
                if (getSimilarPopUnit(thisPop) != null)
                    lifeQuality.Add(Options.PopSameCultureMigrationPreference);

                if (country.getCulture() != thisPop.culture && country.minorityPolicy.getValue() != MinorityPolicy.Equality)
                    lifeQuality.Subtract(0.3f, false);

                // reforms preferences
                if (thisPop.Type.isPoorStrata())
                {
                    lifeQuality.Add(Country.unemploymentSubsidies.getValue().ID * 2);
                    lifeQuality.Add(Country.minimalWage.getValue().ID * 1);
                    lifeQuality.Add(Country.taxationForRich.getValue().ID * 1);
                }
                else if (thisPop.Type.isRichStrata())
                {
                    if (Country.economy.getValue() == Economy.LaissezFaire)
                        lifeQuality.Add(5f);
                    else if (Country.economy.getValue() == Economy.Interventionism)
                        lifeQuality.Add(2f);
                }
                if (!thisPop.canVote(Country.government.getTypedValue())) // includes Minority politics
                    lifeQuality.Subtract(-10f, false);

                if (thisPop.loyalty.get() < 0.3f)
                    lifeQuality.Add(5, false);
                //todo - serfdom

                return lifeQuality;
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

        public string getWayOfLifeString(PopUnit pop)
        {
            if (pop.Country == this.Country)
                return "migrated";
            else
                return "immigrated";
        }
    }
}