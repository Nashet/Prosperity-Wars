
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Nashet.UnityUIUtils;

namespace Nashet.EconomicSimulation
{
    public class BottomPanel : Hideable, IRefreshable
    {
        [SerializeField]
        private Text generalText;
        // Use this for initialization
        void Awake()
        {
            MainCamera.bottomPanel = this;
            Hide();
        }

        public override void Show()
        {
            base.Show();
            //panelRectTransform.SetAsLastSibling();
            Refresh();
        }
        public void Refresh()
        {
            generalText.text = "Economic Simulation Demo v0.16.0";
        }

        public void onStatisticsClick()
        {
            if (MainCamera.StatisticPanel.isActiveAndEnabled)
                MainCamera.StatisticPanel.Hide();
            else
                MainCamera.StatisticPanel.show(true);
        }
        public void onddMapModesChange(int newMapMode)
        {
            if (Game.getMapMode() != newMapMode)
                Game.redrawMapAccordingToMapMode(newMapMode);

        }
    }
}