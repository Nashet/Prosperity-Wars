﻿using Nashet.Utils;
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
        internal static readonly Procent GovernmentTakesShareOfGoldOutput = new Procent(0.5f);
        internal static readonly int ProvinceChanceToGetCore = 70;
        internal static readonly Value CountryMinStorage = new Value(5f);
        internal static readonly Value CountryMaxStorage = new Value(50f);
        internal static readonly Value CountryBuyProductsForXDays = new Value(20f);
        internal static readonly Value CountrySaveProductsDaysMaximum = new Value(40f);
        internal static readonly Value CountryPopConsumptionLimitPE = new Value(40f);
        internal static readonly int CountryInvestmentRate = 45;
        internal static readonly Procent CountryOwnershipRiskRestoreSpeed = new Procent(0.001f);
        internal static readonly Procent CountryOwnershipRiskDropOnNationalization = new Procent(0.01f);

        // MAP
        internal static readonly int ProvinceLakeShance = 9; // bigger - less lakes

        internal static readonly int ProvincesPerCountry = 6;// don't do it lees than 3 - ugly
        internal static readonly float cellMultiplier = 1f;
        internal static readonly int MapRedrawRate = 20; // bigger number - less often redraw

        // MARKET
        internal static readonly MoneyView minPrice = new MoneyView(0.001m);

        internal static readonly MoneyView maxPrice = new MoneyView(999.99m);
        internal static readonly float MarketInfiniteDSB = 999f;
        internal static readonly float MarketEqualityDSB = 1f;
        internal static readonly float MarketZeroDSB = 0f;
        internal static readonly float defaultPriceLimitMultiplier = 5f;

        //FACTORIES
        internal static readonly float goldToCoinsConvert = 10f;

        internal static readonly Procent minWorkforceFullfillingToUpgradeFactory = new Procent(0.75f);
        internal static readonly Procent BuyInTimeFactoryUpgradeNeeds = new Procent(0.1f);
        internal static readonly int minUnemploymentToInvestInFactory = 10;
        internal static readonly int maximumFactoriesInUpgradeToBuildNew = 2;
        internal static readonly byte maxFactoryLevel = 255;
        internal static readonly Procent minMarginToInvest = new Procent(0.005f);

        internal static readonly int maxDaysUnprofitableBeforeFactoryClosing = 90;
        internal static readonly int maxDaysBuildingBeforeRemoving = 90; // 180;
        internal static readonly int maxDaysClosedBeforeRemovingFactory = 90;
        internal static readonly int minDaysBeforeSalaryCut = 2;
        internal static readonly int howOftenCheckForFactoryReopenning = 30;

        internal static readonly MoneyView factoryMoneyReservePerLevel = new MoneyView(20m);
        internal static readonly Procent minMarginToRiseSalary = new Procent(0.01f);
        internal static readonly float factoryEachLevelEfficiencyBonus = 0.05f;

        //internal static float factoryHaveResourceInProvinceBonus = 0.2f;
        internal static readonly int maxFactoryFireHireSpeed = 50;

        internal static readonly Procent minFactoryWorkforceFulfillingToInvest = new Procent(0.70f);

        internal static readonly int fabricConstructionTimeWithoutCapitalism = 20;
        internal static readonly byte FactoryInputReservInDays = 2;
        internal static readonly int FactoryMediumTierLevels = 10;
        internal static readonly int FactoryMediumHighLevels = 16;
        internal static readonly MoneyView FactoryMinPossibleSallary = new Money(0.001m);

        internal static readonly MoneyView FactoryReduceSalaryOnNonProfit = new MoneyView(0.01m);
        internal static readonly MoneyView FactoryReduceSalaryOnMarket = new MoneyView(0.001m);

        //Province
        /// <summary>In procent of unemployed</summary>
        public static Procent ProvinceExcessWorkforce = new Procent(0.15f);

        /// <summary>In procent of unemployed</summary>
        public static Procent ProvinceLackWorkforce = new Procent(0.05f);

        //POP MIFRATION?PROMOTION
        internal static readonly ReadOnlyValue PopPopulationChangeChance = new ReadOnlyValue(0.1f);

        ///<summary> When popUnit can't fulfill needs it would demote to another class or migrate/immigrate</summary>
        public static readonly Procent PopNeedsEscapingLimit = new Procent(0.333f);//0.33f

        /// <summary> New life should this better to start escaping</summary>
        public static readonly Procent PopNeedsEscapingBarrier = new Procent(0.01f); // was 0.1

        /// <summary> Pops richer than that would promote</summary>
        internal static readonly Procent PopNeedsPromotionLimit = new Procent(0.4f); //0.5f);

        public static readonly Procent PopGrowthSpeed = new Procent(0.02f);
        public static readonly Procent PopStarvationSpeed = new Procent(0.1f);

        ///<summary> When popUnit can't fulfill needs it would demote to another class or migrate/immigrate</summary>
        public static readonly Procent PopEscapingSpeed = new Procent(0.04f);

        //public static readonly Procent PopMigrationSpeed = new Procent(0.01f);
        //public static readonly Procent PopImmigrationSpeed = new Procent(0.01f);
        ///<summary> promotion  - when popUnit has chance to get better place in hierarchy</summary>
        public static readonly Procent PopPromotionSpeed = new Procent(0.02f);

        public static readonly Procent PopAssimilationSpeed = new Procent(0.02f);
        public static readonly Procent PopAssimilationSpeedWithEquality = new Procent(0.01f);

        ///<summary> Pop wouldn't select new life if there is unemployment hire than</summary>
        internal static readonly ReadOnlyValue PopMigrationUnemploymentLimit = new ReadOnlyValue(0.1f);

        internal static readonly ReadOnlyValue PopMigrationToUnknowAreaChance = new ReadOnlyValue(0.1f);
        internal static readonly ReadOnlyValue PopSameCultureMigrationPreference = new ReadOnlyValue(0.1f);

        /// currently not used
        public static readonly int PopSizeConsolidationLimit = 100;

        /// <summary> Time before which pop wouldn't be wipe out by Pop change methods like promote\ assimilate\migrate</summary>
        public static readonly int PopAgeLimitToWipeOut = 50; //250;

        //ARTISANS
        internal static readonly float ArtisansProductionModifier = 0.5f;

        internal static readonly Procent ArtisansChangeProductionRate = new Procent(0.01f);

        /// <summary> change production with needs fulfilling lower than that /// </summary>
        internal static readonly Value ArtisansChangeProductionLevel = new Value(0.3f);

        //POPS
        internal static readonly float votingPassBillLimit = 0.5f;

        internal static readonly float votingForcedReformPenalty = 0.5f;
        internal static readonly int familySize = 5;
        internal static readonly Procent savePopMoneyReserv = new Procent(0.5f);//0.66666f);
        internal static readonly float PopMinLandForTribemen = 1f;
        internal static readonly float PopMinLandForFarmers = 0.25f;
        internal static readonly float PopMinLandForTownspeople = 0.01f;

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
        internal static readonly MoneyView PopUnlimitedConsumptionLimit = new MoneyView(110m);

        internal static readonly float aristocratsFoodReserv = 50;

        internal static readonly Value PopStrataWeight = new Value(3f); // meaning 1 / 3
        internal static readonly float PopOneThird = 0.333f;
        internal static readonly float PopTwoThird = 0.666f;

        // INVESTING
        internal static readonly Procent InvestingForeignCountrySecurity = new Procent(0.95f);

        /// <summary>
        /// There is bigger chance to loose property in another province
        /// </summary>
        internal static readonly Procent InvestingAnotherProvinceSecurity = new Procent(0.90f);

        internal static readonly Procent InvestorEmploymentSafety = new Procent(0.70f);
        internal static readonly Procent RelationImpactOnGovernmentInvestment = new Procent(0.05f);
        internal static readonly int PopDaysReservesBeforePuttingMoneyInBak = 10;

        /// <summary>For every nationalized enterprise</summary>
        internal static readonly Procent PopLoyaltyDropOnNationalization = new Procent(0.1f);

        internal static readonly Procent PopBuyAssetsAtTime = new Procent(0.05f);
        internal static readonly Procent PopMarginToSellShares = new Procent(0.005f);
        internal static readonly int PopInvestRate = 15;

        //EDUCATION
        internal static readonly Procent PopEducationGrowthRate = new Procent(0.002f);

        internal static readonly ReadOnlyValue PopEducationRegressChance = new ReadOnlyValue(0.01f);
        internal static readonly ReadOnlyValue PopLearnByWorkingChance = new ReadOnlyValue(0.1f);
        internal static readonly ReadOnlyValue PopLearnByWorkingLimit = new ReadOnlyValue(0.25f);

        internal static readonly Date AIFisrtAllowedAttackOnHuman = new Date(30);
        internal static readonly ReadOnlyValue PopMinorityMigrationBarier = new ReadOnlyValue(0.4f);

        //internal static readonly Procent PopMinLoyaltyToMobilizeForGovernment = new Procent(0.12f);
    }
}