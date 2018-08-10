using System.Text;
using Nashet.UnityUIUtils;
using UnityEngine;
using UnityEngine.UI;

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
        private void Start()
        {
            MainCamera.inventionsPanel = this;
            inventButton.interactable = false;
            GetComponent<RectTransform>().position = new Vector2(0f, -458f + Screen.height);
            Hide();
        }

        public void onInventClick()
        {
            if (!Game.Player.Science.IsInvented(selectedInvention) && Game.Player.Science.Points >= selectedInvention.getCost().get())
            {
                Game.Player.Science.Invent(selectedInvention);
                inventButton.interactable = false;
                MainCamera.topPanel.Refresh();
                if (MainCamera.buildPanel.isActiveAndEnabled) MainCamera.buildPanel.Refresh();
                if (MainCamera.politicsPanel.isActiveAndEnabled) MainCamera.politicsPanel.Refresh();
                if (MainCamera.factoryPanel.isActiveAndEnabled) MainCamera.factoryPanel.Refresh();
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
            sb.Append("Science points: ").Append(Game.Player.Science.Points.ToString("F0"));//.Append(" + ");
            //sb.Append(Options.defaultSciencePointMultiplier * spModifier).Append(" Modifiers: ").Append(Options.defaultSciencePointMultiplier * scienceModifier);
            if (selectedInvention == null)
            {
                inventButton.interactable = false;
                inventButton.GetComponentInChildren<Text>().text = "Select from left";
                sb.Append("\n\nSelect invention from left panel");
            }
            else
            {
                sb.Append("\n\n").Append(selectedInvention).Append(" : ").Append(selectedInvention.FullName);

                // invention available
                if (!Game.Player.Science.IsInvented(selectedInvention) && Game.Player.Science.Points >= selectedInvention.getCost().get())
                {
                    inventButton.GetComponentInChildren<Text>().text = "Invent " + selectedInvention;
                    inventButton.interactable = true;
                }
                else
                {
                    inventButton.interactable = false;
                    if (Game.Player.Science.IsInvented(selectedInvention))
                        inventButton.GetComponentInChildren<Text>().text = "Already invented " + selectedInvention;
                    else
                        inventButton.GetComponentInChildren<Text>().text = "Not enough Science points to invent " + selectedInvention;
                }
            }
            descriptionText.text = sb.ToString();
        }
    }
}