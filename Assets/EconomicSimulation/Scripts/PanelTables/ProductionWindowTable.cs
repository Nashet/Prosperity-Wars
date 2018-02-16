using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
using Nashet.Utils;

namespace Nashet.EconomicSimulation
{
    public class ProductionWindowTable : UITableNew<Factory>
    {
        private SortOrder typeOrder, provinceOrder, productionOrder, resourcesOrder, workForceOrder, profitOrder,
            profitabilityOrder, salaryOrder, unemploymentOrder;

        private void Start()
        {

            typeOrder = new SortOrder(this, x => x.Type.GetNameWeight());
            provinceOrder = new SortOrder(this, x => x.GetProvince().GetNameWeight());
            productionOrder = new SortOrder(this, x => x.getGainGoodsThisTurn().get());
            resourcesOrder = new SortOrder(this, x => x.getInputFactor().get());
            workForceOrder = new SortOrder(this, x => x.getWorkForce());
            profitOrder = new SortOrder(this, x => x.getProfit());
            profitabilityOrder = new SortOrder(this, x => x.GetMargin().get());
            salaryOrder = new SortOrder(this, x => x.getSalary().get());
            unemploymentOrder = new SortOrder(this, x => x.GetProvince().getUnemployedWorkers());
        }
        protected override IEnumerable<Factory> ContentSelector()
        {
            var selectedProvince = MainCamera.productionWindow.SelectedProvince;
            if (selectedProvince == null)
                return Game.Player.getAllFactories();
            else
                return selectedProvince.getAllFactories();
        }
        protected override void AddRow(Factory factory, int number)
        {
            // Adding shownFactory name 
            AddCell(factory.GetDescription(), factory);

            // Adding province 
            AddCell(factory.GetProvince().ToString(), factory.GetProvince(), () => "Click to select this province");

            ////Adding production
            AddCell(factory.getGainGoodsThisTurn().ToString(), factory);

            ////Adding effective resource income
            AddCell(factory.getInputFactor().ToString(), factory);

            ////Adding workforce
            AddCell(factory.getWorkForce().ToString(), factory);

            ////Adding profit
            if (factory.GetCountry().economy.getValue() == Economy.PlannedEconomy)
                AddCell("none", factory);
            else
                AddCell(factory.getProfit().ToString("F3"), factory);

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
                        if (factory.GetCountry().economy.getValue() == Economy.PlannedEconomy)
                            AddCell("none", factory);
                        else
                            AddCell(factory.GetMargin().ToString(), factory, () => "Tax included");
                    }
                }
            }

            ////Adding salary
            //if (Game.player.isInvented(InventionType.capitalism))
            if (factory.GetCountry().economy.getValue() == Economy.PlannedEconomy)
                AddCell("centralized", factory);
            else
            {
                if (factory.GetCountry().economy.getValue() == Economy.NaturalEconomy)
                    AddCell(factory.getSalary().ToString() + " food", factory);
                else
                    AddCell(factory.getSalary().ToString(), factory);
            }

            //Adding unemployment
            AddCell(factory.GetProvince().getUnemployedWorkers().ToString("N0"), factory);
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

            AddCell("Unemployed" + unemploymentOrder.getSymbol(), unemploymentOrder, () => "Unemployed in province");
        }
    }
}