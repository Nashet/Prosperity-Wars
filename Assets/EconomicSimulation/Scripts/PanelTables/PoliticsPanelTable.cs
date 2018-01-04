using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

// represen each opunit record in table

public class PoliticsPanelTable : MyTable
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
    protected void AddButton(string text, AbstractReform type)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        //newButton.transform.SetParent(contentPanel, false);
        newButton.transform.SetParent(gameObject.transform);
        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        //if (inventionType == null)
        //    sampleButton.Setup(text, this, null);
        //else
        sampleButton.Setup(text, type);
    }
    override protected void AddButtons()
    {
        int counter = 0;
        addHeader();
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

    protected override void addHeader()
    {
        // Adding reform name
        AddButton("Reform", null);

        ////Adding Status
        AddButton("Status", null);

        ////Adding Can change possibility
        // AddButton("Can change", null);
    }
}