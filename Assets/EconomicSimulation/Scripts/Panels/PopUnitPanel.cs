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
            issues, money, caption, property;
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
                    if (isArtisan.Type == null)
                        sb.Append("nothing");
                    else
                        sb.Append(isArtisan.Type.basicProduction.Product);
                }
                sb.Append("\nGain goods: ").Append(pop.getGainGoodsThisTurn().ToString());
                sb.Append("\nSent to market: ").Append(pop.getSentToMarket());  // hide it            

                               
                    sb.Append("\nPopulation change: ").Append(pop.getAllPopulationChanges().Sum(x=>x.Value)).Append("\n")
                    .Append(pop.getAllPopulationChanges().getString("\n", pop, "Total change: "));

                //sb.Append("\nAssimilation: ");

                //if (pop.culture != pop.Country.getCulture() && pop.getAssimilationSize() > 0)
                //    sb.Append(pop.Country.getCulture()).Append(" ").Append(pop.getAssimilationSize());
                //else
                //    sb.Append("none");

                //sb.Append("\nGrowth: ").Append(pop.getGrowthSize());
                sb.Append("\nUnemployment: ").Append(pop.getUnemployment());
                sb.Append("\nLoyalty: ").Append(pop.loyalty);

                if (pop.loans.get() > 0f)
                    sb.Append("\nLoan: ").Append(pop.loans.ToString());// hide it
                if (pop.deposits.get() > 0f)
                    sb.Append("\nDeposit: ").Append(pop.deposits.ToString());// hide it
                if (Game.devMode)
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
                lifeNeedsText.GetComponent<ToolTipHandler>().SetTextDynamic(() => " Life needs wants:\n" + pop.getRealLifeNeeds().getString("\n"));
                lifeNeedsText.text = sb.ToString();

                sb.Clear();
                sb.Append("Everyday needs: ").Append(pop.getEveryDayNeedsFullfilling().ToString()).Append(" fulfilled");
                everyDayNeedsText.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Everyday needs wants:\n" + pop.getRealEveryDayNeeds().getString("\n"));
                everyDayNeedsText.text = sb.ToString();

                sb.Clear();
                sb.Append("Luxury needs: ").Append(pop.getLuxuryNeedsFullfilling().ToString()).Append(" fulfilled");
                luxuryNeedsText.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Luxury needs wants:\n" + pop.getRealLuxuryNeeds().getString("\n"));
                luxuryNeedsText.text = sb.ToString();

                sb.Clear();
                sb.Append("Cash: ").Append(pop.Cash.ToString());
                money.text = sb.ToString();
                money.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Money income: " + pop.moneyIncomeThisTurn
                + "\nIncome tax (inc. foreign jurisdictions): " + pop.incomeTaxPayed
                + "\nConsumed cost: " + Game.market.getCost(pop.getConsumed()));

                if (pop.Type.isProducer())
                {
                    efficiencyText.gameObject.SetActive(true);
                    efficiencyText.text = "Efficiency: " + PopUnit.modEfficiency.getModifier(pop);
                    efficiencyText.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Efficiency: " + PopUnit.modEfficiency.GetDescription(pop));
                }
                else
                {
                    efficiencyText.gameObject.SetActive(false);
                    //efficiencyText.GetComponent<ToolTipHandler>().SetText("");//it's disabled anyway
                }
                var thisInvestor = pop as Investor;
                if (thisInvestor != null)
                {
                    property.gameObject.SetActive(true);
                    var found = World.GetAllShares(thisInvestor).OrderByDescending(x => x.Value.get());
                    property.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Owns:\n" + found.getString(", ", "\n"));
                }
                else
                    property.gameObject.SetActive(false);

                issues.GetComponent<ToolTipHandler>().SetTextDynamic(
                    delegate ()
                    {
                        //var items = from pair in pop.getIssues()
                        //            orderby pair.Value descending
                        //            select pair;
                        var items = pop.getIssues().OrderByDescending(x => x.Value);
                        return "Issues:\n" + items.getString(" willing ", "\n");
                    }
                    );
            }
        }        

        public void show(PopUnit ipopUnit)
        {
            pop = ipopUnit;
            Show();
        }
    }
}