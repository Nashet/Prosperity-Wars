using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;

public class PopUnitPanel : DragPanel
{
    public Text generaltext, luxuryNeedsText, everyDayNeedsText, lifeNeedsText, efficiencyText, issues, money;
    private PopUnit pop;
    // Use this for initialization
    void Start()
    {
        MainCamera.popUnitPanel = this;
        GetComponent<RectTransform>().position = new Vector2(900f, -58 + Screen.height);
        hide();
    }
    public PopUnit whomShowing()
    {
        return pop;
    }
    // Update is called once per frame
    void Update()
    {
        //refresh();
    }
    public void refresh()
    {
        if (pop != null)
        {
            var sb = new StringBuilder();

            sb.Append(pop);
            sb.Append("\nPopulation: ").Append(pop.getPopulation());

            //if (Game.devMode)
            sb.Append("\nStorage: ").Append(pop.storageNow.ToString());
            Artisans isArtisan = pop as Artisans;
            if (isArtisan != null)
            {
                sb.Append(", input products:  ").Append(isArtisan.getInputProducts());
                if (isArtisan.getType() != null)
                    sb.Append("\nProducing: ").Append(isArtisan.getType().basicProduction.getProduct());
            }
            sb.Append("\nGain goods: ").Append(pop.gainGoodsThisTurn.ToString());
            sb.Append("\nSent to market: ").Append(pop.sentToMarket);  // hide it

            makeLine(sb, pop.getRichestDemotionTarget(), pop.getDemotionSize(), "Demotion: ", pop.wantsToDemote());
            makeLine(sb, pop.getRichestPromotionTarget(), pop.getPromotionSize(), "Promotion: ", pop.wantsToPromote());

            makeLine(sb, pop.getRichestMigrationTarget(), pop.getMigrationSize(), "Migration: ", pop.wantsToMigrate());
            makeLineC(sb, pop.getRichestImmigrationTarget(), pop.getImmigrationSize(), "Immigration: ", pop.wantsToImmigrate());

            sb.Append("\nAssimilation: ");
            if (pop.culture != pop.getCountry().getCulture() && pop.getAssimilationSize() > 0)
                sb.Append(pop.getCountry().getCulture()).Append(" ").Append(pop.getAssimilationSize());
            else
                sb.Append("none");

            sb.Append("\nGrowth: ").Append(pop.getGrowthSize());
            sb.Append("\nUnemployment: ").Append(pop.getUnemployedProcent());
            sb.Append("\nLoyalty: ").Append(pop.loyalty);

            if (pop.loans.get() > 0f)
                sb.Append("\nLoan: ").Append(pop.loans.ToString());// hide it
            if (pop.deposits.get() > 0f)
                sb.Append("\nDeposit: ").Append(pop.deposits.ToString());// hide it

            sb.Append("\nAge: ").Append(pop.getAge());
            sb.Append("\nMobilized: ").Append(pop.getMobilized());
            if (pop.getMovement() != null)
                sb.Append("\nMember of ").Append(pop.getMovement());
            sb.Append("\nConsumed: ").Append(pop.consumedTotal);

            if (Game.devMode)
            sb.Append("\nConsumedLT: ").Append(pop.consumedLastTurn).Append(" cost: ").Append(Game.market.getCost(pop.consumedLastTurn)
                ).Append("\nConsumedIM: ").Append(pop.consumedInMarket).Append(" cost: ").Append(Game.market.getCost(pop.consumedInMarket));

            generaltext.text = sb.ToString();

            sb.Clear();
            sb.Append("Life needs: ").Append(pop.getLifeNeedsFullfilling().ToString()).Append(" fulfilled");
            lifeNeedsText.GetComponentInChildren<ToolTipHandler>().setDynamicString(() => " Life needs wants:\n" + pop.getRealLifeNeeds().getString("\n"));
            lifeNeedsText.text = sb.ToString();

            sb.Clear();
            sb.Append("Everyday needs: ").Append(pop.getEveryDayNeedsFullfilling().ToString()).Append(" fulfilled");
            everyDayNeedsText.GetComponentInChildren<ToolTipHandler>().setDynamicString(() => "Everyday needs wants:\n" + pop.getRealEveryDayNeeds().getString("\n"));
            everyDayNeedsText.text = sb.ToString();

            sb.Clear();
            sb.Append("Luxury needs: ").Append(pop.getLuxuryNeedsFullfilling().ToString()).Append(" fulfilled");
            luxuryNeedsText.GetComponentInChildren<ToolTipHandler>().setDynamicString(() => "Luxury needs wants:\n" + pop.getRealLuxuryNeeds().getString("\n"));
            luxuryNeedsText.text = sb.ToString();

            sb.Clear();
            sb.Append("Cash: ").Append(pop.cash.ToString());
            money.text = sb.ToString();
            money.GetComponentInChildren<ToolTipHandler>().setDynamicString(() => "Money income: " + pop.moneyIncomethisTurn
            + "\nIncome tax: " + pop.incomeTaxPayed
            + "\nConsumed cost: " + Game.market.getCost(pop.consumedTotal));

            efficiencyText.text = "Efficiency: " + PopUnit.modEfficiency.getModifier(pop, out efficiencyText.GetComponentInChildren<ToolTipHandler>().tooltip);
            issues.GetComponentInChildren<ToolTipHandler>().setDynamicString(() => pop.getIssues().getString(" willing ", "\n"));
        }
    }
    static private void makeLine(StringBuilder sb, PopType target, int size, string header, bool boolCheck)
    {
        //sb.Clear();
        sb.Append("\n").Append(header);

        if (boolCheck && target != null && size > 0)
            sb.Append(target).Append(" ").Append(size);
        else
            sb.Append("none");
    }
    static private void makeLine(StringBuilder sb, Province target, int size, string header, bool boolCheck)
    {
        //sb.Clear();
        sb.Append("\n").Append(header);
        if (boolCheck && target != null && size > 0)
            sb.Append(target).Append(" ").Append(size);
        else
            sb.Append("none");
    }
    static private void makeLineC(StringBuilder sb, Province target, int size, string header, bool boolCheck)
    {
        //sb.Clear();
        sb.Append("\n").Append(header);
        if (boolCheck && target != null && size > 0)
            sb.Append(target.getCountry()).Append(" (").Append(target).Append(") ").Append(size);
        else
            sb.Append("none");
    }
    public void show(PopUnit ipopUnit)
    {
        gameObject.SetActive(true);
        pop = ipopUnit;
        panelRectTransform.SetAsLastSibling();
    }
}
