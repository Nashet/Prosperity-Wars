using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nashet.ValueSpace;
namespace Nashet.EconomicSimulation
{
    public static class Options
    {
        // ARMY
        internal static readonly float ArmyDefenceBonus = 0.5f;
        internal static readonly float ArmyMobilizationFactor = 0.2f;
        internal static readonly float ArmyMaxMoralChangePerTic = 0.05f;

        // COUNTRY
        internal static readonly int CountryForHowMuchDaysMakeReservs = 20;
        internal static readonly float CountryBadBoyWorldLimit = 0.1f;
        internal static readonly int CountryTimeToForgetBattle = 40;
        internal static readonly float defaultSciencePointMultiplier = 1.1f;//0.00001f; 
        internal static readonly float GovernmentTakesShareOfGoldOutput = 0.5f;
        internal static readonly int ProvinceChanceToGetCore = 70;
        internal static readonly Value CountryMinStorage = new Value(5f);
        internal static readonly Value CountryMaxStorage = new Value(50f);
        internal static readonly Value CountryBuyProductsForXDays = new Value(20f);
        internal static readonly Value CountrySaveProductsDaysMaximum = new Value(40f);
        internal static readonly Value CountryPopConsumptionLimitPE = new Value(40f);

        // MAP
        internal static readonly int ProvinceLakeShance = 9; // bigger - less lakes
        internal static readonly int ProvincesPerCountry = 6;// don't do it lees than 3 - ugly    
        internal static readonly float cellMultiplier = 1f;
        internal static readonly int MapRedrawRate = 20; // bigger number - less often redraw   

        // MARKET
        internal static readonly float minPrice = 0.001f;
        internal static readonly float maxPrice = 999.99f;
        internal static readonly float MarketInfiniteDSB = 999f;
        internal static readonly float MarketEqualityDSB = 1f;
        internal static readonly float MarketZeroDSB = 0f;
        internal static readonly Value defaultPriceLimitMultiplier = new Value(5f);

        //FACTORIES
        internal static readonly float goldToCoinsConvert = 10f;
        internal static readonly Procent minWorkforceFullfillingToUpgradeFactory = new Procent(0.75f);
        internal static readonly Procent BuyInTimeFactoryUpgradeNeeds = new Procent(0.1f);
        internal static readonly int minUnemploymentToBuldFactory = 10;
        internal static readonly int maximumFactoriesInUpgradeToBuildNew = 2;
        internal static readonly byte maxFactoryLevel = 255;
        internal static readonly float minMarginToUpgrade = 0.005f;

        internal static readonly int maxDaysUnprofitableBeforeFactoryClosing = 180;
        internal static readonly int maxDaysBuildingBeforeRemoving = 180; // 180;
        internal static readonly int maxDaysClosedBeforeRemovingFactory = 180;
        internal static readonly int minDaysBeforeSalaryCut = 2;
        internal static readonly int howOftenCheckForFactoryReopenning = 30;

        internal static readonly float factoryMoneyReservePerLevel = 20f;
        internal static readonly float minMarginToRiseSalary = 0.01f;
        internal static readonly float factoryEachLevelEfficiencyBonus = 0.05f;
        //internal static float factoryHaveResourceInProvinceBonus = 0.2f;
        internal static readonly int maxFactoryFireHireSpeed = 50;
        internal static readonly float minFactoryWorkforceFullfillingToBuildNew = 0.75f;

        internal static readonly int fabricConstructionTimeWithoutCapitalism = 20;
        internal static readonly byte FactoryInputReservInDays = 5;
        internal static readonly int FactoryMediumTierLevels = 8 + 1;
        internal static readonly int FactoryMediumHighLevels = 15 + 1;
        internal static readonly float FactoryMinPossibleSallary = 0.001f;

        //POPS

        internal static readonly float votingPassBillLimit = 0.5f;
        internal static readonly float votingForcedReformPenalty = 0.5f;
        internal static readonly int familySize = 5;
        internal static readonly Procent savePopMoneyReserv = new Procent(0.66666f);
        internal static readonly float PopMinLandForTribemen = 1f;
        internal static readonly float PopMinLandForFarmers = 0.25f;
        internal static readonly float PopMinLandForTownspeople = 0.0025f;

        public static readonly Procent PopGrowthSpeed = new Procent(0.002f);
        public static readonly Procent PopStarvationSpeed = new Procent(0.01f);
        ///<summary> When popUnit can't fulfill needs it would demote to another class or migrate/immigrate</summary>
        public static readonly Procent PopEscapingSpeed = new Procent(0.01f);
        //public static readonly Procent PopMigrationSpeed = new Procent(0.01f);
        //public static readonly Procent PopImmigrationSpeed = new Procent(0.01f);
        ///<summary> promotion  - when popUnit has chance to get better place in hierarchy</summary>
        public static readonly Procent PopPromotionSpeed = new Procent(0.01f);
        public static readonly Procent PopAssimilationSpeed = new Procent(0.002f);
        public static readonly Procent PopAssimilationSpeedWithEquality = new Procent(0.001f);

        ///<summary> When popUnit can't fulfill needs it would demote to another class or migrate/immigrate</summary>
        public static readonly Procent PopNeedsEscapingLimit = new Procent(0.33f);
        /// <summary> New life should this better to start escaping</summary>
        public static readonly Procent PopNeedsEscapingBarrier = new Procent(0.01f); // was 0.1

        //public static readonly Procent PopNeedsMigrationLimit = new Procent(0.33f);
        //public static readonly Procent PopNeedsImmigrationLimit = new Procent(0.33f);
        /// <summary> Pops richer than that would promote</summary>
        internal static readonly Procent PopNeedsPromotionLimit = new Procent(0.4f); //0.5f);

        public static readonly int PopSizeConsolidationLimit = 100;
        /// <summary> Time before which pop wouldn't be wipe out by Pop change methods like promote\ assimilate\migrate</summary>
        public static readonly int PopAgeLimitToWipeOut = 50; //250;

        internal static readonly int PopDaysUpsetByForcedReform = 30;
        internal static readonly float PopAttritionFactor = 0.2f;

        internal static readonly int PopRichStrataVotePower = 10;
        internal static readonly int PopMiddleStrataVotePower = 3;
        internal static readonly int PopMinimalMobilazation = 50;
        internal static readonly Value PopLowLoyaltyToJoinMovevent = new Value(0.3f);
        internal static readonly Value PopHighLoyaltyToleaveMovevent = new Value(0.4f);
        internal static readonly Value PopLoyaltyLimitToRevolt = new Value(0.1f);
        internal static readonly Procent PopLoyaltyBoostOnRevolutionWon = new Procent(0.8f);
        internal static readonly Procent PopLoyaltyBoostOnRevolutionLost = new Procent(0.3f);
        internal static readonly int PopChangeMovementRate = 30;
        internal static readonly Procent MovementStrenthToStartRebellion = new Procent(1f);
        internal static readonly Procent PopLoyaltyChangeOnAnnexStateCulture = new Procent(0.3f);
        internal static readonly Procent PopLoyaltyChangeOnAnnexNonStateCulture = new Procent(0.2f);
        internal static readonly Value PopUnlimitedConsumptionLimit = new Value(100f);


        internal static readonly float aristocratsFoodReserv = 50;
        internal static readonly float ArtisansProductionModifier = 0.5f;
        internal static readonly int ArtisansChangeProductionRate = 60;
        internal static readonly Value PopStrataWeight = new Value(3f); // meaning 1 / 3
        internal static readonly float PopOneThird = 0.333f;
        internal static readonly float PopTwoThird = 0.666f;
        /// <summary>/// change pr with needs fulfilling lower than that /// </summary>
        internal static readonly Value ArtisansChangeProductionLevel = new Value(0.2f);
        internal static readonly float PopDaysReservesBeforePuttingMoneyInBak = 10f;


        //internal static readonly Procent PopMinLoyaltyToMobilizeForGovernment = new Procent(0.12f);
    }
}