using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;


public class PopulationPanelTable : MyTable
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
        newButton.transform.SetParent(contentPanel, true);
        //newButton.transform.SetParent(contentPanel);
        //newButton.transform.localScale = Vector3.one;

        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, record);
    }
    protected void AddButton(string text, PopUnit record, string toolTip)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(contentPanel, true);
        //newButton.transform.SetParent(contentPanel);
        //newButton.transform.localScale = Vector3.one;

        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, record);
        newButton.GetComponentInChildren<ToolTipHandler>().tooltip = toolTip;
        newButton.GetComponentInChildren<ToolTipHandler>().tip = MainTooltip.thatObj;
    }
    protected void AddButton(string text, PopUnit record, Func<string> dynamicTooltip)
    {
        GameObject newButton = buttonObjectPool.GetObject();
        newButton.transform.SetParent(contentPanel, true);
        //newButton.transform.SetParent(contentPanel);
        //newButton.transform.localScale = Vector3.one;

        SampleButton sampleButton = newButton.GetComponent<SampleButton>();
        sampleButton.Setup(text, this, record);
        newButton.GetComponentInChildren<ToolTipHandler>().dynamicString = dynamicTooltip;
        newButton.GetComponentInChildren<ToolTipHandler>().tip = MainTooltip.thatObj;
    }
   
    override protected void AddButtons()
    {
        int counter = 0;
        if (Game.popsToShowInPopulationPanel != null)
        {
            // Adding nomber
            //AddButton("Number");
            // Adding PopType
            AddButton("Type");
            ////Adding population
            AddButton("Population");
            ////Adding culture
            AddButton("Culture");
            ////Adding province
            AddButton("Province");
            ////Adding education
            AddButton("Education");
            ////Adding storage
            //if (null.storage != null)
            AddButton("Cash");
            //else AddButton("Administration");
            ////Adding needs fulfilling
            AddButton("Needs fulfilled");
            ////Adding loyalty
            AddButton("Loyalty");
            ////Adding loyalty
            AddButton("Unemployment");
            foreach (PopUnit record in Game.popsToShowInPopulationPanel)
            {               
                //AddButton(Convert.ToString(counter), record);
                // Adding PopType
                AddButton(record.type.ToString(), record);
                ////Adding population
                AddButton(System.Convert.ToString(record.getPopulation()), record);
                ////Adding culture
                AddButton(record.culture.name, record);
                ////Adding province
                AddButton(record.province.ToString(), record.province, "Click to select this province");
                ////Adding education
                AddButton(record.education.ToString(), record);   
                
                ////Adding cash
                AddButton(record.wallet.ToString(), record);

                ////Adding needs fulfilling

                PopUnit ert = record;
                AddButton(record.needsFullfilled.ToString(), record,
                    () => ert.consumedTotal.ToStringWithLines()
                    //delegate () { return ert.consumedTotal.ToString(); }
                    //record.consumedTotal.ToString()
                    );

                ////Adding loyalty
                string accu;
                record.modifiersLoyaltyChange.getModifier(Game.player, out accu);
                AddButton(record.loyalty.ToString(), record, accu);

                //Adding Unemployment
                AddButton(record.getUnemployedProcent().ToString(), record);
                counter++;
                //contentPanel.r
            }
        }
    }
}