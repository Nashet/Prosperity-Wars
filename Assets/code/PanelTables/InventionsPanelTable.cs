using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

// represen each opunit record in table

public class InventionsPanelTable : MyTable
{    
    override protected void Refresh()
    {
        ////if (Game.date != 0)
        {
            base.RemoveButtons();
            AddButtons();
            contentPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentPanel.childCount / this.columnsAmount * rowHeight + 50);
        }
    }
    protected void AddButton(string text, InventionType inventionType)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(contentPanel, false);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        //if (inventionType == null)
        //    sampleButton.Setup(text, this, null);
        //else
            sampleButton.Setup(text, this, inventionType);
    }
    override protected void AddButtons()
    {
        int counter = 0;

        // Adding invention name 
        AddButton("Invention", null);
        ////Adding possibleStatues
        AddButton("Status", null);
        ////Adding invention price
        AddButton("Science points", null);
        if (Game.player != null)
            foreach (var next in Game.player.inventions.list)
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
}