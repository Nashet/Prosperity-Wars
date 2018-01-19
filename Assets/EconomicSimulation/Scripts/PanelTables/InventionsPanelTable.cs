using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
using System.Linq;

namespace Nashet.EconomicSimulation
{

    public class InventionsPanelTable : UITableNew<KeyValuePair<Invention, bool>>
    {
        protected override IEnumerable<KeyValuePair<Invention, bool>> ContentSelector()
        {
            return Game.Player.getAvailableInventions();
        }
        
        protected override void AddRow(KeyValuePair<Invention, bool> invention, int number)
        {
            // Adding invention name 
            AddCell(invention.Key.ToString(), invention.Key);
            ////Adding possibleStatues
            if (invention.Value)
                AddCell("Invented", invention.Key);
            else
                AddCell("Uninvented", invention.Key);
            ////Adding invention price
            AddCell(invention.Key.getCost().ToString(), invention.Key);
        }
        protected override void AddHeader()
        {
            // Adding invention name 
            AddCell("Invention");
            ////Adding possibleStatues
            AddCell("Status");
            ////Adding invention price
            AddCell("Science points");
        }
    }
}