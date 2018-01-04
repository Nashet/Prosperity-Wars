using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

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
        GetComponent<RectTransform>().position = new Vector2(0f, -458f + Screen.height);
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
        if (!Game.Player.isInvented(selectedInvention) && Game.Player.sciencePoints.get() >= selectedInvention.cost.get())
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
        var sb = new StringBuilder();
        string scienceModifier;
        var spModifier = Country.modSciencePoints.getModifier(Game.Player, out scienceModifier);
        sb.Append("Science points: ").Append(Game.Player.sciencePoints).Append(" + ");
        sb.Append(Game.Player.getSciencePointsBase().multiplyOutside(spModifier)).Append(" Modifiers: ").Append(scienceModifier);
        if (selectedInvention != null)
        {
            sb.Append("\n\n").Append(selectedInvention).Append(" : ").Append(selectedInvention.getDescription());

            // invention available
            if (!Game.Player.isInvented(selectedInvention) && Game.Player.sciencePoints.get() >= selectedInvention.cost.get())
            {
                inventButton.GetComponentInChildren<Text>().text = "Invent " + selectedInvention.ToString();
                inventButton.interactable = true;
            }
            else
            {
                inventButton.interactable = false;
                if (Game.Player.isInvented(selectedInvention))
                    inventButton.GetComponentInChildren<Text>().text = "Already invented " + selectedInvention.ToString();
                else
                    inventButton.GetComponentInChildren<Text>().text = "Not enough Science points to invent " + selectedInvention.ToString();
            }
        }
        else
        {
            inventButton.interactable = false;
            sb.Append("\n\nSelect invention from left panel");
        }
        descriptionText.text = sb.ToString();
        show(false);
    }
    // Update is called once per frame
    //   void Update () {

    //}
}
