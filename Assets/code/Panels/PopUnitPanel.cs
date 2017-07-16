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
            efficiencyText.text = "Efficiency: " + PopUnit.modEfficiency.getModifier(pop, out efficiencyText.GetComponentInChildren<ToolTipHandler>().tooltip);

            issues.GetComponentInChildren<ToolTipHandler>().setDynamicString(() => pop.getIssues().getString(" willing ", "\n"));

            string demotionText;
            var target = pop.getRichestDemotionTarget();
            if (pop.wantsToDemote() && target != null && pop.getDemotionSize() > 0)
                demotionText = target + " " + pop.getDemotionSize();
            else
                demotionText = "none";

            string promotionText;
            var targetPro = pop.getRichestPromotionTarget();
            if (pop.wantsToPromote() && targetPro != null && pop.getPromotionSize() > 0)
                promotionText = targetPro + " " + pop.getPromotionSize();
            else
                promotionText = "none";

            string migrationText;
            var targetM = pop.getRichestMigrationTarget();
            if (pop.wantsToMigrate() && targetM != null && pop.getMigrationSize() > 0)
                migrationText = targetM + " " + pop.getMigrationSize();
            else
                migrationText = "none";

            string immigrationText;
            var targetIM = pop.getRichestImmigrationTarget();
            if (pop.wantsToImmigrate() && targetIM != null && pop.getImmigrationSize() > 0)
                immigrationText = targetIM + " (" + targetIM.getCountry() + ") " + pop.getImmigrationSize();
            else
                immigrationText = "none";

            string assimilationText;
            if (pop.culture != pop.province.getCountry().getCulture() && pop.getAssimilationSize() > 0)
                assimilationText = pop.province.getCountry().getCulture() + " " + pop.getAssimilationSize();
            else
                assimilationText = "none";

            sb.Append(pop);
            sb.Append("\nPopulation: ").Append(pop.getPopulation());

            //if (Game.devMode)
            sb.Append("\nStorage: ").Append(pop.storageNow.ToString());
            sb.Append("\nGain goods: ").Append(pop.gainGoodsThisTurn.ToString());
            sb.Append("\nSent to market: ").Append(pop.sentToMarket);  // hide it

            sb.Append("\nDemotion: ").Append(demotionText);
            sb.Append("\nPromotion: ").Append(promotionText);
            sb.Append("\nMigration: ").Append(migrationText);
            sb.Append("\nImmigration: ").Append(immigrationText);
            sb.Append("\nAssimilation: ").Append(assimilationText);
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
                generaltext.text += "\nConsumedLT: " + pop.consumedLastTurn + " cost: " + Game.market.getCost(pop.consumedLastTurn)
                + "\nConsumedIM: " + pop.consumedInMarket + " cost: " + Game.market.getCost(pop.consumedInMarket);

            generaltext.text = sb.ToString();

            sb.Clear();
            sb.Append("Life needs: ").Append(pop.getLifeNeedsFullfilling().ToString()).Append(" fulfilled");
            lifeNeedsText.GetComponentInChildren<ToolTipHandler>().setDynamicString(() => "Wants:\n" + pop.getRealLifeNeeds().getString("\n"));
            lifeNeedsText.text = sb.ToString();

            sb.Clear();
            sb.Append("Everyday needs: ").Append(pop.getEveryDayNeedsFullfilling().ToString()).Append(" fulfilled");
            everyDayNeedsText.GetComponentInChildren<ToolTipHandler>().setDynamicString(() => "Wants:\n" + pop.getRealEveryDayNeeds().getString("\n"));
            everyDayNeedsText.text = sb.ToString();

            sb.Clear();
            sb.Append("Luxury needs: ").Append(pop.getLuxuryNeedsFullfilling().ToString()).Append(" fulfilled");
            luxuryNeedsText.GetComponentInChildren<ToolTipHandler>().setDynamicString(() => "Wants:\n" + pop.getRealLuxuryNeeds().getString("\n"));
            luxuryNeedsText.text = sb.ToString();

            sb.Clear();
            sb.Append("Cash: ").Append(pop.cash.ToString());
            money.text = sb.ToString();
            money.GetComponentInChildren<ToolTipHandler>().setDynamicString(() => "Money income: " + pop.moneyIncomethisTurn
            + "\nIncome tax: " + pop.incomeTaxPayed
            + "\nConsumed cost: " + Game.market.getCost(pop.consumedTotal));
        }
    }
    public void show(PopUnit ipopUnit)
    {
        gameObject.SetActive(true);
        pop = ipopUnit;
        panelRectTransform.SetAsLastSibling();
    }
}
