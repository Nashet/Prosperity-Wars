using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
using System.Linq;

namespace Nashet.EconomicSimulation
{

    public class BuildPanelTable : UITableNew<FactoryType>
    {
        protected override IEnumerable<FactoryType> ContentSelector()
        {
            
            if (Game.selectedProvince == null)
                return Enumerable.Empty<FactoryType>();
            else
                return FactoryType.getAllInventedTypes(Game.Player).Where(x => x.canBuildNewFactory(Game.selectedProvince));
        }
        protected override void AddRow(FactoryType factoryType, int number)
        {
            // Adding shownFactory type
            AddCell(factoryType.ToString(), factoryType);

            ////Adding cost
            //if (Game.player.isInvented(InventionType.capitalism))
            if (Economy.isMarket.checkIftrue(Game.Player))
                AddCell(factoryType.GetBuildCost().ToString(), factoryType);
            else
                AddCell(factoryType.GetBuildNeeds().ToString(), factoryType);


            ////Adding resource needed
            //AddButton(next.resourceInput.ToString(), next);

            ////Adding potential output
            AddCell(factoryType.basicProduction.ToString(), factoryType);

            ////Adding potential profit
            if (Game.Player.economy.getValue() == Economy.PlannedEconomy)
                AddCell("unknown", factoryType);
            else
                AddCell(factoryType.GetPossibleMargin(Game.selectedProvince).ToString(), factoryType);
        }

        protected override void AddHeader()
        {
            // Adding shownFactory type
            AddCell("Name");

            ////Adding cost
            AddCell("Cost");

            ////Adding resource needed
            //AddButton("Input", null);

            ////Adding potential output
            AddCell("Output");

            ////Adding potential profit
            AddCell("Potential margin");
        }
    }
}