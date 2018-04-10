using System.Linq;
using System.Text;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class PopUnitPanel : DragPanel
    {
        [SerializeField]
        private Text generaltext, luxuryNeedsText, everyDayNeedsText, lifeNeedsText, efficiencyText,
            issues, money, caption, property, populationChange;

        private PopUnit pop;

        // Use this for initialization
        private void Start()
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
                if (pop.storage.isNotZero())
                    if (pop.Type == PopType.Aristocrats)
                        sb.Append("\nStorage: ").Append(pop.storage);
                    else
                        sb.Append("\nUnsold: ").Append(pop.storage);
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
                if (pop.Type == PopType.Aristocrats || pop.Type == PopType.Soldiers)
                    sb.Append("\nGained: ").Append(pop.getGainGoodsThisTurn());
                else
                    sb.Append("\nProduced: ").Append(pop.getGainGoodsThisTurn());
                sb.Append("\nSent to market: ").Append(pop.getSentToMarket());  // hide it

                //sb.Append("\nAssimilation: ");

                //if (pop.culture != pop.Country.getCulture() && pop.getAssimilationSize() > 0)
                //    sb.Append(pop.Country.getCulture()).Append(" ").Append(pop.getAssimilationSize());
                //else
                //    sb.Append("none");

                //sb.Append("\nGrowth: ").Append(pop.getGrowthSize());
                sb.Append("\nUnemployment: ").Append(pop.getUnemployment());
                sb.Append("\nLoyalty: ").Append(pop.loyalty);

                if (pop.loans.isNotZero())
                    sb.Append("\nLoan: ").Append(pop.loans);// hide it
                if (pop.deposits.isNotZero())
                    sb.Append("\nDeposit: ").Append(pop.deposits);// hide it
                                                                             //if (Game.devMode)
                sb.Append("\nAge: ").Append(pop.getAge());
                sb.Append("\nMobilized: ").Append(pop.getMobilized());
                if (pop.getMovement() != null)
                    sb.Append("\nMember of ").Append(pop.getMovement());
                sb.Append("\nConsumed: ").Append(pop.getConsumed());

                //if (Game.devMode)
                //    sb.Append("\nConsumedLT: ").Append(pop.getConsumedLastTurn()).Append(" cost: ").Append(World.market.getCost(pop.getConsumedLastTurn())
                //        ).Append("\nConsumedIM: ").Append(pop.getConsumedInMarket()).Append(" cost: ").Append(World.market.getCost(pop.getConsumedInMarket()));

                generaltext.text = sb.ToString();

                sb.Clear();
                sb.Append("Life needs: ").Append(pop.getLifeNeedsFullfilling()).Append(" fulfilled");
                lifeNeedsText.GetComponent<ToolTipHandler>().SetTextDynamic(() => " Life needs wants:\n" + pop.getRealLifeNeeds().getString("\n"));
                lifeNeedsText.text = sb.ToString();

                sb.Clear();
                sb.Append("Everyday needs: ").Append(pop.getEveryDayNeedsFullfilling()).Append(" fulfilled");
                everyDayNeedsText.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Everyday needs wants:\n" + pop.getRealEveryDayNeeds().getString("\n"));
                everyDayNeedsText.text = sb.ToString();

                sb.Clear();
                sb.Append("Luxury needs: ").Append(pop.getLuxuryNeedsFullfilling()).Append(" fulfilled");
                luxuryNeedsText.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Luxury needs wants:\n" + pop.getRealLuxuryNeeds().getString("\n"));
                luxuryNeedsText.text = sb.ToString();

                sb.Clear();
                sb.Append("Cash: ").Append(pop.Cash);
                money.text = sb.ToString();
                money.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Money income: " + pop.moneyIncomeThisTurn
                + "\nIncome tax (inc. foreign jurisdictions): " + pop.incomeTaxPayed
                + "\nConsumed cost: " + World.market.getCost(pop.getConsumed()));

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
                    delegate
                    {
                        //var items = from pair in pop.getIssues()
                        //            orderby pair.Value descending
                        //            select pair;
                        var items = pop.getIssues().OrderByDescending(x => x.Value);
                        return "Issues:\n" + items.getString(" willing ", "\n");
                    }
                    );
                populationChange.text = "Population change: " + pop.getAllPopulationChanges().Sum(x => x.Value);
                populationChange.GetComponent<ToolTipHandler>().SetTextDynamic(() =>
                "Population change:\n" +
                pop.getAllPopulationChanges().getString("\n", pop, "Total change: "));
            }
        }

        public void show(PopUnit ipopUnit)
        {
            pop = ipopUnit;
            Show();
        }
    }
}