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

        protected override void AddRow(Factory factory)
        {
            // Adding shownFactory name 
            AddButton(factory.getType().name + " L" + factory.getLevel(), factory);

            // Adding province 
            AddButton(factory.getProvince().ToString(), factory.getProvince());

            ////Adding production
            AddButton(factory.getGainGoodsThisTurn().ToString(), factory);

            ////Adding effective resource income
            AddButton(factory.getInputFactor().ToString(), factory);

            ////Adding workforce
            AddButton(factory.getWorkForce().ToString(), factory);

            ////Adding profit
            if (factory.getCountry().economy.getValue() == Economy.PlannedEconomy)
                AddButton("none", factory);
            else
                AddButton(factory.getProfit().ToString("F3"), factory);

            ////Adding margin
            if (factory.isUpgrading())
                AddButton("Upgrading", factory);
            else
            {
                if (factory.isBuilding())
                    AddButton("Building", factory);
                else
                {
                    if (!factory.isWorking())
                        AddButton("Closed", factory);
                    else
                    {
                        if (factory.getCountry().economy.getValue() == Economy.PlannedEconomy)
                            AddButton("none", factory);
                        else
                            AddButton(factory.getMargin().ToString(), factory);
                    }
                }
            }

            ////Adding salary
            //if (Game.player.isInvented(InventionType.capitalism))
            if (factory.getCountry().economy.getValue() == Economy.PlannedEconomy)
                AddButton("centralized", factory);
            else
            {
                if (factory.getCountry().economy.getValue() == Economy.NaturalEconomy)
                    AddButton(factory.getSalary().ToString() + " food", factory);
                else
                    AddButton(factory.getSalary().ToString() + " coins", factory);
            }
            AddButton(factory.getProvince().getUnemployedWorkers().ToString("N0"), factory);
        }
        protected override void AddHeader()
        {
            // Adding product name 
            AddButton("Type");

            // Adding province 
            AddButton("Province");

            ////Adding production
            AddButton("Production");

            ////Adding effective resource income
            AddButton("Resources");

            ////Adding workforce
            AddButton("Workforce");

            ////Adding money income
            AddButton("Profit");

            ////Adding profit
            AddButton("Profitability");

            ////Adding salary
            AddButton("Salary");

            AddButton("Unemployed", null, () => "Unemployed in province");
        }
    }
}