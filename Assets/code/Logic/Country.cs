using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.Serialization;

public class Country : Staff
{
    public readonly static List<Country> allCountries = new List<Country>();
    internal static readonly Country NullCountry;

    internal readonly Government government;
    internal readonly Economy economy;
    internal readonly Serfdom serfdom;
    internal readonly MinimalWage minimalWage;
    internal readonly UnemploymentSubsidies unemploymentSubsidies;
    internal readonly TaxationForPoor taxationForPoor;
    internal readonly TaxationForRich taxationForRich;
    internal readonly MinorityPolicy minorityPolicy;

    public List<Province> ownedProvinces = new List<Province>();

    private readonly Dictionary<Country, Procent> opinionOf = new Dictionary<Country, Procent>();
    private readonly Dictionary<Country, DateTime> myLastAttackDate = new Dictionary<Country, DateTime>();
    private readonly Dictionary<Invention, bool> inventions = new Dictionary<Invention, bool>();

    public readonly List<AbstractReform> reforms = new List<AbstractReform>();
    public readonly List<Movement> movements = new List<Movement>();
    public readonly StorageSet storageSet = new StorageSet();

    private TextMesh meshCapitalText;
    private Material borderMaterial;

    private readonly string name;
    private readonly Culture culture;
    private readonly Color nationalColor;
    private Province capital;
    private bool alive = true;

    private readonly Value soldiersWage = new Value(0f);
    public readonly Value sciencePoints = new Value(0f);
    public bool failedToPaySoldiers;
    public int autoPutInBankLimit = 2000;

    private readonly Value poorTaxIncome = new Value(0f);
    private readonly Value richTaxIncome = new Value(0f);
    private readonly Value goldMinesIncome = new Value(0f);
    private readonly Value ownedFactoriesIncome = new Value(0f);

    private readonly Value unemploymentSubsidiesExpense = new Value(0f);
    private readonly Value soldiersWageExpense = new Value(0f);
    private readonly Value factorySubsidiesExpense = new Value(0f);
    private readonly Value storageBuyingExpense = new Value(0f);

    private readonly Modifier modXHasMyCores;
    public readonly ModifiersList modMyOpinionOfXCountry;
    public static readonly ConditionsListForDoubleObjects canAttack = new ConditionsListForDoubleObjects(new List<Condition>
    {
        new ConditionForDoubleObjects((country, province)=>(province as Province).isNeighbor(country as Country), x=>"Is neighbor province", true),
        //new ConditionForDoubleObjects((country, province)=>(province as Province).getCountry().government.getValue, x=>"Is neighbor province", false),
        new ConditionForDoubleObjects((country, province)=>!Government.isDemocracy.checkIftrue(country)
        || !Government.isDemocracy.checkIftrue((province as Province).getCountry()), x=>"Democracies can't attack each other", true),
    });

    internal void annexTo(Country country)
    {
        foreach (var item in ownedProvinces.ToList())
        {
            item.secedeTo(country, false);
        }
    }

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
    });

    static Country()
    {
        NullCountry = new Country("Uncolonized lands", new Culture("Ancient tribes"), Color.yellow, null);
    }
    public Country(string iname, Culture iculture, Color color, Province capital) : base(null)
    {
        foreach (var each in Invention.allInventions)
            inventions.Add(each, false);
        place = this;
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
            new Modifier (x=>isThereBadboyCountry() ==x,"You are very bad boy", -0.05f, false)
            });
        setBank(new Bank());
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
        name = iname;
        allCountries.Add(this);

        culture = iculture;
        nationalColor = color;
        this.capital = capital;
        //if (!Game.devMode)
        {
            economy.setValue(Economy.StateCapitalism);
            //economy.setValue( Economy.NaturalEconomy);
            serfdom.status = Serfdom.Abolished;
            //government.setValue(Government.Tribal, false);
            government.status = Government.Aristocracy;
            markInvented(Invention.Farming);

            markInvented(Invention.Banking);
            //markInvented(Invention.metal);
            //markInvented(Invention.individualRights);
            //markInvented(Invention.ProfessionalArmy);
            //markInvented(Invention.Welfare);

            //markInvented(Invention.Manufactories);
        }
    }

    internal void onSeparatismWon(Country oldCountry)
    {
        foreach (var item in oldCountry.ownedProvinces.ToList())
            if (item.isCoreFor(this))
            {
                item.secedeTo(this, false);
            }
        moveCapitalTo(getRandomOwnedProvince());
        foreach (var item in oldCountry.getInvented()) // copying inventions
        {
            this.markInvented(item.Key);
        }
        setPrefix();
        alive = true;
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
            item.ownedProvinces[0].setBorderMaterials(false);
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
    internal int getSize()
    {
        return ownedProvinces.Count;
    }

    public IEnumerable<KeyValuePair<Invention, bool>> getAvailable()
    {
        foreach (var invention in inventions)
            if (invention.Key.isAvailable(this))
                yield return invention;
    }
    public IEnumerable<KeyValuePair<Invention, bool>> getUninvented()
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
    public bool isInvented(Invention type)
    {
        bool result = false;
        inventions.TryGetValue(type, out result);
        return result;
    }
    internal void setPrefix()
    {
        meshCapitalText.text = getDescription();
    }
    public List<Country> getAllCoresOnMyland()
    {
        var res = new List<Country>();
        foreach (var province in ownedProvinces)
        {
            foreach (var core in province.getCores())
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
        soldiersWage.set(value);
    }
    internal float getSoldierWage()
    {
        return soldiersWage.get();
    }
    public Procent getAverageLoyalty()
    {
        Procent result = new Procent(0f);
        int calculatedPopulation = 0;
        foreach (var province in ownedProvinces)
            foreach (var pop in province.allPopUnits)
            {
                result.addPoportionally(calculatedPopulation, pop.getPopulation(), pop.loyalty);
                calculatedPopulation += pop.getPopulation();
            }
        return result;
    }
    /// <summary>
    /// Little bugged - returns RANDOM badboy, not biggest
    /// </summary>
    /// <returns></returns>
    private static DateTime DateOfisThereBadboyCountry;
    private static Country BadboyCountry;
    public static Country isThereBadboyCountry()
    {
        if (DateOfisThereBadboyCountry != Game.date)
        {
            DateOfisThereBadboyCountry = Game.date;
            float worldStrenght = 0f;
            foreach (var item in Country.getExisting())
                worldStrenght += item.getStregth(null);
            float streghtLimit = worldStrenght * Options.CountryBadBoyWorldLimit;
            BadboyCountry = Country.allCountries.FindAll(x => x != Country.NullCountry && x.getStregth(null) >= streghtLimit).MaxBy(x => x.getStregth(null));
        }
        return BadboyCountry;

    }
    private bool isThreatenBy(Country country)
    {
        if (country == this)
            return false;
        if (country.getStregth(null) > this.getStregth(null) * 2)
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

    public bool isAlive()
    {
        return alive;
    }
    static public IEnumerable<Country> getExisting()
    {
        foreach (var c in allCountries)
            if (c.isAlive() && c != Country.NullCountry)
                yield return c;

    }
    internal void killCountry(Country byWhom)
    {
        if (meshCapitalText != null) //todo WTF!!
            UnityEngine.Object.Destroy(meshCapitalText.gameObject);
        setStatisticToZero();

        //take all money from bank
        if (byWhom.isInvented(Invention.Banking))
            byWhom.getBank().add(this.getBank());
        else
            this.getBank().destroy(byWhom);

        //byWhom.storageSet.
        this.sendAllAvailableMoney(byWhom);
        this.getBank().defaultLoaner(this);
        storageSet.sendAll(byWhom.storageSet);

        if (!this.isAI())
            new Message("Disaster!!", "It looks like we lost our last province\n\nMaybe we would rise again?", "Okay");
        alive = false;
    }

    internal bool isOneProvince()
    {
        return ownedProvinces.Count == 1;
    }
    internal Province getCapital()
    {
        return capital;
    }
    override internal void sendArmy(Province target, Procent procent)
    {
        base.sendArmy(target, procent);
        //myLastAttackDate.AddMy(target.getCountry(), Game.date);
        if (this.myLastAttackDate.ContainsKey(target.getCountry()))
            myLastAttackDate[target.getCountry()] = Game.date;
        else
            myLastAttackDate.Add(target.getCountry(), Game.date);


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

    internal void setCapitalTextMesh(Province province)
    {
        Transform txtMeshTransform = GameObject.Instantiate(Game.r3dTextPrefab).transform;
        txtMeshTransform.SetParent(province.getRootGameObject().transform, false);

        Vector3 capitalTextPosition = province.getPosition();
        capitalTextPosition.y += 2f;
        capitalTextPosition.z -= 5f;
        txtMeshTransform.position = capitalTextPosition;

        meshCapitalText = txtMeshTransform.GetComponent<TextMesh>();
        meshCapitalText.text = getDescription();
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
        foreach (PopType type in PopType.getAllPopTypes())
        {
            divisionVotersResult.Add(type, 0);
            divisionPopulationResult.Add(type, 0);
            foreach (Province province in this.ownedProvinces)
            {
                foreach (PopUnit pop in province.getAllPopUnits(type))
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

    internal float getMinSalary()
    {
        var res = (minimalWage.getValue() as MinimalWage.ReformValue).getWage();
        if (res == 0f) res = Options.FactoryMinPossibleSallary;
        return res;
        //return minSalary.get();
    }
    public string getName()
    {
        return name;
    }
    public string getDescription()
    {
        if (this == Game.Player)
            return name + " " + government.getPrefix() + " (you are)";
        else
            return name + " " + government.getPrefix();
    }

    override public string ToString()
    {
        return getDescription();
    }
    public Value getSciencePointsBase()
    {
        if (Game.devMode)
            return new Value(this.getMenPopulation());
        else
            return //new Value(this.getMenPopulation() * Options.defaultSciencePointMultiplier);
            new Value(Options.defaultSciencePointMultiplier);
    }
    internal void simulate()
    {
        base.simulate();
        var spBase = getSciencePointsBase();
        spBase.multiply(modSciencePoints.getModifier(this));
        sciencePoints.add(spBase);

        if (this.autoPutInBankLimit > 0f)
        {
            float extraMoney = cash.get() - (float)this.autoPutInBankLimit;
            if (extraMoney > 0f)
                getBank().takeMoney(this, new Value(extraMoney));
        }
        //buyNeeds();
        foreach (var item in getAllArmies())
        {
            item.consume();
        }

        consumeNeeds(); // Should go After all Armies consumption
                        //Procent opinion;
        foreach (var item in Country.getExisting())
            if (item != this)
            {
                var procent = getRelationTo(item);
                procent.add(modMyOpinionOfXCountry.getModifier(item), false);
                procent.clamp100();
            }
        movements.RemoveAll(x => x.isEmpty());
        foreach (var item in movements.ToArray())
            item.simulate();


    }
    /// <summary>
    /// For AI only
    /// </summary>
    public void AIThink()
    {
        if (!isOnlyCountry())
            if (Game.Random.Next(10) == 1)
            {
                var possibleTarget = getNeighborProvinces().MinBy(x => getRelationTo(x.getCountry()).get());
                if (possibleTarget != null
                    && (getRelationTo(possibleTarget.getCountry()).get() < 1f || Game.Random.Next(200) == 1)
                    && this.getStregth(null) > 0
                    && (this.getAverageMorale().get() > 0.5f || getAllArmiesSize() == 0)
                    && (this.getStregth(null) > possibleTarget.getCountry().getStregth(null) * 0.25f
                        || possibleTarget.getCountry() == Country.NullCountry
                        || possibleTarget.getCountry().isAI() && this.getStregth(null) > possibleTarget.getCountry().getStregth(null) * 0.1f)
                    && Country.canAttack.isAllTrue(this, possibleTarget)
                    )
                {
                    mobilize(ownedProvinces);
                    sendArmy(possibleTarget, Procent.HundredProcent);
                }
            }
        if (Game.Random.Next(90) == 1)
            aiInvent();
        if (isInvented(Invention.ProfessionalArmy) && Game.Random.Next(10) == 1)
        {
            float newWage;
            var soldierAllNeedsCost = Game.market.getCost(PopType.Soldiers.getAllNeedsPer1000()).get();
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
    }
    public IEnumerable<PopUnit> getAllPopUnits()
    {
        foreach (var province in ownedProvinces)
            foreach (var pops in province.allPopUnits)
                yield return pops;
    }

    private void aiInvent()
    {
        var invention = getUninvented().ToList().PickRandom(x => this.sciencePoints.isBiggerOrEqual(x.Key.cost));
        if (invention.Key != null)
            invent(invention.Key);
    }
    public bool invent(Invention invention)
    {
        if (sciencePoints.isBiggerOrEqual(invention.cost))
        {
            markInvented(invention);
            sciencePoints.subtract(invention.cost);
            return true;
        }
        else return false;
    }
    public override void consumeNeeds()
    {
        var needs = getRealNeeds();
        //buy 1 day needs
        foreach (var need in needs)
            if (!storageSet.has(need)) // may reduce extra circles
            {
                // if I want to buy             
                //Storage toBuy = new Storage(need.getProduct(), need.get() - storageSet.getStorage(need.getProduct()).get(), false);
                Storage realNeed;
                if (need.isAbstractProduct())
                    realNeed = storageSet.convertToBiggestStorageProduct(need);
                else
                    realNeed = need;
                //Storage toBuy = need.subtractOutside(realNeed);            

                if (realNeed.isNotZero())
                    buyNeeds(realNeed);  // todo - return result? - no
            }
        //buy x day needs
        foreach (var need in needs)
        {
            Storage toBuy = new Storage(need.getProduct(),
                need.get() * Options.CountryForHowMuchDaysMakeReservs - storageSet.getBiggestStorage(need.getProduct()).get(), false);
            if (toBuy.isNotZero())
                buyNeeds(toBuy);
        }
    }

    void buyNeeds(Storage toBuy)
    {
        //if (toBuy.get() < 10f) toBuy.set(10);
        Storage realyBougth = Game.market.buy(this, toBuy, null);
        storageSet.add(realyBougth);
        storageBuyingExpenseAdd(new Value(Game.market.getCost(realyBougth)));
    }

    public Value getGDP()
    {
        Value result = new Value(0);
        foreach (var prov in ownedProvinces)
        {
            foreach (var producer in prov.allFactories)
                if (producer.gainGoodsThisTurn.get() > 0f)
                    result.add(Game.market.getCost(producer.gainGoodsThisTurn).get() - Game.market.getCost(producer.getConsumedTotal()).get());

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
    public int getPopulationAmountByType(PopType ipopType)
    {
        int result = 0;
        foreach (Province pro in ownedProvinces)
            result += pro.getPopulationAmountByType(ipopType);
        return result;
    }

    internal float getGDPPer1000()
    {
        return getGDP().get() * 1000f  / getFamilyPopulation();
        // overflows
        //res.multiply(1000);
        //res.divide(getFamilyPopulation());

        //return res;
    }
    //****************************
    internal Value getAllExpenses()
    {
        Value result = new Value(0f);
        result.add(unemploymentSubsidiesExpense);
        result.add(factorySubsidiesExpense);
        result.add(storageBuyingExpense);
        result.add(soldiersWageExpense);
        return result;
    }
    internal float getBalance()
    {
        return moneyIncomethisTurn.get() - getAllExpenses().get();
    }

    override public void setStatisticToZero()
    {
        base.setStatisticToZero();
        failedToPaySoldiers = false;
        poorTaxIncome.set(0f);
        richTaxIncome.set(0f);
        goldMinesIncome.set(0f);
        unemploymentSubsidiesExpense.set(0f);
        ownedFactoriesIncome.set(0f);
        factorySubsidiesExpense.set(0f);
        storageBuyingExpense.set(0f);
        soldiersWageExpense.setZero();
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
    internal void soldiersWageExpenseAdd(Value payCheck)
    {
        soldiersWageExpense.add(payCheck);
    }
    internal float getSoldiersWageExpense()
    {
        return soldiersWageExpense.get();
    }
    internal float getFactorySubsidiesExpense()
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
    override public Country getCountry()
    {
        return this;
    }
}
