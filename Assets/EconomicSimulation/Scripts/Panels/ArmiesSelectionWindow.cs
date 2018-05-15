using Nashet.UnityUIUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nashet.EconomicSimulation
{

    public class ArmiesSelectionWindow : DragPanel
    {
        [SerializeField]
        private Text caption;

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
            if (Game.selectedUnits.Count == 0)
                Hide();
            else if (Game.selectedUnits.Count == 1)
                caption.text = "Selected 1 army";
            else
                caption.text = "Selected " + Game.selectedUnits.Count + " armies";
        }

    }
}