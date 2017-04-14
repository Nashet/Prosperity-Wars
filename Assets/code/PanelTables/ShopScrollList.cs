using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class ShopScrollList : MyTable
{

    override protected void Refresh()
    {
        base.RemoveButtons();
        AddButtons();
        contentPanel.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentPanel.childCount / this.columnsAmount * rowHeight + 50);
    }
    protected void AddButton(string text, PopUnit record)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(contentPanel, false);
        //newButton.transform.SetParent(contentPanel);
        //newButton.transform.localScale = Vector3.one;

        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, record);
    }
    protected void AddButton(string text, PopUnit record, string toolTip)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(contentPanel, false);
        //newButton.transform.SetParent(contentPanel);
        //newButton.transform.localScale = Vector3.one;

        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, record);
        newButton.GetComponentInChildren<ToolTipHandler>().tooltip = toolTip;



        newButton.GetComponentInChildren<ToolTipHandler>().tip = MainTooltip.thatObj;



    }
    override protected void AddButtons()
    {
        int counter = 0;
        if (Game.popsToShowInPopulationPanel != null)
        {
            // Adding nomber
            AddButton("Number", null);
            // Adding PopType
            AddButton("Type", null);
            ////Adding population
            AddButton("Population", null);
            ////Adding culture
            AddButton("Culture", null);
            ////Adding province
            AddButton("Province", null);
            ////Adding education
            AddButton("Education", null);
            ////Adding storage
            //if (null.storage != null)
            AddButton("Cash", null);
            //else AddButton("Administration", null);
            ////Adding needs fulfilling
            AddButton("Needs fullfilled", null);
            ////Adding loyalty
            AddButton("Loyalty", null);
            foreach (PopUnit record in Game.popsToShowInPopulationPanel)
            {

                // Adding nomber
                AddButton(Convert.ToString(counter), record);
                // Adding PopType
                AddButton(record.type.ToString(), record);
                ////Adding population
                AddButton(System.Convert.ToString(record.population), record);
                ////Adding culture
                AddButton(record.culture.name, record);
                ////Adding province
                AddButton(record.province.ToString(), record);
                ////Adding education
                AddButton(record.education.ToString(), record);
                ////Adding storage
                //if (record.storage != null)
                //    AddButton(record.storage.ToString(), record);
                //else AddButton("Administration", record);        
                AddButton(record.wallet.ToString(), record);
                ////Adding needs fulfilling
                AddButton(record.NeedsFullfilled.ToString(), record);

                ////Adding loyalty
                string accu;
                record.modifiersLoyaltyChange.getModifier(Game.player, out accu);
                AddButton(record.loyalty.ToString(), record, accu);

                counter++;
                //contentPanel.r
            }
        }
    }
}