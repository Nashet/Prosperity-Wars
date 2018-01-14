using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;

namespace Nashet.EconomicSimulation
{

    public class BuildPanelTable : UITableNew
    {
        public override void Refresh()
        {
            ////if (Game.date != 0)
            StartUpdate();
            {
                base.RemoveButtons();
                AddHeader();
                AddButtons();
            }
            EndUpdate();
        }

        private void AddButtons()
        {
            int counter = 0;
            if (Game.selectedProvince != null)
            {
                var factoryList = Game.selectedProvince.whatFactoriesCouldBeBuild();
                var howMuchRowsShow = CalcSize(factoryList.Count);

                //foreach (var next in factoryList)
                for (int i = 0; i < howMuchRowsShow; i++)
                {
                    var factoryType = factoryList[i + GetRowOffset()];
                    // Adding shownFactory type
                    AddButton(factoryType.ToString(), factoryType);

                    ////Adding cost
                    //if (Game.player.isInvented(InventionType.capitalism))
                    if (Economy.isMarket.checkIftrue(Game.Player))
                        AddButton(factoryType.getMinimalMoneyToBuild().ToString(), factoryType);
                    else
                        AddButton(factoryType.getBuildNeeds().ToString(), factoryType);


                    ////Adding resource needed
                    //AddButton(next.resourceInput.ToString(), next);

                    ////Adding potential output
                    AddButton(factoryType.basicProduction.ToString(), factoryType);

                    ////Adding potential profit
                    if (Game.Player.economy.getValue() == Economy.PlannedEconomy)
                        AddButton("unknown", factoryType);
                    else
                        AddButton(factoryType.getPossibleMargin(Game.selectedProvince).ToString(), factoryType);

                    counter++;
                }
            }
        }



        protected override void AddHeader()
        {
            // Adding shownFactory type
            AddButton("Name");

            ////Adding cost
            AddButton("Cost");

            ////Adding resource needed
            //AddButton("Input", null);

            ////Adding potential output
            AddButton("Output");

            ////Adding potential profit
            AddButton("Potential margin");
        }
    }
}