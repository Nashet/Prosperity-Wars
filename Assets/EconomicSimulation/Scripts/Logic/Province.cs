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
    public class Province : Name, IEscapeTarget, IHasGetCountry, IClickable, ISortableName, IDescribable
    {
        public enum TerrainTypes
        {
            Plains, Mountains
        };
        public static readonly DoubleConditionsList canGetIndependence = new DoubleConditionsList(new List<Condition>
    {
        new DoubleCondition((province, country)=>(province as Province).hasCore(x=>x!=country), x=>"Has another core", true),
        new DoubleCondition((province, country)=>(province as Province).GetCountry()==country, x=>"That's your province", true),
    });
        public static readonly DoubleCondition doesCountryOwn =
        new DoubleCondition((country, province) => (province as Province).isBelongsTo(country as Country),
            x =>
            {
                if ((x as Country) == Game.Player)
                    return "You (" + (x as Country).GetFullName() + ") own that province";
                else
                    return (x as Country).GetFullName() + " owns that province";
            }
        , true);


        public readonly static List<Province> allProvinces = new List<Province>();
        public static readonly Predicate<Province> All = x => true;


        private Province here
        {
            get { return this; }
        }
        private readonly int ID;
        private readonly Color colorID;

        public readonly List<PopUnit> allPopUnits = new List<PopUnit>();

        //private readonly Dictionary<Province, byte> distances = new Dictionary<Province, byte>();
        private readonly List<Province> neighbors = new List<Province>();
        private Product resource;
        private Vector3 position;
        private Color color;

        private GameObject rootGameObject;
        private MeshRenderer meshRenderer;

        private Country owner;

        private readonly List<Factory> allFactories = new List<Factory>();

        private readonly int fertileSoil;
        private readonly List<Country> cores = new List<Country>();
        private readonly Dictionary<Province, MeshRenderer> bordersMeshes = new Dictionary<Province, MeshRenderer>();
        private TerrainTypes terrain;
        private readonly Dictionary<Mod, Date> modifiers = new Dictionary<Mod, Date>();
        private readonly float nameWeight;
        //empty province constructor
        public Province(string name, int iID, Color icolorID, Product resource) : base(name)
        {
            nameWeight = name.GetWeight();
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

        internal int howMuchFactories()
        {
            return allFactories.Count;
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
                    if (GetCountry() == border.Key.GetCountry())
                    {
                        if (this != Game.selectedProvince || reWriteSelection)
                            border.Value.material = Game.defaultProvinceBorderMaterial;
                        if (border.Key != Game.selectedProvince || reWriteSelection)
                            border.Key.bordersMeshes[this].material = Game.defaultProvinceBorderMaterial;
                    }
                    else
                    {
                        if (this != Game.selectedProvince || reWriteSelection)
                            if (GetCountry() == Country.NullCountry)
                                border.Value.material = Game.defaultProvinceBorderMaterial;
                            else
                                border.Value.material = GetCountry().getBorderMaterial();
                        if ((border.Key != Game.selectedProvince || reWriteSelection) && border.Key.GetCountry() != null)
                            if (border.Key.GetCountry() == Country.NullCountry)
                                border.Key.bordersMeshes[this].material = Game.defaultProvinceBorderMaterial;
                            else
                                border.Key.bordersMeshes[this].material = border.Key.GetCountry().getBorderMaterial();
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

        public Country GetCountry()
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

            //if (taker.ownedProvinces == null)
            //    taker.ownedProvinces = new List<Province>();
            taker.ownedProvinces.Add(this);
            color = taker.getColor().getAlmostSameColor();

            if (taker != Country.NullCountry)
                cores.Add(taker);
        }
        public void simulate()
        {
            if (Game.Random.Next(Options.ProvinceChanceToGetCore) == 1)
                if (neighbors.Any(x => x.isCoreFor(GetCountry())) && !cores.Contains(GetCountry()) && getMajorCulture() == GetCountry().getCulture())
                    cores.Add(GetCountry());
            // modifiers.LastOrDefault()
            //foreach (var item in modifiers)
            //{
            //    if (item.Value.isDatePassed())
            //}
            modifiers.RemoveAll((modifier, date) => date != null && date.isDatePassed());
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
                return cores[0].GetShortName();
            else
            {
                StringBuilder sb = new StringBuilder();
                cores.ForEach(x => sb.Append(x.GetShortName()).Append("; "));
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
        /// </summary>    
        public void secedeTo(Country taker, bool addModifier)
        {
            Country oldCountry = GetCountry();
            //refuse pay back loans to old country bank
            foreach (var agent in getAllAgents())
            {
                if (agent.loans.isNotZero())
                    agent.getBank().defaultLoaner(agent);
                //take back deposits            
                oldCountry.getBank().returnAllMoney(agent);
                agent.setBank(taker.getBank());
            }

            // transfer government owned factories
            // don't do government property revoking for now            

            allFactories.PerformAction(x => x.ownership.TransferAll(oldCountry, taker, false));


            oldCountry.demobilize(x => x.getPopUnit().GetProvince() == this);


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

            //transfer province
            if (oldCountry != null)
                oldCountry.ownedProvinces.Remove(this);
            owner = taker;
            taker.ownedProvinces.Add(this);

            taker.government.onReformEnacted(this);

            //kill country or move capital
            if (oldCountry.ownedProvinces.Count == 0)
                oldCountry.OnKillCountry(taker);
            else if (oldCountry.Capital == this)
                oldCountry.moveCapitalTo(oldCountry.getRandomOwnedProvince());

            //graphic stuff
            color = taker.getColor().getAlmostSameColor();
            meshRenderer.material.color = this.getColorAccordingToMapMode();

            setBorderMaterials(false);
            if (addModifier)
                if (modifiers.ContainsKey(Mod.recentlyConquered))
                    modifiers[Mod.recentlyConquered].set(Date.Today.getNewDate(20));
                else
                    modifiers.Add(Mod.recentlyConquered, Date.Today.getNewDate(20));
        }
        public int howFarFromCapital()
        {
            return 0;
        }
        public Dictionary<Mod, Date> getModifiers()
        {
            return modifiers;
        }
        //internal bool isCapital()
        //{
        //    return GetCountry().Capital == this;
        //}

        internal static Province getRandomProvinceInWorld(Predicate<Province> predicate)
        {
            return allProvinces.Random(predicate);
        }
        internal List<Province> getNeigbors(Predicate<Province> predicate)
        {
            return neighbors.FindAll(predicate);

        }
        public IEnumerable<Producer> getAllProducers()
        {
            foreach (Factory factory in allFactories)
                yield return factory;
            foreach (PopUnit pop in allPopUnits)
                if (pop.popType.isProducer())
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
        public IEnumerable<Producer> getAllConsumers()
        {
            foreach (Factory factory in allFactories)
                //if (!factory.Type.isResourceGathering())// every fabric is consumer (upgrading)
                yield return factory;
            foreach (PopUnit pop in allPopUnits)
                //if (pop.canBuyProducts())
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
        //        if (pop.popType.canBeUnemployed())
        //            result += pop.getPopulation();
        //    return result;
        //}

        internal bool isBelongsTo(Country country)
        {
            return this.GetCountry() == country;
        }
        internal bool isNeighborButNotOwn(Country country)
        {
            return neighbors.Any(x => x.GetCountry() == country && this.GetCountry() != country);
        }
        internal bool isNeighbor(Province province)
        {
            return neighbors.Contains(province);
        }

        public int getFamilyPopulation()
        {
            //return getMenPopulation() * Options.familySize;
            return getAllPopUnits().Sum(x => x.getPopulation()) * Options.familySize;
        }

        internal float getIncomeTax()
        {
            float res = 0f;
            allPopUnits.ForEach(x => res += x.incomeTaxPayed.get());
            return res;
        }



        internal void mobilize()
        {
            GetCountry().mobilize(new List<Province> { this });
        }

        public static bool isProvinceCreated(Color color)
        {
            foreach (Province anyProvince in allProvinces)
                if (anyProvince.colorID == color)
                    return true;
            return false;
        }

        public List<PopUnit> getAllPopUnitsList(PopType ipopType)
        {
            List<PopUnit> result = new List<PopUnit>();
            foreach (PopUnit pop in allPopUnits)
                if (pop.popType == ipopType)
                    result.Add(pop);
            return result;
        }
        public IEnumerable<PopUnit> getAllPopUnits(PopType ipopType)
        {
            foreach (PopUnit pop in allPopUnits)
                if (pop.popType == ipopType)
                    yield return pop;
        }
        public IEnumerable<PopUnit> getAllPopUnits()
        {
            foreach (PopUnit pop in allPopUnits)
                yield return pop;
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
            int result = 0;
            foreach (PopUnit pop in getAllPopUnits(ipopType))
                if (pop.popType == ipopType)
                    result += pop.getPopulation();
            return result;
        }
        //not called with capitalism
        internal void shareWithAllAristocrats(Storage fromWho, Value taxTotalToPay)
        {
            int aristoctratAmount = 0;
            foreach (Aristocrats aristocrats in getAllPopUnits(PopType.Aristocrats))
                aristoctratAmount += aristocrats.getPopulation();
            foreach (Aristocrats aristocrat in getAllPopUnits(PopType.Aristocrats))
            {
                Storage howMuch = new Storage(fromWho.getProduct(), taxTotalToPay.get() * (float)aristocrat.getPopulation() / (float)aristoctratAmount);
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

        /// <summary>
        /// Returns result divided on groups of factories (List) each with own level of salary or priority given in orderMethod(Factory)
        /// </summary>    
        private IEnumerable<List<Factory>> getFactoriesDescendingOrder(Func<Factory, float> orderMethod)
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
            List<PopUnit> workforceList = this.getAllPopUnitsList(PopType.Workers);
            int unemplyedWorkForce = workforceList.Sum(x => x.getPopulation());

            if (unemplyedWorkForce > 0)
            {
                // workforceList = workforceList.OrderByDescending(o => o.population).ToList();            
                Func<Factory, float> order;
                if (GetCountry().economy.getValue() == Economy.PlannedEconomy)
                    order = (x => x.getPriority());
                else
                    order = (x => x.getSalary().get());

                foreach (List<Factory> factoryGroup in getFactoriesDescendingOrder(order))
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
                        if (factory.getSalary().isNotZero() || GetCountry().economy.getValue() == Economy.PlannedEconomy)
                        {
                            int factoryWants = factory.howMuchWorkForceWants();

                            int toHire;
                            if (factoriesInGroupWantsTotal == 0 || unemplyedWorkForce == 0 || factoryWants == 0)
                                toHire = 0;
                            else
                                toHire = unemplyedWorkForce * factoryWants / factoriesInGroupWantsTotal;
                            if (toHire > factoryWants)
                                toHire = factoryWants;
                            hiredInThatGroup += factory.hireWorkforce(toHire, workforceList);

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
                if (factory.Type.basicProduction.getProduct() == resource)
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



        internal bool hasFactory(FactoryType type)
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
        //        if (predicate(item.popType))
        //        {
        //            if (item.popType.canBeUnemployed())
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
            txtMesh.color = Color.red; // Set the text's color to red
            group.SetLODs(lods);
#if UNITY_WEBGL
            group.size = 20; //was 30 for webgl
#else
            group.size = 20; // for others
#endif
            //group.RecalculateBounds();
        }

        internal Factory findFactory(FactoryType proposition)
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
                    if (provinceFactory.getGainGoodsThisTurn().isNotZero() && provinceFactory.Type.basicProduction.getProduct().isSameProduct(inputNeed.getProduct())
                          )
                        return true;
            return false;
        }
        /// <summary>
        /// Adjusted to use in modifiers 
        /// </summary>    
        internal float getOverpopulationAdjusted(PopUnit pop)
        {
            if (pop.popType == PopType.Tribesmen || pop.popType == PopType.Farmers)
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
                if (pop.popType == PopType.Tribesmen)
                    usedLand += pop.getPopulation() * Options.PopMinLandForTribemen;
                else if (pop.popType == PopType.Farmers)
                    usedLand += pop.getPopulation() * Options.PopMinLandForFarmers;
                else
                    usedLand += pop.getPopulation() * Options.PopMinLandForTownspeople;

            return new Procent(usedLand, fertileSoil);
        }
        /// <summary> call it BEFORE opening enterprise
        /// Returns salary of a factory with lowest salary in province. If only one factory in province, then returns Country.minsalary
        /// \nCould auto-drop salary on minSalary of there is problems with inputs 
        /// Returns new value</summary>

        internal Money getLocalMinSalary()
        {
            Money res;
            if (allFactories.Count <= 1) // first enterprise in province
                res = GetCountry().getMinSalary();
            else
            {
                Money minSalary;
                minSalary = getLocalMaxSalary();

                foreach (Factory factory in allFactories)
                    if (factory.IsOpen && factory.HasAnyWorforce())//&& !factory.isJustHiredPeople()
                    {
                        if (factory.getSalary().isSmallerThan(minSalary))
                            minSalary = factory.getSalary();
                    }
                res = minSalary.Add(0.012f);//connected to ChangeSalary()
            }
            //if (res == 0f)
            //    res = Options.FactoryMinPossibleSalary;
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
        /// <summary>Returns salary of a factory with maximum salary in province. If no factory in province, then returns Country.minSalary
        /// New value
        ///</summary>
        internal Money getLocalMaxSalary()
        {
            if (allFactories.Count <= 1)
                return GetCountry().getMinSalary();
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
            return allPopUnits.Random(predicate);
        }
        private PopUnit getRandomPop()
        {
            return allPopUnits.Random();
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
        public Color getColorAccordingToMapMode()
        {
            switch (Game.getMapMode())
            {
                case 0: //political mode                
                    return getColor();
                case 3: //resource mode                
                    {
                        if (getResource() == null)
                            return Color.gray;
                        else
                            return getResource().getColor();
                    }
                case 1: //culture mode
                    return Country.allCountries.Find(x => x.getCulture() == getMajorCulture()).getColor();
                case 2: //cores mode
                    if (Game.selectedProvince == null)
                    {
                        if (isCoreFor(GetCountry()))
                            return GetCountry().getColor();
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
                        if (isCoreFor(Game.selectedProvince.GetCountry()))
                            return Game.selectedProvince.GetCountry().getColor();
                        else
                        {
                            if (isCoreFor(GetCountry()))
                                return GetCountry().getColor();
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
                default:
                    return default(Color);
            }
        }
        public Value getGDP()
        {
            Value result = new Value(0);
            foreach (var producer in getAllAgents())
                if (producer.getGainGoodsThisTurn().get() > 0f)
                    result.Add(Game.market.getCost(producer.getGainGoodsThisTurn()).get()); //- Game.market.getCost(producer.getConsumedTotal()).get());
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
            var list = getAllPopUnits().Where(x => x.popType == type).ToList();
            if (list.Count == 0)
                return Procent.HundredProcent.Copy();
            else
                return list.GetAverageProcent(x => x.needsFulfilled);

            //Value result = new Value(0);
            //int allPopulation = 0;
            //IEnumerable<PopUnit> selector;
            //if (type == null)
            //    selector = getAllPopUnits();
            //else
            //    selector = getAllPopUnits(type);

            //foreach (PopUnit pop in getAllPopUnits(type))
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

            var buildInvestments = FactoryType.getAllInventedTypes(GetCountry()).Where(x => x.canBuildNewFactory(this, investor));
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

        //    var buildInvestments = FactoryType.getAllInventedTypes(GetCountry(), x => x.canBuildNewFactory(this)).Cast<IInvestable>();
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
        //    FactoryType.getAllInventedTypes(getCountry(), x => x.canBuildNewFactory(this) && predicate(x)).PerformAction(x => res.Add(x));
        //    return res;
        //}
        internal bool canUpgradeFactory(FactoryType type)
        {
            if (!hasFactory(type))
                return false;
            var factory = findFactory(type);
            if (factory.canUpgrade())
                return true;
            else
                return false;
        }

        public bool HasJobsFor(PopType popType, Province province)
        {
            return popType.HasJobsFor(popType, here);
        }

        public float GetNameWeight()
        {
            return nameWeight;
        }
        public Factory BuildFactory(IShareOwner investor, FactoryType type, Value cost)
        {
            if (getAllFactories().Any(x => x.Type == type))
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

        public string GetDescription()
        {
            return this + ", " + GetCountry();
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



}