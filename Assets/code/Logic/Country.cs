using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Runtime.Serialization;


public class CountryStorageSet : PrimitiveStorageSet
{
    PrimitiveStorageSet consumedLastTurn = new PrimitiveStorageSet();

    internal Value getConsumption(Product whom)
    {
        foreach (Storage stor in consumedLastTurn)
            if (stor.getProduct() == whom)
                return stor;
        return new Value(0f);
    }
    internal void setStatisticToZero()
    {
        consumedLastTurn.SetZero();
    }

    /// / next - inherited


    public void set(Storage inn)
    {
        base.set(inn);
        throw new DontUseThatMethod();
    }
    ///// <summary>
    ///// If duplicated than adds
    ///// </summary>
    //internal void add(Storage need)
    //{
    //    base.add(need);
    //    consumedLastTurn.add(need)
    //}

    ///// <summary>
    ///// If duplicated than adds
    ///// </summary>
    //internal void add(PrimitiveStorageSet need)
    //{ }

    /// <summary>
    /// Do checks outside
    /// </summary>   
    public bool send(Producer whom, Storage what)
    {
        if (base.send(whom, what))
        {
            consumedLastTurn.add(what);
            return true;
        }
        else
            return false;
    }

    public void take(Storage fromHhom, Value howMuch)
    {
        base.take(fromHhom, howMuch);
        throw new DontUseThatMethod();
    }
    /// <summary>
    /// //todo !!! if someone would change returning object then country consumption logic would be broken!!
    /// </summary>    
    internal Value getStorage(Product whom)
    {
        return base.getStorage(whom);
    }

    internal void SetZero()
    {
        base.SetZero();
        throw new DontUseThatMethod();
    }
    //internal PrimitiveStorageSet Divide(float v)
    //{
    //    PrimitiveStorageSet result = new PrimitiveStorageSet();
    //    foreach (Storage stor in container)
    //        result.Set(new Storage(stor.getProduct(), stor.get() / v));
    //    return result;
    //}

    internal bool subtract(Storage stor)
    {
        if (base.subtract(stor))
        {
            consumedLastTurn.add(stor);
            return true;
        }
        else
            return false;
    }

    //internal Storage subtractOutside(Storage stor)
    //{
    //    Storage find = this.findStorage(stor.getProduct());
    //    if (find == null)
    //        return new Storage(stor);
    //    else
    //        return new Storage(stor.getProduct(), find.subtractOutside(stor).get());
    //}
    internal void subtract(PrimitiveStorageSet set)
    {
        base.subtract(set);
        throw new DontUseThatMethod();
    }
    internal void copyDataFrom(PrimitiveStorageSet consumed)
    {
        base.copyDataFrom(consumed);
        throw new DontUseThatMethod();
    }
    internal void sendAll(PrimitiveStorageSet toWhom)
    {
        consumedLastTurn.add(this);
        base.sendAll(toWhom);
    }

}
public class Country : Consumer
{
    public string name;
    public static List<Country> allCountries = new List<Country>();
    public List<Province> ownedProvinces = new List<Province>();

    public CountryStorageSet storageSet = new CountryStorageSet();
    //public Procent countryTax;
    public Procent aristocrstTax;//= new Procent(0.10f);
    public InventionsList inventions = new InventionsList();

    internal Government government;
    internal Economy economy;
    internal Serfdom serfdom;
    internal MinimalWage minimalWage;
    internal UnemploymentSubsidies unemploymentSubsidies;
    internal TaxationForPoor taxationForPoor;
    internal TaxationForRich taxationForRich;
    internal List<AbstractReform> reforms = new List<AbstractReform>();
    public Culture culture;
    Color nationalColor;
    Province capital;

    internal Army homeArmy;
    internal Army sendingArmy;
    internal List<Army> allArmies = new List<Army>();

    TextMesh messhCapitalText;

    public Bank bank = new Bank();

    /// <summary>
    /// per 1000 men
    /// </summary>
    //private Value minSalary = new Value(0.5f);
    public Value sciencePoints = new Value(0f);
    internal static readonly Country NullCountry = new Country("Uncolonized lands", new Culture("Ancient tribes"), new CountryWallet(0f), Color.yellow, null);


    public Country(string iname, Culture iculture, CountryWallet wallet, Color color, Province capital) : base(wallet)
    {

        homeArmy = new Army(this);
        sendingArmy = new Army(this);
        government = new Government(this);

        economy = new Economy(this);
        serfdom = new Serfdom(this);

        minimalWage = new MinimalWage(this);
        unemploymentSubsidies = new UnemploymentSubsidies(this);
        taxationForPoor = new TaxationForPoor(this);
        taxationForRich = new TaxationForRich(this);
        name = iname;
        allCountries.Add(this);

        culture = iculture;
        nationalColor = color;
        this.capital = capital;
        //if (!Game.devMode)
        {
            government.status = Government.Aristocracy;

            economy.status = Economy.StateCapitalism;

            inventions.MarkInvented(InventionType.farming);
            inventions.MarkInvented(InventionType.manufactories);
            inventions.MarkInvented(InventionType.banking);
            // inventions.MarkInvented(InventionType.individualRights);
            serfdom.status = Serfdom.Abolished;
        }
    }

    internal void demobilize()
    {
        //ownedProvinces.ForEach(x => x.demobilize());
        allArmies.ForEach(x => x.demobilize());
    }

    internal void demobilize(Province province)
    {
        province.demobilize();
    }

    public bool isExist()
    {
        return ownedProvinces.Count > 0;
    }
    internal static IEnumerable<Country> allExisting = getExisting();

    static IEnumerable<Country> getExisting()
    {
        foreach (var c in allCountries)
            if (c.isExist() && c != Country.NullCountry)
                yield return c;

    }
    internal void killCountry(Country byWhom)
    {
        if (messhCapitalText != null) //todo WTF!!
            UnityEngine.Object.Destroy(messhCapitalText.gameObject);
        getCountryWallet().setSatisticToZero();
        //take all money from bank
        byWhom.bank.add(this.bank);

        //byWhom.storageSet.
        storageSet.sendAll(byWhom.storageSet);

        if (this == Game.player)
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

    internal void sendArmy(Army sendingArmy, Province province)
    {
        sendingArmy.moveTo(province);
        //walkingArmies.Add(new Army(sendingArmy));
        //allArmies.Add(sendingArmy);
        //sendingArmy.clear();
    }

    internal void mobilize()
    {
        foreach (var province in ownedProvinces)
            province.mobilize();
    }

    internal List<Province> getNeighborProvinces()
    {
        List<Province> result = new List<Province>();
        foreach (var province in ownedProvinces)
            result.AddRange(
                province.getNeigbors(p => p.getOwner() != this && !result.Contains(p))
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
        foreach (var any in Country.allExisting)
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
        txtMeshTransform.SetParent(capital.gameObject.transform, false);


        Vector3 capitalTextPosition = capital.centre;
        capitalTextPosition.y += 2f;
        txtMeshTransform.position = capitalTextPosition;

        messhCapitalText = txtMeshTransform.GetComponent<TextMesh>();
        messhCapitalText.text = this.ToString();
        if (this == Game.player)

        {
            messhCapitalText.color = Color.blue;
            messhCapitalText.fontSize += messhCapitalText.fontSize / 2;

        }
        else
        {
            messhCapitalText.color = Color.cyan; // Set the text's color to red
            messhCapitalText.fontSize += messhCapitalText.fontSize / 3;
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
                        votersSayedYes += pop.getPopulation();
                        populationSayedYes += pop.getPopulation();
                    }
                    votingPopulation += pop.getPopulation();
                }
                else
                {
                    if (pop.getSayingYes(reform))
                        populationSayedYes += pop.getPopulation();
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
    /// <summary>
    /// Not finished, dont use it
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
                        divisionPopulationResult[type] += pop.getPopulation();
                        if (pop.canVote())
                            divisionVotersResult[type] += pop.getPopulation();
                    }
            }
        }
        return divisionVotersResult;
    }
    public bool isInvented(InventionType type)
    {

        return inventions.isInvented(type);
    }
    public bool isInvented(Product product)
    {
        if ((product == Product.Metal || product == Product.MetallOre) && !isInvented(InventionType.metal)
            || (!product.isResource() && !isInvented(InventionType.manufactories)))
            return false;
        else
            return true;
    }
    internal float getMinSalary()
    {
        return (minimalWage.getValue() as MinimalWage.ReformValue).getWage();
        //return minSalary.get();
    }
    override public string ToString()
    {
        if (this == Game.player)
            return name + " country (you are)";
        else
            return name + " country";
    }
    internal void Think()
    {
        if (Game.devMode)
            sciencePoints.add(this.getMenPopulation());
        else
            sciencePoints.add(this.getMenPopulation() * Options.defaultSciencePointMultiplier);

        if (isInvented(InventionType.banking) && wallet.haveMoney.get() <= 1000f)
            bank.PutOnDeposit(wallet, new Value(wallet.moneyIncomethisTurn.get() / 2f));
        else
            bank.PutOnDeposit(wallet, new Value(wallet.moneyIncomethisTurn.get()));


        allArmies.ForEach(x => x.consume());
        buyNeeds(); // Should go After all Armies consumption
        if (isAI() && !isOnlyCountry())
            if (Game.random.Next(10) == 1)
            {
                var possibleTarget = getRandomNeighborProvince();
                if (possibleTarget != null)
                    if ((this.getStreght() * 1.5f > possibleTarget.getOwner().getStreght() && possibleTarget.getOwner() == Game.player) || possibleTarget.getOwner() == NullCountry
                        || possibleTarget.getOwner() != Game.player && this.getStreght() < possibleTarget.getOwner().getStreght() * 0.5f)
                    {
                        mobilize();
                        sendArmy(homeArmy, possibleTarget);
                    }
                //mobilize();
                //if (homeArmy.getSize() > 50 + Game.random.Next(100))
                //    sendArmy(homeArmy, getRandomNeighborProvince());
            }
    }

    public override void buyNeeds()
    {
        foreach (var pro in Product.allProducts)
        {
            // if I want to buy
            if (storageSet.getStorage(pro).get() > storageSet.getConsumption(pro).get() * 10)
                ;
            else
            {
                Storage toBuy = new Storage(pro, storageSet.getConsumption(pro).get() * 10 - storageSet.getStorage(pro).get());
                toBuy.multiple(Game.market.buy(this, toBuy, null));
                storageSet.add(toBuy);
                getCountryWallet().storageBuyingExpenseAdd(new Value(Game.market.getCost(toBuy)));


            }

        }
    }

    private float getStreght()
    {
        return howMuchCanMobilize();
    }

    private float howMuchCanMobilize()
    {
        float result = 0f;
        foreach (var pr in ownedProvinces)
            foreach (var po in pr.allPopUnits)
                if (po.type.canMobilize())
                    result += po.howMuchCanMobilize();
        return result;
    }

    private bool isAI()
    {
        return Game.player != this;
    }

    internal int getMenPopulation()
    {
        int result = 0;
        foreach (Province pr in ownedProvinces)
            result += pr.getMenPopulation();
        return result;
    }
    public int FindPopulationAmountByType(PopType ipopType)
    {
        int result = 0;
        foreach (Province pro in ownedProvinces)
            result += pro.getPopulationAmountByType(ipopType);
        return result;
    }


}
public class DontUseThatMethod : Exception
{
    /// <summary>
    /// Just create the exception
    /// </summary>
    public DontUseThatMethod()
      : base()
    {
    }

    /// <summary>
    /// Create the exception with description
    /// </summary>
    /// <param name="message">Exception description</param>
    public DontUseThatMethod(String message)
      : base(message)
    {
    }

    /// <summary>
    /// Create the exception with description and inner cause
    /// </summary>
    /// <param name="message">Exception description</param>
    /// <param name="innerException">Exception inner cause</param>
    public DontUseThatMethod(String message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Create the exception from serialized data.
    /// Usual scenario is when exception is occured somewhere on the remote workstation
    /// and we have to re-create/re-throw the exception on the local machine
    /// </summary>
    /// <param name="info">Serialization info</param>
    /// <param name="context">Serialization context</param>
    protected DontUseThatMethod(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
}