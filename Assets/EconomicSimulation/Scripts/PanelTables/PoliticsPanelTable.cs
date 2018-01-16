using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Nashet.UnityUIUtils;
using System.Linq;

namespace Nashet.EconomicSimulation
{
    public class PoliticsPanelTable : UITableNew<AbstractReform>
    {
        protected override List<AbstractReform> ContentSelector()
        {
            return Game.Player.reforms.ToList();
        }

        protected override void AddRow(AbstractReform reform)
        {
            // Adding reform name
            AddButton(reform.ToString(), reform);

            ////Adding Status
            AddButton(reform.getValue().ToString(), reform);

            ////Adding Can change possibility
            //if (next.canChange())
            //    AddButton("Yep", next);
            //else
            //    AddButton("Nope", next);           
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