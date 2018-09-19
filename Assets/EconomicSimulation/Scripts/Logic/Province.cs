using Nashet.Conditions;
using Nashet.EconomicSimulation.Reforms;
using Nashet.MarchingSquares;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class Province : AbstractProvince, IWayOfLifeChange, IHasCountry, IClickable, ISortableName, IPopulated
    {
        public enum TerrainTypes
        {
            Plains, Mountains
        }

        public static readonly DoubleConditionsList canGetIndependence = new DoubleConditionsList(new List<Condition>
    {
        new DoubleCondition((province, country)=>(province as Province).hasCore(x=>x!=country), x=>"Has another core", true),
        new DoubleCondition((province, country)=>(province as Province).Country==country, x=>"That's your province", true)
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

        private Province here { get { return this; } }

        public Color ProvinceColor { get; protected set; }

        private readonly List<PopUnit> allPopUnits = new List<PopUnit>();
        private readonly List<Factory> allFactories = new List<Factory>();
        private readonly List<Army> standingArmies = new List<Army>(); // military units
        //private readonly Dictionary<Province, byte> distances = new Dictionary<Province, byte>();
        protected readonly List<Province> neighbors = new List<Province>();
        private readonly List<Country> cores = new List<Country>();

        private Product resource;

        private Country country;

        private readonly int fertileSoil;

        private readonly Dictionary<Province, MeshRenderer> bordersMeshes = new Dictionary<Province, MeshRenderer>();
        public TerrainTypes Terrain { get; protected set; }


        private readonly Dictionary<TemporaryModifier, Date> modifiers = new Dictionary<TemporaryModifier, Date>();

        public Province(string name, int ID, Color colorID, Product resource) : base(name, ID, colorID)
        {
            country = World.UncolonizedLand;
            ProvinceColor = country.NationalColor.getAlmostSameColor();
            setResource(resource);
            fertileSoil = 5000;

        }

        public Province(AbstractProvince p, Product product) : this(p.ShortName, p.ID, p.ColorID, product)
        {

        }

        public void SetBorderMaterials()
        {
            foreach (var border in bordersMeshes)
            {
                if (border.Key.isNeighbor(this))
                {
                    if (Country == border.Key.Country) // same country
                    {
                        border.Value.material = LinksManager.Get.defaultProvinceBorderMaterial;
                        border.Key.bordersMeshes[this].material = LinksManager.Get.defaultProvinceBorderMaterial;
                    }
                    else
                    {
                        if (Country == World.UncolonizedLand)
                            border.Value.material = LinksManager.Get.defaultProvinceBorderMaterial;
                        else
                            border.Value.material = Country.getBorderMaterial();

                        if (border.Key.Country == World.UncolonizedLand)
                            border.Key.bordersMeshes[this].material = LinksManager.Get.defaultProvinceBorderMaterial;
                        else
                            border.Key.bordersMeshes[this].material = border.Key.Country.getBorderMaterial();
                    }
                }
                else
                {
                    border.Value.material = LinksManager.Get.impassableBorder;
                    border.Key.bordersMeshes[this].material = LinksManager.Get.impassableBorder;
                }
            }

        }

        /// <summary>
        /// returns
        /// </summary>
        public Country Country
        {
            get { return country; }
        }

        public void AddCore(Country ini)
        {
            if (ini != World.UncolonizedLand)
                cores.Add(country);
        }

        public void simulate()
        {
            if (Rand.Get.Next(Options.ProvinceChanceToGetCore) == 1)
                if (neighbors.Any(x => x.isCoreFor(Country)) && !cores.Contains(Country) && getMajorCulture() == Country.Culture)
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
            return cores.Any(x => x.Culture == pop.culture);
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

        public IEnumerable<Country> AllCores()
        {
            foreach (var core in cores)
                yield return core;
        }


        /// <summary>
        /// Secedes province to Taker. Also kills old province owner if it was last province
        /// Call it only from Country.TakeProvince()
        /// </summary>        
        public void OnSecedeTo(Country taker, bool addModifier)
        {
            // rise event on day passed
            EventHandler<OwnerChangedEventArgs> handler = OwnerChanged;
            if (handler != null)
            {
                handler(this, new OwnerChangedEventArgs { oldOwner = Country });
            }

            Country oldCountry = Country;
            // transfer government owned factories
            // don't do government property revoking for now
            allFactories.PerformAction(x => x.ownership.TransferAll(oldCountry, taker, false));
            oldCountry.demobilize(x => x.getPopUnit().Province == this);

            // add loyalty penalty for conquered province // temp
            foreach (var pop in allPopUnits)
            {
                if (pop.culture == taker.Culture)
                    pop.loyalty.Add(Options.PopLoyaltyChangeOnAnnexStateCulture);
                else
                    pop.loyalty.Subtract(Options.PopLoyaltyChangeOnAnnexNonStateCulture, false);
                pop.loyalty.clamp100();
                Movement.leave(pop);
            }

            //refuse pay back loans to old country bank
            foreach (var agent in AllAgents)
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
            ProvinceColor = taker.NationalColor.getAlmostSameColor();
            if (meshRenderer != null)
                meshRenderer.material.color = getColorAccordingToMapMode();
            SetBorderMaterials();
        }

        public int howFarFromCapital()
        {
            return 0;
        }

        public Dictionary<TemporaryModifier, Date> getModifiers()
        {
            return modifiers;
        }

        //public bool isCapital()
        //{
        //    return Country.Capital == this;
        //}

        public IEnumerable<Province> AllNeighbors()
        {
            foreach (var item in neighbors)
                yield return item;
        }

        public IEnumerable<Producer> AllProducers
        {
            get
            {
                foreach (Factory factory in allFactories)
                    yield return factory;
                foreach (PopUnit pop in allPopUnits)
                    if (pop.Type.isProducer())
                        yield return pop;
            }
        }
        public IEnumerable<ISeller> AllSellers
        {
            get
            {
                foreach (var agent in AllProducers)
                    yield return agent;

            }
        }
        public IEnumerable<Consumer> AllConsumers
        {
            get
            {
                foreach (Factory factory in allFactories)
                    // if (!factory.Type.isResourceGathering()) // every fabric is buyer (upgrading)
                    yield return factory;
                foreach (PopUnit pop in allPopUnits)
                    if (pop.canTrade())
                        yield return pop;
            }
        }

        public IEnumerable<Agent> AllAgents
        {
            get
            {
                foreach (Factory factory in allFactories)
                    yield return factory;
                foreach (PopUnit pop in allPopUnits)
                    yield return pop;
            }
        }

        public IEnumerable<Factory> AllFactories
        {
            get
            {
                for (int i = 0; i < allFactories.Count; i++)
                {
                    yield return allFactories[i];
                }
            }
        }
        public IEnumerable<Workers> AllWorkers
        {
            get
            {
                foreach (Workers pop in allPopUnits.Where(x => x.Type == PopType.Workers))
                    yield return pop;
            }
        }
        public IEnumerable<PopUnit> AllPops
        {
            get
            {
                foreach (PopUnit pop in allPopUnits)
                    yield return pop;
            }
        }

        public Culture getMajorCulture()
        {
            Dictionary<Culture, int> cultures = new Dictionary<Culture, int>();

            foreach (var pop in allPopUnits)
                //if (cultures.ContainsKey(pop.culture))
                //    cultures[pop.culture] += pop.population.Get();
                //else
                //    cultures.Add(pop.culture, pop.population.Get());
                cultures.AddAndSum(pop.culture, pop.population.Get());
            ///allPopUnits.ForEach(x=>cultures.Add(x.culture, x.population.Get()));
            return cultures.MaxBy(y => y.Value).Key as Culture;
        }

        public bool isBelongsTo(Country country)
        {
            return Country == country;
        }

        public int getFamilyPopulation()
        {
            //return getMenPopulation() * Options.familySize;
            return AllPops.Sum(x => x.population.Get()) * Options.familySize;
        }

        public void mobilize()
        {
            Country.mobilize(new List<Province> { this });
        }

        //not called with capitalism
        public void shareWithAllAristocrats(Storage fromWho, Value taxTotalToPay)
        {
            int aristoctratAmount = 0;
            foreach (Aristocrats aristocrats in AllPops.Where(x => x.Type == PopType.Aristocrats))
                aristoctratAmount += aristocrats.population.Get();
            foreach (Aristocrats aristocrat in AllPops.Where(x => x.Type == PopType.Aristocrats))
            {
                Storage howMuch = new Storage(fromWho.Product, taxTotalToPay.get() * (float)aristocrat.population.Get() / (float)aristoctratAmount);
                fromWho.send(aristocrat.storage, howMuch);
                aristocrat.addProduct(howMuch);
                aristocrat.SentExtraGoodsToMarket();
                //aristocrat.sentToMarket.set(aristocrat.gainGoodsThisTurn);
            }
        }

        public void SetColor(Color color)
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

        /// <summary>
        /// Returns result divided on groups of factories (List) each with own level of salary or priority given in orderMethod(Factory)
        /// </summary>
        private IEnumerable<List<Factory>> AllFactoriesDescendingOrder(Func<Factory, float> orderMethod)
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

        protected void FireAllWorkers()
        {
            foreach (var worker in AllWorkers)
            {
                //var worker = item as Workers;
                //if (worker != null)
                worker.Fire();
            }
            AllFactories.PerformAction(x => x.ClearWorkforce());
        }

        public void BalanceEmployableWorkForce()
        {
            FireAllWorkers();

            // List<PopUnit> workforceList = this.GetAllPopulation(PopType.Workers).ToList();
            int unemplyedWorkForce = AllPops.Where(x => x.Type == PopType.Workers).Sum(x => x.population.Get());

            if (unemplyedWorkForce > 0)
            {
                // workforceList = workforceList.OrderByDescending(o => o.population).ToList();
                Func<Factory, float> order;
                if (Country.economy == Economy.PlannedEconomy)
                    order = x => x.getPriority();
                else
                    order = x => (float)x.getSalary().Get();

                foreach (List<Factory> factoryGroup in AllFactoriesDescendingOrder(order))
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
                        if (factory.getSalary().isNotZero() || Country.economy == Economy.PlannedEconomy)
                        {
                            int factoryWants = factory.howMuchWorkForceWants();

                            int toHire;
                            if (factoriesInGroupWantsTotal == 0 || unemplyedWorkForce == 0 || factoryWants == 0)
                                toHire = 0;
                            else
                                toHire = unemplyedWorkForce * factoryWants / factoriesInGroupWantsTotal;

                            if (toHire > factoryWants)
                                toHire = factoryWants;

                            hiredInThatGroup += factory.hireWorkers(toHire, AllPops.Where(x => x.Type == PopType.Workers));

                            //if (popsLeft <= 0) break;
                            // don't do breaks to clear old workforce records
                        }
                    //now it fires above
                    //else 
                    //{
                    //    factory.hireWorkers(0, null);
                    //}
                    unemplyedWorkForce -= hiredInThatGroup;
                }

                // now if there are benefits, put all unemployed workers on social benefits
                if (Country.unemploymentSubsidies != UnemploymentSubsidies.None 
                    || Country.PovertyAid != PovertyAid.None
                    || !Country.UBI.IsMoreConservativeThan(UBI.Middle)
                   && Country.economy != Economy.PlannedEconomy
                   //&& Country.Politics.LastTurnDefaultedSocialObligations.isZero())
                   && Register.Account.PovertyAid.GetIncomeAccount(Country.FailedPayments).isZero()
                   && Register.Account.UBISubsidies.GetIncomeAccount(Country.FailedPayments).isZero()
                   && Register.Account.UnemploymentSubsidies.GetIncomeAccount(Country.FailedPayments).isZero())
                    foreach (var worker in AllWorkers)
                    {
                        // sit on benefits:                    
                        if (!worker.LastTurnDidntGetPromisedSocialBenefits)
                            worker.SitOnSocialBenefits(worker.GetSeekingJobInt());
                    }
            }
        }

        public void DestroyAllMarkedfactories()
        {
            allFactories.RemoveAll(x => x.isToRemove());
        }

        public void setResource(Product inres)
        {
            resource = inres;
            if (resource == Product.Stone || resource == Product.Gold || resource == Product.MetalOre || resource == Product.Coal)
                Terrain = TerrainTypes.Mountains;
            else
                Terrain = TerrainTypes.Plains;
        }

        public Product getResource()
        {
            if (resource.IsInventedByAnyOne())
                return resource;
            else
                return null;
        }

        public Factory getExistingResourceFactory()
        {
            foreach (Factory factory in allFactories)
                if (factory.Type.basicProduction.Product == resource)
                    return factory;
            return null;
        }

        /// <summary>
        /// check type for null outside
        /// </summary>
        public bool hasFactory(ProductionType type)
        {
            foreach (Factory f in allFactories)
                if (f.Type == type)
                    return true;
            return false;
        }

        public void DestroyFactory(Factory factory)
        {
            allFactories.Remove(factory);
        }

        
        // todo improve Very heavy method        
        public int getUnemployedWorkers()
        {
            int totalWorkforce = AllPops.Where(x => x.Type == PopType.Workers).Sum(x => x.population.Get());
            if (totalWorkforce == 0)
                return 0;
            int employed = allFactories.Sum(x => x.getWorkForce());

            //foreach (Factory factory in allFactories)
            //    employed += factory.getWorkForce();
            return totalWorkforce - employed;
        }

        
        public int getSeeksForJob()
        {
            return AllWorkers.Sum(x => x.GetSeekingJobInt());            
        }

        public bool isThereFactoriesInUpgradeMoreThan(int limit)
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



        public Factory findFactory(ProductionType proposition)
        {
            foreach (Factory f in allFactories)
                if (f.Type == proposition)
                    return f;
            return null;
        }

        public bool isProducingOnEnterprises(StorageSet resourceInput)
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
        public float getOverpopulationAdjusted(PopUnit pop)
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
        public Procent GetOverpopulation()
        {
            float usedLand = 0f;
            foreach (PopUnit pop in allPopUnits)
                if (pop.Type == PopType.Tribesmen)
                    usedLand += pop.population.Get() * Options.PopMinLandForTribemen;
                else if (pop.Type == PopType.Farmers)
                    usedLand += pop.population.Get() * Options.PopMinLandForFarmers;
                else
                    usedLand += pop.population.Get() * Options.PopMinLandForTownspeople;

            return new Procent(usedLand, fertileSoil);
        }

        /// <summary> call it BEFORE opening enterprise
        /// Returns salary of a factory with lowest salary in province. If only one factory in province, then returns Country.minsalary
        /// \nCould auto-drop salary on minSalary of there is problems with inputs
        /// Returns new value</summary>

        public MoneyView getLocalMinSalary()
        {
            MoneyView res;
            if (allFactories.Count <= 1) // first enterprise in province
                res = Country.getMinSalary();
            else
            {
                Money minSalary = getLocalMaxSalary().Copy();

                foreach (Factory factory in allFactories)
                    if (factory.IsOpen && factory.HasAnyWorkforce())//&& !factory.isJustHiredPeople()
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
        public MoneyView getLocalMaxSalary()
        {
            var openEnterprises = allFactories.FirstOrDefault(x => x.IsOpen);
            //if (allFactories.Count(x=>x.IsOpen) <= 1)
            if (openEnterprises == null)
                return Country.getMinSalary();
            else
            {
                Money maxSalary = openEnterprises.getSalary();
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
        //        PopUnit popToMerge = GetAllPopulation().Where(x => x.population.Get() < Options.PopSizeConsolidationLimit).Random();
        //        //PopUnit popToMerge = getSmallerPop((x) => x.population.Get() < Options.PopSizeConsolidationLimit);
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
            return allPopUnits.FindAll(predicate).MaxBy(x => x.population.Get());
        }

        private PopUnit getSmallerPop(Predicate<PopUnit> predicate)
        {
            return allPopUnits.FindAll(predicate).MinBy(x => x.population.Get());
        }

        public bool hasAnotherPop(PopType type)
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
        public void SetColorAccordingToMapMode()
        {
            SetColor(getColorAccordingToMapMode());
        }
        protected Color getColorAccordingToMapMode()
        {
            switch (Game.MapMode)
            {
                case Game.MapModes.Political:
                    return ProvinceColor;

                case Game.MapModes.Cultures: //culture mode
                    //return World.getAllExistingCountries().FirstOrDefault(x => x.Culture == getMajorCulture()).getColor();
                    var culture = getMajorCulture();
                    if (culture == null)
                        return Color.white;
                    else
                        return culture.getColor();

                case Game.MapModes.Cores: //cores mode
                    if (Game.selectedProvince == null)
                    {
                        if (isCoreFor(Country))
                            return Country.NationalColor;
                        else
                        {
                            var randomCore = AllCores().Random();
                            if (randomCore == null)
                                return Color.yellow;
                            else
                                return randomCore.NationalColor;
                        }
                    }
                    else
                    {
                        if (isCoreFor(Game.selectedProvince.Country))
                            return Game.selectedProvince.Country.NationalColor;
                        else
                        {
                            if (isCoreFor(Country))
                                return Country.NationalColor;
                            else
                            {
                                var so = AllCores().Where(x => x.IsAlive).Random();
                                if (so != null)
                                    return so.NationalColor;
                                else
                                {
                                    var c = AllCores().Random();
                                    if (c == null)
                                        return Color.yellow;
                                    else
                                        return c.NationalColor;
                                }
                            }
                        }
                    }
                case Game.MapModes.Resources: //resource mode
                    {
                        if (getResource() == null)
                            return Color.gray;
                        else
                            return getResource().getColor();
                    }
                case Game.MapModes.PopulationChange: //population change mode
                    {
                        if (Game.selectedProvince == null)
                        {
                            float maxColor = 3000;
                            //can improve performance
                            var change = Country.Provinces.AllPops.Sum(x => x.getAllPopulationChanges()
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
                            var change = AllPops.Sum(x => x.getAllPopulationChanges()
                            .Where(y => y.Key == null || y.Key is Province || y.Key is Staff).Sum(y => y.Value));
                            if (change > 0)
                                return Color.Lerp(Color.grey, Color.green, change / maxColor);
                            else if (change == 0)
                                return Color.gray;
                            else
                                return Color.Lerp(Color.grey, Color.red, -1f * change / maxColor);
                        }
                    }
                case Game.MapModes.PopulationDensity: //population density mode
                    {
                        float maxPopultion = 50000;
                        var population = AllPops.Sum(x => x.population.Get());
                        return Color.Lerp(Color.white, Color.red, population / maxPopultion);
                    }
                case Game.MapModes.Prosperity: //prosperity map
                    {
                        float minValue = 0.25f;
                        float maxValue = 0.5f - minValue;
                        var needsfulfilling = AllPops.GetAverageProcent(x => x.needsFulfilled).get();
                        needsfulfilling -= minValue;
                        if (needsfulfilling < 0f)
                            needsfulfilling = 0f;
                        return Color.Lerp(Color.white, Color.yellow, needsfulfilling / maxValue);
                    }

                default:
                    return default(Color);
            }
        }

        public MoneyView getGDP()
        {
            Money result = new Money(0m);
            foreach (var producer in AllProducers)
                if (producer.getGainGoodsThisTurn().get() > 0f)
                    result.Add(Country.market.getCost(producer.getGainGoodsThisTurn())); //- Country.market.getCost(producer.getConsumedTotal()).get());
            return result;
        }

        /// <summary>
        /// If type is null than return average value for ALL Pops. New value
        /// </summary>
        public Value getAverageNeedsFulfilling(PopType type)
        {
            var list = AllPops.Where(x => x.Type == type).ToList();
            if (list.Count == 0)
                if (Rand.Chance(Options.PopMigrationToUnknowAreaChance))
                    return Procent.HundredProcent.Copy();
                else
                    return Procent.ZeroProcent.Copy();
            else
                return list.GetAverageProcent(x => x.needsFulfilled);
        }

        public void OnClicked()
        {
            //MainCamera.selectProvince(this.getID());
            MainCamera.Get.FocusOnProvince(this, true);
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
        /// Doesn't check if enterprise is invented, also doesn't check
        /// conNotLForNotCountry, conAllowsForeignInvestments, conHaveMoneyOrResourcesToUpgrade
        /// </summary>
        public IEnumerable<IInvestable> AllInvestmentProjects()//Agent investor
        {
            var upgradeInvestments = AllFactories.Where(x =>
                Factory.conditionsUpgrade.isAllTrue(null, x)//investor
                                                            //x.Province.CanUpgradeFactory(x.Type, investor)
                && x.GetWorkForceFulFilling().isBiggerThan(Options.minFactoryWorkforceFulfillingToInvest)
                );
            foreach (var item in upgradeInvestments)
                yield return item;

            var buildInvestments = ProductionType.getAllInventedByAnyoneFactories().Where(x => x.canBuildNewFactory(this, null)); //investor
            foreach (var item in buildInvestments)
                yield return new NewFactoryProject(this, item);

            // Don't need extra check (notLf, allowsForeignInvestments) in 2 next circle.
            //Because AI Countries use it only for themselves, Aristocrats use it only in won province
            foreach (var item in GetSales())
                yield return item;

            var reopenEnterprises = AllFactories.Where(x => x.IsClosed && !x.isBuilding());
            foreach (var item in reopenEnterprises)
                yield return item;
        }

        /// <summary>
        /// Returns true if seeking for less than Options.PopMigrationUnemploymentLimit
        /// </summary>        
        public bool HasJobsFor(PopType popType)
        {
            if (popType == PopType.Workers)
            {
                if (!allFactories.Any(x => x.IsOpen))
                    return false;
                return AllPops.Where(x => x.Type == PopType.Workers)
                        .GetAverageProcent(x => x.GetSeekingJob()).isSmallerThan(Options.PopMigrationUnemploymentLimit);
            }
            else if (popType == PopType.Farmers || popType == PopType.Tribesmen)
                return GetOverpopulation().isSmallerThan(Procent.HundredProcent);
            else
                return true;
        }

        public Factory BuildFactory(IShareOwner investor, ProductionType type, MoneyView cost, bool instantBuild = false)
        {
            //if (getAllFactories().Any(x => x.Type == type)) //todo temporally
            //{
            //    throw new Exception("Can't have 2 same factory types");
            //}
            //else
            {
                var res = new Factory(this, investor, type, cost, instantBuild);
                allFactories.Add(res);
                return res;
            }
        }

        public void RegisterPop(PopUnit pop)
        {
            if (AllPops.Any(x => x.Type == pop.Type && x.culture == pop.culture)) //temporally
            {
                throw new Exception("Can't have 2 same popunits");
            }
            else
                allPopUnits.Add(pop);
        }

        public void RemoveDeadPops()
        {
            allPopUnits.RemoveAll(x => !x.IsAlive);
        }

        public override string FullName
        {
            get { return this + ", " + Country; }
        }

        /// <summary>
        /// Assuming that Type is same, province is changing
        /// </summary>        
        public ReadOnlyValue getLifeQuality(PopUnit pop)
        {
            if (!HasJobsFor(pop.Type))
                return ReadOnlyValue.Zero;
            else
            {
                // common part
                var lifeQuality = getAverageNeedsFulfilling(pop.Type);

                if (!lifeQuality.isBiggerThan(pop.needsFulfilled, Options.PopNeedsEscapingBarrier))
                    return ReadOnlyValue.Zero;

                // checks for same culture and type
                if (getSimilarPopUnit(pop) != null)
                    lifeQuality.Add(Options.PopSameCultureMigrationPreference);


                if (this.Country == pop.Country)
                // migration part
                {
                    if (!pop.isStateCulture() && !isCoreFor(pop))
                        lifeQuality.Subtract(0.2f, false);
                }
                else // immigration part
                {
                    // reforms preferences
                    if (pop.Type.isPoorStrata())
                    {
                        lifeQuality.Add(Country.unemploymentSubsidies.LifeQualityImpact);// . ID * 2 / 100f);
                        lifeQuality.Add(Country.minimalWage.LifeQualityImpact);//.ID * 1 / 100f);
                        lifeQuality.Add(Country.taxationForRich.LifeQualityImpact);//.ID * 1 / 100f);
                    }
                    else if (pop.Type.isRichStrata())
                    {
                        if (Country.economy == Economy.LaissezFaire)
                            lifeQuality.Add(0.05f);
                        else if (Country.economy == Economy.Interventionism)
                            lifeQuality.Add(0.02f);
                    }

                    if (pop.loyalty.get() < 0.3f)
                        lifeQuality.Add(0.05f, false);
                    //todo - serfdom

                    if (!pop.CanVoteWithThatGovernment(Country.government.typedValue)) // includes Minority politics, but not only
                        lifeQuality.Subtract(-0.10f, false);

                    if (country.Culture != pop.culture && country.minorityPolicy != MinorityPolicy.Equality)
                        //lifeQuality.Subtract(Options.PopMinorityMigrationBarier, false);
                        return ReadOnlyValue.Zero;
                }

                return lifeQuality;
            }
        }

        /// <summary>
        /// Returns last escape type - demotion, migration or immigration
        /// </summary>
        public IEnumerable<KeyValuePair<IWayOfLifeChange, int>> AllPopsChanges
        {
            get
            {
                foreach (var item in AllPops)
                    foreach (var record in item.getAllPopulationChanges())
                        yield return record;
            }
        }

        public string getWayOfLifeString(PopUnit pop)
        {
            if (pop.Country == Country)
                return "migrated";
            else
                return "immigrated";
        }

        /// <summary>
        ///  If byWhom == Game.Player checks money/resources availability. If not then not.
        /// </summary>
        public bool CanUpgradeFactory(ProductionType type, Agent byWhom)
        {
            var factory = findFactory(type);
            if (factory == null)
                return false;
            else
                return Factory.conditionsUpgrade.isAllTrue(byWhom, factory);
        }

        public static int FindByCollider(Collider collider)
        {
            if (collider != null)
            {
                MeshCollider meshCollider = collider as MeshCollider;
                if (meshCollider == null || meshCollider.sharedMesh == null)
                    return -2;
                Mesh mesh = meshCollider.sharedMesh;
                int provinceNumber = Convert.ToInt32(mesh.name);
                return provinceNumber;
            }
            else
                return -1;
        }
        public static event EventHandler<OwnerChangedEventArgs> OwnerChanged;
        public class OwnerChangedEventArgs : EventArgs
        {
            public Country oldOwner { get; set; }
        }
        public override void setUnityAPI(MeshStructure meshStructure, Dictionary<AbstractProvince, MeshStructure> neighborBorders)
        {
            base.setUnityAPI(meshStructure, neighborBorders);
            MeshCollider groundMeshCollider = GameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
            groundMeshCollider.sharedMesh = MeshFilter.mesh;



            meshRenderer.material.shader = Shader.Find("Standard");// Province");

            meshRenderer.material.color = ProvinceColor;

            //var graph = World.Get.GetComponent<AstarPath>();


            // setting neighbors
            //making meshes for border
            foreach (var border in neighborBorders)
            {
                //each color is one neighbor (non repeating)
                var neighbor = border.Key as Province;
                if (neighbor != null)
                {
                    if (!(Terrain == TerrainTypes.Mountains && neighbor.Terrain == TerrainTypes.Mountains))
                    //this.getTerrain() == TerrainTypes.Plains || neighbor.terrain == TerrainTypes.Plains)
                    {
                        neighbors.Add(neighbor);
                        //var newNode = new Pathfinding.PointNode(AstarPath.active);
                        //newNode.gameObject = txtMeshGl;
                        //graph.data.pointGraph.AddNode(newNode, (Pathfinding.Int3)neighbor.getPosition());

                    }

                    GameObject borderObject = new GameObject("Border with " + neighbor);

                    //Add Components
                    MeshFilter = borderObject.AddComponent<MeshFilter>();
                    MeshRenderer meshRenderer = borderObject.AddComponent<MeshRenderer>();

                    borderObject.transform.parent = GameObject.transform;

                    Mesh borderMesh = MeshFilter.mesh;
                    borderMesh.Clear();

                    borderMesh.vertices = border.Value.getVertices().ToArray();
                    borderMesh.triangles = border.Value.getTriangles().ToArray();
                    borderMesh.uv = border.Value.getUVmap().ToArray();
                    borderMesh.RecalculateNormals();
                    borderMesh.RecalculateBounds();
                    meshRenderer.material = LinksManager.Get.defaultProvinceBorderMaterial;
                    borderMesh.name = "Border with " + neighbor;

                    bordersMeshes.Add(neighbor, meshRenderer);
                }
            }
            var node = GameObject.AddComponent<Node>();
        }
        public IEnumerable<Army> AllStandingArmies()
        {
            foreach (var item in standingArmies)
            {
                yield return item;
            }
        }
        public void AddArmy(Army army)
        {
            standingArmies.Add(army);
            //Debug.Log("Added " + army);
        }
        public void RemoveArmy(Army army)
        {
            standingArmies.Remove(army);
        }
        public bool isNeighbor(Province province)
        {
            return neighbors.Contains(province);
        }
    }
}