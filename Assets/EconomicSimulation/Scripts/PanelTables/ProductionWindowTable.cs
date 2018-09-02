using System;
using System.Collections.Generic;
using Nashet.EconomicSimulation.Reforms;
using Nashet.UnityUIUtils;

namespace Nashet.EconomicSimulation
{
    public class ProductionWindowTable : UITableNew<Factory>
    {
        private SortOrder typeOrder, provinceOrder, productionOrder, resourcesOrder, workForceOrder, profitOrder,
            profitabilityOrder, salaryOrder, unemploymentOrder;

        private IEnumerable<Factory> content;

        private void Start()
        {
            typeOrder = new SortOrder(this, x => x.Type.NameWeight);
            provinceOrder = new SortOrder(this, x => x.Province.NameWeight);
            productionOrder = new SortOrder(this, x => x.getGainGoodsThisTurn().get());
            resourcesOrder = new SortOrder(this, x => x.getInputFactor().get());
            workForceOrder = new SortOrder(this, x => x.getWorkForce());
            profitOrder = new SortOrder(this, x => (float)x.getProfit());
            profitabilityOrder = new SortOrder(this, x => x.GetMargin().get());
            salaryOrder = new SortOrder(this, x => (float)x.getSalary().Get());
            unemploymentOrder = new SortOrder(this, x => x.Province.getUnemployedWorkers());
        }

        protected override IEnumerable<Factory> ContentSelector()
        {
            if (content == null)
            {
                var selectedProvince = MainCamera.productionWindow.SelectedProvince;
                if (selectedProvince == null)
                    return Game.Player.Provinces.AllFactories;
                else
                    return selectedProvince.AllFactories;
            }
            else
                return content;
        }

        public void SetContent(IEnumerable<Factory> content)
        {
            this.content = content;
        }

        protected override void AddRow(Factory factory, int number)
        {
            // Adding shownFactory name
            AddCell(factory.ShortName, factory);

            // Adding province
            AddCell(factory.Province.ToString(), factory.Province, () => "Click to select this province");

            ////Adding production
            AddCell(factory.getGainGoodsThisTurn().ToString(), factory);

            ////Adding effective resource income
            AddCell(factory.getInputFactor().ToString(), factory);

            ////Adding workforce
            AddCell(factory.getWorkForce().ToString(), factory);

            ////Adding profit
            if (factory.Country.economy == Economy.PlannedEconomy)
                AddCell("none", factory);
            else
                AddCell(factory.getProfit().ToString("F3") + " Gold", factory);

            ////Adding margin
            if (factory.isUpgrading())
                AddCell("Upgrading", factory, () => "Margin (tax included) is " + factory.GetMargin());
            else
            {
                if (factory.isBuilding())
                    AddCell("Building", factory, () => "Proposed margin (tax included) is " + factory.GetMargin());
                else
                {
                    if (factory.IsClosed)
                        AddCell("Closed", factory, () => "Proposed margin (tax included) is " + factory.GetMargin());
                    else
                    {
                        if (factory.Country.economy == Economy.PlannedEconomy)
                            AddCell("none", factory);
                        else
                            AddCell(factory.GetMargin().ToString(), factory, () => "Tax included");
                    }
                }
            }

            ////Adding salary
            //if (Game.player.isInvented(InventionType.capitalism))
            if (factory.Country.economy == Economy.PlannedEconomy)
                AddCell("centralized", factory);
            else
            {
                if (factory.Country.economy == Economy.NaturalEconomy)
                    AddCell(factory.getSalary() + " food", factory);
                else
                    AddCell(factory.getSalary().ToString(), factory);
            }

            //Adding unemployment
            AddCell(factory.Province.getSeeksForJob().ToString("N0"), factory);
        }

        protected override void AddHeader()
        {
            if (typeOrder == null)
                Start();
            // Adding product name
            AddCell("Type" + typeOrder.getSymbol(), typeOrder);

            // Adding province
            AddCell("Province" + provinceOrder.getSymbol(), provinceOrder);

            ////Adding production
            AddCell("Production" + productionOrder.getSymbol(), productionOrder);

            ////Adding effective resource income
            AddCell("Resources" + resourcesOrder.getSymbol(), resourcesOrder);

            ////Adding workforce
            AddCell("Workforce" + workForceOrder.getSymbol(), workForceOrder);

            ////Adding money income
            AddCell("Profit" + profitOrder.getSymbol(), profitOrder);

            ////Adding profit
            AddCell("Profitability" + profitabilityOrder.getSymbol(), profitabilityOrder);

            ////Adding salary
            AddCell("Salary" + salaryOrder.getSymbol(), salaryOrder);

            AddCell("Seeks job" + unemploymentOrder.getSymbol(), unemploymentOrder, () => "How much pops seek for a job in province.\nSome pops might sit on social benefits not willing to work");
        }
    }
}