using Nashet.Utils;
using Nashet.ValueSpace;

namespace Nashet.EconomicSimulation
{
    public static class Options
    {
        // ARMY
        public static readonly float ArmyDefenceBonus = 0.5f;

        public static readonly float ArmyMobilizationFactor = 0.2f;
        public static readonly float ArmyMaxMoralChangePerTic = 0.05f;

        // COUNTRY
        public static readonly int CountryForHowMuchDaysMakeReservs = 20;

        public static readonly float CountryBadBoyWorldLimit = 0.1f;
        public static readonly int CountryTimeToForgetBattle = 40;
        public static readonly float defaultSciencePointMultiplier = 1.1f;//0.00001f;
        public static readonly Procent GovernmentTakesShareOfGoldOutput = new Procent(0.5f);
        public static readonly int ProvinceChanceToGetCore = 70;
        public static readonly Value CountryMinStorage = new Value(5f);
        public static readonly Value CountryMaxStorage = new Value(50f);
        public static readonly Value CountryBuyProductsForXDays = new Value(20f);
        public static readonly Value CountrySaveProductsDaysMaximum = new Value(40f);
        public static readonly Value CountryPopConsumptionLimitPE = new Value(40f);
        
        public static readonly Procent CountryOwnershipRiskRestoreSpeed = new Procent(0.001f);
        public static readonly Procent CountryOwnershipRiskDropOnNationalization = new Procent(0.01f);

        // MAP
        public static readonly int ProvinceLakeShance = 9; // bigger - less lakes

        public static readonly int ProvincesPerCountry = 6;// don't do it lees than 3 - ugly
        public static readonly float cellMultiplier = 1f;
        public static readonly int MapRedrawRate = 20; // bigger number - less often redraw

        // MARKET
        public static readonly MoneyView minPrice = new MoneyView(0.001m);

        public static readonly MoneyView maxPrice = new MoneyView(999.99m);
        public static readonly float MarketInfiniteDSB = 999f;
        public static readonly float MarketEqualityDSB = 1f;
        public static readonly float MarketZeroDSB = 0f;
        public static readonly float defaultPriceLimitMultiplier = 5f;

        //FACTORIES
        public static readonly float goldToCoinsConvert = 10f;

        public static readonly Procent minWorkforceFullfillingToUpgradeFactory = new Procent(0.75f);
        public static readonly Procent BuyInTimeFactoryUpgradeNeeds = new Procent(0.1f);
        public static readonly int minUnemploymentToInvestInFactory = 10;
        public static readonly int maximumFactoriesInUpgradeToBuildNew = 2;
        public static readonly byte maxFactoryLevel = 255;
        public static readonly Procent minMarginToInvest = new Procent(0.005f);

        public static readonly int maxDaysUnprofitableBeforeFactoryClosing = 90;
        public static readonly int maxDaysBuildingBeforeRemoving = 90; // 180;
        public static readonly int maxDaysClosedBeforeRemovingFactory = 90;
        public static readonly int minDaysBeforeSalaryCut = 2;
        public static readonly int howOftenCheckForFactoryReopenning = 30;

        public static readonly MoneyView factoryMoneyReservePerLevel = new MoneyView(20m);
        public static readonly ReadOnlyValue FactoryMarginToRiseSalary = new ReadOnlyValue(0.01f);
        public static readonly ReadOnlyValue FactoryMarginToDecreaseSalary = new ReadOnlyValue(0.005f);

        public static readonly float factoryEachLevelEfficiencyBonus = 0.05f;

        //public static float factoryHaveResourceInProvinceBonus = 0.2f;
        public static readonly int maxFactoryFireHireSpeed = 50;

        public static readonly Procent minFactoryWorkforceFulfillingToInvest = new Procent(0.70f);

        public static readonly int fabricConstructionTimeWithoutCapitalism = 20;
        public static readonly byte FactoryInputReservInDays = 2;
        public static readonly int FactoryMediumTierLevels = 10;
        public static readonly int FactoryMediumHighLevels = 16;
        public static readonly MoneyView FactoryMinPossibleSallary = new Money(0.001m);

        public static readonly Procent FactoryReduceSalaryOnNonProfit = new Procent(0.8f);//0.01m);
        public static readonly Procent FactoryReduceSalaryOnMarket = new Procent(0.9f);//0.001m);

        //Province
        /// <summary>In procent of unemployed</summary>
        public static Procent ProvinceExcessWorkforce = new Procent(0.15f);

        /// <summary>In procent of unemployed</summary>
        public static Procent ProvinceLackWorkforce = new Procent(0.05f);

        //POP MIFRATION?PROMOTION

        public static readonly ReadOnlyValue PopMinorityMigrationBarier = new ReadOnlyValue(0.6f);
        public static readonly ReadOnlyValue PopPopulationChangeChance = new ReadOnlyValue(0.1f);

        ///<summary> When popUnit can't fulfill needs it would demote to another class</summary>
        public static readonly Procent PopNeedsEscapingLimit = new Procent(0.333f);//0.33f

        /// <summary> New life should this better to start escaping</summary>
        public static readonly Procent PopNeedsEscapingBarrier = new Procent(0.01f); // was 0.1

        /// <summary> Pops richer than that would promote</summary>
        public static readonly Procent PopNeedsPromotionLimit = new Procent(0.4f); //0.5f);

        public static readonly Procent PopGrowthSpeed = new Procent(0.02f);
        public static readonly Procent PopStarvationSpeed = new Procent(0.1f);

        ///<summary> When popUnit can't fulfill needs it would demote to another class or migrate/immigrate</summary>
        public static readonly Procent PopDemotingSpeed = new Procent(0.04f);
        public static readonly Procent PopMigrationSpeed = new Procent(0.08f);

        //public static readonly Procent PopMigrationSpeed = new Procent(0.01f);
        //public static readonly Procent PopImmigrationSpeed = new Procent(0.01f);
        ///<summary> promotion  - when popUnit has chance to get better place in hierarchy</summary>
        public static readonly Procent PopPromotionSpeed = new Procent(0.02f);

        public static readonly Procent PopAssimilationSpeed = new Procent(0.04f);
        public static readonly Procent PopAssimilationSpeedWithEquality = new Procent(0.02f);

        ///<summary> Pop wouldn't select new life if there is unemployment higher than</summary>
        public static readonly ReadOnlyValue PopMigrationUnemploymentLimit = new ReadOnlyValue(0.1f);

        public static readonly ReadOnlyValue PopMigrationToUnknowAreaChance = new ReadOnlyValue(0.1f);
        public static readonly ReadOnlyValue PopSameCultureMigrationPreference = new ReadOnlyValue(0.1f);

        /// currently not used
        public static readonly int PopSizeConsolidationLimit = 100;

        /// <summary> Time before which pop wouldn't be wipe out by Pop change methods like promote\ assimilate\migrate</summary>
        public static readonly int PopAgeLimitToWipeOut = 50; //250;

        //ARTISANS
        public static readonly float ArtisansProductionModifier = 0.5f;

        public static readonly Procent ArtisansChangeProductionRate = new Procent(0.01f);

        /// <summary> change production with needs fulfilling lower than that /// </summary>
        public static readonly Value ArtisansChangeProductionLevel = new Value(0.3f);

        //POPS
        public static readonly float votingPassBillLimit = 0.5f;

        public static readonly float votingForcedReformPenalty = 0.5f;
        public static readonly int familySize = 5;
        public static readonly Procent savePopMoneyReserv = new Procent(0.5f);//0.66666f);
        public static readonly float PopMinLandForTribemen = 1f;
        public static readonly float PopMinLandForFarmers = 0.25f;
        public static readonly float PopMinLandForTownspeople = 0.01f;

        public static readonly int PopDaysUpsetByForcedReform = 30;
        public static readonly float PopAttritionFactor = 0.2f;

        public static readonly int PopRichStrataVotePower = 10;
        public static readonly int PopMiddleStrataVotePower = 3;
        public static readonly int PopMinimalMobilazation = 50;
        public static readonly Value PopLowLoyaltyToJoinMovevent = new Value(0.3f);
        public static readonly Value PopHighLoyaltyToleaveMovevent = new Value(0.4f);
        public static readonly Value PopLoyaltyLimitToRevolt = new Value(0.1f);
        public static readonly Procent PopLoyaltyBoostOnDiseredReformEnacted = new Procent(0.2f);
        public static readonly Procent PopLoyaltyBoostOnRevolutionWon = new Procent(0.6f);
        public static readonly Procent PopLoyaltyBoostOnRevolutionLost = new Procent(0.3f);
        public static readonly int PopChangeMovementRate = 30;
        public static readonly Procent MovementStrenthToStartRebellion = new Procent(1f);
        public static readonly Procent PopLoyaltyChangeOnAnnexStateCulture = new Procent(0.3f);
        public static readonly Procent PopLoyaltyChangeOnAnnexNonStateCulture = new Procent(0.2f);
        public static readonly MoneyView PopUnlimitedConsumptionLimit = new MoneyView(110m);

        public static readonly float aristocratsFoodReserv = 50;

        public static readonly Value PopStrataWeight = new Value(3f); // meaning 1 / 3
        public static readonly float PopOneThird = 0.333f;
        public static readonly float PopTwoThird = 0.666f;

        // INVESTING
        public static readonly Procent InvestingForeignCountrySecurity = new Procent(0.80f);

        /// <summary>
        /// There is bigger chance to loose property in another province
        /// </summary>
        public static readonly Procent InvestingAnotherProvinceSecurity = new Procent(0.90f);

        public static readonly Procent InvestorEmploymentSafety = new Procent(0.80f);
        public static readonly Procent RelationImpactOnGovernmentInvestment = new Procent(0.05f);
        public static readonly int PopDaysReservesBeforePuttingMoneyInBak = 10;

        /// <summary>For every nationalized enterprise</summary>
        public static readonly Procent PopLoyaltyDropOnNationalization = new Procent(0.1f);

        public static readonly Procent PopBuyAssetsAtTime = new Procent(0.05f);
        public static readonly Procent PopMarginToSellShares = new Procent(0.005f);
        public static readonly int PopInvestRate = 25;
        public static readonly int CountryInvestmentRate = 55;

        //EDUCATION
        public static readonly Procent PopEducationGrowthRate = new Procent(0.002f);

        public static readonly ReadOnlyValue PopEducationRegressChance = new ReadOnlyValue(0.01f);
        public static readonly ReadOnlyValue PopLearnByWorkingChance = new ReadOnlyValue(0.1f);
        public static readonly ReadOnlyValue PopLearnByWorkingLimit = new ReadOnlyValue(0.25f);

        public static readonly Date AIFisrtAllowedAttackOnHuman = new Date(30);

        public static readonly int ArmyTimeToOccupy = 12;


        //public static readonly Procent PopMinLoyaltyToMobilizeForGovernment = new Procent(0.12f);
    }
}