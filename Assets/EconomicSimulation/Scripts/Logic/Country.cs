using System.Collections.Generic;
using System.Linq;
using Nashet.Conditions;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class Country : MultiSeller, IClickable, IShareOwner, ISortableName, INameable
    {
        public readonly Government government;
        public readonly Economy economy;
        public readonly Serfdom serfdom;
        public readonly MinimalWage minimalWage;
        public readonly UnemploymentSubsidies unemploymentSubsidies;
        public readonly TaxationForPoor taxationForPoor;
        public readonly TaxationForRich taxationForRich;
        public readonly Dictionary<Country, Date> LastAttackDate = new Dictionary<Country, Date>();
        /// <summary> could be null</summary>
        private readonly Bank bank;

        public Bank Bank { get { return bank; } }
        public Market market;
        public readonly MinorityPolicy minorityPolicy;

        private readonly List<Province> ownedProvinces = new List<Province>();

        private readonly Dictionary<Country, Procent> opinionOf = new Dictionary<Country, Procent>();

        private readonly Dictionary<Invention, bool> inventions = new Dictionary<Invention, bool>();

        public readonly List<AbstractReform> reforms = new List<AbstractReform>();
        public readonly List<Movement> movements = new List<Movement>();

        private string name;
        private readonly Culture culture;
        private readonly Color nationalColor;
        private Province capital;
        private bool alive = true;

        private readonly Money soldiersWage = new Money(0m);
        public readonly Value sciencePoints = new Value(0f);
        public bool failedToPaySoldiers;
        public Money autoPutInBankLimit = new Money(2000);

        private readonly Procent ownershipSecurity = Procent.HundredProcent.Copy();

        /// <summary> Read only, new value</summary>
        public Procent OwnershipSecurity
        {
            get { return ownershipSecurity.Copy(); }
        }

        private readonly Money incomeTaxStaticticPoor = new Money(0m);

        public Money IncomeTaxStaticticPoor
        {
            get { return incomeTaxStaticticPoor.Copy(); }
        }

        private readonly Money incomeTaxStatisticRich = new Money(0m);

        public Money IncomeTaxStatisticRich
        {
            get { return incomeTaxStatisticRich.Copy(); }
        }

        private readonly Money incomeTaxForeigner = new Money(0m);

        public Money IncomeTaxForeigner
        {
            get { return incomeTaxForeigner.Copy(); }
        }

        private readonly Money goldMinesIncome = new Money(0m);
        public Money GoldMinesIncome { get { return goldMinesIncome.Copy(); } }

        private readonly Money ownedFactoriesIncome = new Money(0m);
        public Money OwnedFactoriesIncome { get { return ownedFactoriesIncome.Copy(); } }

        public Money RestIncome
        {
            get { return moneyIncomeThisTurn.Copy().Subtract(getIncome()); }
        }

        private readonly Money unemploymentSubsidiesExpense = new Money(0m);
        public Money UnemploymentSubsidiesExpense { get { return unemploymentSubsidiesExpense.Copy(); } }

        private readonly Money soldiersWageExpense = new Money(0m);
        public Money SoldiersWageExpense { get { return soldiersWageExpense.Copy(); } }

        private readonly Money factorySubsidiesExpense = new Money(0m);
        public Money FactorySubsidiesExpense { get { return factorySubsidiesExpense.Copy(); } }

        private readonly Money storageBuyingExpense = new Money(0m);
        public Money StorageBuyingExpense { get { return storageBuyingExpense.Copy(); } }

        private float nameWeight;

        private TextMesh meshCapitalText;
        private Material borderMaterial;

        private readonly Modifier modXHasMyCores;
        public readonly ModifiersList modMyOpinionOfXCountry;

        public static readonly DoubleConditionsList canAttack = new DoubleConditionsList(new List<Condition>
    {
        new DoubleCondition((province, country)=>(province as Province).getAllNeighbors().Any(x => x.Country == country)
        && (province as Province) .Country != country, x=>"Is neighbor province", true),
        new DoubleCondition((province, country)=>!Government.isDemocracy.checkIfTrue(country)
        || !Government.isDemocracy.checkIfTrue((province as Province).Country), x=>"Democracies can't attack each other", true)
    });

        public static readonly ModifiersList modSciencePoints = new ModifiersList(new List<Condition>
        {
        //new Modifier(Government.isTribal, 0f, false),
        //new Modifier(Government.isTheocracy, 0f, false),
        new Modifier(Government.isDespotism, Government.Despotism.getScienceModifier(), false),
        new Modifier(Government.isJunta, Government.Junta.getScienceModifier(), false),
        new Modifier(Government.isAristocracy, Government.Aristocracy.getScienceModifier(), false),
        new Modifier(Government.isProletarianDictatorship, Government.ProletarianDictatorship.getScienceModifier(), false),
        new Modifier(Government.isDemocracy, Government.Democracy.getScienceModifier(), false),
        new Modifier(Government.isPolis, Government.Polis.getScienceModifier(), false),
        new Modifier(Government.isWealthDemocracy, Government.WealthDemocracy.getScienceModifier(), false),
        new Modifier(Government.isBourgeoisDictatorship, Government.BourgeoisDictatorship.getScienceModifier(), false),
        new Modifier(x=>(x as Country).GetAllPopulation().GetAverageProcent(y=>y.Education).RawUIntValue, "Education", 1f / Procent.Precision, false)
    });

        /// <summary>
        /// Don't call it directly, only from World.cs
        /// </summary>
        public Country(string name, Culture culture, Color color, Province capital, float money) : base(money, null)
        {
            allInvestmentProjects = new CashedData<Dictionary<IInvestable, Procent>>(GetAllInvestmentProjects2);
            SetName(name);
            foreach (var each in Invention.getAll())
                inventions.Add(each, false);
            Country = this;
            market = Market.TemporalSingleMarket; // new Market();
            modXHasMyCores = new Modifier(x => (x as Country).hasCores(this), "You have my cores", -0.05f, false);
            modMyOpinionOfXCountry = new ModifiersList(new List<Condition> { modXHasMyCores,
            new Modifier(x=>(x as Country).government.getValue() != government.getValue(), "You have different form of government", -0.002f, false),
            new Modifier (x=>(x as Country).getLastAttackDateOn(this).getYearsSince() > Options.CountryTimeToForgetBattle
            && getLastAttackDateOn(x as Country).getYearsSince() > Options.CountryTimeToForgetBattle,"You live in peace with us", 0.005f, false),
            new Modifier (x=>!((x as Country).getLastAttackDateOn(this).getYearsSince() > Options.CountryTimeToForgetBattle) && (x as Country).getLastAttackDateOn(this).getYearsSince() < 15,
            "Recently attacked us", -0.06f, false), //x=>(x as Country).getLastAttackDateOn(this).getYearsSince() > 0
            new Modifier (x=> isThreatenBy(x as Country),"We are weaker", -0.05f, false),
            new Modifier (delegate(object x) {isThereBadboyCountry();  return isThereBadboyCountry()!= null && isThereBadboyCountry()!= x as Country  && isThereBadboyCountry()!= this; },
                delegate  { return "There is bigger threat to the world - " + isThereBadboyCountry(); },  0.05f, false),
            new Modifier (x=>isThereBadboyCountry() ==x,"You are very bad boy", -0.05f, false),
            new Modifier(x=>(x as Country).government.getValue() == government.getValue() && government.getValue()==Government.ProletarianDictatorship,
            "Comintern aka Third International", 0.2f, false)
            });

            bank = new Bank(this);
            //staff = new GeneralStaff(this);
            //homeArmy = new Army(this);
            //sendingArmy = new Army(this);
            government = new Government(this);

            economy = new Economy(this);
            serfdom = new Serfdom(this);

            minimalWage = new MinimalWage(this);
            unemploymentSubsidies = new UnemploymentSubsidies(this);
            taxationForPoor = new TaxationForPoor(this);
            taxationForRich = new TaxationForRich(this);
            minorityPolicy = new MinorityPolicy(this);


            this.culture = culture;
            nationalColor = color;
            this.capital = capital;

            if (capital != null)
            {
                ownedProvinces.Add(capital);
                capital.OnSecedeTo(this, false);
                capital.setInitial(this);
            }


            //economy.setValue( Economy.NaturalEconomy);
            serfdom.setValue(Serfdom.Abolished);
            //government.setValue(Government.Tribal, false);

            government.setValue(Government.Aristocracy);
            //economy.setValue(Economy.StateCapitalism);
            taxationForRich.setValue(TaxationForRich.PossibleStatuses[2]);

            markInvented(Invention.Farming);

            markInvented(Invention.Banking);

            

            //markInvented(Invention.individualRights);
            //markInvented(Invention.ProfessionalArmy);
            //markInvented(Invention.Welfare);

            //markInvented(Invention.Collectivism);

        }

        public void SetName(string name)
        {
            nameWeight = name.GetWeight();
            this.name = name;
        }

        private void ressurect(Province capital, Government.ReformValue newGovernment)
        {
            alive = true;
            MoveCapitalTo(capital);
            government.setValue(newGovernment);
            setPrefix();
        }

        public void onGrantedProvince(Province province)
        {
            var oldCountry = province.Country;
            changeRelation(oldCountry, 1.00f);
            oldCountry.changeRelation(this, 1.00f);
            if (!isAlive())
            {
                ressurect(province, oldCountry.government.getTypedValue());
            }
            //province.secedeTo(this, false);
            TakeProvince(province, false);
        }

        public void TakeProvince(Province province, bool addModifier)
        {
            Country oldCountry = province.Country;

            province.Country.ownedProvinces.Remove(province);
            ownedProvinces.Add(province);
            province.OnSecedeTo(this, addModifier);
            province.OnSecedeGraphic(this);

            //kill country or move capital
            if (oldCountry.ownedProvinces.Count == 0)
                oldCountry.OnKillCountry(this);
            else if (province == oldCountry.Capital)
            {
                oldCountry.MoveCapitalTo(oldCountry.ChooseNewCapital());
            }

            government.onReformEnacted(province);
        }

        private Province ChooseNewCapital()
        {
            var newCapital = AllProvinces().Where(x => x.isCoreFor(this)).MaxBy(x => x.getFamilyPopulation());
            if (newCapital == null)
                newCapital = AllProvinces().Where(x => x.getMajorCulture() == this.culture).MaxBy(x => x.getFamilyPopulation());
            if (newCapital == null)
                newCapital = AllProvinces().Random();
            return newCapital;
        }
        public void onSeparatismWon(Country oldCountry)
        {
            foreach (var item in oldCountry.ownedProvinces.ToList())
                if (item.isCoreFor(this))
                {
                    TakeProvince(item, false);
                    //item.secedeTo(this, false);
                }
            ressurect(ChooseNewCapital(), government.getTypedValue());
            foreach (var item in oldCountry.getInvented()) // copying inventions
            {
                markInvented(item.Key);
            }
        }

        public static void setUnityAPI()
        {
            foreach (var country in World.getAllExistingCountries())
            {
                // can't do it before cause graphics isn't loaded
                if (country != World.UncolonizedLand)
                    country.MoveCapitalTo(country.ownedProvinces[0]);
                //if (capital != null) // not null-country

                country.borderMaterial = new Material(LinksManager.Get.defaultCountryBorderMaterial) { color = country.nationalColor.getNegative() };
                //item.ownedProvinces[0].setBorderMaterial(Game.defaultProvinceBorderMaterial);
                country.ownedProvinces[0].SetBorderMaterials();
                country.AllProvinces().PerformAction(x => x.OnSecedeGraphic(x.Country));
                country.Flag = Nashet.Flag.Generate(128, 128);
            }
            World.UncolonizedLand.AllProvinces().PerformAction(x => x.OnSecedeGraphic(World.UncolonizedLand));
        }

        public int getSize()
        {
            return ownedProvinces.Count;
        }

        public static int howMuchCountriesAlive()
        {
            int res = 0;
            foreach (var item in World.getAllExistingCountries())
                res++;
            return res;
        }

        //public IEnumerable<KeyValuePair<Invention, bool>> getAllI()
        //{
        //    foreach (var invention in inventions)
        //        if (invention.Key.isAvailable(this))
        //            yield return invention;
        //}
        public IEnumerable<KeyValuePair<Invention, bool>> getAllAvailableInventions()
        {
            foreach (var invention in inventions)
                if (invention.Key.isAvailable(this))
                    yield return invention;
        }

        public IEnumerable<KeyValuePair<Invention, bool>> GetAllUninvented()
        {
            foreach (var invention in inventions)
                if (invention.Value == false && invention.Key.isAvailable(this))
                    yield return invention;
        }

        public IEnumerable<KeyValuePair<Invention, bool>> getInvented()
        {
            foreach (var invention in inventions)
                if (invention.Value && invention.Key.isAvailable(this))
                    yield return invention;
        }

        public void markInvented(Invention type)
        {
            inventions[type] = true;
        }

        public bool Invented(Invention type)
        {
            bool result = false;
            inventions.TryGetValue(type, out result);
            return result;
        }

        public bool Invented(Product product)
        {
            if (product.isAbstract())
                return true;
            if (
                ((product == Product.Metal || product == Product.MetalOre || product == Product.ColdArms) && !Invented(Invention.Metal))
                || (!Invented(Invention.SteamPower) && (product == Product.Machinery))//|| product == Product.Cement))
                || ((product == Product.Artillery || product == Product.Ammunition) && !Invented(Invention.Gunpowder))
                || (product == Product.Firearms && !Invented(Invention.Firearms))
                || (product == Product.Coal && !Invented(Invention.Coal))
                //|| (product == Cattle && !country.isInvented(Invention.Domestication))
                || (!Invented(Invention.CombustionEngine) && (product == Product.Oil || product == Product.MotorFuel || product == Product.Rubber || product == Product.Cars))
                || (!Invented(Invention.Tanks) && product == Product.Tanks)
                || (!Invented(Invention.Airplanes) && product == Product.Airplanes)
                || (product == Product.Tobacco && !Invented(Invention.Tobacco))
                || (product == Product.Electronics && !Invented(Invention.Electronics))
                //|| (!isResource() && !country.isInvented(Invention.Manufactories))
                || (product == Product.Education && !Invented(Invention.Universities))
                )
                return false;
            else
                return true;
        }

        public bool InventedFactory(ProductionType production)
        {
            //if (!Invented(production.basicProduction.Product)
            // || production.IsResourceProcessing() && !Invented(Invention.Manufactures)
            // || (production.basicProduction.Product == Product.Cattle && !Invented(Invention.Domestication))
            if (!InventedArtisanship(production)
                 || production.IsResourceProcessing() && !Invented(Invention.Manufactures)
             )
                return false;
            else
                return true;
        }

        public bool InventedArtisanship(ProductionType production)
        {
            if (!Invented(production.basicProduction.Product)
             || (production.basicProduction.Product == Product.Cattle && !Invented(Invention.Domestication))
             )
                return false;
            else
                return true;
        }

        public void setPrefix()
        {
            if (meshCapitalText != null)
                meshCapitalText.text = FullName;
        }

        public List<Country> getAllCoresOnMyland()
        {
            var res = new List<Country>();
            foreach (var province in ownedProvinces)
            {
                foreach (var core in province.getAllCores())
                {
                    if (!res.Contains(core))
                        res.Add(core);
                }
            }
            return res;
        }

        public List<Country> getPotentialSeparatists()
        {
            var res = new List<Country>();
            foreach (var item in getAllCoresOnMyland())
            {
                if (!item.isAlive())
                    res.Add(item);
            }
            return res;
        }

        public void setSoldierWage(MoneyView value)
        {
            soldiersWage.Set(value);
        }

        public MoneyView getSoldierWage()
        {
            return soldiersWage;
        }

        //public Procent GetAveragePop(Func<PopUnit, Procent> selector)
        //{
        //    Procent result = new Procent(0f);
        //    int calculatedPopulation = 0;
        //    foreach (var province in ownedProvinces)
        //        foreach (var pop in province.allPopUnits)
        //        {
        //            result.AddPoportionally(calculatedPopulation, pop.population.Get(), selector(pop));
        //            calculatedPopulation += pop.population.Get();
        //        }
        //    return result;
        //}
        //public Procent getAverageLoyalty()
        //{
        //    Procent result = new Procent(0f);
        //    int calculatedPopulation = 0;
        //    foreach (var province in ownedProvinces)
        //        foreach (var pop in province.allPopUnits)
        //        {
        //            result.addPoportionally(calculatedPopulation, pop.population.Get(), pop.loyalty);
        //            calculatedPopulation += pop.population.Get();
        //        }
        //    return result;
        //}
        //public Procent getAverageNeedsFulfilling()
        //{
        //    Procent result = new Procent(0f);
        //    int calculatedPopulation = 0;
        //    foreach (var province in ownedProvinces)
        //        foreach (var pop in province.allPopUnits)
        //        {
        //            result.addPoportionally(calculatedPopulation, pop.population.Get(), pop.needsFulfilled);
        //            calculatedPopulation += pop.population.Get();
        //        }
        //    return result;
        //}
        /// <summary>
        /// Little bugged - returns RANDOM badboy, not biggest
        /// </summary>
        /// <returns></returns>
        private static Date DateOfIsThereBadboyCountry = new Date(Date.Never);

        private static Country BadboyCountry;

        public static Country isThereBadboyCountry()
        {
            if (!DateOfIsThereBadboyCountry.IsToday)
            {
                DateOfIsThereBadboyCountry.set(Date.Today);
                float worldStrenght = 0f;
                foreach (var item in World.getAllExistingCountries())
                    worldStrenght += item.getStrengthExluding(null);
                float streghtLimit = worldStrenght * Options.CountryBadBoyWorldLimit;
                BadboyCountry = World.getAllExistingCountries().Where(x => x != World.UncolonizedLand && x.getStrengthExluding(null) >= streghtLimit).MaxBy(x => x.getStrengthExluding(null));
            }
            return BadboyCountry;
        }

        //todo performance hit 7% 420 calls 1.4mb 82 ms
        private bool isThreatenBy(Country country)
        {
            if (country == this)
                return false;
            if (country.getStrengthExluding(null) > getStrengthExluding(null) * 2)
                return true;
            else
                return false;
        }



        private bool hasCores(Country country)
        {
            return ownedProvinces.Any(x => x.isCoreFor(country));
        }

        /// <summary>
        /// Returns null if used on itself
        /// </summary>
        /// <param name="country"></param>
        public Procent getRelationTo(Country country)
        {
            if (this == country)
                return null;
            Procent opinion;
            if (opinionOf.TryGetValue(country, out opinion))
                return opinion;
            else
            {
                opinion = new Procent(0.5f);
                opinionOf.Add(country, opinion);
                return opinion;
            }
        }

        /// <summary>
        /// Changes that country opinion of another country
        /// </summary>
        public void changeRelation(Country another, float change)
        {
            var relation = getRelationTo(another);
            relation.Add(change, false);
            relation.clamp100();
        }

        public Culture getCulture()
        {
            return culture;
        }

        public bool isAlive()
        {
            return alive;
        }

        /// <summary>
        /// Also transfers enterprises to local governments
        /// </summary>
        public void OnKillCountry(Country byWhom)
        {
            if (meshCapitalText != null) //todo WTF!!
                Object.Destroy(meshCapitalText.gameObject);

            if (this != Game.Player)
            {
                //take all money from bank
                if (byWhom.Invented(Invention.Banking))
                    byWhom.Bank.Annex(Bank); // deposits transfered in province.OnSecede()
                else
                    Bank.destroy(byWhom);

                //byWhom.storageSet.

                PayAllAvailableMoney(byWhom);

                Bank.OnLoanerRefusesToPay(this);
            }
            countryStorageSet.sendAll(byWhom.countryStorageSet);
            //foreach (var item in World.GetAllShares(this).ToList())// transfer all enterprises to local governments
            foreach (var item in World.GetAllFactories())// transfer all enterprises to local governments
                if (item.ownership.HasOwner(this))
                    item.ownership.TransferAll(this, item.Country);

            if (IsHuman)
                Message.NewMessage("Disaster!!", "It looks like we lost our last province\n\nMaybe we would rise again?", "Okay", false, capital.Position);
            alive = false;

            SetStatisticToZero();
        }

        public Province Capital
        {
            get { return capital; }
        }

        public override void sendAllArmies(Province target)
        {
            base.sendAllArmies(target);
        }

        //public bool canAttack(Province province)
        //{
        //    //!province.isBelongsTo(this) &&
        //    return province.isNeighbor(this);
        //}
        /// <summary>
        /// Has duplicates!
        /// </summary>
        public IEnumerable<Province> AllNeighborProvinces()
        {
            //var res = Enumerable.Empty<Province>();
            foreach (var province in ownedProvinces)
                foreach (var neighbor in province.getAllNeighbors().Where(p => p.Country != this))
                    yield return neighbor;

            //List<Province> result = new List<Province>();
            //foreach (var province in ownedProvinces)
            //    result.AddRange(
            //        province.getAllNeighbors().Where(p => p.Country != this && !result.Contains(p))
            //        );
            //return result;
        }
        /// <summary>
        /// Has duplicates!
        /// </summary>
        public IEnumerable<Country> AllNeighborCountries()
        {
            //var res = Enumerable.Empty<Province>();
            foreach (var province in ownedProvinces)
                foreach (var neighbor in province.getAllNeighbors().Where(neigbor => neigbor.Country != this))
                    yield return neighbor.Country;

            //List<Province> result = new List<Province>();
            //foreach (var province in ownedProvinces)
            //    result.AddRange(
            //        province.getAllNeighbors().Where(p => p.Country != this && !result.Contains(p))
            //        );
            //return result;
        }

        private bool isOnlyCountry()
        {
            if (World.getAllExistingCountries().Any(x => x != this))
                return false;
            else
                return true;
        }



        public void setCapitalTextMesh(Province province)
        {
            Transform txtMeshTransform = GameObject.Instantiate(LinksManager.Get.r3DCountryTextPrefab).transform;
            txtMeshTransform.SetParent(province.GameObject.transform, false);

            Vector3 capitalTextPosition = province.Position;
            capitalTextPosition.y += 2f;
            //capitalTextPosition.z -= 5f;
            txtMeshTransform.position = capitalTextPosition;

            meshCapitalText = txtMeshTransform.GetComponent<TextMesh>();
            meshCapitalText.text = FullName;
            // meshCapitalText.fontSize *= 2;
            if (this == Game.Player)
            {
                meshCapitalText.color = Color.blue;
                meshCapitalText.fontSize += 10;
            }
            else
            {
                meshCapitalText.color = Color.cyan; // Set the text's color to red
                                                    //messhCapitalText.fontSize += messhCapitalText.fontSize / 3;
            }
        }

        public void MoveCapitalTo(Province newCapital)
        {
            if (meshCapitalText == null)
                setCapitalTextMesh(newCapital);
            else
            {
                Vector3 capitalTextPosition = newCapital.Position;
                capitalTextPosition.y += 2f;
                capitalTextPosition.z -= 5f;
                meshCapitalText.transform.position = capitalTextPosition;
            }
            capital = newCapital;
        }

        public Color getColor()
        {
            return nationalColor;
        }

        public Procent getYesVotes(AbstractReformValue reform, ref Procent procentPopulationSayedYes)
        {
            // calculate how much of population wants selected reform
            int totalPopulation = GetAllPopulation().Sum(x => x.population.Get());
            int votingPopulation = 0;
            int populationSayedYes = 0;
            int votersSayedYes = 0;
            Procent procentVotersSayedYes = new Procent(0);
            //Procent procentPopulationSayedYes = new Procent(0f);
            foreach (Province province in ownedProvinces)
                foreach (PopUnit pop in province.GetAllPopulation())
                {
                    if (pop.canVote())
                    {
                        if (pop.getSayingYes(reform))
                        {
                            votersSayedYes += pop.population.Get();// * pop.getVotingPower();
                            populationSayedYes += pop.population.Get();// * pop.getVotingPower();
                        }
                        votingPopulation += pop.population.Get();// * pop.getVotingPower();
                    }
                    else
                    {
                        if (pop.getSayingYes(reform))
                            populationSayedYes += pop.population.Get();// * pop.getVotingPower();
                    }
                }
            if (totalPopulation != 0)
                procentPopulationSayedYes.Set((float)populationSayedYes / totalPopulation);
            else
                procentPopulationSayedYes.Set(0);

            if (votingPopulation == 0)
                procentVotersSayedYes.Set(0);
            else
                procentVotersSayedYes.Set((float)votersSayedYes / votingPopulation);
            return procentVotersSayedYes;
        }

        public Material getBorderMaterial()
        {
            return borderMaterial;
        }

        /// <summary>
        /// Not finished, don't use it
        /// </summary>
        /// <param name="reform"></param>
        public Procent getYesVotes2(AbstractReformValue reform, ref Procent procentPopulationSayedYes)
        {
            int totalPopulation = GetAllPopulation().Sum(x => x.population.Get());
            int votingPopulation = 0;
            int populationSayedYes = 0;
            int votersSayedYes = 0;
            Procent procentVotersSayedYes = new Procent(0f);
            Dictionary<PopType, int> divisionPopulationResult = new Dictionary<PopType, int>();
            Dictionary<PopType, int> divisionVotersResult = getYesVotesByType(reform, ref divisionPopulationResult);
            foreach (KeyValuePair<PopType, int> next in divisionVotersResult)
                votersSayedYes += next.Value;

            if (totalPopulation != 0)
                procentPopulationSayedYes.Set((float)populationSayedYes / totalPopulation);
            else
                procentPopulationSayedYes.Set(0);

            if (votingPopulation == 0)
                procentVotersSayedYes.Set(0);
            else
                procentVotersSayedYes.Set((float)votersSayedYes / votingPopulation);
            return procentVotersSayedYes;
        }

        public Dictionary<PopType, int> getYesVotesByType(AbstractReformValue reform, ref Dictionary<PopType, int> divisionPopulationResult)
        {  // division by pop types
            Dictionary<PopType, int> divisionVotersResult = new Dictionary<PopType, int>();
            foreach (PopType type in PopType.getAllPopTypes())
            {
                divisionVotersResult.Add(type, 0);
                divisionPopulationResult.Add(type, 0);
                foreach (Province province in ownedProvinces)
                {
                    foreach (PopUnit pop in province.GetAllPopulation(type))
                        if (pop.getSayingYes(reform))
                        {
                            divisionPopulationResult[type] += pop.population.Get();// * pop.getVotingPower();
                            if (pop.canVote())
                                divisionVotersResult[type] += pop.population.Get();// * pop.getVotingPower();
                        }
                }
            }
            return divisionVotersResult;
        }

        //public bool isInvented(Invention type)
        //{
        //    return inventions.isInvented(type);
        //}

        /// <summary>
        /// returns new value
        /// </summary>
        public MoneyView getMinSalary()
        {
            var res = (minimalWage.getValue() as MinimalWage.ReformValue).getMinimalWage(this.market);
            if (res.isZero())
                return Options.FactoryMinPossibleSallary;
            else
                return res;
            //return minSalary.get();
        }

        public string ShortName
        {
            get { return name; }
        }

        public string FullName
        {
            get
            {
                if (Game.devMode && this == Game.Player)
                    return name + " " + government.getPrefix() + " (you are)";
                else
                    return name + " " + government.getPrefix();
            }
        }



        public override string ToString()
        {
            return FullName;
        }

        /// <summary>
        /// Returns true if succeeded
        /// </summary>
        private bool buildIfCanPE(ProductionType propositionFactory, Province province)
        {
            // could it give uninvented factory?
            if (propositionFactory != null)
            {
                var buildNeeds = countryStorageSet.hasAllOfConvertToBiggest(propositionFactory.GetBuildNeeds());
                if (buildNeeds != null)
                {
                    var newFactory = province.BuildFactory(this, propositionFactory, Country.market.getCost(buildNeeds));
                    consumeFromCountryStorage(buildNeeds, this);
                    return true;
                    //newFactory.constructionNeeds.setZero();
                }
            }
            return false;
        }

        /// <summary>
        ///  Returns null if needs are satisfied
        /// </summary>
        private Product getMostDeficitProductAllowedHere(IEnumerable<Product> selector, Province province)
        {
            Storage minFound = null;
            foreach (var item in selector)
                if (Invented(item))
                {
                    var proposition = ProductionType.whoCanProduce(item);
                    if (proposition != null)
                        if (proposition.canBuildNewFactory(province, this) || province.CanUpgradeFactory(proposition, this))
                        {
                            var found = countryStorageSet.GetFirstSubstituteStorage(item);
                            if (minFound == null || found.isSmallerThan(minFound))
                                minFound = found;
                        }
                }
            if (minFound == null)
                return null;
            else
                return minFound.Product;
        }

        // todo should be redone as country-wise method
        public void invest(Province province)
        {
            if (economy.getValue() == Economy.PlannedEconomy && Invented(Invention.Manufactures))
                if (!province.isThereFactoriesInUpgradeMoreThan(1)//Options.maximumFactoriesInUpgradeToBuildNew)
                    && province.getUnemployedWorkers() > 0)
                {
                    var industrialProduct = getMostDeficitProductAllowedHere(Product.getAllSpecificProductsInvented(x => x.isIndustrial(), this), province);
                    if (industrialProduct == null)
                    {
                        var militaryProduct = getMostDeficitProductAllowedHere(Product.getAllSpecificProductsInvented(x => x.isMilitary(), this), province);
                        if (militaryProduct == null)
                        {
                            var consumerProduct = getMostDeficitProductAllowedHere(Product.getAllSpecificProductsInvented(x => x.isConsumerProduct(), this), province);
                            if (consumerProduct != null)
                            {
                                //if there is no enough some consumer product - build it
                                var proposition = ProductionType.whoCanProduce(consumerProduct);
                                if (proposition.canBuildNewFactory(province, this))
                                    buildIfCanPE(proposition, province);
                                else
                                {
                                    var factory = province.findFactory(proposition);
                                    if (countryStorageSet.has(factory.getUpgradeNeeds()))
                                        factory.upgrade(this);
                                }
                            }
                            //else - all needs are satisfied
                        }
                        else
                        {
                            //if there is no enough some military product - build it
                            var proposition = ProductionType.whoCanProduce(militaryProduct);
                            if (proposition.canBuildNewFactory(province, this))
                                buildIfCanPE(proposition, province);
                            else
                            {
                                var factory = province.findFactory(proposition);
                                if (countryStorageSet.has(factory.getUpgradeNeeds()))
                                    factory.upgrade(this);
                            }
                        }
                    }
                    else
                    {
                        //if there is no enough some industrial product - build it
                        var proposition = ProductionType.whoCanProduce(industrialProduct);
                        if (proposition.canBuildNewFactory(province, this))
                            buildIfCanPE(proposition, province);
                        else
                        {
                            var factory = province.findFactory(proposition);
                            if (countryStorageSet.has(factory.getUpgradeNeeds()))
                                factory.upgrade(this);
                        }
                    }
                }
        }

        /// <summary>
        /// For AI only
        /// </summary>
        public void AIThink()
        {
            // attacking neighbors
            if (!isOnlyCountry())
                if (Rand.Get.Next(6) == 1)
                {
                    if ((getAverageMorale().get() > 0.3f) || getAllArmiesSize() == 0)// because zero army has zero morale
                    {
                        var thisStrength = getStrengthExluding(null);
                        if (thisStrength > 0)
                        {
                            var targetCountry = AllNeighborCountries().Distinct()
                                .Where(x => getRelationTo(x).get() < 0.9f || Rand.Get.Next(200) == 1)
                                .MinBy(x => getRelationTo(x.Country).get());


                            var targetPool = AllNeighborProvinces().Distinct().Where(x => x.Country == targetCountry).ToList();
                            var targetProvince = targetPool.Where(x => x.isCoreFor(this)).FirstOrDefault();
                            if (targetProvince == null)
                                targetProvince = targetPool.Where(x => x.getMajorCulture() == this.getCulture()).FirstOrDefault();
                            if (targetProvince == null)
                                targetProvince = targetPool.Random();

                            if (targetProvince != null
                            && (thisStrength > targetProvince.Country.getStrengthExluding(null) * 0.25f
                                || targetProvince.Country == World.UncolonizedLand
                                || targetProvince.Country.isAI() && getStrengthExluding(null) > targetProvince.Country.getStrengthExluding(null) * 0.1f)
                            && canAttack.isAllTrue(targetProvince, this)
                            && (targetProvince.Country.isAI() || Options.AIFisrtAllowedAttackOnHuman.isPassed())
                            )
                            {
                                mobilize(ownedProvinces);
                                foreach (var army in AllArmies())
                                {
                                    army.SetPathTo(targetProvince, x => x.Country == this || x.Country == targetCountry);
                                    //if (army.Path==null)
                                }

                            }
                        }
                    }
                }
            if (Rand.Get.Next(90) == 1)
                aiInvent();
            // changing salary for soldiers
            if (economy.getValue() != Economy.PlannedEconomy)
                if (Invented(Invention.ProfessionalArmy) && Rand.Get.Next(10) == 1)
                {
                    Money newWage;
                    Money soldierAllNeedsCost = Country.market.getCost(PopType.Soldiers.getAllNeedsPer1000Men()).Copy();
                    if (failedToPaySoldiers)
                    {
                        newWage = getSoldierWage().Copy().Multiply(0.8m);
                        //getSoldierWage().Get() - getSoldierWage().Get() * 0.2m;
                    }
                    else
                    {
                        var balance = getBalance();

                        if (balance > 200f)
                            newWage = getSoldierWage().Copy().Add(soldierAllNeedsCost.Copy().Multiply(0.002m).Add(1m));
                        else if (balance > 50f)
                            newWage = getSoldierWage().Copy().Add(soldierAllNeedsCost.Copy().Multiply(0.0005m).Add(0.1m));
                        else if (balance < -800f)
                            newWage = new Money(0m);
                        else if (balance < 0f)
                            newWage = getSoldierWage().Copy().Multiply(0.5m);
                        else
                            newWage = getSoldierWage().Copy(); // don't change wage
                    }
                    //newWage = newWage.Clamp(0, soldierAllNeedsCost * 2m);
                    var limit = soldierAllNeedsCost.Copy().Multiply(2m);
                    if (newWage.isBiggerThan(limit))
                        newWage.Set(limit);
                    setSoldierWage(newWage);
                }
            // dealing with enterprises
            if (economy.getValue() == Economy.Interventionism)
                Rand.Call(() => getAllFactories().Where(
                    x => x.ownership.HowMuchOwns(this).Copy().Subtract(x.ownership.HowMuchSelling(this))
                    .isBiggerOrEqual(Procent._50Procent)).PerformAction(
                    x => x.ownership.SetToSell(this, Options.PopBuyAssetsAtTime)),
                    30);
            else
            //State Capitalism invests in own country only, Interventionists don't invests in any country
            if (economy.getValue() == Economy.StateCapitalism)
                Rand.Call(
                    () =>
                    {
                        if (Game.logInvestments)
                        {
                            var c = allInvestmentProjects.Get().ToList();
                            c = c.OrderByDescending(x => x.Value.get()).ToList();
                            var d = c.MaxBy(x => x.Value.get());
                            var e = c.MaxByRandom(x => x.Value.get());
                        }
                        // copied from Capitalist.Invest()
                        // doesn't care about risks
                        var project = allInvestmentProjects.Get().Where(
                            //x => x.Value.isBiggerThan(Options.minMarginToInvest) && x.Key.CanProduce Invented
                            delegate (KeyValuePair<IInvestable, Procent> x)
                            {
                                var isFactory = x.Key as Factory;
                                if (isFactory != null)
                                    return InventedFactory(isFactory.Type);
                                else
                                {
                                    var newFactory = x.Key as NewFactoryProject;
                                    if (newFactory != null)
                                        return InventedFactory(newFactory.Type);
                                    else
                                    {
                                        var isBuyingShare = x.Key as Owners;
                                        if (isBuyingShare != null)
                                            if (isBuyingShare.HowMuchSelling(this).isNotZero())
                                                return false;
                                    }
                                }
                                return true;
                            }
                            ).MaxByRandom(x => x.Value.get());
                        if (!project.Equals(default(KeyValuePair<IInvestable, Procent>)) && project.Value.isBiggerThan(Options.minMarginToInvest.Copy().Multiply(Options.InvestorEmploymentSafety)))
                        {
                            MoneyView investmentCost = project.Key.GetInvestmentCost(market);
                            if (!CanPay(investmentCost))
                                Bank.GiveLackingMoneyInCredit(this, investmentCost);
                            if (CanPay(investmentCost))
                            {
                                project.Value.Set(Procent.Zero);
                                Factory factory = project.Key as Factory;
                                if (factory != null)
                                {
                                    if (factory.IsOpen)// upgrade existing factory
                                        factory.upgrade(this);
                                    else
                                        factory.open(this, true);
                                }
                                else
                                {
                                    Owners buyShare = project.Key as Owners;
                                    if (buyShare != null) // buy part of existing factory
                                        buyShare.BuyStandardShare(this);
                                    else
                                    {
                                        var factoryProject = project.Key as NewFactoryProject;
                                        if (factoryProject != null)
                                        {
                                            Factory factory2 = factoryProject.Province.BuildFactory(this, factoryProject.Type, investmentCost);
                                            PayWithoutRecord(factory2, investmentCost);
                                        }
                                        else
                                            Debug.Log("Unknown investment type");
                                    }
                                }
                            }
                        }
                    }, Options.CountryInvestmentRate);
        }

        public void simulate()
        {
            // military staff
            base.simulate();

            ownershipSecurity.Add(Options.CountryOwnershipRiskRestoreSpeed, false);
            ownershipSecurity.clamp100();
            // get science points
            sciencePoints.Add(Options.defaultSciencePointMultiplier * modSciencePoints.getModifier(this));

            // put extra money in bank
            if (economy.getValue() != Economy.PlannedEconomy)
                if (autoPutInBankLimit.isNotZero())
                {
                    var extraMoney = Cash.Copy().Subtract(autoPutInBankLimit, false);
                    //float extraMoney = Cash.get() - (float)this.autoPutInBankLimit;
                    if (extraMoney.isNotZero())
                        Bank.ReceiveMoney(this, extraMoney);
                }

            //todo performance - do it not every tick?
            //International opinion;
            foreach (var item in World.getAllExistingCountries())
                if (item != this)
                {
                    changeRelation(item, modMyOpinionOfXCountry.getModifier(item));
                }
            //movements
            movements.RemoveAll(x => x.isEmpty());
            foreach (var item in movements.ToArray())
                item.Simulate();
            if (economy.getValue() == Economy.LaissezFaire)
                Rand.Call(() => getAllFactories().PerformAction(x => x.ownership.SetToSell(this, Procent.HundredProcent, false)), 30);
        }

        private void aiInvent()
        {
            var invention = GetAllUninvented().Where(x => sciencePoints.isBiggerOrEqual(x.Key.getCost())).Random();//.ToList()
            if (invention.Key != null)
                invent(invention.Key);
        }

        /// <summary>
        /// Checks ouside
        /// </summary>
        /// <param name="invention"></param>
        public void invent(Invention invention)
        {
            //if (sciencePoints.isBiggerOrEqual(invention.getCost()))
            {
                markInvented(invention);
                sciencePoints.Subtract(invention.getCost());
                //return true;
            }
            //else return false;
        }

        private void tradeNonPE(bool usePlayerTradeSettings)//, int buyProductsForXDays)
        {
            // firstly, buy last tick expenses -NO, buy as set in trade sliders
            // then by rest but avoid huge market interference
            //1 day trade
            //TODO add x day buying or split buying somehow

            foreach (var product in Product.getAll().Where(x => !x.isAbstract()))
                //if (product.isInventedBy(this) || product == Product.Cattle)
                if (product.isTradable())
                {
                    Storage maxLimit;
                    Storage minLimit;

                    if (usePlayerTradeSettings)
                    {
                        maxLimit = getSellIfMoreLimits(product);
                        minLimit = getBuyIfLessLimits(product);
                        //if (buyProductsForXDays > 1)
                        //    minLimit.multiply(buyProductsForXDays);
                    }
                    else
                    {
                        var takenFromStorage = new Storage(countryStorageSet.used.GetFirstSubstituteStorage(product));

                        if (takenFromStorage.isZero())
                        {
                            minLimit = new Storage(takenFromStorage.Product, Options.CountryMinStorage);
                            maxLimit = new Storage(takenFromStorage.Product, Options.CountryMaxStorage);// todo change
                        }
                        else
                        {
                            minLimit = new Storage(takenFromStorage);
                            maxLimit = takenFromStorage.Multiply(Options.CountrySaveProductsDaysMaximum);
                        }
                    }
                    var howMuchHave = countryStorageSet.GetFirstSubstituteStorage(product);
                    if (howMuchHave.isBiggerThan(maxLimit))
                    {
                        var howMuchToSell = howMuchHave.Copy().subtract(maxLimit);
                        if (howMuchToSell.isNotZero())
                            SendToMarket(howMuchToSell);
                    }
                    else
                    {
                        if (howMuchHave.isSmallerThan(minLimit))
                        {
                            var howMuchToBuy = minLimit.Copy().subtract(howMuchHave);
                            buyNeeds(howMuchToBuy);
                        }
                    }
                    if (getMoneyAvailable().isZero()) // no more money to buy
                        break;
                }
        }

        private void tradeWithPE(bool usePlayerTradeSettings)
        {
            // planned economy buying
            //1 day buying
            foreach (var product in Product.getAllNonAbstractTradableInPEOrder(this))
            //if (product.isInvented(this)) // already checked
            //foreach (var currentStorage in countryStorageSet)
            {
                Storage desiredMinimum;
                if (usePlayerTradeSettings)
                    desiredMinimum = getBuyIfLessLimits(product);
                else
                {
                    desiredMinimum = new Storage(countryStorageSet.used.GetFirstSubstituteStorage(product));
                    if (desiredMinimum.isZero())
                        desiredMinimum.Add(Options.CountryMinStorage);
                }
                var toBuy = desiredMinimum.Copy().subtract(countryStorageSet.GetFirstSubstituteStorage(product), false);
                if (toBuy.isBiggerThan(Value.Zero))
                    buyNeeds(toBuy);//go buying
            }
            // x day buying +sells
            //foreach (var currentStorage in countryStorageSet)
            foreach (var product in Product.getAllNonAbstractTradableInPEOrder(this))
            {
                var takenFromStorage = new Storage(countryStorageSet.used.GetFirstSubstituteStorage(product));
                Storage desiredMinimum;
                if (usePlayerTradeSettings)
                    desiredMinimum = getBuyIfLessLimits(product);
                else
                {
                    if (takenFromStorage.isZero())
                        desiredMinimum = new Storage(takenFromStorage.Product, Options.CountryMinStorage);// todo change
                    else
                        desiredMinimum = takenFromStorage.Copy().Multiply(Options.CountryBuyProductsForXDays);
                }
                var toBuy = desiredMinimum.Copy().subtract(countryStorageSet.GetFirstSubstituteStorage(product), false);
                if (toBuy.isBiggerThan(Value.Zero)) // have less than desiredMinimum
                    buyNeeds(toBuy);//go buying
                else    // no need to buy anything
                {
                    Storage desiredMaximum;
                    if (usePlayerTradeSettings)
                        desiredMaximum = getSellIfMoreLimits(product);
                    else
                    {
                        if (takenFromStorage.isZero())
                            desiredMaximum = new Storage(takenFromStorage.Product, Options.CountryMaxStorage);// todo change
                        else
                            desiredMaximum = takenFromStorage.Copy().Multiply(Options.CountrySaveProductsDaysMaximum);
                    }
                    var toSell = countryStorageSet.GetFirstSubstituteStorage(product).Copy().subtract(desiredMaximum, false);
                    if (toSell.isBiggerThan(Value.Zero))   // have more than desiredMaximum
                    {
                        if (toSell.isNotZero())
                            SendToMarket(toSell);//go sell
                    }
                }
            }
        }

        /// <summary>
        /// Represents buying and/or consuming needs.
        /// </summary>
        public override void consumeNeeds()
        {
            foreach (var item in AllArmies())
            {
                item.consume();
            }
            // Should go After all Armies consumption

            if (economy.getValue() == Economy.PlannedEconomy)
                tradeWithPE(!isAI());
            else
            {
                tradeNonPE(!isAI());   //non PE - trade as PE but in normal order
            }

            //var needs = getRealAllNeeds();
            ////buy 1 day needs
            //foreach (var need in needs)
            //    if (!countryStorageSet.has(need)) // may reduce extra circles
            //    {
            //        // if I want to buy
            //        //Storage toBuy = new Storage(need.Product, need.get() - storageSet.getStorage(need.Product).get(), false);
            //        Storage realNeed;
            //        if (need.isAbstractProduct())
            //            realNeed = countryStorageSet.convertToBiggestStorageProduct(need);
            //        else
            //            realNeed = need;
            //        //Storage toBuy = need.subtractOutside(realNeed);

            //        if (realNeed.isNotZero())
            //            buyNeeds(realNeed);  // todo - return result? - no
            //    }
            ////buy x day needs
            //foreach (var need in needs)
            //{
            //    Storage toBuy = new Storage(need.Product,
            //        need.get() * Options.CountryForHowMuchDaysMakeReservs - countryStorageSet.getBiggestStorage(need.Product).get(), false);
            //    if (toBuy.isNotZero())
            //        buyNeeds(toBuy);
            //}
        }

        private void buyNeeds(Storage toBuy)
        {
            Storage realyBougth = Buy(toBuy, null);
            if (realyBougth.isNotZero())
            {
                countryStorageSet.Add(realyBougth);
                storageBuyingExpenseAdd(Country.market.getCost(realyBougth));
            }
        }

        //public Procent getUnemployment()
        //{
        //return GetAllPopulation().GetAverageProcent(x => x.getUnemployedProcent());

        //Procent result = new Procent(0f);
        //int calculatedBase = 0;
        //foreach (var item in ownedProvinces)
        //{
        //    //int population = item.getMenPopulationEmployable();
        //    int population = item.GetAllPopulation().Where(x => x.Type.canBeUnemployed()).Sum(x=>x.population.Get());
        //    result.AddPoportionally(calculatedBase, population, item.getUnemployment(PopType.All));
        //    //result.AddPoportionally(calculatedBase,(float) population, item.GetAllPopulation().Sum(x=>x.getUnemployedProcent().get()));
        //    calculatedBase += population;
        //}
        //return result;
        //}
        //public int getMenPopulation()
        //{
        //    int result = 0;
        //    foreach (Province pr in ownedProvinces)
        //        result += pr.getMenPopulation();
        //    return result;
        //}
        public int getFamilyPopulation()
        {
            //return  getMenPopulation() * Options.familySize;
            return GetAllPopulation().Sum(x => x.population.Get()) * Options.familySize;
        }

        public int getPopulationAmountByType(PopType ipopType)
        {
            int result = 0;
            foreach (Province province in ownedProvinces)
                result += province.GetAllPopulation(ipopType).Sum(x => x.population.Get());
            return result;
        }

        public IEnumerable<PopUnit> GetAllPopulation(PopType type)
        {
            foreach (var province in ownedProvinces)
            {
                foreach (var item in province.GetAllPopulation(type))
                {
                    yield return item;
                }
            }
        }

        public IEnumerable<PopUnit> GetAllPopulation()
        {
            foreach (var province in ownedProvinces)
                foreach (var pops in province.GetAllPopulation())
                    yield return pops;
        }

        public IEnumerable<Province> AllProvinces()
        {
            foreach (var province in ownedProvinces)
                yield return province;
        }

        public IEnumerable<Agent> getAllAgents()
        {
            foreach (var province in ownedProvinces)
                foreach (var agent in province.getAllAgents())
                    yield return agent;
            if (Bank != null)
                yield return Bank;
        }

        public IEnumerable<Factory> getAllFactories()
        {
            foreach (var province in ownedProvinces)
            {
                foreach (var item in province.getAllFactories())
                {
                    yield return item;
                }
            }
        }

        public MoneyView getGDP()
        {
            Money result = new Money(0m);
            foreach (var province in ownedProvinces)
            {
                result.Add(province.getGDP());
            }
            return result;
        }

        public MoneyView getGDPPer1000()
        {
            return (getGDP().Copy()).Multiply(1000m).Divide(getFamilyPopulation(), false);
            // overflows
            //res.multiply(1000);
            //res.divide(getFamilyPopulation());

            //return res;
        }

        private static int ValueOrder(KeyValuePair<Country, Value> x, KeyValuePair<Country, Value> y)
        {
            float sumX = x.Value.get();
            float sumY = y.Value.get();
            return sumY.CompareTo(sumX); // bigger - first
        }

        private static int ProcentOrder(KeyValuePair<Culture, Procent> x, KeyValuePair<Culture, Procent> y)
        {
            float sumX = x.Value.get();
            float sumY = y.Value.get();
            return sumY.CompareTo(sumX); // bigger - first
        }

        private static int FloatOrder(KeyValuePair<Country, float> x, KeyValuePair<Country, float> y)
        {
            float sumX = x.Value;
            float sumY = y.Value;
            return sumY.CompareTo(sumX); // bigger - first
        }

        private static int IntOrder(KeyValuePair<Country, int> x, KeyValuePair<Country, int> y)
        {
            float sumX = x.Value;
            float sumY = y.Value;
            return sumY.CompareTo(sumX); // bigger - first
        }

        /// <summary>
        /// Returns 0 if failed
        /// </summary>
        public int getGDPRank()
        {
            var list = World.getAllExistingCountries().OrderByDescending(x => x.getGDP().Get());
            return list.FindIndex(x => x == this) + 1; // starts with zero;

            //var list = new List<KeyValuePair<Country, Value>>();
            //foreach (var item in World.getAllExistingCountries())
            //{
            //    list.Add(new KeyValuePair<Country, Value>(item, item.getGDP()));
            //}
            //list.Sort(ValueOrder);
            //return list.FindIndex(x => x.Key == this) + 1; // starts with zero;
        }

        /// <summary>
        /// Returns 0 if failed
        /// </summary>
        public int getGDPPer1000Rank()
        {
            var list = new List<KeyValuePair<Country, float>>();
            foreach (var item in World.getAllExistingCountries())
            {
                list.Add(new KeyValuePair<Country, float>(item, (float)item.getGDPPer1000().Get()));
            }
            list.Sort(FloatOrder);
            return list.FindIndex(x => x.Key == this) + 1; // starts with zero
        }

        public Procent getGDPShare()
        {
            Money worldGDP = new Money(0m);
            foreach (var item in World.getAllExistingCountries())
            {
                worldGDP.Add(item.getGDP());
            }
            return new Procent(getGDP(), worldGDP);
        }

        /// <summary>
        /// Returns 0 if failed
        /// </summary>
        public int getPopulationRank()
        {
            var list = new List<KeyValuePair<Country, int>>();
            foreach (var item in World.getAllExistingCountries())
            {
                list.Add(new KeyValuePair<Country, int>(item, item.getFamilyPopulation()));
            }
            list.Sort(IntOrder);
            return list.FindIndex(x => x.Key == this) + 1; // starts with zero
        }

        /// <summary>
        /// Returns 0 if failed
        /// </summary>
        public int getSizeRank()
        {
            var list = new List<KeyValuePair<Country, int>>();
            foreach (var item in World.getAllExistingCountries())
            {
                list.Add(new KeyValuePair<Country, int>(item, item.getSize()));
            }
            list.Sort(IntOrder);
            return list.FindIndex(x => x.Key == this) + 1; // starts with zero
        }

        public override void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            incomeTaxForeigner.SetZero();
            countryStorageSet.SetStatisticToZero();
            failedToPaySoldiers = false;
            incomeTaxStaticticPoor.SetZero();
            incomeTaxStatisticRich.SetZero();
            goldMinesIncome.SetZero();
            unemploymentSubsidiesExpense.SetZero();
            ownedFactoriesIncome.SetZero();
            factorySubsidiesExpense.SetZero();
            storageBuyingExpense.SetZero();
            soldiersWageExpense.SetZero();
        }

        public float getBalance()
        {
            return (float)(moneyIncomeThisTurn.Get() - getExpenses().Get());
        }

        public MoneyView getExpenses()
        {
            Money result = MoneyView.Zero.Copy();
            result.Add(unemploymentSubsidiesExpense);
            result.Add(factorySubsidiesExpense);
            result.Add(storageBuyingExpense);
            result.Add(soldiersWageExpense);
            return result;
        }

        public Money getIncome()
        {
            Money result = new Money(0m);
            result.Add(incomeTaxStaticticPoor);
            result.Add(incomeTaxStatisticRich);
            result.Add(incomeTaxForeigner);
            result.Add(goldMinesIncome);
            result.Add(ownedFactoriesIncome);
            result.Add(getCostOfAllSellsByGovernment());
            return result;
        }

        /// <summary>
        /// Returns true if was able to give a subsidy
        /// </summary>
        public bool GiveFactorySubsidies(Consumer byWhom, MoneyView howMuch)
        {
            if (CanPay(howMuch))
            {
                PayWithoutRecord(byWhom, howMuch);
                factorySubsidiesExpense.Add(howMuch);
                return true;
            }
            else
                return false;
            //{
            //    //sendAll(byWhom.wallet);
            //    payWithoutRecord(byWhom, byWhom.Cash);
            //    factorySubsidiesExpense.Add(byWhom.Cash);
            //}
        }

        public void soldiersWageExpenseAdd(MoneyView payCheck)
        {
            soldiersWageExpense.Add(payCheck);
        }

        //public float getPoorTaxIncome()
        //{
        //    return poorTaxIncome.get();
        //}

        //public float getRichTaxIncome()
        //{
        //    return richTaxIncome.get();
        //}

        /// <summary>
        /// Forces payer to pay tax from taxable. Returns how much payed (new value)
        /// Don't call it manually, it called from Agent.Pay() automatically
        /// </summary>
        public MoneyView TakeIncomeTaxFrom(Agent taxPayer, MoneyView taxable, bool isPoorStrata)
        {
            var pop = taxPayer as PopUnit;
            if (pop != null
                && pop.Type == PopType.Aristocrats
                //&& Serfdom.IsNotAbolishedInAnyWay.checkIfTrue(Country))
                && government.getTypedValue() == Government.Aristocracy)
                return MoneyView.Zero; // don't pay with monarchy
            Procent tax;
            Money statistics;
            if (isPoorStrata)
            {
                tax = taxationForPoor.getTypedValue().tax;
                statistics = incomeTaxStaticticPoor;
            }
            else //if (type is TaxationForRich)
            {
                tax = taxationForRich.getTypedValue().tax;
                statistics = incomeTaxStatisticRich;
            }
            if (!(taxPayer is Market) && taxPayer.Country != this) //foreigner
                statistics = incomeTaxForeigner;

            var taxSize = taxable.Copy().Multiply(tax);
            if (taxPayer.CanPay(taxSize))
            {
                taxPayer.incomeTaxPayed.Add(taxSize);
                statistics.Add(taxSize);
                moneyIncomeThisTurn.Add(taxSize);
                taxPayer.PayWithoutRecord(this, taxSize);
                return taxSize;
            }
            else
            {
                var hadMoney = taxPayer.getMoneyAvailable().Copy();
                taxPayer.incomeTaxPayed.Add(taxPayer.getMoneyAvailable());
                statistics.Add(taxPayer.getMoneyAvailable());
                moneyIncomeThisTurn.Add(taxPayer.getMoneyAvailable());
                taxPayer.PayAllAvailableMoneyWithoutRecord(this);
                return hadMoney;
            }
        }

        public bool TakeNaturalTax(PopUnit pop, Procent tax)
        {
            var howMuchSend = pop.getGainGoodsThisTurn().Multiply(tax);

            if (pop.storage.isBiggerOrEqual(howMuchSend))
            {
                pop.storage.send(countryStorageSet, howMuchSend);
                return true;
            }
            else
            {
                pop.storage.sendAll(countryStorageSet);
                return false;
            }
        }

        public void goldMinesIncomeAdd(MoneyView toAdd)
        {
            goldMinesIncome.Add(toAdd);
        }

        public void unemploymentSubsidiesExpenseAdd(MoneyView toAdd)
        {
            unemploymentSubsidiesExpense.Add(toAdd);
        }

        public void storageBuyingExpenseAdd(MoneyView toAdd)
        {
            storageBuyingExpense.Add(toAdd);
        }

        public void ownedFactoriesIncomeAdd(MoneyView toAdd)
        {
            ownedFactoriesIncome.Add(toAdd);
        }

        //override public Country Country
        //{
        //    get { return this; }
        //}
        /// <summary>
        /// Gets reform which can take given value
        /// </summary>
        public AbstractReform getReform(AbstractReformValue abstractReformValue)
        {
            foreach (var item in reforms)
            {
                if (item.canHaveValue(abstractReformValue))
                    return item;
            }
            return null;
        }

        public void OnClicked()
        {
            if (MainCamera.diplomacyPanel.isActiveAndEnabled)
            {
                if (MainCamera.diplomacyPanel.getSelectedCountry() == this)
                    MainCamera.diplomacyPanel.Hide();
                else
                    MainCamera.diplomacyPanel.show(this);
            }
            else
                MainCamera.diplomacyPanel.show(this);
        }

        public float GetNameWeight()
        {
            return nameWeight;
        }

        //private readonly Properties stock = new Properties();
        //public Properties GetOwnership()
        //{
        //    return stock;
        //}
        public void annexTo(Country country)
        {
            foreach (var item in ownedProvinces.ToList())
            {
                country.TakeProvince(item, false);
                //item.secedeTo(country, false);
            }
        }

        /// <summary>
        /// Gives list of allowed IInvestable with pre-calculated Margin in Value. Doesn't check if it's invented
        /// </summary>
        public readonly CashedData<Dictionary<IInvestable, Procent>> allInvestmentProjects;

        private Dictionary<IInvestable, Procent> GetAllInvestmentProjects2()
        {
            return GetAllInvestmentProjects().ToDictionary(y => y, x => x.GetMargin());
        }

        private IEnumerable<IInvestable> GetAllInvestmentProjects()//Agent investor
        {
            foreach (var province in ownedProvinces)
                foreach (var item in province.getAllInvestmentProjects())//investor
                {
                    yield return item;
                }
        }

        public void Nationilize(Factory factory)
        {
            foreach (var owner in factory.ownership.GetAll().ToList())
                if (owner.Key != this)
                {
                    factory.ownership.TransferAll(owner.Key, Game.Player);
                    ownershipSecurity.Subtract(Options.CountryOwnershipRiskDropOnNationalization, false);
                    var popOwner = owner.Key as PopUnit;
                    if (popOwner != null && popOwner.Country == this)
                        popOwner.loyalty.Subtract(Options.PopLoyaltyDropOnNationalization, false);
                    else
                    {
                        var countryOwner = owner.Key as Country;
                        if (countryOwner != null)
                            countryOwner.changeRelation(this, Options.PopLoyaltyDropOnNationalization.get());
                    }
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
        public Date getLastAttackDateOn(Country country)
        {
            if (LastAttackDate.ContainsKey(country))
                return LastAttackDate[country];
            else
                return Date.Never.Copy();
        }
    }
}