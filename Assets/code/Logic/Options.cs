using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Options

{
    internal static readonly float minPrice = 0.001f;
    internal static readonly float maxPrice = 999.99f;
    internal static readonly int familySize = 5;

    internal static readonly float cellMultiplier = 2f;
    internal static readonly float goldToCoinsConvert = 10f;
    internal static readonly float minWorkforceFullfillingToUpgradeFactory = 0.75f;
    internal static readonly Procent BuyInTimeFactoryUpgradeNeeds = new Procent(0.1f);
    internal static readonly int minUnemploymentToBuldFactory = 10;
    internal static readonly int maximumFactoriesInUpgradeToBuildNew = 2;
    internal static readonly byte maxFactoryLevel = 255;
    internal static readonly float minMarginToUpgrade = 0.005f;
    internal static readonly float minLandForTribemen = 1f;
    internal static readonly float minLandForFarmers = 0.25f;
    internal static readonly int maxDaysUnprofitableBeforeFactoryClosing = 180;
    internal static readonly int maxDaysBuildingBeforeRemoving = 180; // 180;
    internal static readonly int maxDaysClosedBeforeRemovingFactory = 180;
    internal static readonly int minDaysBeforeSalaryCut = 2;
    internal static readonly int howOftenCheckForFactoryReopenning = 30;
    internal static readonly Procent savePopMoneyReserv = new Procent(0.66666f);
    internal static readonly float factoryMoneyReservPerLevel = 20f;
    internal static readonly float minMarginToRiseSalary = 0.1f;
    internal static readonly float factoryEachLevelEfficiencyBonus = 0.05f;
    //internal static float factoryHaveResourceInProvinceBonus = 0.2f;
    internal static readonly int maxFactoryFireHireSpeed = 50;
    internal static readonly float minFactoryWorkforceFullfillingToBuildNew = 0.75f;
    internal static readonly float defaultSciencePointMultiplier = 0.0001f; //0.00001f;
    internal static readonly int fabricConstructionTimeWithoutCapitalism = 20;
    internal static readonly float aristocratsFoodReserv = 50;
    internal static readonly float votingPassBillLimit = 0.5f;
    internal static readonly float votingForcedReformPenalty = 0.5f;


    internal static readonly Value defaultPriceLimitMultiplier = new Value(5f);
    internal static readonly int PopDaysUpsetByForcedReform = 30;
    internal static readonly float GovernmentTakesShareOfGoldOutput = 0.5f;
    internal static readonly byte factoryInputReservInDays = 5;
    internal static readonly float mobilizationFactor = 0.2f;

    internal static readonly float PopAttritionFactor = 0.2f;
    internal static readonly float armyDefenceBonus = 0.5f;

    public static readonly Procent PopGrowthSpeed = new Procent(0.002f);
    public static readonly Procent PopStarvationSpeed = new Procent(0.01f);
    ///<summary> demotion  - when popUnit can't fulfill needs</summary>
    public static readonly Procent PopDemotionSpeed = new Procent(0.01f);
    ///<summary> promotion  - when popUnit has chance to get better place in hierarchy</summary>
    public static readonly Procent PopPromotionSpeed = new Procent(0.01f);
    public static readonly Procent PopAssimilationSpeed = new Procent(0.002f);
    public static readonly Procent PopMigrationSpeed = new Procent(0.01f);
    public static readonly Procent PopImmigrationSpeed = new Procent(0.01f);

    public static readonly Procent PopNeedsDemotionLimit = new Procent(0.5f);
    public static readonly Procent PopNeedsMigrationLimit = new Procent(0.33f);
    internal static readonly Procent PopNeedsImmigrationLimit = new Procent(0.33f);
    /// <summary> Pops richer than that would promote</summary>
    internal static readonly Procent PopNeedsPromotionLimit = new Procent(0.5f); //0.5f);

    public static readonly int PopSizeConsolidationLimit = 100;
    /// <summary> Time before which pop wouldn't be wipe out by Pop change methods like promote\ assimilate\migrate</summary>
    public static readonly int PopAgeLimitToWipeOut = 250;

    internal static readonly float MaxMoralChangePerTic = 0.05f;
    internal static readonly int PopRichStrataVotePower = 5;
    internal static readonly int CountryForHowMuchDaysMakeReservs = 20;
    internal static readonly int ProvinceChanceToGetCore = 25;
    internal static readonly float CountryBadBoyWorldLimit = 0.25f;
    internal static readonly int FactoryMediumTierLevels = 8+1;
    internal static readonly int FactoryMediumHighLevels = 15+1;
}
