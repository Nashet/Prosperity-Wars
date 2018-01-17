using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;

namespace Nashet.EconomicSimulation
{
    public class ProductionWindowTable : UITableNew<Factory>
    {
        protected override List<Factory> ContentSelector()
        {
            var factoriesToShow = new List<Factory>();
            foreach (Province province in Game.Player.ownedProvinces)
                foreach (Factory factory in province.allFactories)
                    factoriesToShow.Add(factory);
            return factoriesToShow;
        }
        //public override void onShowAllClick()
        //{
        //    base.onShowAllClick();
            
        //    RemoveFilter(filterSelectedProvince);
        //    //table.ClearAllFiltres();
        //    table.Refresh();
        //}
        protected override void AddRow(Factory factory)
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
                    if (!factory.isWorking())
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
                    AddCell(factory.getSalary().ToString() + " coins", factory);
            }
            AddCell(factory.getProvince().getUnemployedWorkers().ToString("N0"), factory);
        }
        protected override void AddHeader()
        {
            // Adding product name 
            AddCell("Type");

            // Adding province 
            AddCell("Province");

            ////Adding production
            AddCell("Production");

            ////Adding effective resource income
            AddCell("Resources");

            ////Adding workforce
            AddCell("Workforce");

            ////Adding money income
            AddCell("Profit");

            ////Adding profit
            AddCell("Profitability");

            ////Adding salary
            AddCell("Salary");

            AddCell("Unemployed", null, () => "Unemployed in province");
        }
    }
}