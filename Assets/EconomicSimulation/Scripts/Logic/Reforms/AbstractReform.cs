using Nashet.EconomicSimulation;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System;
using System.Collections.Generic;

public abstract class AbstractReform : Component<Country>, INameable, ISortableName, IClickable
{
    private readonly string description;
    protected readonly string name;
    protected readonly float nameWeight;

    protected IReformValue value;
    protected readonly List<IReformValue> possibleValues;

    public AbstractReform(string name, string indescription, Country country, List<IReformValue> possibleValues) : base(country)
    {
        this.name = name;
        if (name != null)
            nameWeight = name.GetWeight();
        description = indescription;
        country.reforms.Add(this);
        this.possibleValues = possibleValues;

    }
    public float getVotingPower(PopUnit forWhom)
    {
        return value.getVotingPower(forWhom);
    }

    public Procent LifeQualityImpact { get { return value.LifeQualityImpact; } }
    //public abstract bool isAvailable(Country country);


    //public abstract bool canHaveThatValue(AbstractNamdRfrmValue abstractNamdRfrmValue);
    public virtual void OnReformEnactedInProvince(Province province)
    { }
    public override bool Equals(Object another)
    {
        if (ReferenceEquals(another, null))
            throw new ArgumentNullException();
        return another is IReformValue && this == (IReformValue)another
            || another is AbstractReform && this == (AbstractReform)another;
    }
    public override int GetHashCode()
    {
        return name.GetHashCode();
    }
    public static bool operator ==(AbstractReform x, IReformValue y)
    {
        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            throw new ArgumentNullException();
        return x.value == y;
    }
    public static bool operator !=(AbstractReform x, IReformValue y)
    {
        return !(x == y);
    }

    public static bool operator ==(AbstractReform x, AbstractReform y)
    {
        if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            throw new ArgumentNullException();
        return x.value == y.value;
    }
    public static bool operator !=(AbstractReform x, AbstractReform y)
    {

        return !(x == y);
    }

    public virtual void SetValue(AbstractReform reform)
    {
        SetValue(reform.value);
    }
    public virtual void SetValue(IReformValue reformValue)
    {

        foreach (PopUnit pop in owner.Provinces.AllPops)
            if (pop.getSayingYes(reformValue))
            {
                pop.loyalty.Add(Options.PopLoyaltyBoostOnDiseredReformEnacted);
                pop.loyalty.clamp100();
            }
        var isThereSuchMovement = owner.movements.Find(x => x.getGoal() == reformValue);
        if (isThereSuchMovement != null)
        {
            isThereSuchMovement.onRevolutionWon(false);
        }
    }

    public string FullName
    {
        get { return description; }
    }
    public string ShortName
    {
        get { return name; }
    }

    public void OnClicked()
    {
        //MainCamera.politicsPanel.selectReform(this);
        MainCamera.politicsPanel.Refresh();
    }

    public float NameWeight
    {
        get
        {
            return nameWeight;
        }
    }
    public IEnumerable<IReformValue> AllPossibleValues
    {
        get
        {
            foreach (var item in possibleValues)
            {
                yield return item;
            }
        }
    }
}