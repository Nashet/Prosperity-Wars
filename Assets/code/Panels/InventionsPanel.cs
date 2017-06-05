using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
public class InventionsPanel : DragPanel
{  
    public ScrollRect table;
    public Text descriptionText;
    public Button inventButton;
    public Invention selectedInvention;
    // Use this for initialization
    void Start()
    {
        MainCamera.inventionsPanel = this;
        inventButton.interactable = false;
        hide();
    }       
    //public void hide()
    //{
    //    inventionsPanel.SetActive(false);
    //    //todo add button removal?      
    //}
    public void show(bool bringOnTop)
    {
        gameObject.SetActive(true);
        if (bringOnTop)
            panelRectTransform.SetAsLastSibling();
    }

    public void onInventClick()
    {
        if (!Game.Player.inventions.isInvented(selectedInvention) && Game.Player.sciencePoints.get() >= selectedInvention.cost.get())
        {
            Game.Player.invent(selectedInvention);
            inventButton.interactable = false;
            MainCamera.topPanel.refresh();
            if (MainCamera.buildPanel.isActiveAndEnabled) MainCamera.buildPanel.refresh();
            if (MainCamera.politicsPanel.isActiveAndEnabled) MainCamera.politicsPanel.refresh(true);
            if (MainCamera.factoryPanel.isActiveAndEnabled) MainCamera.factoryPanel.refresh();
            //Hide();
            //show();
            refresh();
        }
    }
    public void refresh()
    {
        hide();
        if (selectedInvention != null)
        {
            descriptionText.text = "Science points: " + Game.Player.sciencePoints
                + "\n\n" + selectedInvention + " : " + selectedInvention.getDescription();

            // invention available
            if (!Game.Player.inventions.isInvented(selectedInvention) && Game.Player.sciencePoints.get() >= selectedInvention.cost.get())
            {
                inventButton.GetComponentInChildren<Text>().text = "Invent " + selectedInvention.ToString();
                inventButton.interactable = true;
            }
            else
            {
                inventButton.interactable = false;
                if (Game.Player.inventions.isInvented(selectedInvention))
                    inventButton.GetComponentInChildren<Text>().text = "Already invented " + selectedInvention.ToString();
                else
                    inventButton.GetComponentInChildren<Text>().text = "Not enough SP to invent " + selectedInvention.ToString();

            }
        }
        else
        {
            inventButton.interactable = false;
            descriptionText.text = "Select invention from left panel";

        }

        show(false);
    }
    // Update is called once per frame
    //   void Update () {

    //}
}
