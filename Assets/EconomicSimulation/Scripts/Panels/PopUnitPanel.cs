using Nashet.UnityUIUtils;
using Nashet.Utils;
using Nashet.ValueSpace;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{
    public class PopUnitPanel : DragPanel
    {
        [SerializeField]
        private Text generaltext, caption, luxuryNeedsText, everyDayNeedsText, lifeNeedsText, efficiencyText,
            issues, money, property, populationChange;

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
                Artisans isArtisan = pop as Artisans;
                sb.Append("Population: ").Append(pop.population.Get());
                if (isArtisan != null)
                {
                    sb.Append(", Producing: ");
                    if (isArtisan.Type == null)
                        sb.Append("nothing");
                    else
                        sb.Append(isArtisan.Type.basicProduction.Product);
                }
                //if (Game.devMode)
                if (pop.Type == PopType.Aristocrats || pop.Type == PopType.Soldiers)
                    sb.Append("\nGained: ").Append(pop.getGainGoodsThisTurn());
                else
                    sb.Append("\nProduced: ").Append(pop.getGainGoodsThisTurn());
                if (pop.storage.isNotZero())
                    if (pop.Type == PopType.Aristocrats)
                        sb.Append(", Storage: ").Append(pop.storage);
                    else
                        sb.Append(", Unsold: ").Append(pop.storage);

                if (isArtisan != null)
                {
                    sb.Append("\nInput required: ");
                    foreach (Storage next in isArtisan.GetResurceInput())
                        sb.Append(next.get() * isArtisan.population.Get() / Population.PopulationMultiplier).Append(" ").Append(next.Product).Append(";");

                    sb.Append("\nStockpile:  ").Append(isArtisan.getInputProducts()).Append(", Resource availability: ").Append(isArtisan.getInputFactor());
                }

                //sb.Append("\nSent to market: ").Append(pop.getSentToMarket());  // hide it
                sb.Append("\nConsumed: ").Append(pop.getConsumed().ToString(", "));


                // loyalty part
                sb.Append("\n\nCash: ").Append(pop.Cash);
                if (pop.loans.isNotZero())
                    sb.Append("\nLoan: ").Append(pop.loans);// hide it
                if (pop.deposits.isNotZero())
                    sb.Append("\nDeposit: ").Append(pop.deposits);// hide it
                sb.Append("\n\nNeeds fulfilled (total): ").Append(pop.needsFulfilled);
                sb.Append("\nLoyalty: ").Append(pop.loyalty);
                if (pop.getMovement() != null)
                    sb.Append("\nMember of ").Append(pop.getMovement());

                //rest part
                sb.Append("\n\nSeeks job: ").Append(pop.GetSeekingJob());
                sb.Append("\nEducation: ").Append(pop.Education);
                sb.Append("\nCulture: ").Append(pop.culture);
                if (!pop.isStateCulture())
                    sb.Append(", minority");

                if (Game.devMode)
                    sb.Append("\n\nAge: ").Append(pop.getAge());
                sb.Append("\nMobilized: ").Append(pop.getMobilized());



                //if (Game.devMode)
                //    sb.Append("\nConsumedLT: ").Append(pop.getConsumedLastTurn()).Append(" cost: ").Append(Country.market.getCost(pop.getConsumedLastTurn())
                //        ).Append("\nConsumedIM: ").Append(pop.getConsumedInMarket()).Append(" cost: ").Append(Country.market.getCost(pop.getConsumedInMarket()));

                generaltext.text = sb.ToString();

                sb.Clear();
                sb.Append("Life needs: ").Append(pop.getLifeNeedsFullfilling()).Append(" fulfilled");
                lifeNeedsText.GetComponent<ToolTipHandler>().SetTextDynamic(() => " Life needs wants:\n" + pop.population.getRealLifeNeeds().ToString("\n"));
                lifeNeedsText.text = sb.ToString();

                sb.Clear();
                sb.Append("Everyday needs: ").Append(pop.getEveryDayNeedsFullfilling()).Append(" fulfilled");
                everyDayNeedsText.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Everyday needs wants:\n" + pop.population.getRealEveryDayNeeds().ToString("\n"));
                everyDayNeedsText.text = sb.ToString();

                sb.Clear();
                sb.Append("Luxury needs: ").Append(pop.getLuxuryNeedsFullfilling()).Append(" fulfilled");
                luxuryNeedsText.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Luxury needs wants:\n" + pop.population.getRealLuxuryNeeds().ToString("\n"));
                luxuryNeedsText.text = sb.ToString();

                sb.Clear();
                sb.Append("Balance: ").Append(Money.DecimalToString(pop.Register.Balance));
                money.text = sb.ToString();
                money.GetComponent<ToolTipHandler>().SetTextDynamic(() =>
                pop.Register.ToString()
                //"Money income: " + pop.moneyIncomeThisTurn
                //+ "\nIncome tax (inc. foreign jurisdictions): " + pop.incomeTaxPayed
                //+ "\nConsumed cost: " + Game.Player.market.getCost(pop.getConsumed())
                );

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
                    property.GetComponent<ToolTipHandler>().SetTextDynamic(() => "Owns:\n" + found.ToString(", ", "\n"));
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
                        return "Issues:\n" + items.ToString(" willing ", "\n");
                    }
                    );
                populationChange.text = "Population change: " + pop.getAllPopulationChanges().Sum(x => x.Value);
                populationChange.GetComponent<ToolTipHandler>().SetTextDynamic(() =>
                "Population change:\n" +
                pop.getAllPopulationChanges().ToString("\n", pop, "Total change: "));
            }
        }

        public void show(PopUnit ipopUnit)
        {
            pop = ipopUnit;
            Show();
        }
    }
}