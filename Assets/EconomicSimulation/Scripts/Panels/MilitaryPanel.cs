using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text;
using System.Linq;
using Nashet.UnityUIUtils;
using Nashet.ValueSpace;
using Nashet.Utils;
using System;

namespace Nashet.EconomicSimulation
{
    public class MilitaryPanel : DragPanel
    {
        [SerializeField]
        private Dropdown ddProvinceSelect;

        [SerializeField]
        private Text allArmySizeText, captionText, sendingArmySizeText;

        [SerializeField]
        private Slider armySendLimit;

        [SerializeField]
        private Button sendArmy;

        private readonly StringBuilder sb = new StringBuilder();

        private readonly List<Province> availableProvinces = new List<Province>();
        private Army virtualArmyToSend;
        // Use this for initialization
        void Start()
        {
            MainCamera.militaryPanel = this;
            GetComponent<RectTransform>().position = new Vector2(180f, 111);// + Screen.height);
            Hide();
            //rebuildDropDown();
        }


        public override void Refresh()
        {
            refresh(true);
        }
        public void refresh(bool rebuildDropdown)
        {

            if (rebuildDropdown)
            {
                //Game.player.homeArmy.balance(Game.player.sendingArmy, new Procent(armySendLimit.value));
                armySendLimit.value = 0; // cause extra mobilization
                rebuildDropDown();
            }
            sb.Clear();
            sb.Append("Military of ").Append(Game.Player);
            captionText.text = sb.ToString();

            sb.Clear();
            sb.Append("Home army: ").Append(Game.Player.getDefenceForces().getName());
            allArmySizeText.text = sb.ToString();

            if (virtualArmyToSend == null)
                virtualArmyToSend = new Army(Game.Player);
            sb.Clear();
            sb.Append("Sending army: ").Append(virtualArmyToSend.getName());
            sendingArmySizeText.text = sb.ToString();
            //sendArmy.interactable = virtualArmyToSend == "0" ? false : true;
            sendArmy.interactable = virtualArmyToSend.getSize() > 0 ? true : false;

        }

       
        public void onMobilizationClick()
        {
            //if (Game.Player.homeArmy.getSize() == 0)
            //  Game.Player.homeArmy = new Army(Game.Player);
            Game.Player.mobilize(Game.Player.getAllProvinces());
            //onArmyLimitChanged(0f);
            //MainCamera.tradeWindow.refresh();
            refresh(false);
        }
        public void onDemobilizationClick()
        {
            Game.Player.demobilize();
            virtualArmyToSend.demobilize();
            //MainCamera.tradeWindow.refresh();
            refresh(false);
        }
        public void onSendArmyClick()
        {
            if (ddProvinceSelect.value < availableProvinces.Count)
                // province here shouldn't be null
                Game.Player.sendArmy(availableProvinces[ddProvinceSelect.value], new Procent(armySendLimit.value));
            else
                Debug.Log("Failed to send Army");
            refresh(false);
        }
        public void show(Province province)
        {
            Show();
            if (province != null)
            {
                var list = Game.Player.getAllNeighborProvinces().Distinct().Where(x => Country.canAttack.isAllTrue(x, Game.Player)).OrderBy(x => x.Country.GetNameWeight());
                //var found = list.IndexOf(province);
                var found = list.FindIndex(x => x == province);

                ddProvinceSelect.value = found;
                if (found < 0)
                    Debug.Log("Didn't find province " + province);
            }
           
        }

        void rebuildDropDown()
        {
            ddProvinceSelect.interactable = true;
            ddProvinceSelect.ClearOptions();
            byte count = 0;
            availableProvinces.Clear();
            foreach (Province next in Game.Player.getAllNeighborProvinces().Distinct().Where(x => Country.canAttack.isAllTrue(x, Game.Player)).OrderBy(x => x.Country.GetNameWeight()))
            {
                //if (next.isAvailable(Game.player))
                {
                    ddProvinceSelect.options.Add(new Dropdown.OptionData() { text = next.ToString() + " (" + next.Country + ")" });
                    availableProvinces.Add(next);

                    //selectedReformValue = next;
                    // selecting non empty option
                    //ddProvinceSelect.value = count; 
                    count++;
                }
            }
            ddProvinceSelect.RefreshShownValue();
            //onddProvinceSelectValueChanged(); // need it to set correct caption in DropDown
        }
        public void onddProvinceSelectValueChanged()
        {

        }
        public void onArmyLimitChanged(float value)
        {
            //Game.Player.staff.consolidateArmies();
            //actually creates new army here
            //virtualArmyToSend = (Game.Player.staff.consolidateArmies().getSize() * value).ToString("0");
            virtualArmyToSend = Game.Player.consolidateArmies().balance(new Procent(value));
            //virtualArmyToSend = del.getShortName();
            refresh(false);
        }
    }
}