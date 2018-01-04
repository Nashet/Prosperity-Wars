using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
namespace Nashet.EconomicSimulation
{

    public class InventionsPanelTable : MyTable
    {
        override protected void refresh()
        {
            ////if (Game.date != 0)
            {
                base.RemoveButtons();
                AddButtons();
                gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gameObject.transform.childCount / this.columnsAmount * rowHeight + 50);
            }
        }
        protected void AddButton(string text, Invention inventionType)
        {
            GameObject newButton = buttonObjectPool.GetObject();
            newButton.transform.SetParent(gameObject.transform, true);
            SampleButton sampleButton = newButton.GetComponent<SampleButton>();
            //if (inventionType == null)
            //    sampleButton.Setup(text, this, null);
            //else
            sampleButton.Setup(text, inventionType);
        }
        override protected void AddButtons()
        {
            int counter = 0;
            addHeader();
            if (Game.Player != null)
                foreach (var next in Game.Player.getAvailable())
                //if (next.Key.isAvailable(Game.Player))
                {
                    // Adding invention name 
                    AddButton(next.Key.ToString(), next.Key);
                    ////Adding possibleStatues
                    if (next.Value)
                        AddButton("Invented", next.Key);
                    else
                        AddButton("Uninvented", next.Key);
                    ////Adding invention price
                    AddButton(next.Key.cost.ToString(), next.Key);
                    counter++;
                }
        }

        protected override void addHeader()
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