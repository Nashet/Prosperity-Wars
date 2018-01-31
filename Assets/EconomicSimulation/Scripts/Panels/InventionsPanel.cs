using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using Nashet.UnityUIUtils;
namespace Nashet.EconomicSimulation
{
    public class InventionsPanel : DragPanel
    {
        [SerializeField]
        private InventionsPanelTable table;

        [SerializeField]
        private Text descriptionText;

        [SerializeField]
        private Button inventButton;

        private Invention selectedInvention;

        // Use this for initialization
        void Start()
        {
            MainCamera.inventionsPanel = this;
            inventButton.interactable = false;
            GetComponent<RectTransform>().position = new Vector2(0f, -458f + Screen.height);
            Hide();
        }
        public void onInventClick()
        {
            if (!Game.Player.isInvented(selectedInvention) && Game.Player.sciencePoints.get() >= selectedInvention.getCost().get())
            {
                Game.Player.invent(selectedInvention);
                inventButton.interactable = false;
                MainCamera.topPanel.Refresh();
                if (MainCamera.buildPanel.isActiveAndEnabled) MainCamera.buildPanel.Refresh();
                if (MainCamera.politicsPanel.isActiveAndEnabled) MainCamera.politicsPanel.Refresh();
                if (MainCamera.factoryPanel.isActiveAndEnabled) MainCamera.factoryPanel.Refresh();
                //Hide();
                //show();
                Refresh();
            }
        }
        public void selectInvention(Invention newSelection)
        {
            selectedInvention = newSelection;
        }
        public override void Refresh()
        {
            table.Refresh();
            var sb = new StringBuilder();
            string scienceModifier;
            var spModifier = Country.modSciencePoints.getModifier(Game.Player, out scienceModifier);
            sb.Append("Science points: ").Append(Game.Player.sciencePoints).Append(" + ");
            sb.Append(Game.Player.getSciencePointsBase().Copy().multiply(spModifier)).Append(" Modifiers: ").Append(scienceModifier);
            if (selectedInvention == null)
            {
                inventButton.interactable = false;
                inventButton.GetComponentInChildren<Text>().text = "Select from left";
                sb.Append("\n\nSelect invention from left panel");
            }
            else
            {
                sb.Append("\n\n").Append(selectedInvention).Append(" : ").Append(selectedInvention.getDescription());

                // invention available
                if (!Game.Player.isInvented(selectedInvention) && Game.Player.sciencePoints.get() >= selectedInvention.getCost().get())
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
            descriptionText.text = sb.ToString();
        }
    }
}