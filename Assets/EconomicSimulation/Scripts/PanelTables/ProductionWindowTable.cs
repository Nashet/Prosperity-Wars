using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;

namespace Nashet.EconomicSimulation
{
    public class ProductionWindowTable : MyTableNew
    {
        public override void Refresh()
        {
            startUpdate();
            base.RemoveButtons();
            var howMuchRowsShow = calcSize(Game.factoriesToShowInProductionPanel.Count);
            //int counter = 0;
            addHeader();
            for (int i = 0; i < howMuchRowsShow; i++)
            //foreach (Factory next in Game.factoriesToShowInProductionPanel)
            {
                Factory factory = Game.factoriesToShowInProductionPanel[i + getRowOffset()];
                // Adding shownFactory name 
                addButton(factory.getType().name + " L" + factory.getLevel(), factory);

                // Adding province 
                addButton(factory.getProvince().ToString(), factory.getProvince());

                ////Adding production
                addButton(factory.getGainGoodsThisTurn().ToString(), factory);

                ////Adding effective resource income
                addButton(factory.getInputFactor().ToString(), factory);

                ////Adding workforce
                addButton(factory.getWorkForce().ToString(), factory);

                ////Adding profit
                if (factory.getCountry().economy.getValue() == Economy.PlannedEconomy)
                    addButton("none", factory);
                else
                    addButton(factory.getProfit().ToString("F3"), factory);

                ////Adding margin
                if (factory.isUpgrading())
                    addButton("Upgrading", factory);
                else
                {
                    if (factory.isBuilding())
                        addButton("Building", factory);
                    else
                    {
                        if (!factory.isWorking())
                            addButton("Closed", factory);
                        else
                        {
                            if (factory.getCountry().economy.getValue() == Economy.PlannedEconomy)
                                addButton("none", factory);
                            else
                                addButton(factory.getMargin().ToString(), factory);
                        }
                    }
                }

                ////Adding salary
                //if (Game.player.isInvented(InventionType.capitalism))
                if (factory.getCountry().economy.getValue() == Economy.PlannedEconomy)
                    addButton("centralized", factory);
                else
                {
                    if (factory.getCountry().economy.getValue() == Economy.NaturalEconomy)
                        addButton(factory.getSalary().ToString() + " food", factory);
                    else
                        addButton(factory.getSalary().ToString() + " coins", factory);
                }
                addButton(factory.getProvince().getUnemployedWorkers().ToString("N0"), factory);
                //counter++;
                //contentPanel.r
            }
            endUpdate();
        }

        protected override void addHeader()
        {
            // Adding product name 
            addButton("Type");

            // Adding province 
            addButton("Province");

            ////Adding production
            addButton("Production");

            ////Adding effective resource income
            addButton("Resources");

            ////Adding workforce
            addButton("Workforce");

            ////Adding money income
            addButton("Profit");

            ////Adding profit
            addButton("Profitability");

            ////Adding salary
            addButton("Salary");

            addButton("Unemployed", null, () => "Unemployed in province");
        }
    }
}