using Nashet.Conditions;
using Nashet.EconomicSimulation.Reforms;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nashet.EconomicSimulation
{
    public class Country : MultiSeller, IClickable, IShareOwner, ISortableName, INameable, IProvinceOwner, IInventor, IDiplomat
    {
        public readonly Government government;
        public readonly Economy economy;
        public readonly Serfdom serfdom;
        public readonly MinimalWage minimalWage;
        public readonly UnemploymentSubsidies unemploymentSubsidies;
        public readonly TaxationForPoor taxationForPoor;
        public readonly TaxationForRich taxationForRich;

        public readonly UBI UBI;
        public readonly PovertyAid PovertyAid;
        public readonly FamilyPlanning FamilyPlanning;

        public readonly MinorityPolicy minorityPolicy;

        public readonly UIEvents events;

        /// <summary> could be null</summary>
        private readonly Bank bank;

        public Bank Bank { get { return bank; } }
        public Market market;

        /// <summary>Encapsulates ability to own provinces </summary>
        public readonly ProvinceOwner Provinces;

        public Science Science { get; protected set; }
        public Diplomacy Diplomacy { get; protected set; }
        public Politics Politics { get; protected set; }

        /// <summary>
        /// Gives list of allowed IInvestable with pre-calculated Margin in Value. Doesn't check if it's invented
        /// </summary>
        public readonly CashedData<Dictionary<IInvestable, Procent>> allInvestmentProjects;

        private string name;
        public Culture Culture { get; protected set; }
        public Color NationalColor { get; protected set; }
        private Province capital;
        public bool IsAlive { get; protected set; } = true;

        private readonly Money soldiersWage = new Money(0m);

        //public bool failedToPaySoldiers;
        public Money autoPutInBankLimit = new Money(2000);

        private readonly Procent ownershipSecurity = Procent.HundredProcent.Copy();

        /// <summary> Read only, new value</summary>
        public Procent OwnershipSecurity
        {
            get { return ownershipSecurity.Copy(); }
        }

        private float nameWeight;

        private TextMesh meshCapitalText;
        private Material borderMaterial;

        public readonly ModifiersList modMyOpinionOfXCountry;
        private readonly Modifier modXHasMyCores;

        /// <summary>
        /// Don't call it directly, only from World.cs
        /// </summary>
        public Country(string name, Culture culture, Color color, Province capital, float money) : base(money, null)
        {
            events = new UIEvents(this);
            FailedPayments.Enable();

            Provinces = new ProvinceOwner(this);
            Science = new Science(this);
            Diplomacy = new Diplomacy(this);
            Politics = new Politics(this);

            allInvestmentProjects = new CashedData<Dictionary<IInvestable, Procent>>(Provinces.GetAllInvestmentProjects2);
            SetName(name);

            Country = this;
            market = Market.TemporalSingleMarket; // new Market();

            bank = new Bank(this);


            taxationForPoor = new TaxationForPoor(this, 0);
            taxationForRich = new TaxationForRich(this, 1);

            minimalWage = new MinimalWage(this, 4);
            unemploymentSubsidies = new UnemploymentSubsidies(this, 8);

            serfdom = new Serfdom(this, 7);
            minorityPolicy = new MinorityPolicy(this, 5);


            FamilyPlanning = new FamilyPlanning(this, 6);
            UBI = new UBI(this, 10);
            PovertyAid = new PovertyAid(this, 9);

            economy = new Economy(this, 2);
            government = new Government(this, 3);


            Culture = culture;
            NationalColor = color;
            this.capital = capital;

            if (capital != null)
            {
                Provinces.TakeProvince(capital, false);

                capital.AddCore(this);
            }


            //economy.setValue( Econ.NaturalEconomy);
            serfdom.SetValue(Serfdom.Abolished);
            //government.setValue(Government.Tribal, false);

            government.SetValue(Government.Aristocracy);
            //economy.setValue(Econ.StateCapitalism);
            taxationForRich.SetValue(Government.Aristocracy.defaultRichTax);

            Science.Invent(Invention.Farming);

            Science.Invent(Invention.Banking);

            //markInvented(Invention.individualRights);
            //markInvented(Invention.ProfessionalArmy);
            //markInvented(Invention.Welfare);

            //markInvented(Invention.Collectivism);
            modXHasMyCores = new Modifier(x => (x as Country).Provinces.HasCore(this), "You have my cores", -0.05f, false);

            modMyOpinionOfXCountry = new ModifiersList(new List<Condition> { modXHasMyCores,
            new Modifier(x=>(x as Country).government != this.government, "You have different form of government", -0.002f, false),
            new Modifier (x=>(x as Country).Diplomacy.GetLastAttackDateOn(this).getYearsSince() > Options.CountryTimeToForgetBattle
            && Diplomacy.GetLastAttackDateOn(x as Country).getYearsSince() > Options.CountryTimeToForgetBattle,"You live in peace with us", 0.005f, false),
            new Modifier (x=>!((x as Country).Diplomacy.GetLastAttackDateOn(this).getYearsSince() > Options.CountryTimeToForgetBattle) && (x as Country).Diplomacy.GetLastAttackDateOn(this).getYearsSince() < 15,
            "Recently attacked us", -0.06f, false), //x=>(x as Country).getLastAttackDateOn(this).getYearsSince() > 0
            new Modifier (x=> isThreatenBy(x as Country),"We are weaker", -0.05f, false),
            new Modifier (delegate(object x) {World.GetBadboyCountry();  return World.GetBadboyCountry()!= null && World.GetBadboyCountry()!= x as Country  && World.GetBadboyCountry()!= this; },
                delegate  { return "There is bigger threat to the world - " + World.GetBadboyCountry(); },  0.05f, false),
            new Modifier (x=>World.GetBadboyCountry() ==x,"You are very bad boy", -0.05f, false),
            new Modifier(x=>(x as Country).government == this.government && government==Government.ProletarianDictatorship,
            "Comintern aka Third International", 0.2f, false)
            });
        }

        public void SetName(string name)
        {
            nameWeight = name.GetWeight();
            this.name = name;
        }

        private void ressurect(Province capital, Government newGovernment)
        {
            IsAlive = true;
            MoveCapitalTo(capital);
            government.SetValue(newGovernment);
            setPrefix();
        }

        public void onGrantedProvince(Province province)
        {
            var oldCountry = province.Country;
            Diplomacy.ChangeRelation(oldCountry, 1.00f);
            oldCountry.Diplomacy.ChangeRelation(this, 1.00f);
            if (!IsAlive)
            {
                ressurect(province, oldCountry.government);
            }
            //province.secedeTo(this, false);
            Provinces.TakeProvince(province, false);
        }

        public Province BestCapitalCandidate()
        {
            var newCapital = Provinces.AllProvinces.Where(x => x.isCoreFor(this)).MaxBy(x => x.getFamilyPopulation());
            if (newCapital == null)
                newCapital = Provinces.AllProvinces.Where(x => x.getMajorCulture() == this.Culture).MaxBy(x => x.getFamilyPopulation());
            if (newCapital == null)
                newCapital = Provinces.AllProvinces.Random();
            return newCapital;
        }

        public void onSeparatismWon(Country oldCountry)
        {
            foreach (var item in oldCountry.Provinces.AllProvinces.ToList())
                if (item.isCoreFor(this))
                {
                    Provinces.TakeProvince(item, false);
                    //item.secedeTo(this, false);
                }
            ressurect(BestCapitalCandidate(), government);
            foreach (var item in oldCountry.Science.AllInvented()) // copying inventions
            {
                Science.Invent(item);
            }
        }

        public static void setUnityAPI()
        {
            foreach (var country in World.AllExistingCountries())
            {
                // can't do it before cause graphics isn't loaded
                if (country != World.UncolonizedLand)
                    country.MoveCapitalTo(country.AllProvinces.First());
                //if (capital != null) // not null-country

                country.borderMaterial = new Material(LinksManager.Get.defaultCountryBorderMaterial) { color = country.NationalColor.getNegative() };
                //item.ownedProvinces[0].setBorderMaterial(Game.defaultProvinceBorderMaterial);
                foreach (var province in country.AllProvinces)
                {
                    province.SetBorderMaterials();
                }
                country.Provinces.AllProvinces.PerformAction(x => x.OnSecedeGraphic(x.Country));
                country.Flag = Nashet.Flag.Generate(128, 128);
            }
            World.UncolonizedLand.Provinces.AllProvinces.PerformAction(x => x.OnSecedeGraphic(World.UncolonizedLand));
        }

        public void setPrefix()
        {
            if (meshCapitalText != null)
                meshCapitalText.text = FullName;
        }

        public void setSoldierWage(MoneyView value)
        {
            soldiersWage.Set(value);
        }

        public MoneyView getSoldierWage()
        {
            return soldiersWage;
        }

        /// <summary>
        /// Also transfers enterprises to local governments
        /// </summary>
        public void OnKillCountry(Country byWhom)
        {
            if (meshCapitalText != null) //todo WTF!!
                UnityEngine.Object.Destroy(meshCapitalText.gameObject);

            if (this != Game.Player)
            {
                //take all money from bank
                if (byWhom.Science.IsInvented(Invention.Banking))
                    byWhom.Bank.Annex(Bank); // deposits transfered in province.OnSecede()
                else
                    Bank.destroy(byWhom);

                //byWhom.storageSet.

                PayAllAvailableMoney(byWhom, Register.Account.Rest);

                Bank.OnLoanerRefusesToPay(this);
            }
            countryStorageSet.sendAll(byWhom.countryStorageSet);
            //foreach (var item in World.GetAllShares(this).ToList())// transfer all enterprises to local governments
            foreach (var item in World.AllFactories)// transfer all enterprises to local governments
                if (item.ownership.HasOwner(this))
                    item.ownership.TransferAll(this, item.Country);

            if (IsHuman)
                MessageSystem.Instance.NewMessage("Disaster!!", "It looks like we lost our last province\n\nMaybe we would rise again?", "Okay", false, capital.Position);
            IsAlive = false;

            SetStatisticToZero();
        }

        public Province Capital
        {
            get { return capital; }
        }

        private bool isOnlyCountry()
        {
            if (World.AllExistingCountries().Any(x => x != this))
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

        public Material getBorderMaterial()
        {
            return borderMaterial;
        }

        /// <summary>
        /// returns new value
        /// </summary>
        public MoneyView getMinSalary()
        {
            var res = minimalWage.WageSize.Get();
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
                    return name + " " + government.Prefix + " (you are)";
                else
                    return name + " " + government.Prefix;
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
                if (Science.IsInvented(item))
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

        public void invest(Province province)
        {
            if (economy == Economy.PlannedEconomy && Science.IsInvented(Invention.Manufactures))
                if (!province.isThereFactoriesInUpgradeMoreThan(1)//Options.maximumFactoriesInUpgradeToBuildNew)
                    && province.getUnemployedWorkers() > 0)
                {
                    var industrialProduct = getMostDeficitProductAllowedHere(Product.AllNonAbstract().Where(x => x.isIndustrial() && Science.IsInvented(x)), province);
                    if (industrialProduct == null)
                    {
                        var militaryProduct = getMostDeficitProductAllowedHere(Product.AllNonAbstract().Where(x => x.isMilitary() && Science.IsInvented(x)), province);
                        if (militaryProduct == null)
                        {
                            var consumerProduct = getMostDeficitProductAllowedHere(Product.AllNonAbstract().Where(x => x.isConsumerProduct() && Science.IsInvented(x)), province);
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
                            var targetCountry = Provinces.AllNeighborCountries().Distinct()
                                .Where(x => Diplomacy.GetRelationTo(x).get() < 0.9f || Rand.Get.Next(200) == 1)
                                .MinBy(x => Diplomacy.GetRelationTo(x.Country).get());


                            var targetPool = Provinces.AllNeighborProvinces().Distinct().Where(x => x.Country == targetCountry).ToList();
                            var targetProvince = targetPool.Where(x => x.isCoreFor(this)).FirstOrDefault();
                            if (targetProvince == null)
                                targetProvince = targetPool.Where(x => x.getMajorCulture() == this.Culture).FirstOrDefault();
                            if (targetProvince == null)
                                targetProvince = targetPool.Random();

                            if (targetProvince != null
                            && (thisStrength > targetProvince.Country.getStrengthExluding(null) * 0.25f
                                || targetProvince.Country == World.UncolonizedLand
                                || targetProvince.Country.isAI() && getStrengthExluding(null) > targetProvince.Country.getStrengthExluding(null) * 0.1f)
                            && Diplomacy.canAttack.isAllTrue(targetProvince, this)
                            && (targetProvince.Country.isAI() || Options.AIFisrtAllowedAttackOnHuman.isPassed())
                            )
                            {
                                mobilize(Provinces.AllProvinces);
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
            if (economy != Economy.PlannedEconomy)
                if (Science.IsInvented(Invention.ProfessionalArmy) && Rand.Get.Next(10) == 1)
                {
                    Money newWage;
                    Money soldierAllNeedsCost = Country.market.getCost(PopType.Soldiers.getAllNeedsPer1000Men()).Copy();
                    if (Register.Account.Wage.GetIncomeAccount(FailedPayments).isZero())//didn't failedToPaySoldiers
                    {
                        var balance = Register.Balance;

                        if (balance > 200m)
                            newWage = getSoldierWage().Copy().Add(soldierAllNeedsCost.Copy().Multiply(0.002m).Add(1m));
                        else if (balance > 50m)
                            newWage = getSoldierWage().Copy().Add(soldierAllNeedsCost.Copy().Multiply(0.0005m).Add(0.1m));
                        else if (balance < -800m)
                            newWage = new Money(0m);
                        else if (balance < 0m)
                            newWage = getSoldierWage().Copy().Multiply(0.5m);
                        else
                            newWage = getSoldierWage().Copy(); // don't change wage
                    }
                    else
                    {
                        newWage = getSoldierWage().Copy().Multiply(0.8m);
                        //getSoldierWage().Get() - getSoldierWage().Get() * 0.2m;                        
                    }
                    //newWage = newWage.Clamp(0, soldierAllNeedsCost * 2m);
                    var limit = soldierAllNeedsCost.Copy().Multiply(2m);
                    if (newWage.isBiggerThan(limit))
                        newWage.Set(limit);
                    setSoldierWage(newWage);
                }
            // dealing with enterprises
            if (economy == Economy.Interventionism)
                Rand.Call(() => Provinces.AllFactories.Where(
                    x => x.ownership.HowMuchOwns(this).Copy().Subtract(x.ownership.HowMuchSelling(this))
                    .isBiggerOrEqual(Procent._50Procent)).PerformAction(
                    x => x.ownership.SetToSell(this, Options.PopBuyAssetsAtTime)),
                    30);
            else
            //State Capitalism invests in own country only, Interventionists don't invests in any country
            if (economy == Economy.StateCapitalism)
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
                                    return Science.IsInventedFactory(isFactory.Type);
                                else
                                {
                                    var newFactory = x.Key as NewFactoryProject;
                                    if (newFactory != null)
                                        return Science.IsInventedFactory(newFactory.Type);
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
                                            PayWithoutRecord(factory2, investmentCost, Register.Account.Construction);
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
            if (Game.devMode)
                Science.AddPoints(Options.defaultSciencePointMultiplier * Science.modSciencePoints.getModifier(this) * 1000);
            else
                Science.AddPoints(Options.defaultSciencePointMultiplier * Science.modSciencePoints.getModifier(this));

            // put extra money in bank
            if (economy != Economy.PlannedEconomy)
                if (autoPutInBankLimit.isNotZero())
                {
                    var extraMoney = Cash.Copy().Subtract(autoPutInBankLimit, false);
                    //float extraMoney = Cash.get() - (float)this.autoPutInBankLimit;
                    if (extraMoney.isNotZero())
                        Bank.ReceiveMoney(this, extraMoney);
                }

            //todo performance - do it not every tick?
            //International opinion;
            foreach (var item in World.AllExistingCountries())
                if (item != this)
                {
                    Diplomacy.ChangeRelation(item, modMyOpinionOfXCountry.getModifier(item));
                }

            Politics.Simulate();

            if (economy == Economy.LaissezFaire)
                Rand.Call(() => Provinces.AllFactories.PerformAction(x => x.ownership.SetToSell(this, Procent.HundredProcent, false)), 30);
        }

        private void aiInvent()
        {
            var invention = Science.AllUninvented().Where(x => Science.Points >= x.Cost.get()).Random();//.ToList()
            if (invention != null)
                Science.Invent(invention);
        }

        private void tradeNonPE(bool usePlayerTradeSettings)//, int buyProductsForXDays)
        {
            // firstly, buy last tick expenses -NO, buy as set in trade sliders
            // then by rest but avoid huge market interference
            //1 day trade
            //TODO add x day buying or split buying somehow

            foreach (var product in Product.AllNonAbstract())
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
            foreach (var product in Product.AllNonAbstractTradableInPEOrder(this))
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
            foreach (var product in Product.AllNonAbstractTradableInPEOrder(this))
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

            if (economy == Economy.PlannedEconomy)
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
            }
        }

        public MoneyView getGDP()
        {
            Money result = new Money(0m);
            foreach (var province in AllProvinces)
            {
                result.Add(province.getGDP());
            }
            return result;
        }

        public MoneyView getGDPPer1000()
        {
            return (getGDP().Copy()).Multiply(1000m).Divide(Provinces.getFamilyPopulation(), false);
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
            var list = World.AllExistingCountries().OrderByDescending(x => x.getGDP().Get());
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
            foreach (var item in World.AllExistingCountries())
            {
                list.Add(new KeyValuePair<Country, float>(item, (float)item.getGDPPer1000().Get()));
            }
            list.Sort(FloatOrder);
            return list.FindIndex(x => x.Key == this) + 1; // starts with zero
        }

        public Procent getGDPShare()
        {
            Money worldGDP = new Money(0m);
            foreach (var item in World.AllExistingCountries())
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
            foreach (var country in World.AllExistingCountries())
            {
                list.Add(new KeyValuePair<Country, int>(country, country.Provinces.getFamilyPopulation()));
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
            foreach (var country in World.AllExistingCountries())
            {
                list.Add(new KeyValuePair<Country, int>(country, country.Provinces.Count));
            }
            list.Sort(IntOrder);
            return list.FindIndex(x => x.Key == this) + 1; // starts with zero
        }

        public override void SetStatisticToZero()
        {
            base.SetStatisticToZero();
            countryStorageSet.SetStatisticToZero();            
            Politics.SetStatisticToZero();
        }



        /// <summary>
        /// Returns true if was able to give a subsidy
        /// </summary>
        public bool GiveFactorySubsidies(Consumer byWhom, MoneyView howMuch)
        {
            if (CanPay(howMuch))
            {
                PayWithoutRecord(byWhom, howMuch, Register.Account.EnterpriseSubsidies);
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
                && government == Government.Aristocracy)
                return MoneyView.Zero; // don't pay with monarchy
            Procent tax;
            Register.Account account;
            if (isPoorStrata)
            {
                tax = taxationForPoor.tax.Procent;
                account = Register.Account.PoorIncomeTax;
            }
            else //if (type is TaxationForRich)
            {
                tax = taxationForRich.tax.Procent;
                account = Register.Account.RichIncomeTax;
            }
            if (!(taxPayer is Market) && taxPayer.Country != this) //foreigner
            {
                account = Register.Account.ForeignIncomeTax;
            }

            // paying tax
            var taxSize = taxable.Copy().Multiply(tax);
            if (taxPayer.CanPay(taxSize))
            {
                taxPayer.Pay(this, taxSize, account);

                return taxSize;
            }
            else
            {
                var hadMoney = taxPayer.getMoneyAvailable().Copy();
                //var availableMoney = taxPayer.getMoneyAvailable();             

                taxPayer.PayAllAvailableMoney(this, account);

                return hadMoney;
            }
        }

        public bool TakeNaturalTax(PopUnit pop, ProcentReform.ProcentReformValue tax)
        {
            var howMuchSend = pop.getGainGoodsThisTurn().Multiply(tax.Procent);

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

        public void OnClicked()
        {
            Game.Player.events.RiseClickedOn(new CountryEventArgs(this));
            //if (MainCamera.diplomacyPanel.isActiveAndEnabled)
            //{
            //    if (MainCamera.diplomacyPanel.getSelectedCountry() == this)
            //        MainCamera.diplomacyPanel.Hide();
            //    else
            //        MainCamera.diplomacyPanel.show(this);
            //}
            //else
            //    MainCamera.diplomacyPanel.show(this);
        }

        public float NameWeight
        {
            get
            {
                return nameWeight;
            }
        }

        public void annexTo(Country country)
        {
            foreach (var item in Provinces.AllProvinces.ToList())
            {
                country.Provinces.TakeProvince(item, false);
                //item.secedeTo(country, false);
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
                            countryOwner.Diplomacy.ChangeRelation(this, Options.PopLoyaltyDropOnNationalization.get());
                    }
                }
        }

        public IEnumerable<Province> AllProvinces
        {
            get
            {
                foreach (var province in Provinces.AllProvinces)
                    yield return province;
            }
        }

        //todo performance hit 7% 420 calls 1.4mb 82 ms
        private bool isThreatenBy(Staff country)
        {
            if (country == this)
                return false;
            if (country.getStrengthExluding(null) > getStrengthExluding(null) * 2)
                return true;
            else
                return false;
        }
    }
}