using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;
using System.Linq;
using Nashet.UnityUIUtils;
using Nashet.Utils;
namespace Nashet.EconomicSimulation
{
    public class PopUnitPanel : DragPanel
    {
        [SerializeField]
        private Text generaltext, luxuryNeedsText, everyDayNeedsText, lifeNeedsText, efficiencyText,
            issues, money, caption;
        private PopUnit pop;
        // Use this for initialization
        void Start()
        {
            MainCamera.popUnitPanel = this;
            GetComponent<RectTransform>().anchoredPosition = new Vector2(600f, 53f);
            Hide();
        }
        public PopUnit whomShowing()
        {
            return pop;
        }

        public override void Refresh()
        {
            if (pop != null)
            {
                var sb = new StringBuilder();
                caption.text = pop.ToString();
                //sb.Append(pop);
                sb.Append("Population: ").Append(pop.getPopulation());
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
                if (pop.culture != pop.GetCountry().getCulture() && pop.getAssimilationSize() > 0)
                    sb.Append(pop.GetCountry().getCulture()).Append(" ").Append(pop.getAssimilationSize());
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
                lifeNeedsText.GetComponent<ToolTipHandler>().SetDynamicString(() => " Life needs wants:\n" + pop.getRealLifeNeeds().getString("\n"));
                lifeNeedsText.text = sb.ToString();

                sb.Clear();
                sb.Append("Everyday needs: ").Append(pop.getEveryDayNeedsFullfilling().ToString()).Append(" fulfilled");
                everyDayNeedsText.GetComponent<ToolTipHandler>().SetDynamicString(() => "Everyday needs wants:\n" + pop.getRealEveryDayNeeds().getString("\n"));
                everyDayNeedsText.text = sb.ToString();

                sb.Clear();
                sb.Append("Luxury needs: ").Append(pop.getLuxuryNeedsFullfilling().ToString()).Append(" fulfilled");
                luxuryNeedsText.GetComponent<ToolTipHandler>().SetDynamicString(() => "Luxury needs wants:\n" + pop.getRealLuxuryNeeds().getString("\n"));
                luxuryNeedsText.text = sb.ToString();

                sb.Clear();
                sb.Append("Cash: ").Append(pop.cash.ToString());
                money.text = sb.ToString();
                money.GetComponent<ToolTipHandler>().SetDynamicString(() => "Money income: " + pop.moneyIncomethisTurn
                + "\nIncome tax: " + pop.incomeTaxPayed
                + "\nConsumed cost: " + Game.market.getCost(pop.getConsumed()));

                if (pop.popType.isProducer())
                {
                    efficiencyText.enabled = true;
                    efficiencyText.text = "Efficiency: " + PopUnit.modEfficiency.getModifier(pop);
                    efficiencyText.GetComponent<ToolTipHandler>().SetDynamicString(() => PopUnit.modEfficiency.GetDescription(pop));
                }
                else
                {
                    efficiencyText.enabled = false;
                    //efficiencyText.GetComponent<ToolTipHandler>().SetText("");//it's disabled anyway
                }


                issues.GetComponent<ToolTipHandler>().SetDynamicString(
                    delegate ()
                    {
                        var items = from pair in pop.getIssues()
                                    orderby pair.Value descending
                                    select pair;
                        return items.getString(" willing ", "\n");
                    }
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
                    if (pop.GetCountry() == targetIsProvince.GetCountry())
                        sb.Append(targetIsProvince).Append(" ").Append(size);
                    else// immigration
                        sb.Append(targetIsProvince.GetCountry()).Append(" (").Append(target).Append(") ").Append(size);

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
                if (pop.GetCountry() == targetIsProvince.GetCountry())
                {
                    sb.Append("\n").Append("Migration: ");
                    sb.Append(targetIsProvince).Append(" ").Append(size);
                }
                else// immigration
                {
                    sb.Append("\n").Append("Immigration: ");
                    sb.Append(targetIsProvince.GetCountry()).Append(" (").Append(target).Append(") ").Append(size);
                }
            }
        }

        public void show(PopUnit ipopUnit)
        {
            pop = ipopUnit;
            Show();
        }
    }
}