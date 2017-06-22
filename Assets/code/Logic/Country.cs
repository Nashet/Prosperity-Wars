using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.Serialization;

public class Country : Consumer
{
    readonly string name;
    public static List<Country> allCountries = new List<Country>();
    public List<Province> ownedProvinces = new List<Province>();

    public CountryStorageSet storageSet = new CountryStorageSet();
    //public Procent countryTax;
    //public Procent aristocrstTax;//= new Procent(0.10f);
    public InventionsList inventions = new InventionsList();

    internal Government government;
    internal Economy economy;
    internal Serfdom serfdom;
    internal MinimalWage minimalWage;
    internal UnemploymentSubsidies unemploymentSubsidies;
    internal TaxationForPoor taxationForPoor;
    internal TaxationForRich taxationForRich;
    internal MinorityPolicy minorityPolicy;
    internal List<AbstractReform> reforms = new List<AbstractReform>();
    Culture culture;
    Color nationalColor;
    Province capital;

    readonly Dictionary<Country, Procent> opinionOf = new Dictionary<Country, Procent>();
    readonly Dictionary<Country, DateTime> myLastAttackDate = new Dictionary<Country, DateTime>();

    public GeneralStaff staff;

    TextMesh messhCapitalText;
    Material borderMaterial;
    /// <summary>
    /// per 1000 men
    /// </summary>
    //private Value minSalary = new Value(0.5f);
    public Value sciencePoints = new Value(0f);
    internal static readonly Country NullCountry;

    readonly Modifier modXHasMyCores;
    public ModifiersList modMyOpinionOfXCountry;

    Value poorTaxIncome = new Value(0f);
    Value richTaxIncome = new Value(0f);
    Value goldMinesIncome = new Value(0f);
    Value ownedFactoriesIncome = new Value(0f);

    Value unemploymentSubsidiesExpense = new Value(0f);
    Value factorySubsidiesExpense = new Value(0f);
    Value storageBuyingExpense = new Value(0f);
    static Country()
    {
        NullCountry = new Country("Uncolonized lands", new Culture("Ancient tribes"), Color.yellow, null);
    }
    public Country(string iname, Culture iculture, Color color, Province capital) : base(null)
    {

        modXHasMyCores = new Modifier(x => (x as Country).hasCores(this), "Has my cores", -0.05f, false);
        modMyOpinionOfXCountry = new ModifiersList(new List<Condition> { modXHasMyCores,
            new Modifier(x=>(x as Country).government.getValue() == this.government.getValue(), "Same form of government", 0.002f, false),
            new Modifier (x=>(x as Country).getLastAttackDateOn(this).getYearsSince() > Options.CountryTimeToForgetBattle
            && this.getLastAttackDateOn(x as Country).getYearsSince() > Options.CountryTimeToForgetBattle,"Lives in peace with us", 0.005f, false),
            new Modifier (x=>(x as Country).getLastAttackDateOn(this).getYearsSince() > 0 &&  (x as Country).getLastAttackDateOn(this).getYearsSince() < 15,
            "Recently attacked us", -0.05f, false),
            new Modifier (x=> this.isThreatenBy(x as Country),"We are weaker", -0.05f, false),
            new Modifier (delegate(System.Object x) { Country bully = isThereBadboyCountry(); return bully != null && bully!= x as Country  && bully!= this; },"There is bigger threat to the world", 0.05f, false)
            //,            new Modifier (x=>isThereBadboyCountry() ==x,"You are very bad boy", -0.05f, false)
            });
        bank = new Bank();
        staff = new GeneralStaff(this);
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
        name = iname;
        allCountries.Add(this);

        culture = iculture;
        nationalColor = color;
        this.capital = capital;
        //if (!Game.devMode)
        {
            government.status = Government.Aristocracy;

            economy.status = Economy.StateCapitalism;

            inventions.markInvented(Invention.farming);
            inventions.markInvented(Invention.manufactories);
            inventions.markInvented(Invention.banking);
            // inventions.markInvented(Invention.metal);
            // inventions.MarkInvented(InventionType.individualRights);
            serfdom.status = Serfdom.Abolished;
        }



    }
    public static void setUnityAPI()
    {
        foreach (var item in allCountries)
        {
            if (item != Country.NullCountry)
                item.moveCapitalTo(item.ownedProvinces[0]);
            //if (capital != null) // not null-country

            item.borderMaterial = new Material(Game.defaultCountryBorderMaterial);
            item.borderMaterial.color = item.nationalColor.getNegative();
            //item.ownedProvinces[0].setBorderMaterial(Game.defaultProvinceBorderMaterial);
            item.ownedProvinces[0].setUnselectedBorderMaterials();
        }

    }
    internal static void makeCountries(Game game)
    {
        var countryNameGenerator = new CountryNameGenerator();
        var cultureNameGenerator = new CultureNameGenerator();
        int howMuchCountries = Province.allProvinces.Count / Options.ProvincesPerCountry;
        howMuchCountries += Game.Random.Next(6);
        if (howMuchCountries < 8)
            howMuchCountries = 8;
        for (int i = 0; i < howMuchCountries; i++)
        {
            game.updateStatus("Making countries.." + i);
            Culture cul = new Culture(cultureNameGenerator.generateCultureName());

            Province province = Province.getRandomProvinceInWorld((x) => x.getCountry() == null);
            //&& !Game.seaProvinces.Contains(x));// Country.NullCountry);
            Country count = new Country(countryNameGenerator.generateCountryName(), cul, ColorExtensions.getRandomColor(), province);
            //count.setBank(count.bank);
            Game.Player = Country.allCountries[1]; // not wild Tribes, DONT touch that
            province.InitialOwner(count);
            count.cash.add(100f);

        }


        foreach (var pro in Province.allProvinces)
            if (pro.getCountry() == null)
                pro.InitialOwner(Country.NullCountry);
    }

    /// <summary>
    /// Little bugged - returns RANDOM badboy, not biggest
    /// </summary>
    /// <returns></returns>
    public static Country isThereBadboyCountry()
    {
        float worldStrenght = 0f;
        foreach (var item in Country.getExisting())
            worldStrenght += item.getStreght();
        float streghtLimit = worldStrenght * Options.CountryBadBoyWorldLimit;
        Country found = Country.allCountries.Find(x => x.getStreght() >= streghtLimit && x != Country.NullCountry);
        //if (found == Country.NullCountry)
        //    return null;
        //else
        return found;

    }

    private bool isThreatenBy(Country country)
    {
        if (country == this)
            return false;
        if (country.getStreght() > this.getStreght() * 2)
            return true;
        else
            return false;
    }

    public DateTime getLastAttackDateOn(Country country)
    {
        if (myLastAttackDate.ContainsKey(country))
            return myLastAttackDate[country];
        else
            return DateTime.MinValue;
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
    public Culture getCulture()
    {
        return culture;
    }
    internal void demobilize()
    {
        //ownedProvinces.ForEach(x => x.demobilize());
        //allArmies.ForEach(x => x.demobilize());
        staff.demobilizeAllArmies();
    }


    public bool isExist()
    {
        return ownedProvinces.Count > 0;
    }
    //internal static IEnumerable<Country> allExisting = getExisting();
    internal int autoPutInBankLimit = 2000;

    static public IEnumerable<Country> getExisting()
    {
        foreach (var c in allCountries)
            if (c.isExist() && c != Country.NullCountry)
                yield return c;

    }
    internal void killCountry(Country byWhom)
    {
        if (messhCapitalText != null) //todo WTF!!
            UnityEngine.Object.Destroy(messhCapitalText.gameObject);
        setStatisticToZero();

        //take all money from bank
        if (byWhom.isInvented(Invention.banking))
            byWhom.bank.add(this.bank);
        else
            this.bank.destroy(byWhom);

        //byWhom.storageSet.
        this.sendAllAvailableMoney(byWhom);
        this.bank.defaultLoaner(this);
        storageSet.sendAll(byWhom.storageSet);

        if (!this.isAI())
            new Message("Disaster!!", "It looks like we lost our last province\n\nMaybe we would rise again?", "Okay");
    }

    internal bool isOneProvince()
    {
        return ownedProvinces.Count == 1;
    }

    internal Province getCapital()
    {
        return capital;
    }

    internal void sendArmy(Province target, Procent procent)
    {
        staff.sendArmy(target, procent);
        //myLastAttackDate.AddMy(target.getCountry(), Game.date);
        if (this.myLastAttackDate.ContainsKey(target.getCountry()))
            myLastAttackDate[target.getCountry()] = Game.date;
        else
            myLastAttackDate.Add(target.getCountry(), Game.date);


    }

    internal bool canAttack(Province province)
    {
        //!province.isBelongsTo(this) &&
        return province.isNeghbour(this);
    }

    internal List<Province> getNeighborProvinces()
    {
        List<Province> result = new List<Province>();
        foreach (var province in ownedProvinces)
            result.AddRange(
                province.getNeigbors(p => p.getCountry() != this && !result.Contains(p))
                );
        return result;
    }
    internal Province getRandomNeighborProvince()
    {
        if (isOnlyCountry())
            return null;
        else
            return getNeighborProvinces().PickRandom();
    }


    private bool isOnlyCountry()
    {
        foreach (var any in Country.getExisting())
            if (any != this)
                return false;
        return true;
    }

    internal Province getRandomOwnedProvince()
    {
        return ownedProvinces.PickRandom();
    }
    internal Province getRandomOwnedProvince(Predicate<Province> predicate)
    {
        return ownedProvinces.PickRandom(predicate);
    }

    //todo move to Province.cs
    internal void makeCapitalTextMesh()
    {
        Transform txtMeshTransform = GameObject.Instantiate(Game.r3dTextPrefab).transform;
        txtMeshTransform.SetParent(capital.rootGameObject.transform, false);


        Vector3 capitalTextPosition = capital.centre;
        capitalTextPosition.y += 2f;
        txtMeshTransform.position = capitalTextPosition;

        messhCapitalText = txtMeshTransform.GetComponent<TextMesh>();
        messhCapitalText.text = getName();
        messhCapitalText.fontSize *= 2;
        if (this == Game.Player)
        {
            messhCapitalText.color = Color.blue;
            messhCapitalText.fontSize += 10;

        }
        else
        {
            messhCapitalText.color = Color.cyan; // Set the text's color to red
            //messhCapitalText.fontSize += messhCapitalText.fontSize / 3;
        }
    }
    internal void moveCapitalTo(Province newCapital)
    {
        if (messhCapitalText == null)
            makeCapitalTextMesh();
        else
        {
            Vector3 capitalTextPosition = newCapital.centre;
            capitalTextPosition.y += 2f;
            messhCapitalText.transform.position = capitalTextPosition;
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
        int totalPopulation = this.getMenPopulation();
        int votingPopulation = 0;
        int populationSayedYes = 0;
        int votersSayedYes = 0;
        Procent procentVotersSayedYes = new Procent(0);
        //Procent procentPopulationSayedYes = new Procent(0f);
        foreach (Province pro in ownedProvinces)
            foreach (PopUnit pop in pro.allPopUnits)
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
            procentPopulationSayedYes.set((float)populationSayedYes / totalPopulation);
        else
            procentPopulationSayedYes.set(0);

        if (votingPopulation == 0)
            procentVotersSayedYes.set(0);
        else
            procentVotersSayedYes.set((float)votersSayedYes / votingPopulation);
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
        int totalPopulation = this.getMenPopulation();
        int votingPopulation = 0;
        int populationSayedYes = 0;
        int votersSayedYes = 0;
        Procent procentVotersSayedYes = new Procent(0f);
        Dictionary<PopType, int> divisionPopulationResult = new Dictionary<PopType, int>();
        Dictionary<PopType, int> divisionVotersResult = this.getYesVotesByType(reform, ref divisionPopulationResult);
        foreach (KeyValuePair<PopType, int> next in divisionVotersResult)
            votersSayedYes += next.Value;

        if (totalPopulation != 0)
            procentPopulationSayedYes.set((float)populationSayedYes / totalPopulation);
        else
            procentPopulationSayedYes.set(0);

        if (votingPopulation == 0)
            procentVotersSayedYes.set(0);
        else
            procentVotersSayedYes.set((float)votersSayedYes / votingPopulation);
        return procentVotersSayedYes;
    }
    internal Dictionary<PopType, int> getYesVotesByType(AbstractReformValue reform, ref Dictionary<PopType, int> divisionPopulationResult)
    {  // division by pop types
        Dictionary<PopType, int> divisionVotersResult = new Dictionary<PopType, int>();
        foreach (PopType type in PopType.allPopTypes)
        {
            divisionVotersResult.Add(type, 0);
            divisionPopulationResult.Add(type, 0);
            foreach (Province pro in this.ownedProvinces)
            {
                var popList = pro.getAllPopUnits(type);
                foreach (PopUnit pop in popList)
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
    public bool isInvented(Invention type)
    {

        return inventions.isInvented(type);
    }

    internal float getMinSalary()
    {
        return (minimalWage.getValue() as MinimalWage.ReformValue).getWage();
        //return minSalary.get();
    }
    public string getName()
    {
        return name;
    }

    override public string ToString()
    {
        if (this == Game.Player)
            return name + " country (you are)";
        else
            return name + " country";
    }
    internal void think()
    {
        if (Game.devMode)
            sciencePoints.add(this.getMenPopulation());
        else
            sciencePoints.add(this.getMenPopulation() * Options.defaultSciencePointMultiplier);
        //sciencePoints.add(this.getMenPopulation());

        if (this.autoPutInBankLimit > 0f)
        {
            float extraMoney = cash.get() - (float)this.autoPutInBankLimit;
            if (extraMoney > 0f)
                bank.takeMoney(this, new Value(extraMoney));
        }
        staff.consume();

        buyNeeds(); // Should go After all Armies consumption
        //Procent opinion;
        foreach (var item in Country.getExisting())
            if (item != this)
            {
                var procent = getRelationTo(item);
                procent.add(modMyOpinionOfXCountry.getModifier(item), false);
                procent.clamp100();
            }


        if (isAI() && !isOnlyCountry())
            if (Game.Random.Next(10) == 1)
            {
                var possibleTarget = getNeighborProvinces().MinBy(x => getRelationTo(x.getCountry()).get());
                if (possibleTarget != null
                    && (getRelationTo(possibleTarget.getCountry()).get() < 1f || Game.Random.Next(100)==1)
                    && this.getStreght() > 0
                    && (this.getStreght() > possibleTarget.getCountry().getStreght() * 0.25f 
                        || possibleTarget.getCountry() == Country.NullCountry
                        || possibleTarget.getCountry().isAI() && this.getStreght() > possibleTarget.getCountry().getStreght() * 0.1f )
                    )
                {
                    staff.mobilize(ownedProvinces);
                    sendArmy(possibleTarget, Procent.HundredProcent);
                }
            }
        if (isAI() && Game.Random.Next(30) == 1)
            aiInvent();
    }

    public IEnumerable<PopUnit> getAllPopUnits()
    {
        foreach (var province in ownedProvinces)
            foreach (var pops in province.allPopUnits)
                yield return pops;
    }

    private void aiInvent()
    {
        var invention = inventions.getUninvented(this).ToList().PickRandom(x => this.sciencePoints.isBiggerOrEqual(x.Key.cost));
        if (invention.Key != null)
            invent(invention.Key);
    }
    public bool invent(Invention invention)
    {
        if (sciencePoints.isBiggerOrEqual(invention.cost))
        {
            inventions.markInvented(invention);
            sciencePoints.subtract(invention.cost);
            return true;
        }
        else return false;
    }
    public override void buyNeeds()
    {
        var needs = getNeeds();
        //if (wallet.canPay(Game.market.getCost(needs)))
        //buy 1 day needs
        foreach (var pro in Product.allProducts)
        {
            // if I want to buy           
            Storage toBuy = new Storage(pro, needs.getStorage(pro).get() - storageSet.getStorage(pro).get());
            buyNeeds(toBuy);
        }
        //buy x day needs
        foreach (var pro in Product.allProducts)
        {
            Storage toBuy = new Storage(pro, needs.getStorage(pro).get() * Options.CountryForHowMuchDaysMakeReservs - storageSet.getStorage(pro).get());
            buyNeeds(toBuy);
        }
    }

    internal Army getDefenceForces()
    {
        Army a = staff.getDefenceForces();
        if (a == null)
            return new Army(this);
        else
            return a;
    }

    void buyNeeds(Storage toBuy)
    {
        // if I want to buy           
        //Storage toBuy = new Storage(pro, needs.getStorage(pro).get()* days - storageSet.getStorage(pro).get());
        if (toBuy.get() > 0f)
        {
            //if (toBuy.get() < 10f) toBuy.set(10);
            toBuy.multiple(Game.market.buy(this, toBuy, null));
            storageSet.add(toBuy);
            storageBuyingExpenseAdd(new Value(Game.market.getCost(toBuy)));
        }
    }
    public PrimitiveStorageSet getNeeds()
    {
        return staff.getNeeds();
    }
    private float getStreght()
    {
        int size = 0;
        var defArmy = staff.getDefenceForces();
        if (defArmy != null)
            size = defArmy.getSize();
        return howMuchCanMobilize() + size;
    }

    private float howMuchCanMobilize()
    {
        float result = 0f;
        foreach (var pr in ownedProvinces)
            foreach (var po in pr.allPopUnits)
                if (po.popType.canMobilize())
                    result += po.howMuchCanMobilize();
        return result;
    }

    public bool isAI()
    {
        return this != Game.Player || (this == Game.Player && Game.isPlayerSurrended());
    }
    public Value getGDP()
    {
        Value result = new Value(0);
        foreach (var prov in ownedProvinces)
        {
            foreach (var prod in prov.allFactories)
                if (prod.gainGoodsThisTurn.get() > 0f)
                    result.add(Game.market.getCost(prod.gainGoodsThisTurn).get() - Game.market.getCost(prod.consumedTotal).get());

            foreach (var pop in prov.allPopUnits)
                if (pop.popType.isProducer())
                    if (pop.gainGoodsThisTurn.get() > 0f)
                        result.add(Game.market.getCost(pop.gainGoodsThisTurn));
        }
        return result;
    }
    public Procent getUnemployment()
    {
        Procent result = new Procent(0f);
        int calculatedBase = 0;
        foreach (var item in ownedProvinces)
        {
            int population = item.getMenPopulation();
            result.addPoportionally(calculatedBase, population, item.getUnemployment());
            calculatedBase += population;
        }
        return result;
    }
    internal int getMenPopulation()
    {
        int result = 0;
        foreach (Province pr in ownedProvinces)
            result += pr.getMenPopulation();
        return result;
    }
    internal int getFamilyPopulation()
    {
        return getMenPopulation() * Options.familySize;
    }
    public int FindPopulationAmountByType(PopType ipopType)
    {
        int result = 0;
        foreach (Province pro in ownedProvinces)
            result += pro.getPopulationAmountByType(ipopType);
        return result;
    }

    internal Value getGDPPer1000()
    {
        Value res = getGDP();
        res.multiple(1000);
        res.divide(getMenPopulation());

        return res;
    }
    //****************************
    internal Value getAllExpenses()
    {
        Value result = new Value(0f);
        result.add(unemploymentSubsidiesExpense);
        result.add(factorySubsidiesExpense);
        result.add(storageBuyingExpense);
        return result;
    }
    internal float getBalance()
    {
        return moneyIncomethisTurn.get() - getAllExpenses().get();
    }

    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        poorTaxIncome.set(0f);
        richTaxIncome.set(0f);
        goldMinesIncome.set(0f);
        unemploymentSubsidiesExpense.set(0f);
        ownedFactoriesIncome.set(0f);
        factorySubsidiesExpense.set(0f);
        moneyIncomethisTurn.set(0f);
        storageBuyingExpense.set(0f);
    }

    internal void takeFactorySubsidies(Consumer byWhom, Value howMuch)
    {
        if (canPay(howMuch))
        {
            payWithoutRecord(byWhom, howMuch);
            factorySubsidiesExpense.add(howMuch);
        }
        else
        {
            //sendAll(byWhom.wallet);
            payWithoutRecord(byWhom, byWhom.cash);
            factorySubsidiesExpense.add(byWhom.cash);
        }

    }
    internal float getfactorySubsidiesExpense()
    {
        return factorySubsidiesExpense.get();
    }
    internal float getPoorTaxIncome()
    {
        return poorTaxIncome.get();
    }

    internal float getRichTaxIncome()
    {
        return richTaxIncome.get();
    }

    internal float getGoldMinesIncome()
    {
        return goldMinesIncome.get();
    }


    internal float getOwnedFactoriesIncome()
    {
        return ownedFactoriesIncome.get();
    }

    internal float getUnemploymentSubsidiesExpense()
    {
        return unemploymentSubsidiesExpense.get();
    }
    internal float getStorageBuyingExpense()
    {
        return storageBuyingExpense.get();
    }

    internal void poorTaxIncomeAdd(Value toAdd)
    {
        poorTaxIncome.add(toAdd);
    }
    internal void richTaxIncomeAdd(Value toAdd)
    {
        richTaxIncome.add(toAdd);
    }
    internal void goldMinesIncomeAdd(Value toAdd)
    {
        goldMinesIncome.add(toAdd);
    }
    internal void unemploymentSubsidiesExpenseAdd(Value toAdd)
    {
        unemploymentSubsidiesExpense.add(toAdd);
    }
    internal void storageBuyingExpenseAdd(Value toAdd)
    {
        storageBuyingExpense.add(toAdd);
    }
    internal void ownedFactoriesIncomeAdd(Value toAdd)
    {
        ownedFactoriesIncome.add(toAdd);
    }

}
