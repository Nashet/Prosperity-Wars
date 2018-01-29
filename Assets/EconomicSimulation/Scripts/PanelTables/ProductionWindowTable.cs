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
            
            typeOrder = new SortOrder(this, x => x.getType().GetNameWeight());
            provinceOrder = new SortOrder(this, x => x.getProvince().GetNameWeight());
            productionOrder = new SortOrder(this, x => x.getGainGoodsThisTurn().get());
            resourcesOrder = new SortOrder(this, x => x.getInputFactor().get());
            workForceOrder = new SortOrder(this, x => x.getWorkForce());
            profitOrder = new SortOrder(this, x => x.getProfit());
            profitabilityOrder = new SortOrder(this, x => x.getMargin().get());
            salaryOrder = new SortOrder(this, x => x.getSalary());
            unemploymentOrder = new SortOrder(this, x => x.getProvince().getUnemployedWorkers());
        }
        protected override IEnumerable<Factory> ContentSelector()
        {
            return Game.Player.getAllFactories();
            //var factoriesToShow = new List<Factory>();
            //foreach (Province province in Game.Player.ownedProvinces)
            //    foreach (Factory factory in province.allFactories)
            //        factoriesToShow.Add(factory);
            //return factoriesToShow;
        }
        //public override void onShowAllClick()
        //{
        //    base.onShowAllClick();

        //    RemoveFilter(filterSelectedProvince);
        //    //table.ClearAllFiltres();
        //    table.Refresh();
        //}
        protected override void AddRow(Factory factory, int number)
        {
            // Adding shownFactory name 
            AddCell(factory.getType().name + " L" + factory.getLevel(), factory);

            // Adding province 
            AddCell(factory.getProvince().ToString(), factory.getProvince());

            ////Adding production
            AddCell(factory.getGainGoodsThisTurn().ToString(), factory);

            ////Adding effective resource income
            AddCell(factory.getInputFactor().ToString(), factory);

            ////Adding workforce
            AddCell(factory.getWorkForce().ToString(), factory);

            ////Adding profit
            if (factory.getCountry().economy.getValue() == Economy.PlannedEconomy)
                AddCell("none", factory);
            else
                AddCell(factory.getProfit().ToString("F3"), factory);

            ////Adding margin
            if (factory.isUpgrading())
                AddCell("Upgrading", factory);
            else
            {
                if (factory.isBuilding())
                    AddCell("Building", factory);
                else
                {
                    if (factory.IsClosed)
                        AddCell("Closed", factory);
                    else
                    {
                        if (factory.getCountry().economy.getValue() == Economy.PlannedEconomy)
                            AddCell("none", factory);
                        else
                            AddCell(factory.getMargin().ToString(), factory);
                    }
                }
            }

            ////Adding salary
            //if (Game.player.isInvented(InventionType.capitalism))
            if (factory.getCountry().economy.getValue() == Economy.PlannedEconomy)
                AddCell("centralized", factory);
            else
            {
                if (factory.getCountry().economy.getValue() == Economy.NaturalEconomy)
                    AddCell(factory.getSalary().ToString() + " food", factory);
                else
                    AddCell(factory.getSalary().ToString("F3") + " coins", factory);
            }

            //Adding unemployment
            AddCell(factory.getProvince().getUnemployedWorkers().ToString("N0"), factory);
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