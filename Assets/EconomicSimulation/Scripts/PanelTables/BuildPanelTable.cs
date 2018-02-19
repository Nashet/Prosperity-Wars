using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
using Nashet.Utils;
using System.Linq;

namespace Nashet.EconomicSimulation
{

    public class BuildPanelTable : UITableNew<ProductionType>
    {
        protected override IEnumerable<ProductionType> ContentSelector()
        {
            
            if (Game.selectedProvince == null)
                return Enumerable.Empty<ProductionType>();
            else
                return ProductionType.getAllInventedFactories(Game.Player).Where(x => x.canBuildNewFactory(Game.selectedProvince, Game.Player));
        }
        protected override void AddRow(ProductionType factoryType, int number)
        {
            // Adding shownFactory type
            AddCell(factoryType.ToString(), factoryType);

            ////Adding cost
            //if (Game.player.isInvented(InventionType.capitalism))
            if (Economy.isMarket.checkIfTrue(Game.Player))
                AddCell(factoryType.GetBuildCost().ToString(), factoryType);
            else
                AddCell(factoryType.GetBuildNeeds().getString(""), factoryType);


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