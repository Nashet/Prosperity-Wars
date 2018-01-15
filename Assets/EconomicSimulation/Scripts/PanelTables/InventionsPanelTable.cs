using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
using System.Linq;

namespace Nashet.EconomicSimulation
{

    public class InventionsPanelTable : UITableNew
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

            var elementsToShow = Game.Player.getAvailableInventions().ToList();
            //elementsToShow.AddRange(Game.Player.getUninvented().ToList());
            //foreach (var next in Game.Player.getAvailable())
            var howMuchRowsShow = CalcSize(elementsToShow.Count);
            for (int i = 0; i < howMuchRowsShow; i++)
            {
                var invention = elementsToShow[i + GetRowOffset()];
                // Adding invention name 
                AddButton(invention.Key.ToString(), invention.Key);
                ////Adding possibleStatues
                if (invention.Value)
                    AddButton("Invented", invention.Key);
                else
                    AddButton("Uninvented", invention.Key);
                ////Adding invention price
                AddButton(invention.Key.getCost().ToString(), invention.Key);
                counter++;
            }
        }

        protected override void AddHeader()
        {
            // Adding invention name 
            AddButton("Invention");
            ////Adding possibleStatues
            AddButton("Status");
            ////Adding invention price
            AddButton("Science points");
        }
    }
}