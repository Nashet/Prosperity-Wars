using Nashet.EconomicSimulation;
using Nashet.UnityUIUtils;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.UISystem
{
    internal class InventionsPanel : DragPanel
    {
        [SerializeField]
        protected InventionsPanelTable table;

        [SerializeField]
        protected Text descriptionText;

        [SerializeField]
        protected Button inventButton;

        protected Invention selectedInvention;

        // Use this for initialization
        private void Start()
        {
            //MainCamera.inventionsPanel = this;
            inventButton.interactable = false;
            GetComponent<RectTransform>().position = new Vector2(0f, -458f + Screen.height);
            Hide();
        }

        public void onInventClick()
        {
            if (!Game.Player.Science.IsInvented(selectedInvention) && Game.Player.Science.Points >= selectedInvention.Cost.get())
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

        protected void OnClickedOn(object sender, EventArgs e)
        {
            var isInventionArgs = e as InventionEventArgs;
            if (isInventionArgs != null)
            {
                if (isInventionArgs.Invention == null)
                {
                    if (Instance.isActiveAndEnabled)
                        Instance.Hide();
                    else
                        Instance.Show();
                }
                else
                {
                    Instance.selectedInvention = isInventionArgs.Invention;
                    Instance.Show();
                }
            }
        }

        public override void Refresh()
        {
            table.Refresh();
            var sb = new StringBuilder();
            string scienceModifier;
            var spModifier = Science.modSciencePoints.getModifier(Game.Player, out scienceModifier);
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
                if (!Game.Player.Science.IsInvented(selectedInvention) && Game.Player.Science.Points >= selectedInvention.Cost.get())
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

        //todo Instance
        protected static InventionsPanel Instance;
        protected new void Awake()
        {
            base.Awake();
            Instance = this;
            UIEvents.ClickedOn += OnClickedOn;
        }
    }
}