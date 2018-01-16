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
        protected override List<KeyValuePair<Invention, bool>> ContentSelector()
        {
            return Game.Player.getAvailableInventions().ToList();
        }
        
        protected override void AddRow(KeyValuePair<Invention, bool> invention)
        {
            // Adding invention name 
            AddButton(invention.Key.ToString(), invention.Key);
            ////Adding possibleStatues
            if (invention.Value)
                AddButton("Invented", invention.Key);
            else
                AddButton("Uninvented", invention.Key);
            ////Adding invention price
            AddButton(invention.Key.getCost().ToString(), invention.Key);
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