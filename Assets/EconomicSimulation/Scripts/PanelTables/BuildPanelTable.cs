using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;

namespace Nashet.EconomicSimulation
{
    /// <summary>
    /// Obsolete, should be changed to MyTableNew
    /// </summary>
    public class BuildPanelTable : MyTable
    {
        public override void Refresh()
        {
            ////if (Game.date != 0)
            {
                base.RemoveButtons();
                AddButtons();
                gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gameObject.transform.childCount / GetColumnsAmount() * rowHeight + 50);
            }
        }
        
        override protected void AddButtons()
        {
            int counter = 0;
            AddHeader();


            if (Game.selectedProvince != null)
            {
                var factoryList = Game.selectedProvince.whatFactoriesCouldBeBuild();

                foreach (var next in factoryList)
                {
                    // Adding shownFactory type
                    AddButton(next.ToString(), next);

                    ////Adding cost
                    //if (Game.player.isInvented(InventionType.capitalism))
                    if (Economy.isMarket.checkIftrue(Game.Player))
                        AddButton(next.getMinimalMoneyToBuild().ToString(), next);
                    else
                        AddButton(next.getBuildNeeds().ToString(), next);


                    ////Adding resource needed
                    //AddButton(next.resourceInput.ToString(), next);

                    ////Adding potential output
                    AddButton(next.basicProduction.ToString(), next);

                    ////Adding potential profit
                    if (Game.Player.economy.getValue() == Economy.PlannedEconomy)
                        AddButton("unknown", next);
                    else
                        AddButton(next.getPossibleMargin(Game.selectedProvince).ToString(), next);

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