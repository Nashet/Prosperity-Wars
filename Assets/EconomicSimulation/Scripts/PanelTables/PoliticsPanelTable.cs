using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
namespace Nashet.EconomicSimulation
{

    

    public class PoliticsPanelTable : UITableNew
    {
        public override void Refresh()
        {
            ////if (Game.date != 0)
            StartUpdate();
            {
                base.RemoveButtons();
                AddButtons();
            }
            EndUpdate();
        }        
        private void AddButtons()
        {
            int counter = 0;
            AddHeader();
            if (Game.Player != null)
            {
                //var factoryList = Game.player;

                foreach (var next in Game.Player.reforms)
                // if (next.isAvailable(Game.player))
                {
                    // Adding reform name
                    AddButton(next.ToString(), next);

                    ////Adding Status
                    AddButton(next.getValue().ToString(), next);

                    ////Adding Can change possibility
                    //if (next.canChange())
                    //    AddButton("Yep", next);
                    //else
                    //    AddButton("Nope", next);

                    counter++;
                }
            }
        }

        protected override void AddHeader()
        {
            // Adding reform name
            AddButton("Reform");

            ////Adding Status
            AddButton("Status");

            ////Adding Can change possibility
            // AddButton("Can change", null);
        }
    }
}