using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PopUnitPanel : DragPanel
{
    public Text generaltext, luxuryNeedsText, everyDayNeedsText, lifeNeedsText;
    private PopUnit pop;
    // Use this for initialization
    void Start()
    {
        MainCamera.popUnitPanel = this;
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
                immigrationText = targetIM + " ("+targetIM.getCountry() + ") " + pop.getImmigrationSize();
            else
                immigrationText = "none";

            string assimilationText;            
            if (pop.culture != pop.province.getCountry().culture && pop.getAssimilationSize() > 0)
                assimilationText = pop.province.getCountry().culture + " " + pop.getAssimilationSize();
            else
                assimilationText = "none";
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
            if (pop.deposits.get() > 0f)
                loans = "\nDeposit: " + pop.deposits.ToString();

            generaltext.text = pop + "\n" + "Population: " + pop.getPopulation()
                + "\nCulture: " + pop.culture
                + "\nStorage: " + pop.storageNow.ToString()
                + "\nGain goods: " + pop.gainGoodsThisTurn.ToString()
                + "\nSent to market: " + pop.sentToMarket
                + "\nCash: " + pop.haveMoney.ToString()
                + "\nMoney income: " + pop.moneyIncomethisTurn
                
                + "\nDemotion: " + demotionText
                + "\nPromotion: " + promotionText
                + "\nMigration: " + migrationText
                + "\nImmigration: " + immigrationText
                + "\nAssimilation: " + assimilationText
                + "\nGrowth: " + pop.getGrowthSize()
                + "\nUnemployment: " + pop.getUnemployedProcent()
                + loans
                + "\nConsumed: " + pop.consumedTotal + " cost: " + Game.market.getCost(pop.consumedTotal)
                + "\n\nLife needs: " + lifeNeeds + "\nEveryday needs: " + everyDayNeeds + "\nLuxury needs: " + luxuryNeeds
                + "\nAge: " + pop.getAge()
                ;
            //if (Game.devMode)
            //    generaltext.text += "\nConsumedLT: " + pop.consumedLastTurn + " cost: " + Game.market.getCost(pop.consumedLastTurn)
            //    + "\nConsumedIM: " + pop.consumedInMarket + " cost: " + Game.market.getCost(pop.consumedInMarket);

            //+ "\nExpenses:"
        }
    }
    public void show(PopUnit ipopUnit)
    {
        gameObject.SetActive(true);
        pop = ipopUnit;
        panelRectTransform.SetAsLastSibling();
    }
}
