using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PopUnitPanel : DragPanel
{


    public GameObject popUnitPanel;
    public Text generaltext, luxuryNeedsText, everyDayNeedsText, lifeNeedsText;
    private PopUnit pop;
    // Use this for initialization
    void Start()
    {
        MainCamera.popUnitPanel = this;
        Hide();
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
            string demotionText;
            var target = pop.getRichestDemotionTarget();
            if (pop.wantsToDemote() && target != null && pop.getDemotionSize() > 0)
                demotionText = target + " " + pop.getDemotionSize();
            else
                demotionText = "none";
            string migrationText;
            var targetM = pop.getRichestMigrationTarget();
            if (pop.wantsToMigrate() && targetM != null && pop.getMigrationSize() > 0)
                migrationText = targetM + " " + pop.getDemotionSize();
            else
                migrationText = "none";

            string lifeNeeds = ""; string everyDayNeeds = ""; string luxuryNeeds = "";

            var temp = pop.getRealLifeNeeds();
            foreach (Storage next in temp)
                lifeNeeds += next.ToString() + "; ";
            lifeNeeds += pop.getLifeNeedsFullfilling().ToString() + " fulfilled";

            temp = pop.getRealEveryDayNeeds();
            foreach (Storage next in temp)
                everyDayNeeds += next.ToString() + "; ";
            everyDayNeeds += pop.getEveryDayNeedsFullfilling().ToString() + " fulfilled";

            temp = pop.getRealLuxuryNeeds();
            foreach (Storage next in temp)
                luxuryNeeds += next.ToString() + "; ";
            luxuryNeeds += pop.getLuxuryNeedsFullfilling().ToString() + " fulfilled";

            //foreach (Storage next in pop.consumedTotal)
            //    consumedTotal+= next.ToString() + "; ";
            //luxuryNeeds += pop.getLuxuryNeedsFullfilling().ToString() + " fulfilled";

            string loans = "";
            if (pop.loans.get() > 0f)
                loans = "\nLoan: " + pop.loans.ToString();

            generaltext.text = pop + "\n" + "Population: " + pop.getPopulation() + "\nStorage: " + pop.storageNow.ToString()
                + "\nGain goods: " + pop.gainGoodsThisTurn.ToString()
                + "\nSent to market: " + pop.sentToMarket
                + "\nCash: " + pop.wallet.ToString()
                + "\nMoney income: " + pop.wallet.moneyIncomethisTurn
                + "\nConsumed: " + pop.consumedTotal + " cost: " + Game.market.getCost(pop.consumedTotal)
                + "\nDemotion: " + demotionText 
                + "\nMigration: " + migrationText
                + "\nGrowth: " + pop.getGrowthSize()
                + "\nUnemployment: " + pop.getUnemployedProcent() + loans
                + "\n\nLife needs: " + lifeNeeds + "\nEveryday needs: " + everyDayNeeds + "\nLuxury needs: " + luxuryNeeds
                ;
            if (Game.devMode)
                generaltext.text += "\nConsumedLT: " + pop.consumedLastTurn + " cost: " + Game.market.getCost(pop.consumedLastTurn)
                + "\nConsumedIM: " + pop.consumedInMarket + " cost: " + Game.market.getCost(pop.consumedInMarket);

            //+ "\nExpenses:"
        }
    }
    public void show(PopUnit ipopUnit)
    {
        popUnitPanel.SetActive(true);
        pop = ipopUnit;
        panelRectTransform.SetAsLastSibling();
    }
    public void Hide()
    {
        popUnitPanel.SetActive(false);
    }
    public void onCloseClick()
    {
        Hide();
    }


}
