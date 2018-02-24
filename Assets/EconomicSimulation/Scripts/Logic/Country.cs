using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.Serialization;
using Nashet.UnityUIUtils;
using Nashet.Conditions;
using Nashet.ValueSpace;
using Nashet.Utils;
namespace Nashet.EconomicSimulation
{
    public class Country : MultiSeller, IClickable, IShareOwner, ISortableName, INameable
    {
        internal readonly Government government;
        internal readonly Economy economy;
        internal readonly Serfdom serfdom;
        internal readonly MinimalWage minimalWage;
        internal readonly UnemploymentSubsidies unemploymentSubsidies;
        internal readonly TaxationForPoor taxationForPoor;
        internal readonly TaxationForRich taxationForRich;

        /// <summary> could be null</summary>
        private readonly Bank bank;
        public Bank Bank { get { return bank; } }

        internal readonly MinorityPolicy minorityPolicy;

        private readonly List<Province> ownedProvinces = new List<Province>();

        private readonly Dictionary<Country, Procent> opinionOf = new Dictionary<Country, Procent>();
        private readonly Dictionary<Country, Date> myLastAttackDate = new Dictionary<Country, Date>();
        private readonly Dictionary<Invention, bool> inventions = new Dictionary<Invention, bool>();

        public readonly List<AbstractReform> reforms = new List<AbstractReform>();
        public readonly List<Movement> movements = new List<Movement>();



        private readonly string name;
        private readonly Culture culture;
        private readonly Color nationalColor;
        private Province capital;
        private bool alive = true;

        private readonly Money soldiersWage = new Money(0f);
        public readonly Value sciencePoints = new Value(0f);
        public bool failedToPaySoldiers;
        public int autoPutInBankLimit = 2000;

        private readonly Procent ownershipSecurity = Procent.HundredProcent.Copy();
        /// <summary> Read only, new value</summary>
        public Procent OwnershipSecurity
        {
            get { return ownershipSecurity.Copy(); }
        }

        private readonly Money incomeTaxStaticticPoor = new Money(0f);
        public Money IncomeTaxStaticticPoor
        {
            get { return incomeTaxStaticticPoor.Copy(); }
        }

        private readonly Money incomeTaxStatisticRich = new Money(0f);
        public Money IncomeTaxStatisticRich
        {
            get { return incomeTaxStatisticRich.Copy(); }
        }
        private readonly Money incomeTaxForeigner = new Money(0f);
        public Money IncomeTaxForeigner
        {
            get { return incomeTaxForeigner.Copy(); }
        }

        private readonly Money goldMinesIncome = new Money(0f);
        public Money GoldMinesIncome { get { return goldMinesIncome.Copy(); } }

        private readonly Money ownedFactoriesIncome = new Money(0f);
        public Money OwnedFactoriesIncome { get { return ownedFactoriesIncome.Copy(); } }

        public Money RestIncome
        {
            get { return moneyIncomeThisTurn.Copy().Subtract(getIncome()); }
        }



        private readonly Money unemploymentSubsidiesExpense = new Money(0f);
        public Money UnemploymentSubsidiesExpense { get { return unemploymentSubsidiesExpense.Copy(); } }

        private readonly Money soldiersWageExpense = new Money(0f);
        public Money SoldiersWageExpense { get { return soldiersWageExpense.Copy(); } }

        private readonly Money factorySubsidiesExpense = new Money(0f);
        public Money FactorySubsidiesExpense { get { return factorySubsidiesExpense.Copy(); } }

        private readonly Money storageBuyingExpense = new Money(0f);
        public Money StorageBuyingExpense { get { return storageBuyingExpense.Copy(); } }

        private readonly float nameWeight;


        private TextMesh meshCapitalText;
        private Material borderMaterial;


        private readonly Modifier modXHasMyCores;
        public readonly ModifiersList modMyOpinionOfXCountry;
        public static readonly DoubleConditionsList canAttack = new DoubleConditionsList(new List<Condition>
    {
        new DoubleCondition((province, country)=>(province as Province).isNeighborButNotOwn(country as Country), x=>"Is neighbor province", true),
        //new ConditionForDoubleObjects((province, province)=>(province as Province).Country.government.getValue, x=>"Is neighbor province", false),
        new DoubleCondition((province, country)=>!Government.isDemocracy.checkIfTrue(country)
        || !Government.isDemocracy.checkIfTrue((province as Province).Country), x=>"Democracies can't attack each other", true),
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
        new Modifier(x=>(x as Country).GetAllPopulation().GetAverageProcent(y=>y.Education).RawUIntValue, "Education", 1f / Procent.Precision, false),
    });

        /// <summary>
        /// Don't call it directly, only from World.cs
        /// </summary>        
        public Country(string name, Culture culture, Color color, Province capital, float money) : base(money, null)
        {
            nameWeight = name.GetWeight();
            foreach (var each in Invention.getAll())
                inventions.Add(each, false);
            country = this;
            modXHasMyCores = new Modifier(x => (x as Country).hasCores(this), "You have my cores", -0.05f, false);
            modMyOpinionOfXCountry = new ModifiersList(new List<Condition> { modXHasMyCores,
            new Modifier(x=>(x as Country).government.getValue() != this.government.getValue(), "You have different form of government", -0.002f, false),
            new Modifier (x=>(x as Country).getLastAttackDateOn(this).getYearsSince() > Options.CountryTimeToForgetBattle
            && this.getLastAttackDateOn(x as Country).getYearsSince() > Options.CountryTimeToForgetBattle,"You live in peace with us", 0.005f, false),
            new Modifier (x=>(x as Country).getLastAttackDateOn(this).getYearsSince() > 0 &&  (x as Country).getLastAttackDateOn(this).getYearsSince() < 15,
            "Recently attacked us", -0.06f, false),
            new Modifier (x=> this.isThreatenBy(x as Country),"We are weaker", -0.05f, false),
            new Modifier (delegate(object x) {isThereBadboyCountry();  return isThereBadboyCountry()!= null && isThereBadboyCountry()!= x as Country  && isThereBadboyCountry()!= this; },
                delegate  { return "There is bigger threat to the world - " + isThereBadboyCountry(); },  0.05f, false),
            new Modifier (x=>isThereBadboyCountry() ==x,"You are very bad boy", -0.05f, false),
            new Modifier(x=>(x as Country).government.getValue() == this.government.getValue() && government.getValue()==Government.ProletarianDictatorship, "Comintern aka Third International", 0.2f, false),
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
            this.name = name;


            this.culture = culture;
            nationalColor = color;
            this.capital = capital;

            if (capital != null)
            {
                ownedProvinces.Add(capital);
                capital.OnSecedeTo(this, false);
                capital.setInitial(this);
            }
            //if (!Game.devMode)
            {

                //economy.setValue( Economy.NaturalEconomy);
                serfdom.setValue(Serfdom.Abolished);
                //government.setValue(Government.Tribal, false);

                government.setValue(Government.Aristocracy);
                //economy.setValue(Economy.StateCapitalism);
                taxationForRich.setValue(TaxationForRich.PossibleStatuses[2]);

                markInvented(Invention.Farming);

                markInvented(Invention.Banking);

                //markInvented(Invention.Universities);
                //markInvented(Invention.Manufactures);


                //markInvented(Invention.metal);
                //markInvented(Invention.individualRights);
                //markInvented(Invention.ProfessionalArmy);
                //markInvented(Invention.Welfare);

                //markInvented(Invention.Collectivism);
            }
        }
        private void ressurect(Province province, Government.ReformValue newGovernment)
        {
            alive = true;
            moveCapitalTo(province);
            government.setValue(newGovernment);
            setPrefix();
        }

        internal void onGrantedProvince(Province province)
        {
            var oldCountry = province.Country;
            changeRelation(oldCountry, 1.00f);
            oldCountry.changeRelation(this, 1.00f);
            if (!this.isAlive())
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
            this.ownedProvinces.Add(province);
            province.OnSecedeTo(this, addModifier);
            province.OnSecedeGraphic(this);

            //kill country or move capital
            if (oldCountry.ownedProvinces.Count == 0)
                oldCountry.OnKillCountry(this);
            else if (province == oldCountry.Capital)
                oldCountry.moveCapitalTo(oldCountry.getRandomOwnedProvince());


            government.onReformEnacted(province);
        }
        internal void onSeparatismWon(Country oldCountry)
        {
            foreach (var item in oldCountry.ownedProvinces.ToList())
                if (item.isCoreFor(this))
                {
                    TakeProvince(item, false);
                    //item.secedeTo(this, false);
                }
            ressurect(getRandomOwnedProvince(), this.government.getTypedValue());
            foreach (var item in oldCountry.getInvented()) // copying inventions
            {
                this.markInvented(item.Key);
            }
        }
        public static void setUnityAPI()
        {
            foreach (var country in World.getAllExistingCountries())
            {
                // can't do it before cause graphics isn't loaded
                if (country != World.UncolonizedLand)
                    country.moveCapitalTo(country.ownedProvinces[0]);
                //if (capital != null) // not null-country

                country.borderMaterial = new Material(Game.defaultCountryBorderMaterial) { color = country.nationalColor.getNegative() };
                //item.ownedProvinces[0].setBorderMaterial(Game.defaultProvinceBorderMaterial);
                country.ownedProvinces[0].setBorderMaterials(false);
                country.getAllProvinces().PerformAction(x => x.OnSecedeGraphic(x.Country));
            }
            World.UncolonizedLand.getAllProvinces().PerformAction(x => x.OnSecedeGraphic(World.UncolonizedLand));

        }

        internal int getSize()
        {
            return ownedProvinces.Count;
        }
        internal static int howMuchCountriesAlive()
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
                if (invention.Value == true && invention.Key.isAvailable(this))
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
                ((product == Product.Metal || product == Product.MetalOre || product == Product.ColdArms) && !this.Invented(Invention.Metal))
                || (!this.Invented(Invention.SteamPower) && (product == Product.Machinery || product == Product.Cement))
                || ((product == Product.Artillery || product == Product.Ammunition) && !this.Invented(Invention.Gunpowder))
                || (product == Product.Firearms && !this.Invented(Invention.Firearms))
                || (product == Product.Coal && !this.Invented(Invention.Coal))
                //|| (product == Cattle && !country.isInvented(Invention.Domestication))
                || (!this.Invented(Invention.CombustionEngine) && (product == Product.Oil || product == Product.MotorFuel || product == Product.Rubber || product == Product.Cars))
                || (!this.Invented(Invention.Tanks) && product == Product.Tanks)
                || (!this.Invented(Invention.Airplanes) && product == Product.Airplanes)
                || (product == Product.Tobacco && !this.Invented(Invention.Tobacco))
                || (product == Product.Electronics && !this.Invented(Invention.Electronics))
                //|| (!isResource() && !country.isInvented(Invention.Manufactories))
                || (product == Product.Education && !this.Invented(Invention.Universities))
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
        internal void setPrefix()
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
        internal void setSoldierWage(float value)
        {
            soldiersWage.Set(value);
        }
        internal float getSoldierWage()
        {
            return soldiersWage.get();
        }
        //public Procent GetAveragePop(Func<PopUnit, Procent> selector)
        //{
        //    Procent result = new Procent(0f);
        //    int calculatedPopulation = 0;
        //    foreach (var province in ownedProvinces)
        //        foreach (var pop in province.allPopUnits)
        //        {
        //            result.AddPoportionally(calculatedPopulation, pop.getPopulation(), selector(pop));
        //            calculatedPopulation += pop.getPopulation();
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
        //            result.addPoportionally(calculatedPopulation, pop.getPopulation(), pop.loyalty);
        //            calculatedPopulation += pop.getPopulation();
        //        }
        //    return result;
        //}
        //internal Procent getAverageNeedsFulfilling()
        //{
        //    Procent result = new Procent(0f);
        //    int calculatedPopulation = 0;
        //    foreach (var province in ownedProvinces)
        //        foreach (var pop in province.allPopUnits)
        //        {
        //            result.addPoportionally(calculatedPopulation, pop.getPopulation(), pop.needsFulfilled);
        //            calculatedPopulation += pop.getPopulation();
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
        //todo performance hit 132 calls 183kb 82 ms
        private bool isThreatenBy(Country country)
        {
            if (country == this)
                return false;
            if (country.getStrengthExluding(null) > this.getStrengthExluding(null) * 2)
                return true;
            else
                return false;
        }

        public Date getLastAttackDateOn(Country country)
        {
            if (myLastAttackDate.ContainsKey(country))
                return myLastAttackDate[country];
            else
                return Date.Never;
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
        internal void OnKillCountry(Country byWhom)
        {
            if (meshCapitalText != null) //todo WTF!!
                UnityEngine.Object.Destroy(meshCapitalText.gameObject);

            //take all money from bank
            if (byWhom.Invented(Invention.Banking))
                byWhom.Bank.Annex(this.Bank); // deposits transfered in province.OnSecede()
            else
                this.Bank.destroy(byWhom);

            //byWhom.storageSet.
            this.PayAllAvailableMoney(byWhom);
            this.Bank.OnLoanerRefusesToPay(this);
            countryStorageSet.sendAll(byWhom.countryStorageSet);
            //foreach (var item in World.GetAllShares(this).ToList())// transfer all enterprises to local governments            
            foreach (var item in World.GetAllFactories())// transfer all enterprises to local governments            
                if (item.ownership.HasOwner(this))
                    item.ownership.TransferAll(this, item.Country);

            if (this.IsHuman)
                Message.NewMessage("Disaster!!", "It looks like we lost our last province\n\nMaybe we would rise again?", "Okay", false);
            alive = false;

            SetStatisticToZero();
        }

        public Province Capital
        {
            get { return capital; }
        }

        override internal void sendArmy(Province target, Procent procent)
        {
            base.sendArmy(target, procent);
            //myLastAttackDate.AddMy(target.Country, Game.date);
            if (this.myLastAttackDate.ContainsKey(target.Country))
                myLastAttackDate[target.Country].set(Date.Today);
            else
                myLastAttackDate.Add(target.Country, Date.Today);


        }

        //internal bool canAttack(Province province)
        //{
        //    //!province.isBelongsTo(this) &&
        //    return province.isNeighbor(this);
        //}

        internal List<Province> getNeighborProvinces()
        {
            List<Province> result = new List<Province>();
            foreach (var province in ownedProvinces)
                result.AddRange(
                    province.getAllNeigbors().Where(p => p.Country != this && !result.Contains(p))
                    );
            return result;
        }
        private bool isOnlyCountry()
        {
            if (World.getAllExistingCountries().Any(x => x != this))
                return false;
            else
                return true;
        }

        internal Province getRandomOwnedProvince()
        {
            return ownedProvinces.Random();
        }
        internal Province getRandomOwnedProvince(Predicate<Province> predicate)
        {
            return ownedProvinces.Random(predicate);
        }

        internal void setCapitalTextMesh(Province province)
        {
            Transform txtMeshTransform = GameObject.Instantiate(Game.r3dTextPrefab).transform;
            txtMeshTransform.SetParent(province.getRootGameObject().transform, false);

            Vector3 capitalTextPosition = province.getPosition();
            capitalTextPosition.y += 2f;
            capitalTextPosition.z -= 5f;
            txtMeshTransform.position = capitalTextPosition;

            meshCapitalText = txtMeshTransform.GetComponent<TextMesh>();
            meshCapitalText.text = FullName;
            meshCapitalText.fontSize *= 2;
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
        internal void moveCapitalTo(Province newCapital)
        {
            if (meshCapitalText == null)
                setCapitalTextMesh(newCapital);
            else
            {
                Vector3 capitalTextPosition = newCapital.getPosition();
                capitalTextPosition.y += 2f;
                capitalTextPosition.z -= 5f;
                meshCapitalText.transform.position = capitalTextPosition;
            }
            capital = newCapital;
        }
        internal Color getColor()
        {
            return nationalColor;
        }
        internal Procent getYesVotes(AbstractReformValue reform, ref Procent procentPopulationSayedYes)
        {
            // calculate how much of population wants selected reform
            int totalPopulation = this.GetAllPopulation().Sum(x => x.getPopulation());
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
                            votersSayedYes += pop.getPopulation();// * pop.getVotingPower();
                            populationSayedYes += pop.getPopulation();// * pop.getVotingPower();
                        }
                        votingPopulation += pop.getPopulation();// * pop.getVotingPower();
                    }
                    else
                    {
                        if (pop.getSayingYes(reform))
                            populationSayedYes += pop.getPopulation();// * pop.getVotingPower();
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

        internal Material getBorderMaterial()
        {
            return borderMaterial;
        }

        /// <summary>
        /// Not finished, don't use it
        /// </summary>
        /// <param name="reform"></param>   
        internal Procent getYesVotes2(AbstractReformValue reform, ref Procent procentPopulationSayedYes)
        {
            int totalPopulation = this.GetAllPopulation().Sum(x => x.getPopulation());
            int votingPopulation = 0;
            int populationSayedYes = 0;
            int votersSayedYes = 0;
            Procent procentVotersSayedYes = new Procent(0f);
            Dictionary<PopType, int> divisionPopulationResult = new Dictionary<PopType, int>();
            Dictionary<PopType, int> divisionVotersResult = this.getYesVotesByType(reform, ref divisionPopulationResult);
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
        internal Dictionary<PopType, int> getYesVotesByType(AbstractReformValue reform, ref Dictionary<PopType, int> divisionPopulationResult)
        {  // division by pop types
            Dictionary<PopType, int> divisionVotersResult = new Dictionary<PopType, int>();
            foreach (PopType type in PopType.getAllPopTypes())
            {
                divisionVotersResult.Add(type, 0);
                divisionPopulationResult.Add(type, 0);
                foreach (Province province in this.ownedProvinces)
                {
                    foreach (PopUnit pop in province.GetAllPopulation(type))
                        if (pop.getSayingYes(reform))
                        {
                            divisionPopulationResult[type] += pop.getPopulation();// * pop.getVotingPower();
                            if (pop.canVote())
                                divisionVotersResult[type] += pop.getPopulation();// * pop.getVotingPower();
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
        internal Money getMinSalary()
        {
            var res = (minimalWage.getValue() as MinimalWage.ReformValue).getWage();
            if (res.isZero())
                res.set(Options.FactoryMinPossibleSallary);
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
        override public string ToString()
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
                    var newFactory = province.BuildFactory(this, propositionFactory, Game.market.getCost(buildNeeds));
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
                        if (proposition.canBuildNewFactory(province, this) || province.canUpgradeFactory(proposition))
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
        internal void invest(Province province)
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
                if (Game.Random.Next(10) == 1)
                {
                    var thisStrength = this.getStrengthExluding(null);
                    var possibleTarget = getNeighborProvinces().MinBy(x => getRelationTo(x.Country).get());
                    if (possibleTarget != null
                        && (getRelationTo(possibleTarget.Country).get() < 1f || Game.Random.Next(200) == 1)
                        && thisStrength > 0
                        && (this.getAverageMorale().get() > 0.5f || getAllArmiesSize() == 0)
                        && (thisStrength > possibleTarget.Country.getStrengthExluding(null) * 0.25f
                            || possibleTarget.Country == World.UncolonizedLand
                            || possibleTarget.Country.isAI() && this.getStrengthExluding(null) > possibleTarget.Country.getStrengthExluding(null) * 0.1f)
                        && Country.canAttack.isAllTrue(possibleTarget, this)
                        )
                    {
                        mobilize(ownedProvinces);
                        sendArmy(possibleTarget, Procent.HundredProcent);
                    }
                }
            if (Game.Random.Next(90) == 1)
                aiInvent();
            // changing salary for soldiers
            if (economy.getValue() != Economy.PlannedEconomy)
                if (Invented(Invention.ProfessionalArmy) && Game.Random.Next(10) == 1)
                {
                    float newWage;
                    var soldierAllNeedsCost = Game.market.getCost(PopType.Soldiers.getAllNeedsPer1000Men()).get();
                    if (failedToPaySoldiers)
                    {
                        newWage = getSoldierWage() - getSoldierWage() * 0.2f;
                    }
                    else
                    {
                        var balance = getBalance();

                        if (balance > 200f)
                            newWage = getSoldierWage() + soldierAllNeedsCost * 0.002f + 1f;
                        else if (balance > 50f)
                            newWage = getSoldierWage() + soldierAllNeedsCost * 0.0005f + 0.1f;
                        else if (balance < -800f)
                            newWage = 0.0f;
                        else if (balance < 0f)
                            newWage = getSoldierWage() - getSoldierWage() * 0.5f;
                        else
                            newWage = getSoldierWage(); // don't change wage
                    }
                    newWage = Mathf.Clamp(newWage, 0, soldierAllNeedsCost * 2f);
                    setSoldierWage(newWage);
                }
            // dealing with enterprises
            if (economy.getValue() == Economy.Interventionism)
                Rand.Call(() => getAllFactories().PerformAction(
                    x => x.ownership.HowMuchOwns(this).Copy().Subtract(x.ownership.HowMuchSelling(this)).isBiggerOrEqual(Procent._50Procent),
                    x => x.ownership.SetToSell(this, Options.PopBuyAssetsAtTime)),
                    30);
            else
            //State Capitalism invests in own country only, Interventionists. don't invests in any country
            if (economy.getValue() == Economy.StateCapitalism)
                Rand.Call(
                    () =>
                    {
                        // copied from Capitalist.Invest()
                        // doesn't care about risks
                        var project = GetAllInvestmentProjects(this).Where(x => x.GetMargin().isBiggerThan(Options.minMarginToInvest)).MaxByRandom(x => x.GetMargin().get());
                        if (project != null)
                        {
                            Value investmentCost = project.GetInvestmentCost();
                            if (!CanPay(investmentCost))
                                Bank.GiveLackingMoneyInCredit(this, investmentCost);
                            if (CanPay(investmentCost))
                            {
                                Factory factory = project as Factory;
                                if (factory != null)
                                {
                                    if (factory.IsOpen)// upgrade existing factory
                                        factory.upgrade(this);
                                    else
                                        factory.open(this, true);
                                }
                                else
                                {
                                    Owners buyShare = project as Owners;
                                    if (buyShare != null) // buy part of existing factory
                                        buyShare.BuyStandardShare(this);
                                    else
                                    {
                                        var factoryProject = project as NewFactoryProject;
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
        internal void simulate()
        {
            // military staff
            base.simulate();

            ownershipSecurity.Add(Options.CountryOwnershipRiskRestoreSpeed, false);
            ownershipSecurity.clamp100();
            // get science points            
            sciencePoints.Add(Options.defaultSciencePointMultiplier * modSciencePoints.getModifier(this));

            // put extra money in bank
            if (economy.getValue() != Economy.PlannedEconomy)
                if (this.autoPutInBankLimit > 0f)
                {
                    float extraMoney = Cash.get() - (float)this.autoPutInBankLimit;
                    if (extraMoney > 0f)
                        Bank.ReceiveMoney(this, new Value(extraMoney));
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
                item.simulate();
            if (economy.getValue() == Economy.LaissezFaire)
                Rand.Call(() => getAllFactories().PerformAction(x => x.ownership.SetToSell(this, Procent.HundredProcent, false)), 30);
        }



        private void aiInvent()
        {
            var invention = GetAllUninvented().ToList().Random(x => this.sciencePoints.isBiggerOrEqual(x.Key.getCost()));
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
                        sell(howMuchToSell);
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
                        sell(toSell);//go sell
                    }
                }
            }
        }
        /// <summary>
        /// Represents buying and/or consuming needs.
        /// </summary>
        public override void consumeNeeds()
        {
            foreach (var item in getAllArmies())
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

        void buyNeeds(Storage toBuy)
        {
            Storage realyBougth = Game.market.buy(this, toBuy, null);
            if (realyBougth.isNotZero())
            {
                countryStorageSet.Add(realyBougth);
                storageBuyingExpenseAdd(Game.market.getCost(realyBougth));
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
        //    int population = item.GetAllPopulation().Where(x => x.Type.canBeUnemployed()).Sum(x=>x.getPopulation());
        //    result.AddPoportionally(calculatedBase, population, item.getUnemployment(PopType.All));
        //    //result.AddPoportionally(calculatedBase,(float) population, item.GetAllPopulation().Sum(x=>x.getUnemployedProcent().get()));
        //    calculatedBase += population;
        //}
        //return result;
        //}
        //internal int getMenPopulation()
        //{
        //    int result = 0;
        //    foreach (Province pr in ownedProvinces)
        //        result += pr.getMenPopulation();
        //    return result;
        //}
        internal int getFamilyPopulation()
        {
            //return  getMenPopulation() * Options.familySize;
            return GetAllPopulation().Sum(x => x.getPopulation()) * Options.familySize;
        }
        public int getPopulationAmountByType(PopType ipopType)
        {
            int result = 0;
            foreach (Province province in ownedProvinces)
                result += province.GetAllPopulation(ipopType).Sum(x => x.getPopulation());
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
        public IEnumerable<Province> getAllProvinces()
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
        public Value getGDP()
        {
            Value result = new Value(0);
            foreach (var province in ownedProvinces)
            {
                result.Add(province.getGDP());
            }
            return result;
        }
        internal float getGDPPer1000()
        {
            return getGDP().get() * 1000f / getFamilyPopulation();
            // overflows
            //res.multiply(1000);
            //res.divide(getFamilyPopulation());

            //return res;
        }
        static private int ValueOrder(KeyValuePair<Country, Value> x, KeyValuePair<Country, Value> y)
        {
            float sumX = x.Value.get();
            float sumY = y.Value.get();
            return sumY.CompareTo(sumX); // bigger - first
        }
        static private int ProcentOrder(KeyValuePair<Culture, Procent> x, KeyValuePair<Culture, Procent> y)
        {
            float sumX = x.Value.get();
            float sumY = y.Value.get();
            return sumY.CompareTo(sumX); // bigger - first
        }
        static private int FloatOrder(KeyValuePair<Country, float> x, KeyValuePair<Country, float> y)
        {
            float sumX = x.Value;
            float sumY = y.Value;
            return sumY.CompareTo(sumX); // bigger - first
        }
        static private int IntOrder(KeyValuePair<Country, int> x, KeyValuePair<Country, int> y)
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
            var list = new List<KeyValuePair<Country, Value>>();
            foreach (var item in World.getAllExistingCountries())
            {
                list.Add(new KeyValuePair<Country, Value>(item, item.getGDP()));
            }
            list.Sort(ValueOrder);
            return list.FindIndex(x => x.Key == this) + 1; // starts with zero;
        }
        /// <summary>
        /// Returns 0 if failed
        /// </summary>
        public int getGDPPer1000Rank()
        {
            var list = new List<KeyValuePair<Country, float>>();
            foreach (var item in World.getAllExistingCountries())
            {
                list.Add(new KeyValuePair<Country, float>(item, item.getGDPPer1000()));
            }
            list.Sort(FloatOrder);
            return list.FindIndex(x => x.Key == this) + 1; // starts with zero
        }
        public Procent getGDPShare()
        {
            Value worldGDP = new Value(0f);
            foreach (var item in World.getAllExistingCountries())
            {
                worldGDP.Add(item.getGDP());
            }
            return new Procent(this.getGDP(), worldGDP);
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
        public List<KeyValuePair<Culture, Procent>> getCultures()
        {
            var cultures = new Dictionary<Culture, int>();
            var totalPopulation = GetAllPopulation().Sum(x => x.getPopulation());
            foreach (var item in GetAllPopulation())
            {
                cultures.AddMy(item.culture, item.getPopulation());
            }
            var result = new List<KeyValuePair<Culture, Procent>>();
            foreach (var item in cultures)
            {
                result.Add(new KeyValuePair<Culture, Procent>(item.Key, new Procent(item.Value, totalPopulation)));
            }
            result.Sort(ProcentOrder);
            return result;
        }
        //****************************
        override public void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            incomeTaxForeigner.SetZero();
            countryStorageSet.SetStatisticToZero();
            failedToPaySoldiers = false;
            incomeTaxStaticticPoor.Set(0f);
            incomeTaxStatisticRich.Set(0f);
            goldMinesIncome.Set(0f);
            unemploymentSubsidiesExpense.Set(0f);
            ownedFactoriesIncome.Set(0f);
            factorySubsidiesExpense.Set(0f);
            storageBuyingExpense.Set(0f);
            soldiersWageExpense.SetZero();
        }

        internal float getBalance()
        {
            return moneyIncomeThisTurn.get() - getExpenses().get();
        }

        internal Value getExpenses()
        {
            Value result = new Value(0f);
            result.Add(unemploymentSubsidiesExpense);
            result.Add(factorySubsidiesExpense);
            result.Add(storageBuyingExpense);
            result.Add(soldiersWageExpense);
            return result;
        }
        internal Money getIncome()
        {
            Money result = new Money(0f);
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
        internal bool GiveFactorySubsidies(Consumer byWhom, Value howMuch)
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
        internal void soldiersWageExpenseAdd(Value payCheck)
        {
            soldiersWageExpense.Add(payCheck);
        }


        //internal float getPoorTaxIncome()
        //{
        //    return poorTaxIncome.get();
        //}

        //internal float getRichTaxIncome()
        //{
        //    return richTaxIncome.get();
        //}


        /// <summary>
        /// Forces payer to pay tax from taxable. Returns how much payed (new value)
        /// Don't call it manually, it called from Agent.Pay() automatically
        /// </summary>                
        internal ReadOnlyValue TakeIncomeTaxFrom(Agent taxPayer, Value taxable, bool isPoorStrata)
        {
            var pop = taxPayer as PopUnit;
            if (pop != null
                && pop.Type == PopType.Aristocrats
                //&& Serfdom.IsNotAbolishedInAnyWay.checkIfTrue(Country))
                && government.getTypedValue() == Government.Aristocracy)
                return ReadOnlyValue.Zero; // don't pay with monarchy
            Procent tax;
            Value statistics;
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
                var hadMoney = taxPayer.Cash.Copy();
                taxPayer.incomeTaxPayed.Add(taxPayer.Cash);
                statistics.Add(taxPayer.Cash);
                moneyIncomeThisTurn.Add(taxPayer.Cash);
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
        internal void goldMinesIncomeAdd(ReadOnlyValue toAdd)
        {
            goldMinesIncome.Add(toAdd);
        }
        internal void unemploymentSubsidiesExpenseAdd(ReadOnlyValue toAdd)
        {
            unemploymentSubsidiesExpense.Add(toAdd);
        }
        internal void storageBuyingExpenseAdd(ReadOnlyValue toAdd)
        {
            storageBuyingExpense.Add(toAdd);
        }
        internal void ownedFactoriesIncomeAdd(ReadOnlyValue toAdd)
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
        internal AbstractReform getReform(AbstractReformValue abstractReformValue)
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
        internal void annexTo(Country country)
        {
            foreach (var item in ownedProvinces.ToList())
            {
                country.TakeProvince(item, false);
                //item.secedeTo(country, false);
            }
        }
        public IEnumerable<IInvestable> GetAllInvestmentProjects(Agent investor)
        {
            foreach (var province in ownedProvinces)
                foreach (var item in province.getAllInvestmentProjects(investor))
                {
                    yield return item;
                }

        }
        internal void Nationilize(Factory factory)
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
    }
}
