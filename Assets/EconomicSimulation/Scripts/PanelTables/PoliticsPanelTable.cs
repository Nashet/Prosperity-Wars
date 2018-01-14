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
                AddHeader();
                AddButtons();
            }
            EndUpdate();
        }
        private void AddButtons()
        {
            int counter = 0;            
            //foreach (var next in Game.Player.reforms)
            var howMuchRowsShow = CalcSize(Game.Player.reforms.Count);            
            for (int i = 0; i < howMuchRowsShow; i++)
            // if (next.isAvailable(Game.player))
            {
                var reform = Game.Player.reforms[i + GetRowOffset()];
                // Adding reform name
                AddButton(reform.ToString(), reform);

                ////Adding Status
                AddButton(reform.getValue().ToString(), reform);

                ////Adding Can change possibility
                //if (next.canChange())
                //    AddButton("Yep", next);
                //else
                //    AddButton("Nope", next);

                counter++;
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