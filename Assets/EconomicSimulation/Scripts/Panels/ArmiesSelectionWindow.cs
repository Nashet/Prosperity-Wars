using Nashet.UnityUIUtils;
using Nashet.ValueSpace;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{

    public class ArmiesSelectionWindow : DragPanel
    {
        [SerializeField]
        private Text caption;

        [SerializeField]
        private Button merge, split, mobilize;

        private static ArmiesSelectionWindow thisObject;
        public static ArmiesSelectionWindow Get
        {
            get { return thisObject; }
        }

        private void Start()
        {
            //base.Start();
            thisObject = this;
            Hide();
            GUIChanger.Apply(gameObject);
        }

        public override void Refresh()
        {
            if (Game.selectedArmies.Count == 0)
                Hide();
            else if (Game.selectedArmies.Count == 1)
            {
                caption.text = "Selected 1 army - " + Game.selectedArmies[0].FullName;
                if (Game.selectedArmies[0].getCorps().Count() > 1)
                    split.interactable = true;
                else
                    split.interactable = false;
                merge.interactable = false;
            }
            else
            {
                split.interactable = false;
                if (Game.selectedArmies.All(x => x.Province == Game.selectedArmies[0].Province))
                    merge.interactable = true;
                else
                    merge.interactable = false;
                caption.text = "Selected " + Game.selectedArmies.Count + " armies (" + Game.selectedArmies.Sum(x => x.getSize()) + ")";
            }
        }
        public void OnMergeClick()
        {
            while (Game.selectedArmies.Count > 1)
            {
                Game.selectedArmies[0].JoinIn(Game.selectedArmies[1]);
            }
            Refresh();
            Game.provincesToRedrawArmies.Add(Game.selectedArmies[0].Province);
            MainCamera.militaryPanel.Refresh();
        }
        public void OnSplitClick()
        {
            Game.selectedArmies.First().balance(Procent._50Procent);
            Game.provincesToRedrawArmies.Add(Game.selectedArmies.First().Province);
            Refresh();
            MainCamera.militaryPanel.Refresh();
            
        }
        public void OnDemobilizeClick()
        {
            foreach (var item in Game.selectedArmies.ToList())
            {
                item.demobilize();
            }
            MainCamera.militaryPanel.Refresh();
        }
    }
}