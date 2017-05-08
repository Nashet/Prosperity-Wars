using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Options

{
    internal static float minPrice = 0.001f;
    internal static float maxPrice = 999.99f;
    internal static int familySize = 5;

    internal static float cellMultiplier = 2f;
    internal static float goldToCoinsConvert = 10f;
    internal static float minWorkforceFullfillingToUpgradeFactory = 0.75f;
    internal static Procent BuyInTimeFactoryUpgradeNeeds = new Procent(0.1f);
    internal static int minUnemploymentToBuldFactory = 10;
    internal static int maximumFactoriesInUpgradeToBuildNew = 2;
    internal static byte maxFactoryLevel = 255;
    internal static float minMarginToUpgrade = 0.005f;
    internal static float minLandForTribemen = 1f;
    internal static float minLandForFarmers = 0.25f;
    internal static int maxDaysUnprofitableBeforeFactoryClosing = 180;
    internal static int maxDaysBuildingBeforeRemoving = 180; // 180;
    internal static int maxDaysClosedBeforeRemovingFactory = 180;
    internal static int minDaysBeforeSalaryCut = 2;
    internal static int howOftenCheckForFactoryReopenning = 30;
    internal static Procent savePopMoneyReserv = new Procent(0.66666f);
    internal static float factoryMoneyReservPerLevel = 20f;
    internal static float minMarginToRiseSalary = 0.1f;
    internal static float factoryEachLevelEfficiencyBonus = 0.05f;
    //internal static float factoryHaveResourceInProvinceBonus = 0.2f;
    internal static int maxFactoryFireHireSpeed = 50;
    internal static float minFactoryWorkforceFullfillingToBuildNew = 0.75f;
    internal static float defaultSciencePointMultiplier = 0.001f; //0.00001f;
    internal static int fabricConstructionTimeWithoutCapitalism = 20;
    internal static float aristocratsFoodReserv = 50;
    internal static float votingPassBillLimit = 0.5f;
    internal static float votingForcedReformPenalty = 0.5f;
   
    
    internal static Value defaultPriceLimitMultiplier = new Value(5f);
    internal static int PopDaysUpsetByForcedReform = 30;
    internal static float GovernmentTakesShareOfGoldOutput = 0.5f;
    internal static byte factoryInputReservInDays = 5;
    internal static readonly float mobilizationFactor = 0.2f;

    internal static float PopAttritionFactor = 0.2f;
    internal static float armyDefenceBonus = 0.5f;
}
