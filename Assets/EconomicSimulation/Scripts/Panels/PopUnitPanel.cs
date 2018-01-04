using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;
using System.Linq;
namespace Nashet.EconomicSimulation
{
    public class PopUnitPanel : DragPanel
    {
        [SerializeField]
        private Text generaltext, luxuryNeedsText, everyDayNeedsText, lifeNeedsText, efficiencyText, issues, money;
        private PopUnit pop;
        // Use this for initialization
        void Start()
        {
            MainCamera.popUnitPanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(600f, 53f);
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
                sb.Append("\nStorage: ").Append(pop.storage.ToString());
                Artisans isArtisan = pop as Artisans;
                if (isArtisan != null)
                {
                    sb.Append(", input products:  ").Append(isArtisan.getInputProducts());
                    sb.Append("\nProducing: ");
                    if (isArtisan.getType() == null)
                        sb.Append("nothing");
                    else
                        sb.Append(isArtisan.getType().basicProduction.getProduct());
                }
                sb.Append("\nGain goods: ").Append(pop.getGainGoodsThisTurn().ToString());
                sb.Append("\nSent to market: ").Append(pop.getSentToMarket());  // hide it
                makeLine(sb, pop.getRichestPromotionTarget(), pop.getPromotionSize(), "Promotion: ", pop.wantsToPromote());

                if (pop.getLastEscapeSize() != 0)
                    makeLineNew(sb, pop.getLastEscapeTarget(), pop.getLastEscapeSize());
                else
                    sb.Append("\nNo demotions\\migrations\\immigrations");

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
                sb.Append("\nConsumed: ").Append(pop.getConsumed());

                //if (Game.devMode)
                //    sb.Append("\nConsumedLT: ").Append(pop.getConsumedLastTurn()).Append(" cost: ").Append(Game.market.getCost(pop.getConsumedLastTurn())
                //        ).Append("\nConsumedIM: ").Append(pop.getConsumedInMarket()).Append(" cost: ").Append(Game.market.getCost(pop.getConsumedInMarket()));

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
                + "\nConsumed cost: " + Game.market.getCost(pop.getConsumed()));

                efficiencyText.text = "Efficiency: " + PopUnit.modEfficiency.getModifier(pop, out efficiencyText.GetComponentInChildren<ToolTipHandler>().tooltip);
                issues.GetComponentInChildren<ToolTipHandler>().setDynamicString(
                    delegate ()
                    {
                    //var list = pop.getIssues().Values.ToList().Sort();
                    var items = from pair in pop.getIssues()
                                    orderby pair.Value descending
                                    select pair;
                        return items.getString(" willing ", "\n");
                    }
                    //() => pop.getIssues().ToList().getString(" willing ", "\n")

                    );
            }
        }
        private void makeLine(StringBuilder sb, IEscapeTarget target, int size, string header, bool boolCheck)
        {
            sb.Append("\n").Append(header);

            if (boolCheck && target != null && size > 0)
            {
                var targetIsProvince = target as Province;
                if (targetIsProvince == null)
                    sb.Append(target).Append(" ").Append(size);
                else
                {
                    if (pop.getCountry() == targetIsProvince.getCountry())
                        sb.Append(targetIsProvince).Append(" ").Append(size);
                    else// immigration
                        sb.Append(targetIsProvince.getCountry()).Append(" (").Append(target).Append(") ").Append(size);

                }
            }
            else
                sb.Append("none");
        }
        private void makeLineNew(StringBuilder sb, IEscapeTarget target, int size)
        {
            // extra type conversion could be reduced by adding demotion type flag in PopUnit.LastDemotion
            var targetIsProvince = target as Province;
            if (targetIsProvince == null) // Assuming target is PopType
            {
                sb.Append("\n").Append("Demotion: ");
                sb.Append(target).Append(" ").Append(size);
            }
            else // Assuming target is Province
            {
                if (pop.getCountry() == targetIsProvince.getCountry())
                {
                    sb.Append("\n").Append("Migration: ");
                    sb.Append(targetIsProvince).Append(" ").Append(size);
                }
                else// immigration
                {
                    sb.Append("\n").Append("Immigration: ");
                    sb.Append(targetIsProvince.getCountry()).Append(" (").Append(target).Append(") ").Append(size);
                }
            }
        }
        //static private void makeLine(StringBuilder sb, Province target, int size, string header, bool boolCheck)
        //{
        //    //sb.Clear();
        //    sb.Append("\n").Append(header);
        //    if (boolCheck && target != null && size > 0)
        //        sb.Append(target).Append(" ").Append(size);
        //    else
        //        sb.Append("none");
        //}
        //static private void makeLineWithCountry(StringBuilder sb, Province target, int size, string header, bool boolCheck)
        //{
        //    //sb.Clear();
        //    sb.Append("\n").Append(header);
        //    if (boolCheck && target != null && size > 0)
        //        sb.Append(target.getCountry()).Append(" (").Append(target).Append(") ").Append(size);
        //    else
        //        sb.Append("none");
        //}
        public void show(PopUnit ipopUnit)
        {
            gameObject.SetActive(true);
            pop = ipopUnit;
            panelRectTransform.SetAsLastSibling();
        }
    }
}